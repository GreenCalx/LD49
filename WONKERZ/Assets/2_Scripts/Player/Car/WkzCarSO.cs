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
        public float groundAerialSwitchLatency = 0.1f;
        public float weightControlMaxX = 1.0f;
        public float weightControlMaxZ = 1.0f;
        public float weightControlMaxY = 0.0f;
        public float weightControlSpeed = 1.0f;
        public float centrifugalForceMul = 1.0f;
        public float minCentrifugalVelocity = 3.0f;
        [Header("Game Stats")]
        public int nutCapacity = 10;
        public float atkMul  = 1f;
        public float defMul = 1f;

        public void CopyTo(ref WkzCarSO iCopyTarget)
        {
            // NOTE:
            // Maybe it already copy the fields because it serialize/deserialize for instancing?
            // Regardless we explicitly copy for now.
            iCopyTarget = ScriptableObject.Instantiate(this);
            //iCopyTarget = new WkzCarSO();

            iCopyTarget.jumpDef = this.jumpDef;
            iCopyTarget.aerialMaxForce = this.aerialMaxForce;
            iCopyTarget.groundAerialSwitchLatency = this.groundAerialSwitchLatency;
            iCopyTarget.weightControlMaxX = this.weightControlMaxX;
            iCopyTarget.weightControlMaxZ = this.weightControlMaxZ;
            iCopyTarget.weightControlMaxY = this.weightControlMaxY;
            iCopyTarget.weightControlSpeed = this.weightControlSpeed;
            iCopyTarget.centrifugalForceMul = this.centrifugalForceMul;
            iCopyTarget.minCentrifugalVelocity = this.minCentrifugalVelocity;
            iCopyTarget.nutCapacity = this.nutCapacity;
            iCopyTarget.atkMul = this.atkMul;
            iCopyTarget.defMul = this.defMul;
        }
    }
}
