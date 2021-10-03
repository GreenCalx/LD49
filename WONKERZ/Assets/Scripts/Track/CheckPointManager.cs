using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    public List<GameObject> checkpoints;
    public GameObject race_start;
    public FinishLine finishLine;
    public UICheckpoint ui_ref;

    public GameObject player;

    [HideInInspector]
    public GameObject last_checkpoint;


    void Start()
    {
        if (checkpoints.Count <= 0)
        {
            Debug.LogError("NO checkpoints in CP manager. Should be auto. No CPs at all or Init order of CPs versus CPM ?");
            return;
        }

        last_checkpoint = race_start;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("r"))
            loadLastCP();
    }

    public void subscribe(GameObject iCP)
    {
        if (iCP.GetComponent<CheckPoint>())
            checkpoints.Add(iCP);
    }

    public void notifyCP(GameObject iGO)
    {
        if ( iGO.GetComponent<CheckPoint>() )
        {
            if (last_checkpoint==iGO)
                return;

            last_checkpoint = iGO;
            if(!!ui_ref)
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

        Debug.Log("LOAD CP : " + as_cp.gameObject.name);
        
        GameObject respawn = as_cp.getSpawn();

        Rigidbody rb2d = player.GetComponentInChildren<Rigidbody>();
        if (!!rb2d)
        {
            rb2d.velocity = Vector3.zero;
            rb2d.angularVelocity = Vector3.zero;
        }
        player.transform.position = respawn.transform.position;
        player.transform.rotation = respawn.transform.rotation;
    }
}
