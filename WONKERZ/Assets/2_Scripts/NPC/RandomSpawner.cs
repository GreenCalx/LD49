using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RandomSpawner : MonoBehaviour
{
    public Transform[] spawns;

    public void respawn(GameObject iToRespawn, bool isANavMeshAgent)
    {
        if ((spawns==null)||(spawns.Length==0))
            return;
        int n_spawns = spawns.Length;
        int k = Random.Range(0, n_spawns-1);
        if (isANavMeshAgent)
        {
            NavMeshHit navHit;
            NavMesh.SamplePosition (spawns[k].position, out navHit, 25f, -1);
            iToRespawn.transform.position = navHit.position;
        } else {
            iToRespawn.transform.position = spawns[k].position;
        }
        
    }
}
