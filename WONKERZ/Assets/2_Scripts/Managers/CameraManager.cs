using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Debug;
using System.Linq;
using Schnibble;
using Schnibble.Managers;
using Schnibble.Rendering;

namespace Wonkerz
{

    // Known issue :
    //  * If a camera is active in a new scene, it can take the lead over the active_camera
    //  ** FIX : Disable all cameras by default
    public class CameraManager : MonoBehaviour, IControllable
    {
        [Header("Mandatory")]
        public GameObject transitionCameraRef;
        public GameObject deathCameraRef;
        [Header("Debug")]
        GameCamera _active_camera;

        GameCamera _defaultCamera = null;
        GameCamera defaultCamera {
                get {
                    if (_defaultCamera == null) {
                        var camGO = new GameObject("defaultCamera");
                        var cam = camGO.AddComponent<Camera>();
                        _defaultCamera = camGO.AddComponent<UICamera>();
                        _defaultCamera.cam = cam;
                        _defaultCamera.disable();

                        camGO.transform.parent = this.gameObject.transform;
                    }
                    return _defaultCamera;
                }
        }
        public GameCamera active_camera {
            get
            {
                return _active_camera;
            }
            set
            {
                _active_camera = value;
                onCameraChanged?.Invoke();
            }
        }

        public Dictionary<GameCamera.CAM_TYPE, GameCamera> cameras =
            new Dictionary<GameCamera.CAM_TYPE, GameCamera>();
        public bool inTransition = false;

        [Header("Tweaks")]
        public float pressTimeToResetView = 0.01f;
        public float transitionDuration = 2f;
        public float deathCamDuration = 2f;
        public GameCamera.CAM_TYPE[] cameraRotationOrder;

        [Header("Internals")]
        private Transform transitionStart = null;
        private Transform transitionEnd = null;
        private GameCamera nextCamera = null;
        private float transiTime = 0f;
        private GameObject transitionCameraInst;
        private GameObject deathCamInst;
        private float elapsedTimeToResetCamView = 0f;

        public Action onCameraChanged;

        ///
        private static CameraManager inst;

        public static CameraManager Instance
        {
            get { return inst ?? (inst = Access.managers.cameraMgr); }
            private set { inst = value; }
        }

        public void init() {
            this.Log("init.");
            inTransition = false;

            SceneManager.sceneLoaded += OnSceneLoaded;

            Access.managers.playerInputsMgr.player1.Attach(this as IControllable);

            //changeCamera(GameCamera.CAM_TYPE.INIT, false);
        }

        // Update is called once per frame
        void Update()
        {
            if (inTransition)
            {
                if (transition(transitionStart, transitionEnd))
                {
                    inTransition = false;
                    operateCameraSwitch(nextCamera);
                    transitionCameraInst = null;
                }
            }
        }

        void OnDestroy()
        {
            Access.managers.playerInputsMgr.player1.Detach(this as IControllable);
        }

        // Switch cameras between HUB cameras
        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            if ((Entry.Get((int)PlayerInputs.InputCode.CameraReset) as GameInputButton).GetState().down)
            {
                elapsedTimeToResetCamView += Time.unscaledDeltaTime;
                if (elapsedTimeToResetCamView > pressTimeToResetView)
                {
                    active_camera.resetView();
                    elapsedTimeToResetCamView = 0f;
                }
            }
            else
            {
                elapsedTimeToResetCamView = 0f;
            }
        }

        void Reset() {
            if (active_camera) {
                active_camera.disable();
            }
            active_camera = null;
            cameras.Clear();
        }

        // resets CameraManager status (active camera, etc..) upon new scene load
        // Avoid bad transitions and such for now
        // Might need to change if we decide to make a Spyro style entry in the level thru portals
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // If we are loading in single mode
            // we reset the cameraManager and take care of initialization of the new camera in the scene.
            // Else we dont know yet what to do with the cameras, they might not even be initialized at all, etc...
            //if (mode == LoadSceneMode.Single)
            {
                this.Log("OnSceneLoaded :" + scene.name);
                var camera = findCameraInScene(GameCamera.CAM_TYPE.INIT, scene);
                if (camera)
                {
                    var initCamera = camera.GetComponent<InitCamera>();
                    if (initCamera != null && initCamera.nextCam != null)
                    {
                        operateCameraSwitch(initCamera);
                        operateCameraSwitch(initCamera.nextCam);
                    }
                } else {
                    if (active_camera == null) {
                        operateCameraSwitch(null);
                    }
                }
            }
        }

        public void OnTargetChange(Transform t)
        {
            if (active_camera != null && active_camera.TryGetComponent<PlayerCamera>(out PlayerCamera cam))
            {
                cam.playerRef = t.gameObject;
                cam.init();
                cam.resetFocus();
            }
        }

        // Find Camera from its type within current scene
        // Only one per CAM_TYPE is retrieved for now as we don't need more
        // Thus if we want to have multiple cameras for the hub, we'll need to update
        // this logic.
        private GameCamera findCameraInScene(GameCamera.CAM_TYPE iType, Scene scene)
        {
            GameCamera[] game_cams = FindObjectsOfType<GameCamera>(true/*include inactives*/);
            GameCamera retval = null;

            for (int i = 0; i < game_cams.Length; i++)
            {
                GameCamera currcam = game_cams[i];
                if (currcam.camType == iType && currcam.gameObject.scene == scene)
                {
                    retval = currcam;
                    break;
                }
            }
            return retval;
        }

        private List<GameCamera> findCamerasInScene(GameCamera.CAM_TYPE iType)
        {
            GameCamera[] game_cams = FindObjectsOfType<GameCamera>(true/*include inactives*/);
            List<GameCamera> retval = new List<GameCamera>();

            for (int i = 0; i < game_cams.Length; i++)
            {
                GameCamera currcam = game_cams[i];
                if (currcam.camType == iType)
                {
                    retval.Add(currcam);
                }
            }
            return retval;
        }

        void CleanUpNull() {
            var null_keys = cameras.Where(e => e.Value == null).Select(e => e.Key).ToList();
            foreach (var rm_me in null_keys)
            {
                cameras.Remove(rm_me);
            }
        }

        bool findCamera(GameCamera.CAM_TYPE type, out GameCamera result) {
            result = null;
            if (!cameras.ContainsKey(type))
            {
                result = findCameraInScene(type, SceneManager.GetActiveScene());
                if (result == null)
                {
                    this.LogError("Unable to find a camera for type : " + type.ToString());
                    return false;
                }

                return true;
            }

            return cameras.TryGetValue(type, out result);
        }

        // Retrieves only the first found CAM_TYPE camera
        public void changeCamera(GameCamera.CAM_TYPE iNewCamType, bool iTransitionCam)
        {
            this.Log("changeCamera");
            // 0 : Clean up nulls CameraType to free space for new ones
            CleanUpNull();
            // 1 : Do I have a CAM_TYPE available ?
            if (!findCamera(iNewCamType, out nextCamera)) {
                if (cameras.Count > 0)
                {
                    this.LogWarn("Failed to find camera of type" + iNewCamType + "=> Selecting first of the list as fallback.");
                    nextCamera = cameras.Values.GetEnumerator().Current;
                    if (nextCamera == null) {
                        this.LogWarn("Failed to find camera of type" + iNewCamType + "=> use default camera.");
                        nextCamera = defaultCamera;
                        iTransitionCam = false;
                    }
                }
                else
                {
                    this.LogWarn("Failed to find camera of type" + iNewCamType + "=> use default camera.");
                    nextCamera = defaultCamera;
                    iTransitionCam = false;
                }
            }
            // 2 : Deactivate active_camera
            if (!iTransitionCam)
            {
                this.Log("changeCamera is not a transitionCam => operateCameraSwitch to " + nextCamera.name);
                operateCameraSwitch(nextCamera);
                return;
            }

            if ((active_camera != null) && (active_camera.gameObject.scene.IsValid()))
            {
                if (active_camera.gameObject.scene == nextCamera.gameObject.scene)
                initTransition(nextCamera);
                else
                operateCameraSwitch(nextCamera);
            }
            else
            {
                operateCameraSwitch(nextCamera);
            }
        }

        public void changeCamera(GameCamera nextCam, bool iTransitionCam) {
            this.Log("changeCamera");

            nextCamera = nextCam;

            if (!iTransitionCam)
            {
                operateCameraSwitch(nextCamera);
            }

            // transition if previous camera was defined
            if ((active_camera != null) && (active_camera.gameObject.scene.IsValid()))
            {
                if (active_camera.gameObject.scene == nextCamera.gameObject.scene)
                initTransition(nextCamera);
                else
                operateCameraSwitch(nextCamera);
            }
            else
            {
                operateCameraSwitch(nextCamera);
            }
        }

        public void changeCamera(CinematicCamera iCineCam, bool iTransitionCam)
        {
            if (iCineCam == null)
            {
                this.LogError("Unable to changeCamera for cinemati : null input");
            }

            nextCamera = iCineCam;

            if (!iTransitionCam)
            {
                operateCameraSwitch(nextCamera);
            }

            // transition if previous camera was defined
            if ((active_camera != null) && (active_camera.gameObject.scene.IsValid()))
            {
                if (active_camera.gameObject.scene == nextCamera.gameObject.scene)
                initTransition(nextCamera);
                else
                operateCameraSwitch(nextCamera);
            }
            else
            {
                operateCameraSwitch(nextCamera);
            }
        }

        public void initTransition(GameCamera iNextCam)
        {

            if (iNextCam.gameObject == active_camera.gameObject)
            return;

            // switch on intermediate transition camera
            if (transitionCameraInst == null)
            {
                transitionCameraInst = Instantiate(transitionCameraRef);
                switchToonPipeline(active_camera.gameObject, transitionCameraInst);
            }

            transitionCameraInst.transform.position = active_camera.transform.position;
            transitionCameraInst.transform.rotation = active_camera.transform.rotation;

            active_camera.enabled = false;
            active_camera.cam.enabled = false;
            active_camera.gameObject.SetActive(false);
            active_camera = transitionCameraInst.GetComponent<CinematicCamera>();

            inTransition = true;
            transiTime = 0f;

            transitionStart = new GameObject("TRANSI_START").transform; // static start point for LERP
            transitionStart.SetParent(transform);
            transitionStart.transform.position = active_camera.transform.position;
            transitionStart.transform.rotation = active_camera.transform.rotation;

            transitionEnd = iNextCam.transform; // keep the end point dynamic


            active_camera.enabled = true;
            active_camera.cam.enabled = true;
            active_camera.gameObject.SetActive(true);
        }

        public void endTransition()
        {
            inTransition = false;
            GameCamera camToDestroy = findCameraInScene(GameCamera.CAM_TYPE.TRANSITION, SceneManager.GetActiveScene());
            if (camToDestroy != null)
            Destroy(camToDestroy.gameObject);
            if (transitionStart.gameObject != null)
            Destroy(transitionStart.gameObject);
        }

        private void operateCameraSwitch(GameCamera iNewCam)
        {
            if (inTransition)
            {
                this.LogWarn("Trying to operate switch when inTransition is true.");
                return;
            }

            if (iNewCam == null)
            {
                this.LogWarn("Trying to operate camera switch to null camera => use default");
                iNewCam = defaultCamera;
            }

            if (active_camera != null && iNewCam.gameObject == active_camera.gameObject) {
                this.Log("New camera is already the active camera.");
                return;
            }


            //if ((active_camera == null) && (iNewCam.camType != GameCamera.CAM_TYPE.INIT))
            //{
            //   changeCamera(GameCamera.CAM_TYPE.INIT, false);
            //}

            bool sceneTransition =
                (active_camera != null) &&
                (active_camera.camType == GameCamera.CAM_TYPE.INIT) &&
                (iNewCam.camType != GameCamera.CAM_TYPE.INIT);

            if (active_camera != null)
            {
                switchToonPipeline(active_camera.gameObject, iNewCam.gameObject);
                active_camera.disable();
            }

            // For debug purposes, but we log at the end => there can still be errors after those lines, we dont know.
            string new_cam_name  = ((iNewCam == null) ? "null" : iNewCam.gameObject.name);
            string prev_cam_name = ((active_camera == null) ? "null" : active_camera.gameObject.name);

            active_camera = iNewCam;
            active_camera.enable();
            active_camera.init();

            PhysicsMaterialManager PMM = Access.PhysicsMaterialManager();
            if (!!PMM          ) PMM.SetCamera(active_camera.cam);

            //if (sceneTransition) Access.managers.sceneMgr.asyncTransitionLock = false;

            // Debug Cam switches below
            this.Log("CamSwitch : " + new_cam_name + " FROM " + prev_cam_name);
        }

        public bool interpolatePosition(Transform iStart, Transform iEnd)
        {
            float s = transiTime / transitionDuration;
            //s = s*s*(3f-2f*s); // smoothstep formula
            active_camera.transform.position = Vector3.Lerp(
                iStart.position,
                iEnd.position,
                s);
            active_camera.transform.rotation = Quaternion.Lerp(
                iStart.rotation,
                iEnd.rotation,
                s);
            transiTime += Time.deltaTime;

            return transiTime < transitionDuration;
        }

        // Returns true when transition is done, false otherwise
        public bool transition(Transform iStart, Transform iEnd)
        {
            if (active_camera == null)
            { // transition failed?
                return true;
            }

            if ((iStart == null) || (iEnd == null))
            {
                this.LogWarn("CameraManager::Tried to transition from/to null Transform. Forcing transition quit.");
                endTransition();
                return true;
            }

            if (!interpolatePosition(iStart, iEnd))
            {
                active_camera.transform.position = iEnd.position;
                active_camera.transform.rotation = iEnd.rotation;
                endTransition();
                return true;
            }
            return false;
        }

        private void switchToonPipeline(GameObject prevCam, GameObject newCam)
        {
            ToonPipeline prev_tp = prevCam.GetComponent<ToonPipeline>();
            ToonPipeline new_tp = newCam.GetComponent<ToonPipeline>();

            if ((prev_tp == null) || (new_tp == null))
            {
                this.LogWarn("Can't switch ToonPipeline. Previous or New Pipeline is missing from camera.");
                return;
            }

            new_tp.mgr       = prev_tp.mgr;
            new_tp.mainLight = prev_tp.mainLight;

            new_tp.enabled   = true;

            if (!new_tp.init_done) new_tp.init();
        }

        public void launchDeathCam()
        {
            if (deathCamInst != null)
            return;

            deathCamInst = Instantiate(deathCameraRef);
            switchToonPipeline(active_camera.gameObject, deathCamInst);

            deathCamInst.transform.position = active_camera.transform.position;
            deathCamInst.transform.rotation = active_camera.transform.rotation;

            deathCamInst.GetComponent<CinematicCamera>().launch();
            StartCoroutine(endDeathCam());
        }
        private IEnumerator endDeathCam()
        {
            yield return new WaitForSeconds(deathCamDuration);
            deathCamInst.GetComponent<CinematicCamera>().end();
            Destroy(deathCamInst.gameObject);
        }

        public bool TryResetView()
        {
            if (active_camera != null)
            {
                active_camera.resetView();
                return true;
            }
            return false;
        }

        public void changeMainLight(Light iLight)
        {
            ToonPipeline TP = active_camera.GetComponent<ToonPipeline>();
            if (!!TP)
            TP.mainLight = iLight;
        }
    }
}
