using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class OnlinePhysicsSimulator : MonoBehaviour
{
    PhysicsScene physicsScene;
    bool simulatePhysicsScene;
    // Start is called before the first frame update
    void Awake()
    {
        if (NetworkServer.active)
        {
            physicsScene = gameObject.scene.GetPhysicsScene();
            simulatePhysicsScene = physicsScene.IsValid() && physicsScene != Physics.defaultPhysicsScene;
        }
        else
        {
            enabled = false;
        }        
    }

    [ServerCallback]
    void FixedUpdate()
    {
        if (simulatePhysicsScene)
            physicsScene.Simulate(Time.fixedDeltaTime);
    }
}
