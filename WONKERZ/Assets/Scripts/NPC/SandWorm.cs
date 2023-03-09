using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SandWorm : MonoBehaviour
{
    public float wanderRadius;
    public float wanderTimer;

    public ParticleSystem PS_Front;
    public ParticleSystem PS_Back;
    public GroundDetector FrontDetector;
    public GroundDetector BackDetector;
 
    private NavMeshAgent agent;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent> ();
        timer = wanderTimer;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
 
        if (timer >= wanderTimer) {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            Debug.DrawRay(newPos, Vector3.up, Color.red);
            agent.SetDestination(newPos);
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

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask) 
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
 
        randDirection += origin;
 
        NavMeshHit navHit;
 
        NavMesh.SamplePosition (randDirection, out navHit, dist, layermask);
 
        return navHit.position;
    }
}
