using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Schnibble.SchPhysics;

public class SchDefaultRigidBody : MonoBehaviour, IRigidBody
{
    public Vector3 LastPosition;
    public Vector3 Velocity;

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
        return Velocity;
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

    public void FixedUpdate()
    {
        Velocity = (transform.position - LastPosition) / Time.deltaTime;
        LastPosition = transform.position;
    }

    public void OnCollisionStay(Collision c)
    {
        var colliderRB = c.body as Rigidbody;
        colliderRB.AddForce(GetVelocityAtPoint(colliderRB.position) - colliderRB.velocity, ForceMode.VelocityChange);
    }
}
