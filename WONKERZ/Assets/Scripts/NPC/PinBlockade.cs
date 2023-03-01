using UnityEngine;


// Y Angle PID      : Follows target 
// X/Z Angles PID   : Stays up
public class PinBlockade : PIDController
{
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

    public bool autoPowerFromMass = true;
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

    public float ballPowerForceMultiplier = 10f;

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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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
            CarController cc = Access.Player();
            if (!!cc)
                target = cc.gameObject.transform;
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
            if (!iCol.gameObject.GetComponent<Ground>())
            {
                BallPowerObject bpo = iCol.gameObject.GetComponent<BallPowerObject>();
                if (!!bpo)
                {
                    rb.AddForce(bpo.rb.velocity * ballPowerForceMultiplier, ForceMode.VelocityChange);
                    rb.AddTorque(bpo.rb.velocity * ballPowerForceMultiplier);
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
}
