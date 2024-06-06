using UnityEngine;
using Schnibble;

namespace Wonkerz 
{
    [CreateAssetMenu(fileName = "WkzCarDef", menuName = "Car/WkzCarDef", order = 2)]
    public class WkzCarSO : SchCarSO 
    {
        [Header("Gameplay")]
        public WkzCar.SchJumpDef jumpDef;
        public float aerialMaxForce = 5.0f;
        public float weightControlMaxX = 1.0f;
        public float weightControlMaxZ = 1.0f;
        public float weightControlSpeed = 1.0f;
        public float centrifugalForceMul = 1.0f;
        public float minCentrifugalVelocity = 3.0f;
    }
}
