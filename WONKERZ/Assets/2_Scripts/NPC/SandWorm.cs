using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using Schnibble;
using static UnityEngine.Debug;

public class SandWorm : MonoBehaviour
{
    [Header("Debug")]
    public bool drawDebugRays = true;

    [Header("Tweaks")]
    public float wanderRadius;
    public float wanderTimer;

    [Header("Values")]
    private float defaultAgentOffset = 0f;
    public float agentMaxOffsetOnChase = 5f;
    public float agentOffsetStepChange = 0.2f;
    [Range(0f,1f)]
    public float speedThrshldForParticles = 0.2f;

    [Header("Self References")]

    public ParticleSystem PS_FrontUp;
    public ParticleSystem PS_FrontDown;
    public ParticleSystem PS_Back;
    public GroundDetector FrontDetector;
    public GroundDetector BackDetector;

    public ObjectDetector pinEaterDetector;

    private Transform chasedTarget;
    private NavMeshAgent agent;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent> ();
        timer = wanderTimer;
        defaultAgentOffset = agent.baseOffset;

        if (!agent.isOnNavMesh)
        {
            Log("Agent not on navmesh.");
            NavMeshHit navHit;
            int layermask = NavMesh.GetAreaFromName("Ground");
            NavMesh.SamplePosition (transform.position, out navHit, agent.height*2, -1 * layermask);

            agent.Warp(navHit.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
 
        if (pinEaterDetector.objectInRange){
            if (chasedTarget==null)
            {
                chasedTarget = pinEaterDetector.detectedTransform;
            }
            chaseTarget();
        } else {
            chasedTarget = null;
        }

        if (chasedTarget==null)
        {
            if (timer >= wanderTimer) 
            {
                setNewWanderDestination();
                timer = 0f;
            } else if ( agent.remainingDistance <= 10f )
            {
                setNewWanderDestination();
                timer = 0f;
            }
        }
        // Particles
        float agent_speed = (agent.velocity.magnitude/agent.speed);
        if (speedThrshldForParticles < agent_speed)
        {
            if (FrontDetector.crossedGround)
            {
                if (chasedTarget)
                {
                    if (!PS_FrontDown.isPlaying)
                    {
                        PS_FrontDown.Play();
                    }
                }
                else 
                { 
                    PS_FrontDown.Stop(); 
                    if (!PS_FrontUp.isPlaying)
                    { PS_FrontUp.Play(); }
                }
            } else {
                PS_FrontUp.Stop();
            }
            if (BackDetector.crossedGround)
            {
                if (!PS_Back.isPlaying)
                { PS_Back.Play(); }
            } else {
                PS_Back.Stop();
            }
        } else {
            PS_FrontUp.Stop();
            PS_Back.Stop();
            PS_FrontDown.Stop();
        }
    }

    private void setNewWanderDestination()
    {
        if (agent.baseOffset != defaultAgentOffset)
        {
            agent.baseOffset = defaultAgentOffset;
        }

        int layermask = NavMesh.GetAreaFromName("Ground");
        Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1 * layermask);
        
        if (drawDebugRays)
            DrawRay(newPos, Vector3.up, Color.green);

        if(!agent.SetDestination(newPos))
        {
            this.Log("SandWorm failed to reach wander destination");
        }
    }

    private void chaseTarget()
    {
        // check if target is reachable
        NavMeshPath navMeshPath = new NavMeshPath();
        if (!agent.CalculatePath(chasedTarget.position, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
            return;


        Vector3 newPos = chasedTarget.position;
        //newPos.y = transform.position.y; // approx projection on current plane
        NavMeshHit navHit;
        NavMesh.SamplePosition (newPos, out navHit, 50f, -1);
        newPos = navHit.position;

        if (drawDebugRays)
            DrawRay(newPos, Vector3.up*500, Color.blue);

        if (!agent.SetDestination(newPos))
        {
            // Target is not accessible /  behind navmehs obstacle
            setNewWanderDestination();
        }
        if (agent.baseOffset < agentMaxOffsetOnChase)
            agent.baseOffset += agentOffsetStepChange;
    }

    void FixedUpdate()
    {
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
