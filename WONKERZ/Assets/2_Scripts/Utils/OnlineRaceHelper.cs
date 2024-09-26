using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[ExecuteInEditMode]
public class OnlineRaceHelper : MonoBehaviour 
{
    #if UNITY_EDITOR
    public OnlineRaceTrialManager ORTM = null;

    public bool AutoSetNextCheckpoints = false;

    void Update()
    {
        if (Application.isPlaying)
            return;

        if (AutoSetNextCheckpoints)
        {
            AutoSetNextCheckpointsFromRoot();
            AutoSetNextCheckpoints = false;
        }
    }

    private void AutoSetNextCheckpointsFromRoot()
    {
        if (ORTM==null)
        {
            Debug.LogError("Missing ORTM");
            return;
        }

        if(ORTM.CheckpointsHandle==null)
        {
            Debug.LogError("Missing ORTM checkpoint handle");
            return;
        }

        LinkedList<OnlineRaceCheckPoint> checkpoints = new LinkedList<OnlineRaceCheckPoint>(ORTM.CheckpointsHandle.GetComponentsInChildren<OnlineRaceCheckPoint>());
        foreach(OnlineRaceCheckPoint orcp in checkpoints)
        {
            foreach(OnlineRaceCheckPoint prev_orcp in orcp.prev_CPs)
            {
                prev_orcp.next_CPs.Add(orcp);
            }
        }
    }
    #endif
}