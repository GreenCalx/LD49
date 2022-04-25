using System.Collections.Generic; // KeyValuePair
using UnityEngine;

public class CarController : MonoBehaviour
{
    // A car is defined as having 4 springs attached to the body,
    // and 4 wheels attached to the spring.
    // This is defined as an AxleInfo structure, that create everything at runtime.
    // A Car will have two Axle (a front and a rear one), each zith 2 wheels
    //        Wheel
    //       |  /
    //       | /  Width
    //       /
    //     Wheel
    [System.Serializable]
    public struct Wheel
    {
        public float Width;
        public float Radius;
        public float Grip;
        public GameObject Renderer;

        public Vector3 Direction;
        public bool IsGrounded;
    }

    [System.Serializable]
    /// NOTE toffa : This is a spring damper system based on Hooke's law of elasticity.
    /// The inputs are a Frequency(F) and a Damping ratio(Dr). The spring Stiffness(k) and Damping value(Dv) will be deduced from it.
    /// IMPORTANT toffa : We dont take into account mass in our calculations. => m=1
    /// Dr = Dv / 2*sqr(m*k) <=> DV = 2*Dr*sqr(k)
    /// F = sqr(k/m) / 2*pi <=> k = (2*pi*F)²
    /// It could be directly set too if necessary.
    /// Then the spring will always try to get back to the rest position :
    /// Fs = force to apply
    /// L = length
    /// v = velocity
    /// Fs = -k*(Lrest - Lcurrent) - c*v
    ///
    /// Note that (Lrest - Lcurrent) could be seen as the "error" to correct, the formula is then :
    /// Fs = -error*k - c*v
    public struct Spring
    {
        /// Note that MinLength < RestLength < MaxLength
        public float RestLength;
        public float MaxLength;
        public float MinLength;
        /// Frequency <=> Stiffness
        public float Frequency;
        public float Stiffness;
        /// DampRatio <=> DampValue
        public float DampRatio;
        public float DampValue;
        /// Spring is achored to a body at this position
        public GameObject Anchor;
        public float CurrentLength;
    }

    [System.Serializable]
    /// NOTE toffa : A suspension is a Spring that has a Wheel attached at the end \o/
    public struct Suspension
    {
        public Spring Spring;
        public Wheel Wheel;
    }

    Vector3 GetEnd(Suspension S)
    {
        return S.Spring.Anchor.transform.position - transform.up * S.Spring.CurrentLength;
    }

    [System.Serializable]
    /// NOTE toffa : An axle is defined as having two suspension, separated by Width,
    /// and separated from the center of mass of the car by Length.
    /// An axle can be Traction or not, meaning it will apply the motor force or not to its wheels.
    /// An axle can be Direction or not, meaning it will apply the direction or not to its wheels.
    public struct Axle
    {
        public float Width;
        public float Length;
        public Suspension Right;
        public Suspension Left;
        public bool IsTraction;
        public bool IsDirection;
        public bool IsReversedDirection;
    }

    [System.Serializable]
    /// NOTE toffa : A motor has a currentRPM defining its torque
    /// It also has a MaxTorque that it cannot generat more of, and a maxbreak.
    /// The way it works is that breaking is actually applying negative torque.
    /// IMPORTANT toffa : The motor is the breaking system too!!
    public struct Motor
    {
        public float MaxTorque;
        public float MaxBreak;
        public float CurrentRPM;
    }

    /// Everything car related
    [Header("Car")]
    /// A car is for now 2 axles (4 wheels)
    public Motor CarMotor;
    public Axle FrontAxle;
    public Axle RearAxle;
    /// Do we want to set and use a refAxle that will be copied at runtime inside each axle
    /// (same for suspension, all suspensions would be the same)
    public Suspension RefSuspension;
    public GameObject SuspensionRef;
    public Axle RefAxle;
    public bool UseRefs;
    // We redefine the CenterOfMass to be able to change car mecanics
    public GameObject CenterOfMass;
    private Rigidbody RB;
    public float VelocityCorrectionMultiplier;

    public float CurrentAccelerationTime = 0;
    // NOTE toffa : as we are using velocity to quickly correct boundary penetration
    // we need a way to remove this velocity on the next frame or the object will continue to have momentum
    // even after the correction is done.
    // IMPORTANT toffa : it means we are not expecting the physics update to make the correction impossible
    // or we would potentially slam the car into the ground again by substracting the velocity.
    private Vector3 LastVelocityCorrection;
    public int PhysicsSubSteps;

    /// ----------------------------------------------------
    /// BlueCalx for Toffo : put those shit where u want ---
    [Header("Curves")]
    public AnimationCurve TORQUE; // curve itself
    public int torque_movable_keyframe; // the keyframe we want to move in garage w slider
    public AnimationCurve WEIGHT;
    public int weight_movable_keyframe;

    [Header("Debug")]
    public float SpringStiffness;
    public float SpringDamper;
    public float SpringMax;
    public float SpringMultiplier;
    public float SpringMin;
    public float SpringRestPercent;

    public bool SuspensionLock = false;
    public bool FixedUpdateDone = false;
    public bool ApplyForceMultiplier = false;

    public bool IsAircraft = false;
    public bool IsWater = false;
    public float SteeringAngle;

    public float GroundPerturbation;

    public KeyValuePair<AnimationCurve, int> getCurveKVP(UIGarageCurve.CAR_PARAM iParm)
    {
        switch (iParm)
        {
            case UIGarageCurve.CAR_PARAM.UNDEFINED:
                break;
            case UIGarageCurve.CAR_PARAM.TORQUE:
                return new KeyValuePair<AnimationCurve, int>(TORQUE, torque_movable_keyframe);
            case UIGarageCurve.CAR_PARAM.WEIGHT:
                return new KeyValuePair<AnimationCurve, int>(WEIGHT, weight_movable_keyframe);
            default:
                break;
        }
        return new KeyValuePair<AnimationCurve, int>();
    }

    public void setCurve(AnimationCurve iAC, UIGarageCurve.CAR_PARAM iParm)
    {
        switch (iParm)
        {
            case UIGarageCurve.CAR_PARAM.UNDEFINED:
                break;
            case UIGarageCurve.CAR_PARAM.TORQUE:
                TORQUE = iAC;
                break;
            case UIGarageCurve.CAR_PARAM.WEIGHT:
                WEIGHT = iAC;
                break;
            default:
                break;
        }
    }
    /// ----------------------------------------------------
    void UpdateSpring(ref Spring S) {
        S.MaxLength = SpringMax;
        S.MinLength = SpringMin;
        S.DampValue = SpringDamper;
        S.Stiffness = SpringStiffness;
        S.RestLength = S.MinLength + SpringRestPercent * (S.MaxLength-S.MinLength);
    }

    void UpdateSprings() {

        UpdateSpring(ref FrontAxle.Right.Spring);
        UpdateSpring(ref FrontAxle.Left.Spring);
        UpdateSpring(ref RearAxle.Right.Spring);
        UpdateSpring(ref RearAxle.Left.Spring);
    }

    void DrawDebugAxle(Axle A, Color C, Color C2)
    {
        Debug.DrawLine(GetEnd(A.Right), GetEnd(A.Left), C);
        Debug.DrawLine(GetEnd(A.Right), A.Right.Spring.Anchor.transform.position, C2);
        Debug.DrawLine(GetEnd(A.Left), A.Left.Spring.Anchor.transform.position, C2);
    }

    void DrawDebugAxles(Color C, Color C2)
    {
        DrawDebugAxle(FrontAxle, C, C2);
        DrawDebugAxle(RearAxle, C, C2);
    }

    void DrawDebugWheel(ref Suspension S, Color C)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.parent = transform;
        go.transform.position = GetEnd(S);
        var WheelDiameter = S.Wheel.Radius * 2;
        go.transform.localScale = transform.InverseTransformVector(new Vector3(WheelDiameter, WheelDiameter, WheelDiameter));
        go.GetComponent<SphereCollider>().enabled = false;

        S.Wheel.Renderer = go;
    }

    void DrawDebugWheels(Color C)
    {
        DrawDebugWheel(ref FrontAxle.Right, C);
        DrawDebugWheel(ref FrontAxle.Left, C);
        DrawDebugWheel(ref RearAxle.Right, C);
        DrawDebugWheel(ref RearAxle.Left, C);
    }

    void ComputeSuspensionAnchor(ref Axle A)
    {
        RaycastHit Hit;
        var CoM = CenterOfMass.transform.position;
        var SuspensionTemporaryEnd = CoM + A.Length * transform.forward + (A.Width / 2) * transform.right - A.Right.Spring.CurrentLength * transform.up;
        if (Physics.Raycast(SuspensionTemporaryEnd, transform.up, out Hit))
        {
            A.Right.Spring.Anchor = GameObject.Instantiate(SuspensionRef, transform);
            A.Right.Spring.Anchor.transform.position = Hit.point;
        }
        else
        {
            Debug.LogError("Cant compute achor for suspension.");
        }

        SuspensionTemporaryEnd = CoM + A.Length * transform.forward - (A.Width / 2) * transform.right - A.Left.Spring.CurrentLength * transform.up;
        if (Physics.Raycast(SuspensionTemporaryEnd, transform.up, out Hit))
        {
            A.Left.Spring.Anchor = GameObject.Instantiate(SuspensionRef, transform);
            A.Left.Spring.Anchor.transform.position = Hit.point;
        }
        else
        {
            Debug.LogError("Cant compute achor for suspension.");
        }
    }

    void ComputeSuspensionsAnchor()
    {
        ComputeSuspensionAnchor(ref FrontAxle);
        ComputeSuspensionAnchor(ref RearAxle);
    }

    void SpawnSuspensions()
    {
        ComputeSuspensionsAnchor();
        SpawnWheels();
    }

    void SpawnWheels()
    {
#if true
        FrontAxle.Right.Wheel.Renderer = GameObject.Instantiate(FrontAxle.Right.Wheel.Renderer, GetEnd(FrontAxle.Right), FrontAxle.Right.Wheel.Renderer.transform.rotation, transform);
        FrontAxle.Right.Wheel.Renderer.SetActive(true);
        FrontAxle.Left.Wheel.Renderer = GameObject.Instantiate(FrontAxle.Left.Wheel.Renderer, GetEnd(FrontAxle.Left), FrontAxle.Left.Wheel.Renderer.transform.rotation, transform);
        FrontAxle.Left.Wheel.Renderer.SetActive(true);
        RearAxle.Right.Wheel.Renderer = GameObject.Instantiate(RearAxle.Right.Wheel.Renderer, GetEnd(RearAxle.Right), RearAxle.Right.Wheel.Renderer.transform.rotation, transform);
        RearAxle.Right.Wheel.Renderer.SetActive(true);
        RearAxle.Left.Wheel.Renderer = GameObject.Instantiate(RearAxle.Left.Wheel.Renderer, GetEnd(RearAxle.Left), RearAxle.Left.Wheel.Renderer.transform.rotation, transform);
        RearAxle.Left.Wheel.Renderer.SetActive(true);
#endif
    }

    void SpawnAxles()
    {
        SpawnSuspensions();
    }

    void UpdateWheelRenderer(Suspension S)
    {
        S.Wheel.Renderer.transform.position = GetEnd(S);
        S.Wheel.Renderer.transform.localRotation = Quaternion.Euler(0, Quaternion.FromToRotation(transform.forward, S.Wheel.Direction).eulerAngles.y, 90);
    }

    void UpdateWheelsRenderer()
    {
        UpdateWheelRenderer(FrontAxle.Right);
        UpdateWheelRenderer(FrontAxle.Left);
        UpdateWheelRenderer(RearAxle.Right);
        UpdateWheelRenderer(RearAxle.Left);
    }


    // Collision detection and force application
    void ResolveWheel(ref Wheel W)
    {

    }

    Vector3 AddGravityStep(Vector3 A)
    {
        return A + (Physics.gravity * Time.fixedDeltaTime) / 4;
    }

    Vector3 GetWheelVelocity(Vector3 Position)
    {
        return RB.GetPointVelocity(Position) / 4f;
    }

    Vector3 GetNextPointPosition(Vector3 V)
    {
        return V + AddGravityStep(GetWheelVelocity(V) * Time.fixedDeltaTime);
    }

    Vector3 ProjectOnSuspension(Vector3 V)
    {
        return transform.up * Vector3.Dot(transform.up, V);
    }

    Vector3 Mul(Vector3 A, Vector3 B)
    {
        return new Vector3(A.x * B.x, A.y * B.y, A.z * B.z);
    }

    void ResolveSuspension(ref Suspension S)
    {
         var SpringAnchor = S.Spring.Anchor.transform.position;
        // IMPORTANT toffa : the physic step might be too high and miss a collision!
        // Therefore we detect the collision by taking into account the next velocity application in the ray
        // as we are doing the detection by hand.
        // NOTE toffa first we compute the real velocity that will be applied this frame to the point
        // and we project it along the suspension.
        var WheelPosition = GetEnd(S);
        var NextWheelPosition = GetNextPointPosition(WheelPosition);
        var Epsilon = S.Spring.MaxLength + S.Wheel.Radius + ProjectOnSuspension(WheelPosition - NextWheelPosition).magnitude;

        RaycastHit Hit;
        var SpringDirection = -transform.up;
        S.Wheel.IsGrounded = Physics.Raycast(SpringAnchor, SpringDirection, out Hit, Epsilon);
        if (!SuspensionLock)
            S.Spring.CurrentLength = S.Spring.MaxLength;

        // NOTE toffa :
        // We are using su stepping, meaning we are dividing the real deltatime by chunks
        // in order to avoid problem with euler integration over time.
        // This way we should be able to avoid slamming into the ground and jittering.
        if (S.Wheel.IsGrounded || IsAircraft)
        {
            var SubStepDeltaTime = Time.fixedDeltaTime / PhysicsSubSteps;
            var TotalProcessedTime = 0f;
            var TraveledDistance = 0f;
            var ForceToApply = Vector3.zero;
            var InitialSpringVelocity = GetWheelVelocity(SpringAnchor);
            var SpringVelocity = InitialSpringVelocity;

            while (TotalProcessedTime < Time.fixedDeltaTime)
            {
                // Add gravity acceleration to velocity
                SpringVelocity += Physics.gravity * SubStepDeltaTime;
                var SpringVelocityProjected = ProjectOnSuspension(SpringVelocity);

                var StepForce = Vector3.zero;

                // Compute what is the current distance to hit point now
                var StepDistance = Hit.distance + (GroundPerturbation * (Mathf.Sin(SpringAnchor.x * 0.1f)-1)) - TraveledDistance;
                S.Spring.CurrentLength = Mathf.Clamp(StepDistance - S.Wheel.Radius, S.Spring.MinLength, S.Spring.MaxLength);

                S.Wheel.IsGrounded = StepDistance - (S.Spring.CurrentLength + S.Wheel.Radius) <= 0;
                if (S.Wheel.IsGrounded)
                {
                    var SpringLengthError = S.Spring.RestLength - S.Spring.CurrentLength;
                    var SpringForce = -(S.Spring.Stiffness * (ApplyForceMultiplier ? SpringMultiplier : 1) * SpringLengthError) - (S.Spring.DampValue * Vector3.Dot(SpringVelocityProjected, SpringDirection));
                    if (!SuspensionLock)
                        StepForce += SpringForce * SpringDirection;

                    // If suspension is fully compressed then we are hard hitting the ground
                    var IsSpringFullyCompressed = S.Spring.CurrentLength == S.Spring.MinLength;
                    if (IsSpringFullyCompressed)
                    {
                        // NOTE toffa : in this instance we reflect the force as a hard hit on collider
                        var FCollider = Vector3.Reflect(SpringVelocity * VelocityCorrectionMultiplier, Hit.normal);
                        // NOTE toffa : StepForce is actually the diff between what wehave and what we want!
                        // and right now this is a perfect bounce, we can probably compute bouciness factor if needed, according to mass?.
                        // NOTE toffa : We can probably get a sticky effect by removing anny part that is not directly linked to the SpringDirection.
                        StepForce += (FCollider - SpringVelocity);
                    }
                }

                TotalProcessedTime += SubStepDeltaTime;
                var SuspensionForce = ProjectOnSuspension(StepForce + SpringVelocityProjected * SubStepDeltaTime);
                TraveledDistance += SuspensionForce.magnitude * Vector3.Dot(SuspensionForce.normalized, SpringDirection);

                ForceToApply += StepForce;
                SpringVelocity += StepForce;

            }
            RB.AddForceAtPosition(ForceToApply, SpringAnchor, ForceMode.VelocityChange);

            // Direction
            var WheelVelocity = GetWheelVelocity(WheelPosition);
            var ForceDirection = -Vector3.Cross(Hit.normal, Vector3.Cross(Hit.normal, S.Wheel.Direction));
            Debug.DrawLine(Hit.point, Hit.point + ForceDirection.normalized, Color.green);
            var WheelVelocityX = Vector3.Project(WheelVelocity, S.Wheel.Direction);
            var MotorVelocity = CarMotor.CurrentRPM * S.Wheel.Direction;
            var f = Vector3.Cross(transform.up, S.Wheel.Direction);
            var WheelVelocityY = IsWater ? Vector3.zero : Vector3.Project(WheelVelocity, f);
            var T =  -WheelVelocityY + MotorVelocity;
            //T *= Mathf.Pow(Vector3.Dot(transform.up, Vector3.up), 3);
            RB.AddForceAtPosition(T, SpringAnchor, ForceMode.VelocityChange);

            // NOTE toffa : This is a test to apply physics to the ground object if a rigid body existst
            var Collider = Hit.collider?.GetComponent<Rigidbody>();
            if (Collider != null)
            {
                var VelocityGravity = Vector3.Project(GetWheelVelocity(SpringAnchor), Vector3.up);
                VelocityGravity.y += Physics.gravity.y;
                if (VelocityGravity.y < 0)
                    Collider.AddForceAtPosition(VelocityGravity * RB.mass, Hit.point, ForceMode.Force);
            }

        }

    }

    void ResolveAxle(ref Axle A)
    {
        ResolveSuspension(ref A.Right);
        ResolveSuspension(ref A.Left);
    }

    private void SetBodyColor(Color C)
    {
    }

    // TODO toffa : make this better this is a quick fix
    private void OnDestroy()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
    }
    // Start is called before the first frame update
    void Awake()
    {
        if (UseRefs)
        {
            RefAxle.Right = RefSuspension;
            RefAxle.Left = RefSuspension;
            // copy axle in front and left
            FrontAxle = RefAxle;
            RearAxle = RefAxle;
            RearAxle.Length *= -1;
        }

        GetComponent<Rigidbody>().centerOfMass = CenterOfMass.transform.localPosition;
        RB = GetComponent<Rigidbody>();

        SpawnAxles();
        //DrawDebugWheels(Color.yellow);
    }

    void Update()
    {
        if (FixedUpdateDone)
            ApplyForceMultiplier = false;
        FixedUpdateDone = false;
        // TODO toiffa : remove this, it is only for testing the hub
        // note blue : avoid npe if soundmanager is not found atm ( ex : in tracks )
        GameObject sm  = GameObject.Find(Constants.GO_SOUNDMANAGER);
        if (!!sm)
        {
            if (transform.localPosition.z < -300)
            {
                sm.GetComponent<SoundManager>().SwitchClip("desert");
            }
            else
            {
                if (sm.GetComponent<SoundManager>().CurrentClip.Name == "desert")
                    sm.GetComponent<SoundManager>().SwitchClip("theme");
            }
        }

        // end TODO

        DrawDebugAxles(Color.blue, Color.red);
        UpdateWheelsRenderer();

        if (Input.GetKey(KeyCode.J))
        {
            // test jump
            SetSpringSizeMinAndLock();
        } else {
            if (Input.GetKeyUp(KeyCode.J)) {
                ApplyForceMultiplier = true;
            }
            ResetSpringSizeMinAndUnlock();
        }

        float Y = Input.GetAxis("Vertical");
        float X = Input.GetAxis("Horizontal");

        if (IsAircraft) {
            // auto acceleration for now
            // TODO / IMPORTANT toffa : WE REALLY NEED TO FIX THE INPOUTS GOD DAMN IT
            // We need :
            // - button accelerate
            // - button break
            // - up/down for aircraft, might control suspension on car
            // - right/left
            CarMotor.CurrentRPM = CarMotor.MaxTorque;
        } else {
            if (Y != 0) {
                CurrentAccelerationTime += Time.deltaTime;
                CarMotor.CurrentRPM = Y * TORQUE.Evaluate(CurrentAccelerationTime);
            } else {
                CurrentAccelerationTime = 0;
            }
        }

        RearAxle.Right.Wheel.Direction = transform.forward;
        RearAxle.Left.Wheel.Direction = transform.forward;
        if (RearAxle.IsDirection){
            RearAxle.Right.Wheel.Direction = Quaternion.AngleAxis(SteeringAngle * (RearAxle.IsReversedDirection ? X : -X), transform.up) * transform.forward;
            RearAxle.Left.Wheel.Direction = Quaternion.AngleAxis(SteeringAngle * (RearAxle.IsReversedDirection ? X : -X), transform.up) * transform.forward;
        }

        if (!IsAircraft) {
            FrontAxle.Right.Wheel.Direction = transform.forward;
            FrontAxle.Left.Wheel.Direction = transform.forward;
        }
        if (FrontAxle.IsDirection){
            FrontAxle.Right.Wheel.Direction = Quaternion.AngleAxis(SteeringAngle * (FrontAxle.IsReversedDirection ? X : -X), transform.up) * transform.forward;
            FrontAxle.Left.Wheel.Direction = Quaternion.AngleAxis(SteeringAngle * (FrontAxle.IsReversedDirection ? X : -X), transform.up) * transform.forward;
            if (IsAircraft) {
                FrontAxle.Right.Wheel.Direction = Quaternion.AngleAxis(SteeringAngle * (FrontAxle.IsReversedDirection ? Y : -Y), transform.right) * transform.forward;
                FrontAxle.Left.Wheel.Direction = Quaternion.AngleAxis(SteeringAngle * (FrontAxle.IsReversedDirection ? Y : -Y), transform.right) * transform.forward;
            }
        }
    }

    private void ResetSpringSizeMinAndUnlock() {
       SuspensionLock = false;
    }


    private void SetSpringSizeMinAndLock()
    {
        FrontAxle.Right.Spring.CurrentLength = FrontAxle.Right.Spring.MinLength;
        FrontAxle.Left.Spring.CurrentLength = FrontAxle.Left.Spring.MinLength;
        RearAxle.Right.Spring.CurrentLength = RearAxle.Right.Spring.MinLength;
        RearAxle.Left.Spring.CurrentLength = RearAxle.Left.Spring.MinLength;
        SuspensionLock = true;
    }

    public Vector2 MouseLastPosition = Vector2.zero;
    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateSprings();

        ResolveAxle(ref FrontAxle);
        ResolveAxle(ref RearAxle);

        FixedUpdateDone = true;

    }
}
