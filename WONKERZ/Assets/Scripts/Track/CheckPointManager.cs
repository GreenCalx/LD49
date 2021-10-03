using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    public List<GameObject> checkpoints;
    public GameObject race_start;
    public GameObject last_checkpoint;

    public GameObject player;

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
            last_checkpoint = iGO;
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
        player.transform.position = respawn.transform.position;
        player.transform.rotation = respawn.transform.rotation;
    }
}
