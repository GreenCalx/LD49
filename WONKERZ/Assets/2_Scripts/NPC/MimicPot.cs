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
    public float fragmentExplodeForce = 10f; 
    public float timeBeforeFragmentClean = 10f;
    
    public float invulTimeAfterTrigger = 0.5f;
    private float elapsedInvulTimeAfterTrigger = 999f;

    private int pathIndex = -1;
    private GameObject mimicVersionInst;
    
    private bool mimicTriggered = false;
    private NavMeshAgent agent;
    private bool inReversedPath = false;

    private TrackEvent trackEvent;
    public AudioClip breakSFX;
    public AudioSource mimicCompleteSFX;

    public GameObject mimicBreakPS;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        trackEvent = GetComponent<TrackEvent>();

        mimicTriggered = false;
        elapsedInvulTimeAfterTrigger = 999f;

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
            if (invulTimeAfterTrigger > elapsedInvulTimeAfterTrigger)
            { elapsedInvulTimeAfterTrigger += Time.deltaTime; }

            Debug.DrawRay(transform.position, transform.forward * 5, Color.yellow, 3f);
            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning("Agent is not on Navmesh. Waiting for Unity's link agent/navmesh.");
                return;
            }

            if (playerDetector.playerInRange)
            {
                agent.isStopped = false;

                if (playerInPathForNextPoint())
                {
                     inReversedPath = !inReversedPath;
                }
                //tryGoToNextPoint();

                
                if (Vector3.Distance( agent.destination, transform.position) <= agent.stoppingDistance)
                { 
                    if (!agent.hasPath)
                        tryGoToNextPoint(); 
                }
            }
            else if (!agent.hasPath)
            {
                agent.isStopped = true;
            } else {
                agent.isStopped = false;
            }
            //float angle = Vector3.Angle(transform.position, Access.Player().transform.position);

        }
    }

    private bool playerInPathForNextPoint()
    {
        float dist = DistanceLineSegmentPoint(transform.position, mimicPath[pathIndex].position, Access.Player().transform.position);
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
        
        Debug.DrawRay(navHit.position, transform.up * 10, Color.green, 0.5f);
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
            elapsedInvulTimeAfterTrigger = 0f;

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

        // Play SFX
        Schnibble.Utils.SpawnAudioSource( breakSFX, transform);

        // Explode mimic's butt
        List<MeshRenderer> mrs = new List<MeshRenderer>(mimicVersionInst.GetComponentsInChildren<MeshRenderer>());
        foreach (MeshRenderer rend in mrs)
        {
            if (!rend.gameObject.name.Contains("_cell"))
                continue;
            
            rend.transform.parent = null;
            
            Rigidbody loc_rb = rend.gameObject.AddComponent<Rigidbody>();
            MeshCollider loc_mc = rend.gameObject.AddComponent<MeshCollider>();
            loc_mc.convex = true;
            rend.gameObject.layer = LayerMask.NameToLayer("NoPlayerCollision");
            Vector3 forceDir = (-1)*transform.up;
            loc_rb.AddForce(forceDir.normalized * fragmentExplodeForce, ForceMode.Impulse);
        
            Destroy(rend.gameObject, timeBeforeFragmentClean);
        }

    }

    void OnCollisionEnter(Collision iCollision)
    {
        if (!mimicTriggered)
            return;
        
        if (invulTimeAfterTrigger > elapsedInvulTimeAfterTrigger)
            return;

        if (Utils.collisionIsPlayer(iCollision))
        {
            trackEvent.setSolved();

            if (!!mimicCompleteSFX)
                Schnibble.Utils.SpawnAudioSource( mimicCompleteSFX, transform);

            if (!!mimicBreakPS)
            {
                GameObject killPS = Instantiate(mimicBreakPS, transform.position, Quaternion.identity);
                killPS.transform.localScale = new Vector3(5f, 5f, 5f) * 1.2f;
                killPS.GetComponent<ExplosionEffect>().runEffect();
            }
            
            Destroy(gameObject);
        }
    }
}
