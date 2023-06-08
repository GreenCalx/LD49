using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class CheckPointManager : MonoBehaviour, IControllable
{
    public List<GameObject> checkpoints;
    public GameObject race_start;
    public FinishLine finishLine;
    public UICheckpoint ui_ref;

    public GameObject player;
    public GameObject Cam;

    public int MAX_PANELS = 2;
    public int currPanels { get; set; }

    [HideInInspector]
    public GameObject last_checkpoint;

    [HideInInspector]
    public AbstractCameraPoint last_camerapoint;

    void Start()
    {
        if (checkpoints.Count <= 0)
        {
            this.LogWarn("NO checkpoints in CP manager. Should be auto. No CPs at all or Init order of CPs versus CPM ?");
            findCheckpoints();
        }
        if (player == null)
        {
            player = Access.Player().gameObject;
        }
        //refreshCameras();
        last_checkpoint = race_start;
        last_camerapoint = race_start.GetComponent<CheckPoint>();
        if (last_camerapoint == null)
            last_camerapoint = race_start.GetComponent<StartPortal>();

        Utils.attachControllable<CheckPointManager>(this);

        // Init from difficulty
        switch (Access.TrackManager().track_score.selected_diff)
        {
            case DIFFICULTIES.EASY:
                MAX_PANELS = Constants.EASY_N_PANELS;
                break;
            case DIFFICULTIES.MEDIUM:
                MAX_PANELS = Constants.MEDIUM_N_PANELS;
                break;
            case DIFFICULTIES.HARD:
                MAX_PANELS = Constants.HARD_N_PANELS;
                break;
            case DIFFICULTIES.IRONMAN:
                MAX_PANELS = Constants.IRONMAN_N_PANELS;
                foreach(GameObject go in checkpoints)
                { go.transform.parent.gameObject.SetActive(false); }
                break;
            default:
                MAX_PANELS = Constants.EASY_N_PANELS;
                break;
        }
        currPanels = MAX_PANELS;
    }


    void Awake()
    {
    }

    void OnDestroy()
    {
        Utils.detachControllable<CheckPointManager>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        if (Entry.Inputs["Respawn"].IsDown)
            loadLastCP();
    }

    // Update is called once per frame
    void Update()
    {
        refreshCameras();
    }

    private void refreshCameras()
    {
        if (Cam == null)
        {
            Cam = Access.CameraManager().active_camera.gameObject;
            this.LogWarn("Camera Ref refreshed in CheckPointManager.");
            foreach (GameObject cp in checkpoints)
            {
                cp.GetComponent<CheckPoint>().Cam = Cam.GetComponent<Camera>();
            }
        }
    }

    private void findMultiCheckpoints()
    {
        MultiCheckPoint[] mcps = transform.parent.GetComponentsInChildren<MultiCheckPoint>();
        checkpoints = new List<GameObject>();
        foreach (MultiCheckPoint mcp in mcps)
        {
            CheckPoint[] cps = mcp.GetComponentsInChildren<CheckPoint>();
            foreach (CheckPoint cp in cps)
            {
                cp.subscribeToManager(this);
                checkpoints.Add(cp.gameObject);
            }
        }
    }

    private void findCheckpoints()
    {
        CheckPoint[] CPs = transform.parent.GetComponentsInChildren<CheckPoint>();
        checkpoints = new List<GameObject>(CPs.Length);
        for (int i = 0; i < CPs.Length; i++)
        {
            CPs[i].subscribeToManager(this);
            //checkpoints.Add( CPs[i].gameObject );
        }
    }

    public bool subscribe(GameObject iCP)
    {
        if (iCP.GetComponent<CheckPoint>() && !checkpoints.Exists(x => x.gameObject.name == iCP.gameObject.name))
        { checkpoints.Add(iCP); return true; }
        return false;
    }

    public void notifyCP(GameObject iGO, bool setAsLastCPTriggered)
    {
        if (iGO.GetComponent<CheckPoint>())
        {
            if (!!ui_ref)
            {
                ui_ref.displayCP(iGO);
            }

            if (last_checkpoint == iGO)
                return;

            if (!setAsLastCPTriggered)
                return;

            last_checkpoint = iGO;
            last_camerapoint = iGO.GetComponent<CheckPoint>();
            currPanels = MAX_PANELS;

        }
        else
            this.LogWarn("CheckPointManager: Input GO is not a checkpoint.");
    }

    public void notifyGasStation(GasStation iGasStation)
    {
        currPanels = MAX_PANELS;
    }

    void OnDisable()
    {

    }

    public void loadLastCP(bool iFromDeath = false)
    {
        // reset player physx
        Rigidbody rb2d = player.GetComponentInChildren<Rigidbody>();
        if (!!rb2d)
        {
            rb2d.velocity = Vector3.zero;
            rb2d.angularVelocity = Vector3.zero;
        }

        // relocate player
        CheckPoint as_cp = last_checkpoint.GetComponent<CheckPoint>();
        if (as_cp != null)
        {
            last_camerapoint = as_cp;
            GameObject respawn = as_cp.getSpawn();

            player.transform.position = respawn.transform.position;
            player.transform.rotation = respawn.transform.rotation;

            // reset segment resetable items
            // if ( as_cp.MCP != null )
            // {
            //     List<Resetable> resetables = as_cp.MCP.resetables;
            //     resetables.RemoveAll((x => x == null));
            //     foreach (Resetable r in resetables)
            //     {
            //         r.load();
            //     }
            // }
        }
        else
        {
            StartPortal as_sp = last_checkpoint.GetComponent<StartPortal>();
            if (as_sp != null)
            {
                last_camerapoint = as_sp;
            }
            as_sp.relocatePlayer();
        }
        if (iFromDeath)
        {
            Access.CameraManager().launchDeathCam();
            Access.CollectiblesManager().reset();
            return;
        }

        // Update caemra too !
        var Direction = Cam.transform.position - player.transform.position;
        var F = player.transform.forward;
        var DirectionForward = new Vector3(Direction.x * F.x, Direction.y * F.y, Direction.z * F.z);
        if (Direction.y < 0)
        {
            // Camera is below player be sure it is on top
            Cam.transform.position = player.transform.position + (player.transform.up * 1000);
        }

        if (DirectionForward.magnitude > 0)
        {
            // Camera is in front of player, bu sure it is behind
            Cam.transform.position = player.transform.position + (player.transform.forward * -10000);
        }

    }
}
