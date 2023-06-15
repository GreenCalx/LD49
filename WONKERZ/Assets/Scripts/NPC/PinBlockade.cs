using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Schnibble;


// Y Angle PID      : Follows target 
// X/Z Angles PID   : Stays up
public class PinBlockade : PIDController
{
    public bool is_NavAgent = false;
    public string navAgentMaskName = "Walkable";

    private Rigidbody rb;
    private bool freshOfCollision = true;
    private float life_lust_elapsed = 0f;

    [SerializeField]
    PID controller;

    [SerializeField]
    PID controllerX;

    [SerializeField]
    PID controllerZ;

    [SerializeField]
    float power;
    [SerializeField]
    Transform target;

    public bool autoPowerFromMass = false;
    public override float Power
    {
        get
        {
            return power;
        }
        set
        {
            power = value;
        }
    }
    public bool life_lust = true;
    public float life_lust_factor = 25;
    public float life_lust_duration = 10f;

    public bool carHitsLikeBallPower = true;
    public float onFirstHitPowerMultiplier = 10f;

    [Header("NavAgent Specs")]
    private UnityEngine.AI.NavMeshAgent agent;
    private bool pinPIDBehaviourIsActive = true;
    public AnimationCurve heightCurve;
    public float jumpSpeed = 1f;
    public float jumpRange = 25f;
    public int n_jumps_keepDirection = 6;

    private int jumpsDoneInOnDirection = 99;
    private Vector3 prevPosition = Vector3.zero;

    private bool jump_done = true;

    public override PID GetController()
    {
        return controller;
    }

    public override void SetTarget(int index)
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        freshOfCollision = true;
        if (autoPowerFromMass)
        {
            power = rb.mass * 5;
        }
        if (is_NavAgent)
        {
            agent = GetComponent<NavMeshAgent>();
            agent.enabled = true;
        }
    }

    void Update()
    {
        if (!freshOfCollision && life_lust)
        {
            life_lust_elapsed += Time.deltaTime;
            if (life_lust_elapsed >= life_lust_duration)
            {
                life_lust = false;
                power = 0f;
            }
        }

        if (is_NavAgent)
        {
            move();
        }
    }

    private void move()
    {
        if (jump_done)
        {
            Vector3 destination = Vector3.zero;

            if (jumpsDoneInOnDirection > n_jumps_keepDirection)
            {
                // Poll a new position

                destination = AINavigation.GoInRandomDirection(transform, jumpRange, navAgentMaskName);
                
                if (destination==Vector3.zero)
                {   // Had no destination + no destination available
                    is_NavAgent = false;
                    return;
                }

                prevPosition = transform.position;
                jumpsDoneInOnDirection = 1;
                jump_done = false;
            }
            else {
                // Follow same direction
                destination = AINavigation.GetNextMoveInDirection(transform, transform.position - prevPosition, jumpRange, navAgentMaskName);
                
                if (destination==Vector3.zero)
                {   // Had no destination + no destination available
                    is_NavAgent = false;
                    return;
                }
                prevPosition = transform.position;
                jumpsDoneInOnDirection++;
                jump_done = false;
            }
            StopAllCoroutines();
            StartCoroutine(Jump(this,destination));
        }
    }

    private IEnumerator Jump(PinBlockade iJumper, Vector3 iTarget)
    {
        iJumper.agent.enabled = false;
        pinPIDBehaviourIsActive = false;

        //
        Vector3 jumpStart = iJumper.transform.position;
        // animation here

        for (float time = 0f; time < 1f; time += Time.deltaTime * iJumper.jumpSpeed)
        {
            iJumper.transform.position  = Vector3.Lerp( jumpStart, iTarget, time) 
                                            + Vector3.up * heightCurve.Evaluate(time);
            iJumper.transform.rotation = Quaternion.Slerp(iJumper.transform.rotation, Quaternion.LookRotation(iTarget - iJumper.transform.position), time);

            yield return null;
        }

        pinPIDBehaviourIsActive = true;
        iJumper.agent.enabled = true;
        jump_done = true;
        if (NavMesh.SamplePosition(iJumper.transform.position, out NavMeshHit hit, 1f, iJumper.agent.areaMask))
        {
            //iJumper.agent.Warp(hit.position);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!pinPIDBehaviourIsActive)
            return;

        // PID X/Z : Stay up
        var targetUpDir = Vector3.up;
        var upDir = rb.rotation * Vector3.up;
        Debug.DrawRay(transform.position, upDir, Color.green);

        float currentAngleX = Vector3.SignedAngle(Vector3.up, upDir, Vector3.right);
        float currentAngleZ = Vector3.SignedAngle(Vector3.up, upDir, Vector3.forward);

        var targetAngleX = Vector3.SignedAngle(Vector3.up, Vector3.up, Vector3.forward);// continue here
        var targetAngleZ = Vector3.SignedAngle(Vector3.up, Vector3.up, Vector3.right);// continue here

        float xinput = controllerX.UpdateAngle(Time.fixedDeltaTime, currentAngleX, targetAngleX);

        float zinput = controllerZ.UpdateAngle(Time.fixedDeltaTime, currentAngleZ, targetAngleZ);

        // Apply PID torque
        rb.AddTorque(new Vector3(xinput * power, 0, 0));
        rb.AddTorque(new Vector3(0, 0, zinput * power));

        // PID Y : Follow target
        if (target == null)
        {
            PlayerController player = Access.Player();
            if (!!player)
                target = player.transform;
            return;
        }

        var targetPosition = target.position;
        targetPosition.y = rb.position.y;    //ignore difference in Y
        var targetDir = (targetPosition - rb.position).normalized;
        var forwardDir = rb.rotation * Vector3.forward;

        var currentAngle = Vector3.SignedAngle(Vector3.forward, forwardDir, Vector3.up);
        var targetAngle = Vector3.SignedAngle(Vector3.forward, targetDir, Vector3.up);

        float yinput = controller.UpdateAngle(Time.fixedDeltaTime, currentAngle, targetAngle);
        // Apply PID torque
        rb.AddTorque(new Vector3(0, yinput * power, 0));

    }

    void OnCollisionEnter(Collision iCol)
    {
        if (freshOfCollision)
        {
            // Sandworm uc
            if (!!iCol.gameObject.GetComponentInParent<SandWorm>())
            {
                StopAllCoroutines();

                is_NavAgent = false;
                Destroy(agent);
                rb.constraints = RigidbodyConstraints.None;
                pinPIDBehaviourIsActive = false;
                freshOfCollision = false;
                return;
            }


            
            // If player or ball add impact force/torque
            BallPowerObject bpo = iCol.gameObject.GetComponent<BallPowerObject>();
            PlayerController pc = iCol.gameObject.GetComponent<PlayerController>();
            if (!!bpo || !!pc)
            {
                StopAllCoroutines();

                // disable agent, enable PID
                if (is_NavAgent)
                {
                    is_NavAgent = false;
                    agent.enabled = false;
                    pinPIDBehaviourIsActive = true;
                }

                if (!!bpo)
                {
                    rb.AddForce(bpo.rb.velocity * onFirstHitPowerMultiplier, ForceMode.VelocityChange);
                    rb.AddTorque(bpo.rb.velocity * onFirstHitPowerMultiplier, ForceMode.VelocityChange);
                    life_lust = false;
                } else if (carHitsLikeBallPower && !!pc)
                {
                    rb.AddForce(pc.rb.velocity * onFirstHitPowerMultiplier, ForceMode.VelocityChange);
                    rb.AddTorque(pc.rb.velocity * onFirstHitPowerMultiplier, ForceMode.VelocityChange);
                    life_lust = false;
                }
                if (life_lust)
                {
                    power *= life_lust_factor;
                    life_lust_elapsed = 0f;
                }
                rb.constraints = RigidbodyConstraints.None;
                freshOfCollision = false;
            }
        }
    }

    public void kill()
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

}

