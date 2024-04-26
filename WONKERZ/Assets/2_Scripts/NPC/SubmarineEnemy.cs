using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble.AI;

public class SubmarineEnemy : WkzEnemy
{
    [Header("Submarine")]
    public float speedInAggro = 60f;
    public float speedOufOfAggro = 10f;
    public float min_dist = 50f;
    public float max_dist = 150f;
    public float timeBetweenAction = 1f;
    public float extraSearchTime = 5f;

    public Transform periscope;
    public Transform sightOrigin;

    public OctoPump trapOctoPump;

    private float idle_timer;
    private float extraSearch_timer;
    private Vector3 lastKnownTargetPosition;

    // Start is called before the first frame update
    void Start()
    {
        ai_init();
        idle_timer = 0f;
        extraSearch_timer = 0f;
        agent.speed = speedOufOfAggro;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnAggro()
    {
        Debug.Log("Submarine : On Aggro");
        extraSearch_timer = 0f;
        agent.speed = speedInAggro;

        // Launch Player filature

    }

    protected override void InAggro()
    {
        if (idle_timer < timeBetweenAction)
        {
            //facePlayer();
            idle_timer += Time.deltaTime;
            return;
        }
        idle_timer = 0f;

        // Find next position 
        float targetDist = Vector3.Distance(transform.position, playerAggroed.position);
        if (targetDist > max_dist)
        {
            // move closer
            currStrategy = SchAIStrategies.AGENT_STRAT.AIM_FOR;
        }
        else if (targetDist < min_dist)
        {
            // move further
            currStrategy = SchAIStrategies.AGENT_STRAT.FLEE;
        }
        else {
            currStrategy = SchAIStrategies.AGENT_STRAT.ANCHOR;
        }
        Vector3 nextPos = GetNextPositionFromCoordinator();
        nextPos.y = transform.position.y;
        if (!agent.SetDestination(nextPos))
        {  // Destination can be above navmesh ?
            Debug.LogWarning("Failed to find path");
        }

        // Periscope
        // rotate periscope to face player
        if (playerAggroed!=null)
        {
            lastKnownTargetPosition = playerAggroed.position;

            Vector3 difference = lastKnownTargetPosition - transform.position;
            difference.y = 0f;

            float rotationY = Mathf.Atan2(difference.x, difference.z) * Mathf.Rad2Deg;
            periscope.rotation = Quaternion.Euler(new Vector3(0f, rotationY, 0f));
        }
        
        // check line of sight

        float radius = 1f;
        RaycastHit hitInfo;
        bool sightBlocked = Physics.SphereCast( sightOrigin.position, radius, periscope.forward * targetDist, out hitInfo, targetDist);
        if (sightBlocked)
        {
            if (Utils.colliderIsPlayer(hitInfo.collider))
            {
                Debug.DrawRay(sightOrigin.position, periscope.forward * targetDist, Color.yellow, 0.2f);
                trapOctoPump.setTargetInSight(true);
                trapOctoPump.lastKnownTargetPosition = lastKnownTargetPosition;
                // call octopump airstrike
            } else {
                Debug.DrawRay(sightOrigin.position, periscope.forward * targetDist, Color.red, 0.2f);
                trapOctoPump.setTargetInSight(false);
            }
            
        } else {
            Debug.DrawRay(sightOrigin.position, periscope.forward * targetDist, Color.green, 0.2f);
        }
    }

    protected override void OutAggro()
    {
        // Try to keep filature for a few sec
        if (extraSearch_timer < extraSearchTime)
        {
            extraSearch_timer += Time.deltaTime;
            currStrategy = SchAIStrategies.AGENT_STRAT.NONE;
            agent.SetDestination(lastKnownTargetPosition);
        }
        agent.speed = speedOufOfAggro;
    }

    protected override void NotInAggro()
    {
        Vector3 nextPos = GetNextPositionFromCoordinator();
        if (!agent.SetDestination(nextPos))
        {  // Destination can be above navmesh ?
            Debug.LogWarning("Failed to find path");
        }
    }

    protected override void PreLaunchAction()
    {
        // nothing?
    }
    protected override void PreStopAction()
    {
        // nothing?
    }

    public override void kill()
    {
        Debug.Log("KILL");
        ai_kill();

        Destroy(gameObject);
    }

}
