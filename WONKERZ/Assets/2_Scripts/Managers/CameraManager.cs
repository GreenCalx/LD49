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

// Known issue :
//  * If a camera is active in a new scene, it can take the lead over the active_camera
//  ** FIX : Disable all cameras by default
public class CameraManager : MonoBehaviour, IControllable
{
    [Header("Mandatory")]
    public GameObject transitionCameraRef;
    public GameObject deathCameraRef;
    [Header("Debug")]
    public GameCamera active_camera;
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
    public List<CameraFocusable> focusables;

    ///
    private static CameraManager inst;

    public static CameraManager Instance
    {
        get { return inst ?? (inst = Access.CameraManager()); }
        private set { inst = value; }
    }


    // Start is called before the first frame update
    void Start()
    {
        inTransition = false;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Access.PlayerInputsManager().player1.Attach(this as IControllable);
        focusables = new List<CameraFocusable>();
        
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
        Access.PlayerInputsManager().player1.Detach(this as IControllable);
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
        } else {
            elapsedTimeToResetCamView = 0f;
        }
    }

    // resets CameraManager status (active camera, etc..) upon new scene load
    // Avoid bad transitions and such for now
    // Might need to change if we decide to make a Spyro style entry in the level thru portals
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // refresh cameras and disable active_camera
        active_camera = null;
        cameras.Clear();

        // Set to init cam
        changeCamera(GameCamera.CAM_TYPE.INIT, false);
        if (active_camera!=null)
        {
            InitCamera initCam = active_camera.GetComponent<InitCamera>();
            if ((initCam!=null)&&(initCam.nextCam!=null))
            {
                operateCameraSwitch(initCam.nextCam);
            }
        }

    }

    public void OnTargetChange(Transform t) {
        if (active_camera.TryGetComponent<PlayerCamera>(out PlayerCamera cam)) {
            cam.playerRef = t.gameObject;
            cam.init();
        }
    }

    // Find Camera from its type within current scene
    // Only one per CAM_TYPE is retrieved for now as we don't need more
    // Thus if we want to have multiple cameras for the hub, we'll need to update
    // this logic.
    private GameCamera findCameraInScene(GameCamera.CAM_TYPE iType)
    {
        GameCamera[] game_cams = FindObjectsOfType<GameCamera>(true/*include inactives*/);
        GameCamera retval = null;

        for (int i = 0; i < game_cams.Length; i++)
        {
            GameCamera currcam = game_cams[i];
            if (currcam.camType == iType)
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

    // Retrieves only the first found CAM_TYPE camera
    public void changeCamera(GameCamera.CAM_TYPE iNewCamType, bool iTransitionCam)
    {
        // 0 : Clean up nulls CameraType to free space for new ones
        var null_keys = cameras.Where(e => e.Value == null).Select(e => e.Key).ToList();
        foreach (var rm_me in null_keys)
        {
            cameras.Remove(rm_me);
        }

        // 1 : Do I have a CAM_TYPE available ?
        nextCamera = null;
        if (!cameras.ContainsKey(iNewCamType))
        {
            // > N : Try to find CAM_TYPE in scene
            nextCamera = findCameraInScene(iNewCamType);
            if (nextCamera == null)
            {
                this.LogError("Unable to find a camera for type : " + iNewCamType.ToString());
                this.LogError("Failed to switch Camera. Selecting first of the list as fallback.");
                if (cameras.Count > 0)
                {
                    operateCameraSwitch(cameras.Values.GetEnumerator().Current);
                    return;
                }
                else
                {
                    this.LogError("No Camera available in CameraManager. Exiting changeCamera().");
                    return;
                }
            }
            //  > Add it to mgr cams
            cameras.Add(iNewCamType, nextCamera);
        }
        else
        {
            // cam already stored, retrieve it for transition
            cameras.TryGetValue(iNewCamType, out nextCamera);
        }

        // 2 : Deactivate active_camera
        if (!iTransitionCam)
        {
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
        GameCamera camToDestroy = findCameraInScene(GameCamera.CAM_TYPE.TRANSITION);
        if (camToDestroy!=null)
            Destroy(camToDestroy.gameObject);
        if (transitionStart.gameObject!=null)
            Destroy(transitionStart.gameObject);
    }

    private void operateCameraSwitch(GameCamera iNewCam)
    {
        if (inTransition)
            return;
        
        if (active_camera!=null)
        {
            if (iNewCam.gameObject==active_camera.gameObject)
                return;
        }


        if ((active_camera==null)&&(iNewCam.camType!=GameCamera.CAM_TYPE.INIT))
        {
            changeCamera(GameCamera.CAM_TYPE.INIT, false);
        }

        bool sceneTransition =  
            (active_camera!=null)&&
            (active_camera.camType==GameCamera.CAM_TYPE.INIT)&&
            (iNewCam.camType!=GameCamera.CAM_TYPE.INIT);


        if (active_camera != null)
        {
            switchToonPipeline(active_camera.gameObject, iNewCam.gameObject);

            active_camera.gameObject.SetActive(false);
            active_camera.enabled = false;
            active_camera.cam.enabled = false;

            // iNewCam.transform.position = active_camera.transform.position;
            // iNewCam.transform.rotation = active_camera.transform.rotation;
        }

        // Debug Cam switches below
        string new_cam_name = ((iNewCam==null)?"null":iNewCam.gameObject.name);
        string prev_cam_name = ((active_camera==null)?"null":active_camera.gameObject.name);
        this.Log("CamSwitch : " +  new_cam_name  + " FROM " + prev_cam_name);


        active_camera = iNewCam;
        
        active_camera.enabled = true;
        active_camera.cam.enabled = true;
        active_camera.gameObject.SetActive(true);

        active_camera.init();

        PhysicsMaterialManager PMM = Access.PhysicsMaterialManager();
        if (!!PMM)
            PMM.SetCamera(active_camera.cam);

        if (sceneTransition)
            Access.SceneLoader().asyncTransitionLock = false;
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
        if (active_camera==null)
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
        ToonPipeline prev_tp    = prevCam.GetComponent<ToonPipeline>();
        ToonPipeline new_tp     = newCam.GetComponent<ToonPipeline>();

        if ((prev_tp==null)||(new_tp==null))
        {
            this.LogWarn("Can't switch ToonPipeline. Previous or New Pipeline is missing from camera.");
            return;
        }

        new_tp.mgr       = prev_tp.mgr;
        new_tp.mainLight = prev_tp.mainLight;

        new_tp.enabled = true;

        if (!new_tp.init_done)
            new_tp.init();
    }

    public void launchDeathCam()
    {
        if (deathCamInst!=null)
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
        if (active_camera!=null)
        {
            active_camera.resetView();
            return true;
        }
        return false;
    }


    public void addFocusable(CameraFocusable iFocusable)
    {
        if (!focusables.Contains(iFocusable))
            focusables.Add(iFocusable);
    }

    public void removeFocusable(CameraFocusable iFocusable)
    {
        focusables.Remove(iFocusable);
        try {
            PlayerCamera as_playercam = (PlayerCamera)active_camera;
            if (!!as_playercam)
            {
                as_playercam.OnFocusRemove(iFocusable);
            }
        } catch (InvalidCastException e )
        {
            this.Log("removeFocusable called when active_camera is not a PlayerCamera.");
        }

    }

    public void changeMainLight(Light iLight)
    {
        ToonPipeline TP = active_camera.GetComponent<ToonPipeline>();
        if (!!TP)
            TP.mainLight = iLight;
    }
}
