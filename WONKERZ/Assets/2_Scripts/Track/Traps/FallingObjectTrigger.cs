using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wonkerz {
    public class FallingObjectTrigger : MonoBehaviour
    {
        public FallingObject   target;
        public Vector3      normalizedForceToApply;
        public float        force;
        public float        targetMass;
        public int          damageOnPlayerWhenTriggered = 5;

        private bool triggered = false;
        private bool stopped   = false;

        void OnTriggerEnter(Collider iCollider)
        {
            if (triggered)
            return;

            if (Utils.isPlayer(iCollider.gameObject))
            {
                Rigidbody target_rb = target.GetComponent<Rigidbody>();
                if (target_rb==null)
                {
                    target_rb = target.gameObject.AddComponent<Rigidbody>() as Rigidbody;
                }
                target_rb.mass = targetMass;
                target_rb.AddForce( normalizedForceToApply.normalized * force, ForceMode.Impulse);

                // Handle mesh collider
                MeshCollider mc = target.GetComponent<MeshCollider>();
                if (!!mc)
                {
                    mc.convex = true;
                }

                // Player Damager
                if (damageOnPlayerWhenTriggered>0)
                {
                    PlayerDamager pd = target.gameObject.AddComponent<PlayerDamager>() as PlayerDamager;
                    pd.damageOnCollide = damageOnPlayerWhenTriggered;
                }

                target.isFalling    = true;
                triggered           = true;
            }
        }


    }
}
