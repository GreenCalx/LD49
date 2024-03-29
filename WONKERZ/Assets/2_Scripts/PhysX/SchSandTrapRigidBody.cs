using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using static Schnibble.Physics;

public class SchSandTrapRigidBodyBehaviour : SchRigidBodyBehaviourBase
{
    public class SchSandTrapRigidBody : SchRigidBody {
        GameObject obj;
        public float MaxForce;
        /*
        public override Vector3 GetVelocityAtPoint(Vector3 worldPosition)
        {
            var maxLength = obj.transform.lossyScale.x;

            var dir = worldPosition - obj.transform.position;

            var dist = dir.magnitude / maxLength;

            var velocity = MaxForce * (1 - dist) * dir.normalized;

            return velocity;
        }
         */
    }

    public SchRigidBody rb_old;
    public SchSandTrapRigidBody rb_new;
    public void Awake() {
        if (rb_old)
        irb = rb_old.irb;
    }

    public void OnCollisionStay(Collision c)
    {
        var colliderRB = c.body as Rigidbody;
        //colliderRB.AddForce(rb.GetVelocityAtPoint(colliderRB.position) - colliderRB.velocity, ForceMode.VelocityChange);
    }
}
