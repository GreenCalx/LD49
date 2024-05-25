using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz {
    public class WaterCollider : MonoBehaviour
    {
        public void OnTriggerEnter(Collider c)
        {
            if (Utils.colliderIsPlayer(c))
            {
                Access.Player().SetTouchingWater(true);
            }
        }


        public void OnTriggerExit(Collider c)
        {
            if (Utils.colliderIsPlayer(c))
            {
                Access.Player().SetTouchingWater(false);
            }
        }
    }
}
