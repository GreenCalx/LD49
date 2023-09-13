using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameProgressConditional : MonoBehaviour
{
    public string gameProgressEventID = "";

    void Start()
    {
        if (string.IsNullOrEmpty(gameProgressEventID))
            return;

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
