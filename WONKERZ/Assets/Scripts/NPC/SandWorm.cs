using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SandWorm : MonoBehaviour
{
    [Header("Debug")]
    public bool callGroundLevelChange = false;
    public bool drawDebugRays = true;



    [Header("Tweaks")]
    public float wanderRadius;
    public float wanderTimer;
    public float agentOffsetUnderground = 0;
    public float agentOffsetSurface = -10;
    [Header("Values")]
    public bool isUnderground = true;
    [Header("References")]
    public OffMeshLink offMeshLink;
    public Transform endLinkHandler;
    public ParticleSystem PS_Front;
    public ParticleSystem PS_Back;
    public GroundDetector FrontDetector;
    public GroundDetector BackDetector;
 
    private NavMeshAgent agent;
    private float timer;
    private LayerMask groundTargetLayerMask;
    private LayerMask undergroundTargetLayerMask;
    private Vector3 projectionOnOtherGround;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent> ();
        timer = wanderTimer;

        groundTargetLayerMask       = LayerMask.GetMask("Default");
        undergroundTargetLayerMask  = LayerMask.GetMask("OnlyCollideDefault");
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
 
        if (timer >= wanderTimer) 
        {
            setNewDestination();
            timer = 0;
        }
        
        if (FrontDetector.crossedGround)
        {
            if (!PS_Front.isPlaying)
            { PS_Front.Play(); }
        } else {
            PS_Front.Stop();
        }

        if (BackDetector.crossedGround)
        {
            if (!PS_Back.isPlaying)
            { PS_Back.Play(); }
        } else {
            PS_Back.Stop();
        }

        
    }

    private void setNewDestination()
    {
        int layer = -1;
        Vector3 newPos = Vector3.zero;

        if (callGroundLevelChange)
        {
            layer = (isUnderground) ? groundTargetLayerMask.value : undergroundTargetLayerMask.value; // layer change
            offMeshLink.startTransform  = transform;
            offMeshLink.endTransform    = endLinkHandler;

            agent.Warp(projectionOnOtherGround);
            newPos = RandomNavSphere(projectionOnOtherGround, wanderRadius, layer);
            callGroundLevelChange = false;
            isUnderground = !isUnderground;
            agent.baseOffset = (isUnderground) ? agentOffsetUnderground : agentOffsetSurface;
        } else {
            newPos = RandomNavSphere(transform.position, wanderRadius, layer);
            Debug.DrawRay(newPos, Vector3.up, Color.green);
        }

        agent.SetDestination(newPos);
    }

    void FixedUpdate()
    {
        projectionOnOtherGround = (isUnderground) ? getGroundContact() : getUndergroundContact();
        endLinkHandler.position = projectionOnOtherGround;
    }

    public Vector3 getGroundContact()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit, Mathf.Infinity, groundTargetLayerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.up) * hit.distance, Color.yellow);
            return hit.point;
        } else {
            return Vector3.zero;
        }
    }

    public Vector3 getUndergroundContact()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.TransformDirection(Vector3.up), out hit, Mathf.Infinity, undergroundTargetLayerMask))
        {
            Debug.DrawRay(transform.position, -transform.TransformDirection(Vector3.up) * hit.distance, Color.red);
            return hit.point;
        } else {
            return Vector3.zero;
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask) 
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
 
        randDirection += origin;
 
        NavMeshHit navHit;
 
        NavMesh.SamplePosition (randDirection, out navHit, dist, layermask);
 
        return navHit.position;
    }
}
