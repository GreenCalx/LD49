using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabEnemy : WkzEnemy
{
    // Start is called before the first frame update
    void Start()
    {
        ai_init();
        canAggroPlayer = false;
    }

    public override void kill()
    {
        ai_kill();

        Destroy(gameObject);
    }

    protected override void NotInAggro()
    {
        Vector3 nextPos = GetNextPositionFromCoordinator();
        if (!agent.SetDestination(nextPos))
        {  // Destination can be above navmesh ?
            Debug.LogWarning( gameObject.name + " Failed to find path");
        }
    }
}
