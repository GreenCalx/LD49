using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameProgressConditional : MonoBehaviour
{
    public UniqueEvents.UEVENTS gameProgressEventID;

    void Start()
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
