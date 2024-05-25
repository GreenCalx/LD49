using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz {
    public class GarageCarDuplicator : MonoBehaviour
    {
        void Start()
        {
            Access.PlayerCosmeticsManager().addPlayerToCustomize(gameObject);
        }

        void OnDestroy()
        {
            Access.PlayerCosmeticsManager().removePlayerToCustomize(gameObject);
        }
    }
}
