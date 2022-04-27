using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{

    public GameCamera active_camera;
    public Dictionary<GameCamera.CAM_TYPE, GameCamera> cameras = 
        new Dictionary<GameCamera.CAM_TYPE, GameCamera>();
    public GameObject playerRef;
    private static CameraManager inst;

    public static CameraManager Instance
    { 
        get { return inst ?? (inst = GameObject.Find(Constants.GO_MANAGERS).GetComponent<CameraManager>()); }
        private set { inst = value; }
    }


    // Start is called before the first frame update
    void Awake()
    {
        playerRef = Utils.getPlayerRef();

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void changeCamera( GameCamera.CAM_TYPE iNewCamType )
    {
        // 1 : Do I have a CAM_TYPE available ?
        if ( !cameras.ContainsKey(iNewCamType) )
        {
            // > N : Try to find CAM_TYPE in scene
            GameCamera new_cam = findCameraInScene(iNewCamType);
            if (new_cam==null)
            {
                Debug.LogError("Unable to find a camera for type : " + iNewCamType.ToString());
                return;
            }
            //  > Add it to mgr cams
            cameras.Add( iNewCamType, new_cam);
        }

        // 2 : Deactivate active_camera
        if (active_camera!=null)
            active_camera.gameObject.SetActive(false);

        // 3 : Replace active_camera with the new camera, set it as main camera
        if ( !cameras.TryGetValue(iNewCamType, out active_camera) )
        {
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
    }
}
