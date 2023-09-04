using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Schnibble;

public class CheckPointManager : MonoBehaviour, IControllable
{
    public List<GameObject> checkpoints;
    public GameObject race_start;
    public FinishLine finishLine;
    public UICheckpoint ui_cp;

    [HideInInspector]
    public bool player_in_cinematic = false;

    private GameObject player;
    public GameObject Cam;

    // SaveStates
    public GameObject saveStateMarkerRef;
    private GameObject saveStateMarkerInst;
    private Vector3 ss_pos = Vector3.zero;
    private Quaternion ss_rot = Quaternion.identity;
    private bool hasSS;
    public int MAX_PANELS = 2;
    public int currPanels { get; set; }

    [HideInInspector]
    public GameObject last_checkpoint;

    [HideInInspector]
    public AbstractCameraPoint last_camerapoint;

    public float timeToForceCPLoad = 2f;
    public float ss_latch = 0.2f;
    private float elapsedSinceLastSS = 0f;
    private float elapsedSinceLastSSLoad = 0f;
    private float respawnButtonDownElapsed = 0f;
    private bool respawnCalled = false;
    private bool saveStateLoaded = false;

    private bool playerIsFrozen = false;
    private bool anyKeyPressed = false;

    public bool playerInGasStation = false;

    void Start()
    {
        init();
    }

    public void init()
    {
        if (checkpoints.Count <= 0)
        {
            this.LogWarn("NO checkpoints in CP manager. Should be auto. No CPs at all or Init order of CPs versus CPM ?");
            findCheckpoints();
        }

        player = Access.Player().gameObject;
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
        Access.UITurboAndSaves()?.updateAvailablePanels(currPanels);

        hasSS = false;
        respawnCalled = false;
        saveStateLoaded = false;
        player_in_cinematic = false;
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
        if (player_in_cinematic)
            return;

        if (playerIsFrozen)
            anyKeyPressed = Entry.IsAnyKeyDown();

        var plant = Entry.Inputs[(int) GameInputsButtons.SaveStatesPlant].IsDown;
        var load = Entry.Inputs[(int) GameInputsButtons.SaveStatesReturn].IsDown;

        if (saveStateLoaded && (plant||load))
        {
            return;
        } else { saveStateLoaded = false; }


        if (plant) // SAVE
        {
            if (!playerInGasStation)
            {
                if (elapsedSinceLastSS >= ss_latch)
                {
                    putSaveStateDown();
                    elapsedSinceLastSS = 0f;
                }
            }
            return;
        }
        if ((load)&&(elapsedSinceLastSSLoad>ss_latch)) // LOAD CALL
        {
            if (!respawnCalled)
            {
                respawnCalled = true;
                respawnButtonDownElapsed = 0f;
            }
            respawnButtonDownElapsed += Time.deltaTime;
            float fillVal = Mathf.Clamp( respawnButtonDownElapsed / timeToForceCPLoad, 0f, 1f);
            Access.UITurboAndSaves()?.updateCPFillImage(fillVal);
        }

        if (respawnCalled) // ACTUAL LOAD
        {
            if (!load) // input released
            {
                if (respawnButtonDownElapsed>=timeToForceCPLoad)
                {
                    loadLastCP(false);
                    saveStateLoaded = true;
                } else {
                    loadLastSaveState();
                    saveStateLoaded = true;
                }
                respawnButtonDownElapsed = 0f;
                respawnCalled = false;
                Access.UITurboAndSaves()?.updateCPFillImage(0f);
                elapsedSinceLastSSLoad = 0f;
            } else if (respawnButtonDownElapsed>=timeToForceCPLoad)
            {
                loadLastCP(false);
                respawnButtonDownElapsed = 0f;
                respawnCalled = false;
                Access.UITurboAndSaves()?.updateCPFillImage(0f);
                elapsedSinceLastSSLoad = 0f;
                saveStateLoaded = true;
            }
        }

        elapsedSinceLastSS += Time.deltaTime;
        elapsedSinceLastSSLoad += Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        refreshCameras();
    }

    public void putSaveStateDown()
    {
        if (currPanels>0)
        {
            if (!Access.Player().TouchGroundAll())
                return;

            currPanels -= 1;
            ss_pos = player.transform.position;
            ss_rot = player.gameObject.transform.rotation;
            hasSS = true;
            
            Access.UITurboAndSaves()?.updateAvailablePanels(currPanels);
            if (!!saveStateMarkerRef)
            {
                if (!!saveStateMarkerInst)
                    Destroy(saveStateMarkerInst);
                saveStateMarkerInst = Instantiate(saveStateMarkerRef);
                saveStateMarkerInst.transform.position = ss_pos;
                saveStateMarkerInst.transform.rotation = ss_rot;
            }
        }
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
            if (!checkpoints.Contains(CPs[i].gameObject))
                checkpoints.Add( CPs[i].gameObject );
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
        CheckPoint cp = iGO.GetComponent<CheckPoint>();
        if (!!cp)
        {
            if (last_checkpoint == iGO)
                return;

            if (!setAsLastCPTriggered)
                return;

            last_checkpoint = iGO;
            last_camerapoint = cp;
            currPanels = MAX_PANELS;
            
            hasSS = false;
            if (!!saveStateMarkerInst)
                Destroy(saveStateMarkerInst);
            
            Access.UITurboAndSaves()?.updateLastCPTriggered(cp.id.ToString());
            Access.UITurboAndSaves()?.updateAvailablePanels(currPanels);
            if (!!ui_cp)
            {
                ui_cp.displayCP(cp);
            }

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

    private IEnumerator waitInputToResume(PlayerController iPC)
    {
        iPC.Freeze();
        anyKeyPressed = false;
        yield return new WaitForSeconds(0.2f);        
        playerIsFrozen = true;
        while (!anyKeyPressed)
        {
            iPC.rb.velocity = Vector3.zero;
            yield return null;
        }
        iPC.UnFreeze();
        playerIsFrozen = false;
    }

    public void OnPlayerRespawn(Transform respawnSource)
    {
        // reset player physx
        Rigidbody rb2d = player.GetComponentInChildren<Rigidbody>();
        if (!!rb2d)
        {
            rb2d.velocity = Vector3.zero;
            rb2d.angularVelocity = Vector3.zero;
        }   

        // invalidate trick
        TrickTracker tt = player.GetComponent<TrickTracker>();
        if (!!tt && tt.activate_tricks)
        {
            tt.end_line(true);
            tt.storedScore = 0;
            tt.trickUI.displayTricklineScore(0);
        }

        // reset manual camera behind the player
        CameraManager CM = Access.CameraManager();
        if (CM.active_camera!=null)
        {
            ManualCamera manual_cam = CM.active_camera.GetComponent<ManualCamera>();
            if (!!manual_cam)
            { // force realignement
                manual_cam.forceAlignToHorizontal(respawnSource.rotation.eulerAngles.y);
            }
        }
    }

    public void loadLastSaveState()
    {
        if (!hasSS)
        {
            loadLastCP(false);
        } else {
            
            player.gameObject.transform.position = ss_pos;
            player.gameObject.transform.rotation = ss_rot;
            OnPlayerRespawn(saveStateMarkerInst.transform);
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        StartCoroutine(waitInputToResume(pc));

    }

    public void loadLastCP(bool iFromDeath = false)
    {
        // relocate player
        CheckPoint as_cp = last_checkpoint.GetComponent<CheckPoint>();
        if (as_cp != null)
        {
            last_camerapoint = as_cp;
            GameObject respawn = as_cp.getSpawn();

            player.transform.position = respawn.transform.position;
            player.transform.rotation = respawn.transform.rotation;
            OnPlayerRespawn(respawn.transform.parent.transform);

            PlayerController pc = player.GetComponent<PlayerController>();
            StartCoroutine(waitInputToResume(pc));
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
            loadRaceStart();
        }
        if (iFromDeath)
        {
            Access.CameraManager().launchDeathCam();
            Access.CollectiblesManager().jar.collectedNuts = 0;
            return;
        }

    }

    public void loadRaceStart()
    {
        StartPortal as_sp = last_checkpoint.GetComponent<StartPortal>();
        if (as_sp != null)
        {
            last_camerapoint = as_sp;
        }
        as_sp.relocatePlayer();
        PlayerController pc = player.GetComponent<PlayerController>();
        StartCoroutine(waitInputToResume(pc));
    }

    public void updateCamera()
    {
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