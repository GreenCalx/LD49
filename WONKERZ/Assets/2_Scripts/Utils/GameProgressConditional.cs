using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Wonkerz {
    public class GameProgressConditional : MonoBehaviour
    {
        public UniqueEvents.UEVENTS gameProgressEventID;

        [Header("Behaviors on EventID")]
        public bool destroyOnStart = true;
        public bool activateOnStart = false;


        void Start()
        {
            if (activateOnStart)
            {
                if (Access.GameProgressSaveManager().IsUniqueEventDone(gameProgressEventID))
                {
                    foreach(Transform t in transform)
                    {
                        t.gameObject.SetActive(true);
                    }
                }            
            }

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
}
