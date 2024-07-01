using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

namespace Wonkerz
{
    public class WeightIndicator : MonoBehaviour
    {
        public SchRigidbodyBehaviour rb;
        public GameObject indicator;
        public Vector3    offsetLocal;
        public bool       useRotation      = false;
        public bool       useFixedPosition = true; // should we take into account the chassis local center 

        void Update()
        {
            var body     = rb.GetBody();
            var localCOM = body.centerOfMass;

            var initialCOM = localCOM - rb.localCOM;

            var offset = offsetLocal;
            if (useFixedPosition) offset -= rb.localCOM;

            indicator.transform.position = body.transform.TransformPoint(localCOM + offset);

            if (useRotation)
            {
                var diff = (indicator.transform.position - body.transform.TransformPoint(initialCOM)).normalized;
                indicator.transform.rotation = Quaternion.LookRotation(body.transform.forward, diff);
            }
        }
    }
}
