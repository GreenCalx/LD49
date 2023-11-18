using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameProgressConditional : MonoBehaviour
{
    public UniqueEvents.UEVENTS gameProgressEventID;

    [Header("Behaviors on EventID")]
    public bool destroyOnStart = true;

    void Start()
    {
        if (destroyOnStart)
        {
            if (Access.GameProgressSaveManager().IsUniqueEventDone(gameProgressEventID))
            {
                foreach(Transform t in transform)
                {
                    Destroy(t.gameObject);
                }
                Destroy(gameObject);
            }
        }
    }


}
