using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MimicPot : MonoBehaviour
{
    public GameObject mimicVersionRef;
    public PlayerDetector playerDetector;
    public List<Transform> mimicPath;
    public float turnaroundDistThreshold = 10f;

    private int pathIndex = -1;
    private GameObject mimicVersionInst;
    
    private bool mimicTriggered = false;
    private NavMeshAgent agent;
    private LayerMask groundTargetLayerMask;
    private bool inReversedPath = false;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        int pathIndex = -1; // unset
        mimicTriggered = false;
        groundTargetLayerMask       = LayerMask.GetMask("Default");

        if (!agent.isOnNavMesh)
        {
            Debug.Log("Agent not on navmesh.");
            NavMeshHit navHit;
            int layermask = NavMesh.GetAreaFromName("MimicPot");
            NavMesh.SamplePosition (transform.position, out navHit, agent.height*2, -1 * layermask);

            agent.Warp(navHit.position);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (mimicTriggered)
        {
            Debug.DrawRay(transform.position, transform.forward * 5, Color.yellow, 3f);
            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning("Agent is not on Navmesh. Waiting for Unity's link agent/navmesh.");
                return;
            }

            agent.isStopped = !playerDetector.playerInRange;
            float angle = Vector3.Angle(transform.position, Access.Player().transform.position);
            Debug.Log("Angle : " + angle);
            if (playerInPathForNextPoint() && playerDetector.playerInRange)
            {
                inReversedPath = !inReversedPath;
                tryGoToNextPoint();
            }

            if ( Vector3.Distance( agent.destination, transform.position) <= agent.stoppingDistance)
            { tryGoToNextPoint(); }
        }
    }

    private bool playerInPathForNextPoint()
    {
        float dist = DistanceLineSegmentPoint(transform.position, mimicPath[pathIndex].position, Access.Player().transform.position);
        Debug.Log("Distance is " + dist);
        return dist <= turnaroundDistThreshold;
    }

    public static float DistanceLineSegmentPoint(Vector3 start, Vector3 end, Vector3 point)
    {
        var wander = point - start;
        var span = end - start;
     
        // Compute how far along the line is the closest approach to our point.
        float t = Vector3.Dot(wander, span) / span.sqrMagnitude;
     
        // Restrict this point to within the line segment from start to end.
        t = Mathf.Clamp01(t);
     
        Vector3 nearest = start + t * span;
        return (nearest - point).magnitude;
    }


    private void tryGoToNextPoint()
    {
        if (mimicPath.Count <= 0)
        {
            Debug.LogWarning("Empty Path on MimicPot.");
            return;
        }

        if (inReversedPath)
        { tryGoToPreviousPoint(); return; }

        pathIndex = (pathIndex >= mimicPath.Count-1) ? 0 : pathIndex+1;
        
        NavMeshHit navHit;
        // layermasks not working
        int layermask = NavMesh.GetAreaFromName("MimicPot");
        float dist = agent.height*2;

        NavMesh.SamplePosition (mimicPath[pathIndex].position, out navHit, dist, -1 * layermask);

        agent.SetDestination(navHit.position);
    }

    private void tryGoToPreviousPoint()
    {
        if (mimicPath.Count <= 0)
        {
            Debug.LogWarning("Empty Path on MimicPot.");
            return;
        }
        pathIndex = (pathIndex > 0) ? pathIndex-1 : mimicPath.Count-1;
        
        NavMeshHit navHit;
        // layermasks not working
        int layermask = NavMesh.GetAreaFromName("MimicPot");
        float dist = agent.height*2;

        NavMesh.SamplePosition (mimicPath[pathIndex].position, out navHit, dist, -1 * layermask);

        agent.SetDestination(navHit.position);
    }

    public void triggerMimic()
    {
        if (!mimicTriggered)
        {
            Debug.Log("MIMIC");
            spawnMimic();
            agent.enabled = true;

            mimicTriggered = true;
            if ((agent==null) || (mimicPath==null))
            { Debug.LogError("Missing NavMeshAgent or Path on Mimic Pot."); Destroy(gameObject); }
            tryGoToNextPoint();
        }
    }

    private void spawnMimic()
    {
        mimicVersionInst = GameObject.Instantiate(mimicVersionRef, transform);

        mimicVersionInst.transform.position = transform.position;
        mimicVersionInst.transform.rotation = transform.rotation;

        MeshCollider mc = GetComponentInChildren<MeshCollider>();
        if (!!mc)
            mc.enabled = false;
        MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
        if (!!mr)
            mr.enabled = false;
        Destroy(mc.gameObject);
    }
}
