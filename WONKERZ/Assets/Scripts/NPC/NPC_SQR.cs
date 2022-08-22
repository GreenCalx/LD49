using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_SQR : MonoBehaviour
{
    public float walkable_radius = 20f;
    public float idle_duration = 2.0f;
    public float destination_tolerance = 0.5f;

    private NavMeshAgent    navmesh;
    private NavMeshPath     path;
    
    private bool is_running;
    private float idle_elapsed_time;

    public  Animator    animator;
    private const string    run_anim_parm = "RUN";

    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    public void init()
    {
        navmesh = GetComponent<NavMeshAgent>();
        is_running = false;
        path = new NavMeshPath();
        idle_elapsed_time = 0f;

        navmesh.SetDestination(RandomNavmeshLocation(walkable_radius));
    }

    // Update is called once per frame
    void Update()
    {
        if (navmesh.remainingDistance <= destination_tolerance)
        {
            if (is_running)
                idle_elapsed_time = 0f;
            is_running = false;

            if ( idle_elapsed_time > idle_duration )
            {
                navmesh.SetDestination(RandomNavmeshLocation(walkable_radius));
                is_running = true;
            } else {
                idle_elapsed_time += Time.deltaTime;
            }
            
        } else {
            is_running = true;
        }
        animator.SetBool( run_anim_parm, is_running);
    }

    private Vector3 RandomNavmeshLocation(float radius) {
         Vector3 randomDirection = Random.insideUnitSphere * radius;
         randomDirection += transform.position;
         NavMeshHit hit;
         Vector3 finalPosition = Vector3.zero;
         if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
             finalPosition = hit.position;            
         }
         return finalPosition;
     }

    public void updateTarget( Vector3 iTarget)
    {
        NavMesh.CalculatePath(transform.position, iTarget, NavMesh.AllAreas, path);
        navmesh.path = path;
    }

}
