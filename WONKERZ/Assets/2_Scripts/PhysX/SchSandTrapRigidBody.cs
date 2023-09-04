using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using static Schnibble.SchPhysics;

public class SchSandTrapRigidBody : MonoBehaviour, IRigidBody
{
    public float MaxForce;

    public void DrawDebug()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetAngularVelocity()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetCenterOfMass()
    {
        throw new System.NotImplementedException();
    }

    public Matrix4x4 GetInertiaTensorMatrix()
    {
        throw new System.NotImplementedException();
    }

    public Quaternion GetInertiaTensorRotation()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetInertiaTensorVector()
    {
        throw new System.NotImplementedException();
    }

    public Matrix4x4 GetInverseInertiaTensor()
    {
        throw new System.NotImplementedException();
    }

    public Matrix4x4 GetInverseInertiaTensorWorld()
    {
        throw new System.NotImplementedException();
    }

    public float GetInverseMass()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetLastFrameAcceleration()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetLinearVelocity()
    {
        throw new System.NotImplementedException();
    }

    public OrientedBounds GetOrientedBounds()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetPosition()
    {
        throw new System.NotImplementedException();
    }

    public Quaternion GetRotation()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetVelocity()
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetVelocityAtPoint(Vector3 worldPosition)
    {
        var maxLength = transform.lossyScale.x;

        var dir = worldPosition - transform.position;

        var dist = dir.magnitude / maxLength;

        var velocity = MaxForce * (1 - dist) * dir.normalized;

        return velocity;
    }

    public void SetAngularVelocity(Vector3 newVel)
    {
        throw new System.NotImplementedException();
    }

    public void SetLinearVelocity(Vector3 newVel)
    {
        throw new System.NotImplementedException();
    }

    public void SetPosition(Vector3 newPos)
    {
        throw new System.NotImplementedException();
    }

    public void SetRotation(Quaternion newRot)
    {
        throw new System.NotImplementedException();
    }

    public void OnCollisionStay(Collision c)
    {
        var colliderRB = c.body as Rigidbody;
        colliderRB.AddForce(GetVelocityAtPoint(colliderRB.position) - colliderRB.velocity, ForceMode.VelocityChange);
    }
}
