using System.Collections.Generic;
using UnityEngine;

public class CheckPointManager : MonoBehaviour, IControllable
{
    public List<GameObject> checkpoints;
    public GameObject race_start;
    public FinishLine finishLine;
    public UICheckpoint ui_ref;

    public GameObject player;
    public GameObject Cam;

    [HideInInspector]
    public GameObject last_checkpoint;


    void Start()
    {
        if (checkpoints.Count <= 0)
        {
            Debug.LogWarning("NO checkpoints in CP manager. Should be auto. No CPs at all or Init order of CPs versus CPM ?");
            findCheckpoints();
        }
        if (player == null)
        {
            player = Utils.getPlayerRef();
        }
        if (Cam == null)
        {
            Cam = CameraManager.Instance.active_camera.gameObject;
        }
        last_checkpoint = race_start;

        GameObject.Find(Constants.GO_MANAGERS).GetComponent<InputManager>().Attach(this as IControllable);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry) {
        if (Entry.Inputs["Respawn"].IsDown)
            loadLastCP();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void findCheckpoints()
    {
        CheckPoint[] CPs = transform.parent.GetComponentsInChildren<CheckPoint>();
        checkpoints = new List<GameObject>(CPs.Length);
        for (int i=0;i<CPs.Length;i++)
        {
            checkpoints.Add( CPs[i].gameObject );
        }
    }

    public void subscribe(GameObject iCP)
    {
        if (iCP.GetComponent<CheckPoint>())
            checkpoints.Add(iCP);
    }

    public void notifyCP(GameObject iGO)
    {
        if (iGO.GetComponent<CheckPoint>())
        {
            if (last_checkpoint == iGO)
                return;

            last_checkpoint = iGO;
            if (!!ui_ref)
            {
                ui_ref.displayCP(iGO);
            }
        }
        else
            Debug.LogWarning("CheckPointManager: Input GO is not a checkpoint.");
    }

    public void loadLastCP()
    {
        CheckPoint as_cp = last_checkpoint.GetComponent<CheckPoint>();
        if (as_cp == null)
            Debug.Log("not a cp");

        GameObject respawn = as_cp.getSpawn();

        Rigidbody rb2d = player.GetComponentInChildren<Rigidbody>();
        if (!!rb2d)
        {
            rb2d.velocity = Vector3.zero;
            rb2d.angularVelocity = Vector3.zero;

        }
        player.transform.position = respawn.transform.position;
        player.transform.rotation = respawn.transform.rotation;

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
