using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz
{
    public class ExplodeChildBodies : MonoBehaviour
    {
        public enum ExplosionMode { FROM_DIRECTION = 0, RADIAL = 1};

        public Rigidbody[] childBodies;
        private bool triggered = false;
        public float forceMul = 1f;
        public float forceStr = 10f;
        public Vector3 directionSteer;
        public bool AddForce = true;
        public bool AddTorque = true;

        public ForceMode FMToApplyOnForce = ForceMode.Impulse;
        public ForceMode FMToApplyOnTorque = ForceMode.Impulse;
        public ExplosionMode explosionMode = ExplosionMode.RADIAL;

        public float TimeBeforeFragCleanUp = 2f;
        private float elapsedTimeBeforeCleaning = 0f;
        // Start is called before the first frame update
        void Start()
        {
            childBodies = GetComponentsInChildren<Rigidbody>(true);
        }

        void Update()
        {
            if (triggered)
            {
                elapsedTimeBeforeCleaning += Time.deltaTime;
                if (elapsedTimeBeforeCleaning>=TimeBeforeFragCleanUp)
                {
                    foreach (Transform child in transform)
                    {
                        Destroy(child.gameObject);
                        Destroy(gameObject);
                    }
                }
            }
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
            Rigidbody playerRB = Access.Player().GetRigidbody();

            directionSteer  = playerRB.velocity.normalized;
            forceStr        = playerRB.velocity.magnitude;
        }

        public void setExplosionDir(Vector3 iDirectionSteer, float iForceStr)
        {
            directionSteer  = iDirectionSteer;
            forceStr        = iForceStr;
        }

        void explodeChilds()
        {
            if ((childBodies==null)||(childBodies.Length==0))
            {
                childBodies = GetComponentsInChildren<Rigidbody>(true);
            }

            int n_bodies = childBodies.Length;
            for ( int i=0; i < n_bodies ; i++)
            {
                Rigidbody rb = childBodies[i];
                //Vector3 randDirection = Random.insideUnitCircle.normalized;
                //Vector3 dir = randDirection + directionSteer;
                rb.isKinematic = false;


                Vector3 fDir = Vector3.zero;
                switch (explosionMode)
                {
                    case ExplosionMode.FROM_DIRECTION:
                        fDir = rb.worldCenterOfMass - directionSteer;
                        break;
                    case ExplosionMode.RADIAL:
                        fDir = rb.transform.position - transform.position;
                        break;
                    default:
                        break;
                }

                if (AddForce)
                    rb.AddForce(fDir * forceStr * forceMul, FMToApplyOnForce);
                else if (AddTorque)
                    rb.AddForce(fDir * forceStr * forceMul, FMToApplyOnTorque);

                Debug.DrawRay(rb.gameObject.transform.position, fDir,  Color.red);
                elapsedTimeBeforeCleaning = 0f;
            }
            //Destroy(gameObject);
        }
}
}