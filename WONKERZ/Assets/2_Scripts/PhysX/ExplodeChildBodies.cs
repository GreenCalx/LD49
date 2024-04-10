using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz {

    public class ExplodeChildBodies : MonoBehaviour
    {
        public Rigidbody[] childBodies;
        private bool triggered = false;
        public float forceStr = 10f;
        public Vector3 directionSteer;

        // Start is called before the first frame update
        void Start()
        {
            childBodies = GetComponentsInChildren<Rigidbody>(true);
        }

        // Update is called once per frame
        public void triggerExplosion()
        {
            if (triggered)
            return;
            explodeChilds();
            triggered = true;
        }

        public void setExplosionDirToPlayerVelocity()
        {
            directionSteer = Access.Player().GetRigidbody().velocity.normalized;
            forceStr = Access.Player().GetRigidbody().velocity.magnitude;
        }

        void explodeChilds()
        {
            int n_bodies = childBodies.Length;
            for ( int i=0; i < n_bodies ; i++)
            {
                Rigidbody rb = childBodies[i];
                //Vector3 randDirection = Random.insideUnitCircle.normalized;
                //Vector3 dir = randDirection + directionSteer;
                rb.isKinematic = false;

                rb.AddForce(directionSteer*forceStr, ForceMode.Impulse);
                Debug.DrawRay(rb.gameObject.transform.position, directionSteer*forceStr,  Color.red);
            }
            //Destroy(gameObject);
        }
    }
}
