using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble;

namespace Wonkerz
{
    public class WkzBuoyency : SchBuoyancy
    {
        // Start is called before the first frame update
        void Start()
        {
            if (waterCollider == null) {
                waterCollider = SchWaterColliderManager.inst;
            }
        }

    }
}
