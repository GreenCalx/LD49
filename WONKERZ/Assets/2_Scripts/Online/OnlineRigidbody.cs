using System;

using UnityEngine;

using Mirror;

using Schnibble;

public class OnlineRigidbody : NetworkBehaviour {
    public Rigidbody body;
    public SchRigidbodyBehaviour schBody;

    public Transform tf;

    public float positionCorrectionThreshold;
    public float rotationCorrectionThreshold;

    bool wasKinematic;

    void Awake()
    {
        tf   = transform;
        body = GetComponent<Rigidbody>();
        schBody = GetComponent<SchRigidbodyBehaviour>();

        if (body    == null) this.LogError("Prediction: "+ name + " is missing a Rigidbody component.");
        if (schBody == null) this.LogError($"Prediction: "+ name + " is missing a SchRigidbodyBehaviour component.");

        if (body) wasKinematic = body.isKinematic;
    }

    void FixedUpdate() {
        if (isClient)
        {
            // on the client, we own it only if clientAuthority is enabled,
            // and we have authority over this object.
            bool owned = isClient && authority;

            // only set to kinematic if we don't own it
            // otherwise don't touch isKinematic.
            // the authority owner might use it either way.
            if (!owned && body) body.isKinematic = true;
        }
    }

    void Update() {
        if(isServer) {
            SetDirty();
        }
    }

    public override void OnStopServer() {if(body) body.isKinematic = wasKinematic;}
    public override void OnStopClient() { if(body) body.isKinematic = wasKinematic;}

    void OnReceivedState(RigidbodyState state)
    {
        if (Vector3.Distance(state.position, body.position) < positionCorrectionThreshold &&
            Quaternion.Angle(state.rotation, body.rotation) < rotationCorrectionThreshold)
        {
            return;
        }

        tf.SetPositionAndRotation(state.position, state.rotation);

        schBody.velocity = state.velocity;
        schBody.angularVelocity = state.angularVelocity;
    }

    public override void OnSerialize(NetworkWriter writer, bool initialState)
    {
        // Mirror:
        // could be called without having been awaken...
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        if (tf) tf.GetPositionAndRotation(out position, out rotation);

        writer.WriteFloat(Time.deltaTime);
        writer.WriteVector3(position);
        writer.WriteQuaternion(rotation);

        if (body) {
            writer.WriteVector3(body.velocity);
            writer.WriteVector3(body.angularVelocity);
        }
        else {
            writer.WriteVector3(Vector3.zero);
            writer.WriteVector3(Vector3.zero);
        }
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        // deserialize data
        // we want to know the time on the server when this was sent, which is remoteTimestamp.
        double timestamp = NetworkClient.connection.remoteTimeStamp;

        // server send state at the end of the frame.
        // parse and apply the server's delta time to our timestamp.
        // otherwise we see noticeable resets that seem off by one frame.
        double serverDeltaTime = reader.ReadFloat();
        timestamp += serverDeltaTime;

        // parse state
        Vector3 position        = reader.ReadVector3();
        Quaternion rotation     = reader.ReadQuaternion();
        Vector3 velocity        = reader.ReadVector3();
        Vector3 angularVelocity = reader.ReadVector3();

        tf.SetPositionAndRotation(position, rotation);

        if (schBody) {
            schBody.velocity        = velocity;
            schBody.angularVelocity = angularVelocity;
        }
        // process received state
        //OnReceivedState(new RigidbodyState(timestamp, Vector3.zero, position, Quaternion.identity, rotation, Vector3.zero, velocity, Vector3.zero, angularVelocity));
    }

}
