using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz
{
    public class CarWeightIndicator : MonoBehaviour
    {
        public WkzCar     car;
        public GameObject indicator;
        public Vector3    offsetLocal;
        public bool       useRotation     = false;
        public bool       useFixedPosition = true; // should we take into account the chassis local center 

        void Update()
        {
            var body     = car.chassis.GetBody();
            var localCOM = body.centerOfMass;

            var offset = offsetLocal;
            if (useFixedPosition) offset -= car.def.chassis.localCenterOfMass;

            indicator.transform.position = body.transform.TransformPoint(localCOM + offset);

            if (useRotation)
            {
                #if false
                // try to add a lil bit of rotation
                // TODO: make it real! Do not assume COM was at 0, Vector3.up is the up, etc...
                var diff = body.transform.TransformVector(body.centerOfMass + offset).normalized;
                indicator.transform.rotation = Quaternion.LookRotation(body.transform.forward, diff);
                #endif

                var diff = (indicator.transform.position - body.transform.TransformPoint(car.def.chassis.localCenterOfMass)).normalized;
                indicator.transform.rotation = Quaternion.LookRotation(body.transform.forward, diff);
            }
        }
    }
}
