using UnityEngine;
using UnityEngine.AI;

public class NPC_SQR : MonoBehaviour
{
    public enum SQR_BEHAVIOURS
    {
        KICKER, BOMB_DROPPER
    }
    public SQR_BEHAVIOURS behaviour;

    [Header("KickerSQR Tweaks")]
    public float walkable_radius = 20f;
    public float idle_duration = 2.0f;
    public float destination_tolerance = 0.5f;
    public KickAction kickAction;

    private bool is_running;
    private float idle_elapsed_time;

    [Header("BombDropperSQR Tweaks")]
    public Transform bombDropSpot;
    public float timeStepForBombDrop = 5f;
    public float bombDropRate = 1f;
    public GameObject bombRef;
    public float BombDropForce = 1f;
    public float delayBombAnimStart = 0.5f;

    [Range(0f, 1f)]
    public float shootAngleVariationDelta = 5f;
    [Range(1f, 10f)]
    public float shootingForceVariationDelta = 2f;

    [HideInInspector]
    public bool exitReached = false;
    [HideInInspector]
    public bool launchBombDrop = false;

    private float timeOfLastBombDrop = 0f;
    private bool shouldDropBomb = false;

    ///
    private NavMeshAgent navmesh;
    private NavMeshPath path;
    private bool deactivate_sqr;

    [Header("Global Tweaks")]
    public Animator animator;
    private const string run_anim_parm = "RUN";
    private const string kick_anim_parm = "KICK";
    private const string runback_anim_parm = "RUN_BACKWARD";
    private const string dropbomb_anim_parm = "DROP_BOMB";
    private const string jump_anim_parm = "JUMP";
    private const string crowpose_anim_parm = "CROW_POSE";
    private float delayAnimElapsed = 0f;


    public PlayerDetector detector;
    public PlayerDetector attackRangeDetector;

    public Transform fleeGoal;
    public Transform exitJumpPoint;
    [Range(20f, 200f)]
    public float fleeTotDistanceStep = 50f;
    [Range(1f, 20f)]
    public float jumpExitTotDistanceStep = 50f;

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
        delayAnimElapsed = 0f;
        exitReached = false;
        launchBombDrop = false;
        deactivate_sqr = false;

        if (behaviour == SQR_BEHAVIOURS.KICKER)
            navmesh.SetDestination(RandomNavmeshLocation(walkable_radius));
    }

    // Update is called once per frame
    void Update()
    {
        if (deactivate_sqr)
            return;

        if (behaviour == SQR_BEHAVIOURS.KICKER)
            kickerUpdate();
        else if (behaviour == SQR_BEHAVIOURS.BOMB_DROPPER)
            bombDropperUpdate();
    }

    private void bombDropperUpdate()
    {
        if (!launchBombDrop)
        {
            if (detector.playerInRange || detector.dummyInRange)
            {
                launchBombDrop = true;
            }
            else
            {
                return;
            }
        }

        timeOfLastBombDrop += Time.deltaTime;

        // if goal is reached, we exit the character by exit point
        if (exitReached && !!exitJumpPoint)
        {
            // jump to exitPoint
            //navmesh.SetDestination(exitJumpPoint.position);
            navmesh.enabled = false;
            exitToCrowPose();
            return;
        }

        // RunBackward if player is too close
        if (detector.playerInRange || detector.dummyInRange)
        {
            Vector3 nextPoint = Vector3.MoveTowards(transform.position, fleeGoal.position, fleeTotDistanceStep);
            fleeBackwardToPoint(nextPoint);
        }
        else
        {
            chill();
        }

        // poll if bombdrop is needed
        if (shouldDropBomb)
        {
            if (delayAnimElapsed >= delayBombAnimStart)
                dropBomb();
            else
                delayAnimElapsed += Time.deltaTime;
        }
        if (timeOfLastBombDrop >= timeStepForBombDrop)
        {
            startDropBomb();
        }


    }

    private void exitToCrowPose()
    {
        animator.SetBool(dropbomb_anim_parm, false);
        animator.SetBool(jump_anim_parm, true);
        shouldDropBomb = false;

        Vector3 nextPoint = Vector3.MoveTowards(transform.position, exitJumpPoint.position, jumpExitTotDistanceStep);
        if (transform.position == exitJumpPoint.position)
        {
            animator.SetBool(crowpose_anim_parm, true);
            deactivate_sqr = true;
            return;
        }
        transform.position = nextPoint;
    }

    private void fleeBackwardToPoint(Vector3 iGoal)
    {
        animator.SetBool(runback_anim_parm, true);
        navmesh.SetDestination(iGoal);
    }

    private void chill()
    {
        animator.SetBool(run_anim_parm, false);
        animator.SetBool(runback_anim_parm, false);
    }

    private void startDropBomb()
    {
        animator.SetBool(dropbomb_anim_parm, true);
        shouldDropBomb = true;
        delayAnimElapsed = 0f;
        timeOfLastBombDrop = 0f;
    }

    private void dropBomb()
    {
        Vector3 spawn_pos = (bombDropSpot != null) ? bombDropSpot.position : transform.position;
        GameObject bomb = Instantiate(bombRef, spawn_pos, Quaternion.identity);
        Rigidbody bombRB = bomb.GetComponent<Rigidbody>();

        Vector3 shootDir = transform.forward;

        // Vary angle
        float angleVal = Random.Range(-shootAngleVariationDelta, shootAngleVariationDelta);
        shootDir.x = transform.forward.x * Mathf.Cos(angleVal) - transform.forward.z * Mathf.Sin(angleVal);
        shootDir.z = transform.forward.x * Mathf.Sin(angleVal) + transform.forward.z * Mathf.Cos(angleVal);

        // vary force
        float forceVariation = Random.Range(1f, shootingForceVariationDelta);

        // apply
        bombRB.AddForce(shootDir * BombDropForce * forceVariation, ForceMode.VelocityChange);

        shouldDropBomb = false;
        timeOfLastBombDrop = 0f;
        animator.SetBool(dropbomb_anim_parm, false);
    }

    private void kickerUpdate()
    {
        // Player detec for chase
        if (detector.playerInRange)
        {
            if (attackRangeDetector.playerInRange)
            {
                animator.SetBool(run_anim_parm, false);
                animator.SetBool(kick_anim_parm, true);

            }
            else
            {
                navmesh.SetDestination(detector.player.position);
                animator.SetBool(kick_anim_parm, false);
            }
        }

        // Dummy detection for tests
        if (detector.dummyInRange)
        {
            if (attackRangeDetector.dummyInRange)
            {
                animator.SetBool(run_anim_parm, false);
                animator.SetBool(kick_anim_parm, true);

            }
            else
            {
                navmesh.SetDestination(detector.dummy.position);
                animator.SetBool(kick_anim_parm, false);
            }
        }

        // Running
        if (navmesh.remainingDistance <= destination_tolerance)
        {
            if (is_running)
                idle_elapsed_time = 0f;
            is_running = false;

            if (idle_elapsed_time > idle_duration)
            {
                navmesh.SetDestination(RandomNavmeshLocation(walkable_radius));
                is_running = true;
            }
            else
            {
                idle_elapsed_time += Time.deltaTime;
            }

        }
        else
        {
            is_running = true;
        }
        animator.SetBool(run_anim_parm, is_running);
    }

    private Vector3 RandomNavmeshLocation(float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

    public void updateTarget(Vector3 iTarget)
    {
        NavMesh.CalculatePath(transform.position, iTarget, NavMesh.AllAreas, path);
        navmesh.path = path;
    }

}
