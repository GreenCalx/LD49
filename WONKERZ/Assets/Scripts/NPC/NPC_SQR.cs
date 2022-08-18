using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_SQR : MonoBehaviour
{
    public float walkable_radius = 20f;
    public float idle_duration = 2.0f;

    private NavMeshAgent    navmesh;
    private NavMeshPath     path;
    
    private bool is_running;
    private float idle_elapsed_time;

    private Animator        animator;
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
        animator = GetComponent<Animator>();

        updateTarget( RandomNavmeshLocation(walkable_radius));
    }

    // Update is called once per frame
    void Update()
    {
        if (navmesh.remainingDistance == 0f)
        {
            if (is_running)
                idle_elapsed_time = 0f;
            is_running = false;

            if ( idle_elapsed_time > idle_duration )
            {
                //updateTarget( RandomNavmeshLocation(walkable_radius)); // request new target position
                navmesh.SetDestination(RandomNavmeshLocation(walkable_radius));
            } else {
                idle_elapsed_time += Time.deltaTime;
            }
            
        } else {
            is_running = true;
        }
        
        //animator.SetBool( run_anim_parm, is_running);     
    }

    private Vector3 RandomNavmeshLocation(float radius) {
         Vector3 randomDirection = Random.insideUnitSphere * radius;
         randomDirection += transform.position;
         NavMeshHit hit;
         Vector3 finalPosition = Vector3.zero;
         if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
             finalPosition = hit.position;            
         }
         Debug.Log("SQR : Next random position" + finalPosition.ToString());
         return finalPosition;
     }

    public void updateTarget( Vector3 iTarget)
    {
        NavMesh.CalculatePath(transform.position, iTarget, NavMesh.AllAreas, path);
        navmesh.path = path;
    }

}
