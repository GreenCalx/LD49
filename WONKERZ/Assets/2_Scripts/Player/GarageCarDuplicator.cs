using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz {
    public class GarageCarDuplicator : MonoBehaviour
    {
        void Start()
        {
            Access.managers.playerCosmeticsMgr.addPlayerToCustomize(gameObject);
        }

        void OnDestroy()
        {
            Access.managers.playerCosmeticsMgr.removePlayerToCustomize(gameObject);
        }
    }
}
