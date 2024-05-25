using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz {

    [RequireComponent(typeof(Rigidbody))]
    public class TutorialPlayerAnchorPoint : MonoBehaviour
    {
        private bool triggered = false;

        void Update()
        {
            if (triggered)
            {
                transform.localPosition = Vector3.zero; 
                transform.localRotation = Quaternion.identity;
            }
        }

        public void trigger()
        {
            transform.parent = Access.Player().transform;
            triggered = true;
        }

        public void delete()
        {
            Destroy(this.gameObject);
        }
    }
}
