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
    public Animator animator;
 
    private NavMeshAgent agent;
    private float timer;
    private LayerMask groundTargetLayerMask;
    private LayerMask undergroundTargetLayerMask;
    private Vector3 projectionOnOtherGround;
    private readonly string animParmGoSurface = "GoToSurface";
    private readonly string animParmGoUnderground = "GoToUnderground";
    private readonly string animParmGoTurn = "turnClockwise";
    
    private bool warpCC_started = false;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent> ();
        timer = wanderTimer;
        warpCC_started = false;

        groundTargetLayerMask       = LayerMask.GetMask("Default");
        undergroundTargetLayerMask  = LayerMask.GetMask("OnlyCollideDefault");
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
 
        if (callGroundLevelChange)
        {
            setNewGroundLevel();
        }

        if (timer >= wanderTimer) 
        {
            setNewDestination();
            timer = 0;
        }
        
        // Particles
        if (!isUnderground)
        {
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
        } else {
            PS_Front.Stop();
            PS_Back.Stop();
        }
    }

    IEnumerator warpWorm(Vector3 position, bool waitAnimToFinish)
    {
        warpCC_started = true;
        if (waitAnimToFinish)
        {
            while (animator.GetCurrentAnimatorStateInfo(0).IsTag("crawl"))
            {
                 yield return new WaitForSeconds(.01f);
            }
            float motion_progress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            while (motion_progress<=0.8f)
            {
                motion_progress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                yield return new WaitForSeconds(.01f);
            }
            agent.Warp(position);
            isUnderground = !isUnderground;
            agent.baseOffset = (isUnderground) ? agentOffsetUnderground : agentOffsetSurface;
            callGroundLevelChange = false;
            warpCC_started = false;
            timer = wanderTimer + 1f;
            
        } else {
            agent.Warp(position);
            isUnderground = !isUnderground;
            agent.baseOffset = (isUnderground) ? agentOffsetUnderground : agentOffsetSurface;
            while (animator.GetCurrentAnimatorStateInfo(0).IsTag("crawl"))
            {
                 yield return new WaitForSeconds(.01f);
            }
            callGroundLevelChange = false;
            warpCC_started = false;
            timer = wanderTimer + 1f;
        }
    }

    private void setNewGroundLevel()
    {
        if (warpCC_started)
            return;

        int layer = (isUnderground) ? groundTargetLayerMask.value : undergroundTargetLayerMask.value; // layer change
        offMeshLink.startTransform  = transform;
        offMeshLink.endTransform    = endLinkHandler;
        
        animator.SetBool(animParmGoUnderground, !isUnderground);
        animator.SetBool(animParmGoSurface, isUnderground);

        StartCoroutine(warpWorm(projectionOnOtherGround, !isUnderground));
        //StartCoroutine(warpWorm(projectionOnOtherGround, true));
        
    }

    private void setNewDestination()
    {
        Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
        
        if (drawDebugRays)
            Debug.DrawRay(newPos, Vector3.up, Color.green);
        
        animator.SetBool(animParmGoUnderground, false);
        animator.SetBool(animParmGoSurface, false);

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
