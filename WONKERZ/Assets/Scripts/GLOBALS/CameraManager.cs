using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public enum CAM_TYPE {
        UNDEFINED=0,
        HUB=1,
        TRACK=2,
        BOSS=3
    }
    public class CAM2CAM
    { 
        public CAM_TYPE type;
        public Camera   cam;
    }

    public CAM_TYPE curr_camera_type;
    [SerializeField]
    public readonly CAM2CAM[] cameras;
    public GameObject playerRef;
    private static CameraManager inst;

    public static CameraManager Instance
    { 
        get { return inst ?? (inst = new GameObject("CameraManager").AddComponent<CameraManager>()); }
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

    public void changeCamera( CAM_TYPE iNewCamType )
    {
        curr_camera_type = iNewCamType;
        switch(curr_camera_type)
        {
            case CAM_TYPE.HUB:
                setHUBCamera(true);
                setTrackCamera(false);
                break;
            case CAM_TYPE.TRACK:
                setHUBCamera(false);
                setTrackCamera(true);
                break;
            case CAM_TYPE.BOSS:
                break;
            default:
                break;
        }
    }

    private void setHUBCamera(bool iIsActive)
    {
        GameObject cam = GameObject.Find(Constants.GO_HUBCAMERA);
        if (!!cam)
        {
            cam.SetActive(iIsActive);
            ManualCamera mc = cam.GetComponent<ManualCamera>();
            if (!!mc)
            {
                mc.enabled = iIsActive;
                mc.focus = playerRef.transform;
            }
        }
    }

    private void setTrackCamera(bool iIsActive)
    {
        FollowPlayer cam = playerRef.transform.parent.gameObject.GetComponentInChildren<FollowPlayer>();
        if (!!cam)
        {
            cam.Active = iIsActive;
            cam.enabled = iIsActive;
            cam.Following = playerRef;
            GameObject cp_mgr_go = GameObject.Find( Constants.GO_CPManager );
            if (!!cp_mgr_go	)
            {
                CheckPointManager cpm = cp_mgr_go.GetComponent<CheckPointManager>();
                cam.Mng = cpm;
            }
        }
    }
}
