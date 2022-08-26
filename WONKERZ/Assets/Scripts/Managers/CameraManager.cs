using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

// Known issue :
//  * If a camera is active in a new scene, it can take the lead over the active_camera
//  ** FIX : Disable all cameras by default
public class CameraManager : MonoBehaviour
{
    public GameObject transitionCameraRef;
    public GameCamera active_camera;
    public Dictionary<GameCamera.CAM_TYPE, GameCamera> cameras = 
        new Dictionary<GameCamera.CAM_TYPE, GameCamera>();

    public float transitionDuration = 2f;
    public bool inTransition = false;

    private Transform transitionStart = null;
    private Transform transitionEnd = null;
    private GameCamera nextCamera = null;
    private float transiTime = 0f;
    private GameObject transitionCameraInst;

    //public GameObject playerRef;
    private static CameraManager inst;

    public static CameraManager Instance
    { 
        get { return inst ?? (inst = Access.CameraManager()); }
        private set { inst = value; }
    }


    // Start is called before the first frame update
    void Awake()
    {
        inTransition = false;
        SceneManager.sceneLoaded += OnSceneLoaded; 
    }

    // Update is called once per frame
    void Update()
    {
        if (inTransition)
        {
            if (transition( transitionStart, transitionEnd))
            {
                inTransition = false;
                operateCameraSwitch(nextCamera);
                transitionCameraInst = null;
            }
        }
    }

    // resets CameraManager status (active camera, etc..) upon new scene load
    // Avoid bad transitions and such for now
    // Might need to change if we decide to make a Spyro style entry in the level thru portals
    void OnSceneLoaded (Scene scene, LoadSceneMode mode)
    {
        // refresh cameras and disable active_camera
        active_camera = null;
        //cameras.Clear();
    }

    // Find Camera from its type within current scene
    // Only one per CAM_TYPE is retrieved for now as we don't need more
    // Thus if we want to have multiple cameras for the hub, we'll need to update
    // this logic.
    private GameCamera findCameraInScene( GameCamera.CAM_TYPE iType)
    {
        GameCamera[] game_cams = FindObjectsOfType<GameCamera>(true/*include inactives*/);
        GameCamera retval = null;

        for (int i=0; i<game_cams.Length;i++)
        {
            GameCamera currcam = game_cams[i];
            if (currcam.camType==iType)
            {
                retval = currcam;
                break;
            }
        }
        return retval;
    }

    private List<GameCamera> findCamerasInScene( GameCamera.CAM_TYPE iType)
    {
        GameCamera[] game_cams = FindObjectsOfType<GameCamera>(true/*include inactives*/);
        List<GameCamera> retval = new List<GameCamera>();

        for (int i=0; i<game_cams.Length;i++)
        {
            GameCamera currcam = game_cams[i];
            if (currcam.camType==iType)
            {
                retval.Add(currcam);
            }
        }
        return retval;
    }

    // Retrieves only the first found CAM_TYPE camera
    public void changeCamera( GameCamera.CAM_TYPE iNewCamType )
    {
        // 0 : Clean up nulls CameraType to free space for new ones
        var null_keys = cameras.Where( e => e.Value == null)
                                .Select( e => e.Key)
                                .ToList();
        foreach(var rm_me in null_keys )
        {
            cameras.Remove(rm_me);
        }

        // 1 : Do I have a CAM_TYPE available ?
        nextCamera = null;
        if ( !cameras.ContainsKey(iNewCamType) )
        {
            // > N : Try to find CAM_TYPE in scene
            nextCamera = findCameraInScene(iNewCamType);
            if (nextCamera==null)
            {
                Debug.LogError("Unable to find a camera for type : " + iNewCamType.ToString());
                Debug.LogError("Failed to switch Camera. Selecting first of the list as fallback.");
                if ( cameras.Count > 0)
                {
                    active_camera = cameras[0];
                    active_camera.gameObject.SetActive(true);
                } else {
                    Debug.LogError("No Camera available in CameraManager. Exiting changeCamera().");
                    return;
                }
            }
            //  > Add it to mgr cams
            cameras.Add( iNewCamType, nextCamera);
        } else {
            // cam already stored, retrieve it for transition
            cameras.TryGetValue(iNewCamType, out nextCamera);
        }

        // 2 : Deactivate active_camera
        
        if ((active_camera!=null)&&(active_camera.gameObject.scene.IsValid()))
        {
            if (active_camera.gameObject.scene==nextCamera.gameObject.scene)
                initTransition(nextCamera);
            else
                operateCameraSwitch(nextCamera);
        } else {
            operateCameraSwitch(nextCamera);
        }
    }

    public void changeCamera(CinematicCamera iCineCam)
    {
        if (iCineCam==null)
        {
            Debug.LogError("Unable to changeCamera for cinemati : null input");
        }

        nextCamera = iCineCam;

        // transition if previous camera was defined
        if ((active_camera!=null)&&(active_camera.gameObject.scene.IsValid()))
        {
            if (active_camera.gameObject.scene==nextCamera.gameObject.scene)
                initTransition(nextCamera);
            else
                operateCameraSwitch(nextCamera);
        } else {
            operateCameraSwitch(nextCamera);
        }
    }

    public void initTransition(GameCamera iNextCam)
    {
        // disable active camera behaviour
        //active_camera.enabled = false;
        //active_camera.gameObject.SetActive(false);
        Camera camcomp = active_camera.GetComponent<Camera>();
        camcomp.enabled = false;

        // switch on intermediate transition camera
        if (transitionCameraInst==null)
            transitionCameraInst = Instantiate(transitionCameraRef);
        
        transitionCameraInst.transform.position = active_camera.transform.position;
        transitionCameraInst.transform.rotation = active_camera.transform.rotation;
        active_camera = transitionCameraInst.GetComponent<CinematicCamera>();
        
        inTransition = true;
        transiTime = 0f;
        
        transitionStart = new GameObject("TRANSI_START").transform; // static start point for LERP
        transitionStart.SetParent(transform);
        transitionStart.transform.position = active_camera.transform.position; 
        transitionStart.transform.rotation = active_camera.transform.rotation; 
 
        transitionEnd   = iNextCam.transform; // keep the end point dynamic

        active_camera.gameObject.SetActive(true);
        active_camera.enabled = true;
        active_camera.GetComponent<Camera>().enabled = true;
    }

    public void endTransition()
    {
        inTransition = false;
        Destroy(findCameraInScene(GameCamera.CAM_TYPE.TRANSITION).gameObject);
        Destroy(transitionStart.gameObject);
    }

    public void operateCameraSwitch(GameCamera iNewCam)
    {
        if (active_camera!=null)
        {
            active_camera.gameObject.SetActive(false);
            active_camera.enabled = false;
            active_camera.GetComponent<Camera>().enabled = false;
        }
        active_camera = iNewCam;
        active_camera.gameObject.SetActive(true);
        active_camera.enabled = true;
        active_camera.GetComponent<Camera>().enabled = true;
        active_camera.init();

    }

    public bool interpolatePosition(Transform iStart, Transform iEnd)
    {
        float s = transiTime/transitionDuration;
        //s = s*s*(3f-2f*s); // smoothstep formula
        active_camera.transform.position = Vector3.Lerp( 
                                iStart.position,
                                iEnd.position,
                                s );
        active_camera.transform.rotation = Quaternion.Lerp( 
                                        iStart.rotation,
                                        iEnd.rotation,
                                        s);
        transiTime += Time.deltaTime;

        return transiTime < transitionDuration;
    }

    // Returns true when transition is done, false otherwise
    public bool transition( Transform iStart, Transform iEnd)
    {
        if (!interpolatePosition(iStart, iEnd))
        {
            active_camera.transform.position = iEnd.position;
            active_camera.transform.rotation = iEnd.rotation;
            endTransition();
            return true;
        }
        return false;
    }

    public void moveActiveCameraTo(Vector3 iTransform)
    {
        
    }
}
