using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MimicPot : MonoBehaviour
{

    public GameObject mimicVersionRef;
    public List<Transform> mimicPath;
    private int pathIndex = -1;
    private GameObject mimicVersionInst;
    
    private bool mimicTriggered = false;
    private NavMeshAgent agent;
    private LayerMask groundTargetLayerMask;

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
            // NavMeshHit navHit;
            // int layermask = NavMesh.GetAreaFromName("MimicPot");
            // NavMesh.SamplePosition (transform.position, out navHit, agent.height*2, layermask);
            agent.Warp(getGroundContact(transform.position));
            // Debug.DrawRay(navHit.position, Vector3.up * 5f, Color.red,10f );
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
            if (!agent.hasPath)
            { tryGoToNextPoint(); } // first call
            else if (agent.pathStatus==NavMeshPathStatus.PathComplete)
            { tryGoToNextPoint(); }
        }
    }

    private void tryGoToNextPoint()
    {
        if (mimicPath.Count <= 0)
        {
            Debug.LogWarning("Empty Path on MimicPot.");
            return;
        }
        pathIndex = (pathIndex >= mimicPath.Count-1) ? 0 : pathIndex+1;
        
        NavMeshHit navHit;
        // layermasks not working
        //int layermask = NavMesh.GetAreaFromName("MimicPot");
        //float dist = agent.height*2;
        //Debug.DrawRay(mimicPath[pathIndex].position, transform.up * 5, Color.red, 10f);
        //NavMesh.SamplePosition (mimicPath[pathIndex].position, out navHit, dist, layermask);

        agent.SetDestination(getGroundContact(mimicPath[pathIndex].position));
    }

    public Vector3 getGroundContact(Vector3 iPos)
    {
        RaycastHit hit;
        if (Physics.Raycast(iPos, -transform.TransformDirection(Vector3.up), out hit, Mathf.Infinity, groundTargetLayerMask))
        {
            Debug.DrawRay(iPos, -transform.TransformDirection(Vector3.up) * hit.distance, Color.red);
            return hit.point;
        } else {
            return Vector3.zero;
        }
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
