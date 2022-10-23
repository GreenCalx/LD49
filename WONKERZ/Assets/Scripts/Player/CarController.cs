using System;
using UnityEngine;
public class CarController : MonoBehaviour, IControllable
{

    /// ============ Structs =================
    //
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
        public GameObject Trails;
        public Vector3 Direction;
        public bool IsGrounded;
        public Vector3 Velocity;
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
        public GameObject Renderer;
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
    /// and separated from the center of mass of the car by Length
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

    /// ================== Variables ===================
    public GameObject SpeedParticles;
    [Header("Prefab Refs(MAND)")]
    public GameObject OnDeathClone;

    [Header("Car PhysX")]
    /// A car is for now 2 axles (4 wheels)
    public Motor CarMotor;
    public Axle FrontAxle;
    public Axle RearAxle;
    // We redefine the CenterOfMass to be able to change car mecanics
    public GameObject CenterOfMass;

    [Header("Car Refs For Creation")]
    /// Do we want to set and use a refAxle that will be copied at runtime inside each axle
    /// (same for suspension, all suspensions would be the same)
    public Suspension RefSuspension;
    public GameObject SuspensionRef;
    public Axle RefAxle;
    public bool UseRefs;
    public float VelocityCorrectionMultiplier;

    public float CurrentAccelerationTime = 0;

    [Header("Curves")]
    public AnimationCurve TORQUE; // curve itself
    public int torque_movable_keyframe; // the keyframe we want to move in garage w slider
    public AnimationCurve WEIGHT;
    public int weight_movable_keyframe;

    [Header("Turbo")]
    public float turboStrength = 5f;
    public float turboTimeInterval = 0.2f;
    private float turboIntervalElapsedTime = 99f;
    public float turboConsumptionPerTick = 0.02f; // turbo tank Ranges from 0f to 1f

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
    public float SteeringAngle;
    // NOTE toffa : as we are using velocity to quickly correct boundary penetration
    // we need a way to remove this velocity on the next frame or the object will continue to have momentum
    // even after the correction is done.
    // IMPORTANT toffa : it means we are not expecting the physics update to make the correction impossible
    // or we would potentially slam the car into the ground again by substracting the velocity.
    public int PhysicsSubSteps;

    public UIGraphView debugTorque;

    [Header("Mecanics")]
    public bool IsHooked;
    // TODO toffa : remove this hardcoded object
    public GameObject grapin;

    public PlayerFSM stateMachine = new PlayerFSM();

    public FSMState aliveState = new FSMState();
    public FSMState deadState = new FSMState();
    public FSMState frozenState = new FSMState();
    public FSMState invulState = new FSMState();
    public PlayerBoatState boatState = new PlayerBoatState();
    public PlayerGroudState groundState = new PlayerGroudState();
    public PlayerAircraftState aircraftState = new PlayerAircraftState();

    /// =============== Cache ===============
    public Rigidbody RB;

    /// =================== Updates =================

    void UpdateSpring(ref Spring S)
    {
        S.MaxLength = SpringMax;
        S.MinLength = SpringMin;
        S.DampValue = SpringDamper;
        S.Stiffness = SpringStiffness;
        S.RestLength = S.MinLength + SpringRestPercent * (S.MaxLength - S.MinLength);
    }

    void UpdateSprings()
    {

        UpdateSpring(ref FrontAxle.Right.Spring);
        UpdateSpring(ref FrontAxle.Left.Spring);
        UpdateSpring(ref RearAxle.Right.Spring);
        UpdateSpring(ref RearAxle.Left.Spring);
    }

    void UpdateSuspensionRenderer(ref Spring S)
    {
        S.Renderer.transform.position = S.Anchor.transform.position; // - transform.up * S.CurrentLength / 2;
        var scale = S.Renderer.transform.localScale;
        S.Renderer.transform.localScale = new Vector3(scale.x, scale.y, S.CurrentLength / 20);
    }

    void UpdateSuspensionRenderers()
    {
        UpdateSuspensionRenderer(ref FrontAxle.Right.Spring);
        UpdateSuspensionRenderer(ref FrontAxle.Left.Spring);
        UpdateSuspensionRenderer(ref RearAxle.Right.Spring);
        UpdateSuspensionRenderer(ref RearAxle.Left.Spring);
    }

    void UpdateWheelRenderer(Suspension S, bool inverseRot)
    {
        S.Wheel.Renderer.transform.position = GetEnd(S);
        S.Wheel.Renderer.transform.localRotation *= Quaternion.Euler(0, 0, -CarMotor.CurrentRPM * 1 * (inverseRot ? 1 : -1));
#if false
        var angle = Quaternion.FromToRotation(S.Wheel.Renderer.transform.InverseTransformDirection( S.Wheel.Renderer.transform.forward) ,S.Wheel.Renderer.transform.InverseTransformDirection(S.Wheel.Direction)).eulerAngles.y;
        if(angle > 180f) angle -= 180f;
        if(angle < -180f) angle += 360f;
        var angles = S.Wheel.Renderer.transform.localRotation.eulerAngles;
        angles.y += angle;
        S.Wheel.Renderer.transform.localRotation = Quaternion.Euler(angles);
#endif
        S.Wheel.Renderer.transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(-90 * (inverseRot ? 1 : -1), transform.up) * S.Wheel.Direction, S.Wheel.Renderer.transform.up);

    }

    void UpdateWheelsRenderer()
    {
        UpdateWheelRenderer(FrontAxle.Right, false);
        UpdateWheelRenderer(FrontAxle.Left, true);
        UpdateWheelRenderer(RearAxle.Right, false);
        UpdateWheelRenderer(RearAxle.Left, true);
    }

    /// ================== Draw =================

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

    void DebugDrawCircle(float Radius, Vector3 Center, Vector3 Up, int LineCount, Color C)
    {
        float AngleStep = Mathf.PI * 2 / LineCount;
        for (int i = 0; i < LineCount; ++i)
        {
            Vector3 start = Center + (Quaternion.FromToRotation(Vector3.up, Up) * new Vector3(Mathf.Sin(AngleStep * i), 0, Mathf.Cos(AngleStep * i))) * Radius;
            Vector3 end = Center + (Quaternion.FromToRotation(Vector3.up, Up) * new Vector3(Mathf.Sin(AngleStep * (i + 1)), 0, Mathf.Cos(AngleStep * (i + 1)))) * Radius;
            Debug.DrawLine(start, end, C);
        }
    }

    void DrawDebugWheel(ref Suspension S, Color C)
    {
        DebugDrawCircle(S.Wheel.Radius, GetEnd(S), transform.right, 10, S.Wheel.IsGrounded ? Color.magenta : C);
        DebugDrawCircle(S.Wheel.Radius, GetEnd(S) + transform.right * S.Wheel.Width, transform.right, 10, S.Wheel.IsGrounded ? Color.magenta : C);
        DebugDrawCircle(S.Wheel.Radius, GetEnd(S) - transform.right * S.Wheel.Width, transform.right, 10, S.Wheel.IsGrounded ? Color.magenta : C);

        //var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //go.transform.parent = transform;
        //go.transform.position = GetEnd(S);
        //var WheelDiameter = S.Wheel.Radius * 2;
        //go.transform.localScale = transform.InverseTransformVector(new Vector3(WheelDiameter, WheelDiameter, WheelDiameter));
        //go.GetComponent<SphereCollider>().enabled = false;

        //S.Wheel.Renderer = go;
    }

    void DrawDebugWheels(Color C)
    {
        DrawDebugWheel(ref FrontAxle.Right, C);
        DrawDebugWheel(ref FrontAxle.Left, C);
        DrawDebugWheel(ref RearAxle.Right, C);
        DrawDebugWheel(ref RearAxle.Left, C);
    }

    /// =================== Spawn ==================

    void ComputeSuspensionAnchor(ref Axle A)
    {
        var BoxColliders = GetComponents<BoxCollider>();
        BoxCollider BottomCollider = null;
        foreach (BoxCollider C in BoxColliders)
        {
            if (BottomCollider == null || C.center.y < BottomCollider.center.y)
            {
                BottomCollider = C;
            }
        }

        var CarFrameYPlan = BottomCollider.bounds.center.y - BottomCollider.bounds.extents.y;
        var SuspensionTemporaryEnd = BottomCollider.bounds.center + A.Length * transform.forward + (A.Width / 2) * transform.right - A.Right.Spring.CurrentLength * transform.up;
        A.Right.Spring.Anchor = GameObject.Instantiate(SuspensionRef, transform);
        A.Right.Spring.Anchor.transform.position = new Vector3(SuspensionTemporaryEnd.x, CarFrameYPlan, SuspensionTemporaryEnd.z);
        SuspensionTemporaryEnd = BottomCollider.bounds.center + A.Length * transform.forward - (A.Width / 2) * transform.right - A.Left.Spring.CurrentLength * transform.up;
        A.Left.Spring.Anchor = GameObject.Instantiate(SuspensionRef, transform);
        A.Left.Spring.Anchor.transform.position = new Vector3(SuspensionTemporaryEnd.x, CarFrameYPlan, SuspensionTemporaryEnd.z);

#if false
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
#endif
    }

    void ComputeSuspensionsAnchor()
    {
        ComputeSuspensionAnchor(ref FrontAxle);
        ComputeSuspensionAnchor(ref RearAxle);
    }

    void SpawnSuspensionRenderers()
    {
        SpawnSuspensionRenderer(ref FrontAxle.Right.Spring);
        SpawnSuspensionRenderer(ref FrontAxle.Left.Spring);
        SpawnSuspensionRenderer(ref RearAxle.Right.Spring);
        SpawnSuspensionRenderer(ref RearAxle.Left.Spring);
    }

    void SpawnSuspensionRenderer(ref Spring S)
    {
        //S.Renderer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        //S.Renderer.transform.parent = gameObject.transform;
        //S.Renderer.GetComponent<CapsuleCollider>().enabled = false;

        S.Renderer = Instantiate(S.Renderer);
        var rb = S.Renderer.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.detectCollisions = false;
        GetComponent<DeathController>().objects.Add(rb);
        S.Renderer.transform.parent = gameObject.transform;
        S.Renderer.SetActive(true);
    }

    void SpawnSuspensions()
    {
        ComputeSuspensionsAnchor();
        SpawnWheels();

        SpawnSuspensionRenderers();

        UpdateSuspensionRenderers();
    }

    void SpawnWheel(ref Wheel W, Vector3 pos, float rot)
    {
        W.Renderer = GameObject.Instantiate(W.Renderer, pos, Quaternion.Euler(0, rot, 0), transform);


        var rb = W.Renderer.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.detectCollisions = false;
        GetComponent<DeathController>().objects.Add(rb);
        W.Renderer.SetActive(true);
        W.Renderer.transform.localScale = new Vector3(W.Radius, W.Radius, W.Width * 2);
        W.Trails = GameObject.Instantiate(W.Trails, pos, W.Trails.transform.rotation, transform);
    }

    void SpawnWheels()
    {
        SpawnWheel(ref FrontAxle.Right.Wheel, GetEnd(FrontAxle.Right), 90);
        SpawnWheel(ref FrontAxle.Left.Wheel, GetEnd(FrontAxle.Left), -90);
        SpawnWheel(ref RearAxle.Right.Wheel, GetEnd(RearAxle.Right), 90);
        SpawnWheel(ref RearAxle.Left.Wheel, GetEnd(RearAxle.Left), -90);
    }

    void SpawnAxles()
    {
        SpawnSuspensions();
    }

    /// =============== Resolve =============

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

    private bool NeedCheckMaterial = true;

    struct WheelHitInfo
    {
        public Ground.GroundInfos GroundI;
        public float Distance;
        public Vector3 Normal;
        public bool Hit;
        public Vector3 Point;
        public Vector3 PenetrationCorrectionDirection;
        public float PenetrationCorrectionDistance;
        public GameObject Collider;
    };

    WheelHitInfo GetWheelHitInfo(Suspension S)
    {

        WheelHitInfo Result = new WheelHitInfo();
        Result.Distance = float.PositiveInfinity;
        Result.Hit = false;
        Result.Collider = null;
        Result.Normal = Vector3.zero;
        Result.PenetrationCorrectionDirection = Vector3.zero;
        Result.PenetrationCorrectionDistance = 0f;
        Result.Point = Vector3.zero;

        var SpringDirection = -transform.up;
        var SpringAnchor = S.Spring.Anchor.transform.position;
        var WheelPosition = GetEnd(S);
        var WheelDiameter = S.Wheel.Radius * 2f;
        var NextWheelPosition = GetNextPointPosition(WheelPosition);
        // overlap a capsule cast with a box cast to make a wheel colider

        float MaxDistance = S.Spring.MaxLength + ProjectOnSuspension(WheelPosition - NextWheelPosition).magnitude;

        var TestSweep = S.Spring.Anchor.GetComponentInChildren<Rigidbody>().SweepTestAll(SpringDirection, MaxDistance);
        Array.Sort(TestSweep, (a, b) => a.distance < b.distance ? -1 : 1);

        Result.Hit = TestSweep.Length != 0;
        if (Result.Hit)
        {
            Result.Collider = TestSweep[0].collider.gameObject;
            Result.Distance = TestSweep[0].distance;
            Result.Normal = TestSweep[0].normal;
            Result.Point = TestSweep[0].point;

            if (Result.Distance < S.Spring.MinLength)
            {

                Result.Distance = 0;
                if (Physics.ComputePenetration(S.Spring.Anchor.GetComponentInChildren<MeshCollider>(),
                                              SpringAnchor + SpringDirection * S.Spring.MinLength,
                                              Quaternion.LookRotation(S.Wheel.Direction, -SpringDirection),
                                              TestSweep[0].collider,
                                              Result.Collider.transform.position,
                                              Result.Collider.transform.rotation,
                                              out Result.PenetrationCorrectionDirection, out Result.PenetrationCorrectionDistance))
                {
                    //Result.Normal = (Result.PenetrationCorrectionDirection * Result.PenetrationCorrectionDistance).normalized;
                    Result.Point = Vector3.zero;
                }
                else
                {
                    Debug.Log("dist < min but no penetration?");
                }
            }
        }
        else
        {
            // check for overlap?
            var overlap = Physics.OverlapBox(WheelPosition, new Vector3(S.Wheel.Width, S.Wheel.Radius, S.Wheel.Radius), Quaternion.LookRotation(S.Wheel.Direction, -SpringDirection), ~(1 << LayerMask.NameToLayer("No Player Collision")));
            if (overlap.Length != 0)
            {
                Result.Distance = 0;
                Result.Hit = true;
                // IMPORTANT : Physx treat mesh collider as hollow surface : if center of overlap is under, triangle will be culled and
                // no penetration will be found........
                // if (Physics.ComputePenetration(S.Spring.Anchor.GetComponentInChildren<MeshCollider>(),
                //                               WheelPosition,
                //                               Quaternion.LookRotation(S.Wheel.Direction, -SpringDirection),
                //                               overlap[0],
                //                               overlap[0].transform.position,
                //                               overlap[0].transform.rotation,
                //                               out Result.PenetrationCorrectionDirection, out Result.PenetrationCorrectionDistance))
                BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                bc.center = transform.InverseTransformPoint(WheelPosition);
                bc.size = new Vector3(S.Wheel.Width * 2, S.Wheel.Radius * 2, S.Wheel.Radius * 2);
                if (Physics.ComputePenetration(bc, WheelPosition, Quaternion.LookRotation(S.Wheel.Direction, -SpringDirection),
                                               overlap[0], overlap[0].transform.position, overlap[0].transform.rotation,
                                               out Result.PenetrationCorrectionDirection, out Result.PenetrationCorrectionDistance))
                {
                    //Result.Normal = (Result.PenetrationCorrectionDirection * Result.PenetrationCorrectionDistance).normalized;
                    Result.Point = Vector3.zero;
                    Debug.Log("overlap and penetration");
                }
                else
                {
                    Debug.Log("overlap but no penetration?" + "    " + overlap.Length);
                }
                GameObject.DestroyImmediate(bc);

            }
        }

        if (Result.Collider && Result.Collider.GetComponent<Ground>())
        {
            Result.GroundI = Result.Collider.GetComponent<Ground>().GetGroundInfos(Result.Point);
        }
        else
        {
            Result.GroundI = new Ground.GroundInfos();
        }
        return Result;

    }
    [Range(1f, 10f)]
    public float Bounciness = 1;
    Vector3 ComputeSuspensionForce(ref Suspension S, WheelHitInfo HitInfo)
    {
        var SpringAnchor = S.Spring.Anchor.transform.position;
        var SpringDirection = -transform.up;

        var SubStepDeltaTime = Time.fixedDeltaTime / PhysicsSubSteps;
        var TotalProcessedTime = 0f;
        var TraveledDistance = 0f;
        var ForceToApply = Vector3.zero;
        var InitialSpringVelocity = GetWheelVelocity(SpringAnchor);
        var SpringVelocity = InitialSpringVelocity;

        //Debug.Log(HitInfo.Hit);
        // NOTE toffa :
        // We are using su stepping, meaning we are dividing the real deltatime by chunks
        // in order to avoid problem with euler integration over time.
        // This way we should be able to avoid slamming into the ground and jittering.
        while (TotalProcessedTime < Time.fixedDeltaTime)
        {
            // Add gravity acceleration to velocity
            SpringVelocity += Physics.gravity * SubStepDeltaTime;
            var SpringVelocityProjected = ProjectOnSuspension(SpringVelocity);

            var StepForce = Vector3.zero;

            // Compute what is the current distance to hit point now
            var StepDistance = HitInfo.Distance + HitInfo.GroundI.DepthPerturbation.y - TraveledDistance;
            if (!SuspensionLock)
                S.Spring.CurrentLength = Mathf.Clamp(StepDistance, S.Spring.MinLength, S.Spring.MaxLength);

            S.Wheel.IsGrounded = StepDistance < S.Spring.MaxLength;
            //Debug.Log(S.Wheel.IsGrounded);
            if (S.Wheel.IsGrounded)
            {
                var SpringLengthError = S.Spring.RestLength - S.Spring.CurrentLength;
                var SpringForce = -(S.Spring.Stiffness * (ApplyForceMultiplier ? SpringMultiplier : 1) * SpringLengthError) - (S.Spring.DampValue * Vector3.Dot(SpringVelocityProjected, SpringDirection));
                if (!SuspensionLock)
                    StepForce += SpringForce * SpringDirection;
            }

            var SuspensionForce = ProjectOnSuspension((StepForce + SpringVelocityProjected)) * SubStepDeltaTime;
            TraveledDistance += SuspensionForce.magnitude * Vector3.Dot(SuspensionForce.normalized, SpringDirection);

            ForceToApply += StepForce;
            SpringVelocity += StepForce;

            TotalProcessedTime += SubStepDeltaTime;
        }

        var ValidationValue = Utils.Math.ValidateForce(ForceToApply);
        if (!ValidationValue.Item2) Debug.Log("FAILED TO VALIDATE FORCE");

        return ValidationValue.Item1;
    }

    Vector3 ComputeWheelForce(Wheel W, WheelHitInfo HitInfo)
    {
        var WheelVelocity = W.Velocity;
        // Compute according to the ground normal where the forward direction would be
        // it means the ground normal act as if it was the up vector of the car.

        // NOTE toffa : we forst do everything as if we were touching the ground from above at 90 angle.
        var WheelForward = Quaternion.FromToRotation(transform.up, HitInfo.Normal) * W.Direction;
        var ForceDirection = -Vector3.Cross(HitInfo.Normal, Vector3.Cross(HitInfo.Normal, WheelForward));
        var WheelVelocityX = Vector3.Project(WheelVelocity, WheelForward);
        var MotorVelocity = CarMotor.CurrentRPM * WheelForward;
        var f = Vector3.Cross(transform.up, W.Direction);
        var WheelVelocityY = Vector3.Project(WheelVelocity, f);

        // NOTE toffa : dunno why but sometimes gives a runtime error
        // possibly a unity bug
        // just in case, even if i checked, we clamp
        var v = WheelVelocityY.magnitude / WheelVelocity.magnitude;
        var GripY = WEIGHT.Evaluate(Mathf.Clamp01(v));
        var Speed = Vector3.Dot(transform.forward, RB.velocity);
        var MaxSpeed = 100f;
        var RollingResistance = .02f;

        var torque = Mathf.Clamp01(Mathf.Abs(Speed) / MaxSpeed);
        var GripX = TORQUE.Evaluate(torque);

        debugTorque.currentValue = torque;


        var Force = -WheelVelocityY * GripY;
        Force += WheelForward * GripX * CarMotor.CurrentRPM;
        // rolling force is countering current movement
        if (WheelVelocityX.magnitude >= 0.02f)
            Force -= WheelVelocityX * RollingResistance;

        // var T = -WheelVelocityX * HitInfo.GroundI.Friction.x;
        // T -= WheelVelocityY * HitInfo.GroundI.Friction.y;
        // T += MotorVelocity;
        // T *= Mathf.Pow(Mathf.Clamp01(Vector3.Dot(HitInfo.Normal, Vector3.up)), 3);

        var ValidationValue = Utils.Math.ValidateForce(Force);
        if (!ValidationValue.Item2)
        {
            Debug.Log("FAILED TO VALIDATE FORCE");
        }
        return ValidationValue.Item1;
    }

    public void ApplyForceAtPosition(Rigidbody Body, Vector3 Force, Vector3 Position)
    {
        if (Force.sqrMagnitude < 1e-06) return;

        Vector3 distance = Position - Body.worldCenterOfMass;
        Vector3 desired_vec = (Position + Force) - Body.worldCenterOfMass;
        Vector3 torque = Vector3.Cross(Force, distance);
        float angle = Vector3.SignedAngle(distance, desired_vec, torque);
        Body.MoveRotation(Body.rotation * Quaternion.AngleAxis(angle, torque));
    }

    void ApplyConstraints(ref Suspension S, WheelHitInfo HitInfo)
    {
        var WheelVel = AddGravityStep(GetWheelVelocity(GetEnd(S)));
        // If we are already going away from the ground, we do not need to push it ...
        bool NeedReflection = Vector3.Dot(WheelVel.normalized, HitInfo.Normal) < 0;
        // Negate totally force coming directly into the normal of the ground : we really CANT go further in this direction.
        var FNormal = NeedReflection ? -Vector3.Project(WheelVel, HitInfo.Normal) : Vector3.zero;
        //var FCollider = Vector3.Dot(WheelVel.normalized, HitInfo.Normal) < 0 ? Vector3.Reflect(WheelVel, HitInfo.Normal) : Vector3.zero;
        var FCollider = FNormal;

        if (HitInfo.Distance == 0)
        {
            ApplyForceAtPosition(RB, HitInfo.PenetrationCorrectionDistance * HitInfo.PenetrationCorrectionDirection, GetEnd(S));
            RB.AddForceAtPosition(FCollider * Bounciness, GetEnd(S), ForceMode.VelocityChange);
        }
    }

    void ApplyGroundForces(Suspension S, WheelHitInfo HitInfo)
    {

        // NOTE toffa : This is a test to apply physics to the ground object if a rigid body existst
        var SpringAnchor = S.Spring.Anchor.transform.position;
        var Collider = HitInfo.Collider?.GetComponent<Rigidbody>();
        if (Collider != null)
        {
            var VelocityGravity = Vector3.Project(GetWheelVelocity(SpringAnchor), Vector3.up);
            VelocityGravity.y += Physics.gravity.y;
            if (VelocityGravity.y < 0)
                Collider.AddForceAtPosition(VelocityGravity * RB.mass, HitInfo.Point, ForceMode.Force);
        }
        // NOTE toffa :
        // Add current ground velocity to the RB to be able to sit still on moving plateform for instance.
        var RBVelProjected = Vector3.Project(RB.velocity, HitInfo.GroundI.Velocity);
        var VelDiff = Vector3.Scale((HitInfo.GroundI.Velocity - RBVelProjected), new Vector3(1, 0, 1));
        var ValidationValue = Utils.Math.ValidateForce(VelDiff);
        if (!ValidationValue.Item2)
        {
            Debug.Log("FAILED TO VALIDATE FORCE");
        }
        RB.AddForce(ValidationValue.Item1 / 4, ForceMode.VelocityChange);

    }

    void ResolveSuspension(ref Suspension S)
    {
        var HitInfo = GetWheelHitInfo(S);
        S.Wheel.IsGrounded = HitInfo.Hit;

        var SpringAnchor = S.Spring.Anchor.transform.position;
        var SpringDirection = -transform.up;

        if (!SuspensionLock)
            S.Spring.CurrentLength = S.Spring.MaxLength;

        if (S.Wheel.IsGrounded || IsAircraft)
        {
            SetModeFromGround(HitInfo.GroundI.Type);

            ApplyConstraints(ref S, HitInfo);

            var F = ComputeSuspensionForce(ref S, HitInfo);
            RB.AddForceAtPosition(F, GetEnd(S), ForceMode.VelocityChange);
            //Debug.Log("ResolveSuspension : " + F);

            S.Wheel.Velocity = GetWheelVelocity(GetEnd(S));
            var WF = ComputeWheelForce(S.Wheel, HitInfo);
            //Debug.Log("Wheel Force : " + WF);
            if (IsAircraft)
            {
                RB.AddForceAtPosition(WF, SpringAnchor + Vector3.Project(CenterOfMass.transform.position - SpringAnchor, transform.up), ForceMode.VelocityChange);
            }
            else
            {
                if (HitInfo.GroundI.Type == Ground.EType.DESERT)
                    S.Wheel.Trails.GetComponent<ParticleSystem>().Play();

                RB.AddForceAtPosition(WF, GetEnd(S), ForceMode.VelocityChange);
            }

            ApplyGroundForces(S, HitInfo);
        }
        else
        {
            S.Wheel.Trails.GetComponent<ParticleSystem>().Stop();
        }
    }

    void ResolveAxle(ref Axle A)
    {
        NeedCheckMaterial = true;
        ResolveSuspension(ref A.Right);
        Physics.SyncTransforms();
        ResolveSuspension(ref A.Left);
        Physics.SyncTransforms();
    }

    void ResolveWheel(ref Wheel W)
    {

    }

    /// =================== Mecanics ===================

    private Ground.EType CurrentGround = Ground.EType.NONE;

    public enum CarMode
    {
        GROUND,
        WATER,
        DELTA,
        NONE
    };

    public void SetModeFromGround(Ground.EType Mode)
    {
        if (Mode == Ground.EType.NONE) return;
        SetMode(CarMode.GROUND);
        if (Mode == Ground.EType.WATER) SetMode(CarMode.WATER);
    }

    public class PlayerGroudState : FSMState
    {
        public CarController player;
        public override void OnEnter(FSMBase machine)
        {
            base.OnEnter(machine);
            player.FrontAxle.IsTraction = false;
            player.FrontAxle.IsDirection = true;
            player.FrontAxle.IsReversedDirection = false;

            player.RearAxle.IsDirection = false;
            player.RearAxle.IsTraction = true;
            player.RearAxle.IsReversedDirection = true;
        }
    }

    public class PlayerBoatState : FSMState
    {
        public CarController player;
        public override void OnEnter(FSMBase machine)
        {
            base.OnEnter(machine);
            player.FrontAxle.IsTraction = false;
            player.FrontAxle.IsDirection = false;
            player.FrontAxle.IsReversedDirection = false;

            player.RearAxle.IsDirection = true;
            player.RearAxle.IsTraction = true;
            player.RearAxle.IsReversedDirection = true;
        }
    }

    public class PlayerAircraftState : FSMState
    {
        public CarController player;
        public override void OnEnter(FSMBase machine)
        {
            base.OnEnter(machine);
            player.FrontAxle.IsTraction = true;
            player.FrontAxle.IsDirection = true;
            player.FrontAxle.IsReversedDirection = false;

            player.RearAxle.IsDirection = true;
            player.RearAxle.IsTraction = true;
            player.RearAxle.IsReversedDirection = true;
        }
    }

    public class PlayerAircraftCondition : FSMCondition
    {
        public CarController player;
        public override bool Check(FSMBase machine)
        {
            return player.CurrentMode == CarMode.DELTA;
        }
    }

    public class PlayerCarCondition : FSMCondition
    {
        public CarController player;
        public override bool Check(FSMBase machine)
        {
            return player.CurrentMode == CarMode.GROUND || player.CurrentMode == CarMode.NONE;
        }
    }

    public class PlayerBoatCondition : FSMCondition
    {
        public CarController player;
        public override bool Check(FSMBase machine)
        {
            return player.CurrentMode == CarMode.WATER;
        }
    }

    public CarMode CurrentMode = CarMode.NONE;
    public void SetMode(CarMode Mode)
    {
        if (Mode == CurrentMode) return;
        CurrentMode = Mode;
    }

    private void SetBodyColor(Color C)
    {
    }

    private void ResetSpringSizeMinAndUnlock()
    {
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

    private void clonedForTestInit()
    {
        RB.centerOfMass = CenterOfMass.transform.localPosition;
    }

    public Vector2 MouseLastPosition = Vector2.zero;
    // Update is called once per frame
    public float TorqueForce;

    public float currentSpeed = 0f;
    private float updateCurrentSpeed()
    {
        if (!RB)
            return 0f;
        var PlayerVelocity = RB.velocity;
        var PlayerForward = RB.transform.forward;
        currentSpeed = new Vector3(PlayerVelocity.x * PlayerForward.x, PlayerVelocity.y * PlayerForward.y, PlayerVelocity.z * PlayerForward.z).magnitude;
        return currentSpeed;
    }

    /// =============== Game Logic ==================
    public Vector3 repulseForce;
    public void takeDamage(int iDamage, ContactPoint iCP)
    {
        if (stateMachine.currentState == invulState || stateMachine.currentState == deadState || stateMachine.currentState == frozenState)
            return;

        // lose nuts
        CollectiblesManager cm = Access.CollectiblesManager();
        int n_nuts = cm.getCollectedNuts();

        Vector3 contactNormal = iCP.normal;
        Vector3 contactPoint = iCP.point;
        Debug.DrawRay(contactPoint, contactNormal * 5, Color.red, 5, false);

        Vector3 repulseDir = contactPoint + contactNormal;
        repulseForce = -repulseDir * 5;

        for (int i = 0; i < n_nuts; i++)
        {
            GameObject nutFromDamage = Instantiate(cm.nutCollectibleRef);
            nutFromDamage.GetComponent<CollectibleNut>().setSpawnedFromDamage(transform.position);
        }
        cm.loseNuts(iDamage);
        RB.AddForce(repulseForce, ForceMode.Impulse);

        stateMachine.ForceState(invulState);
    }

    public void useTurbo()
    {
        turboIntervalElapsedTime += Time.deltaTime;
        if (turboIntervalElapsedTime < turboTimeInterval)
            return;

        if (!Access.CollectiblesManager().tryConsumeTurbo(turboConsumptionPerTick))
            return;

        Vector3 turboDir = transform.position.normalized + transform.forward.normalized;
        Debug.DrawRay(transform.position, turboDir, Color.yellow, 4, false);
        RB.AddForce(turboDir * turboStrength, ForceMode.VelocityChange);

        turboIntervalElapsedTime = 0f;
    }

    public void kill(Vector3 iSteer = default(Vector3))
    {
        GameObject dummy_player = Instantiate(OnDeathClone, transform.position, transform.rotation);
        Destroy(dummy_player.GetComponent<CarController>());

        // add dummy's suspension/ wheels to DC objects
        DeathController dc = dummy_player.GetComponent<DeathController>();
        dc.objects.Clear();
        foreach (Transform child in dummy_player.transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (!!rb)
                dc.objects.Add(rb);
        }

        dc.Activate(iSteer);
    }

    /// =============== Unity ==================

    private void OnDestroy()
    {
        Utils.detachControllable<CarController>(this);
    }

    public class PlayerFSM : FSMBase
    {
    }

    public class PrintStateName : FSMAction
    {
        public override void Execute(FSMBase machine)
        {
            base.Execute(machine);
            // player alive logic
            //Debug.Log(machine.currentState.name);
        }
    }

    public class PlayerDeadAction : FSMAction
    {
        public override void Execute(FSMBase machine)
        {
            base.Execute(machine);
            // player alive logic
            Debug.Log("dead");
        }
    }
    public class PlayerIsAlive : FSMCondition
    {
        public override bool Check(FSMBase machine)
        {
            CollectiblesManager cm = Access.CollectiblesManager();
            int n_nuts = cm.getCollectedNuts();
            return (n_nuts >= 0);
        }
    }

    public class PlayerDieTransition : FSMTransition
    {
        public CarController player;
        public override void OnTransition(FSMBase machine, FSMState toState)
        {
            Debug.Log("die");
            player.kill(player.repulseForce);
            Access.CheckPointManager().loadLastCP(true);
            base.OnTransition(machine, toState);
        }
    }

    public class PlayerUpdateRenderer : FSMAction
    {
        public CarController player;
        public override void Execute(FSMBase machine)
        {
            base.Execute(machine);

            player.UpdateWheelsRenderer();
            player.UpdateSuspensionRenderers();
        }
    }

    public class PlayerSpeedEffect : FSMAction
    {
        public CarController player;
        public override void Execute(FSMBase machine)
        {
            base.Execute(machine);
            // Speed effect on camera
            if (player.updateCurrentSpeed() > 5f)
            {
                // update camera FOV/DIST if a PlayerCamera
                CameraManager CamMgr = Access.CameraManager();
                if (CamMgr.active_camera is PlayerCamera)
                {
                    PlayerCamera pc = (PlayerCamera)CamMgr.active_camera;
                    pc.applySpeedEffect(player.currentSpeed);
                }
            }

            var SpeedDirection = player.RB.velocity;
            var particules = player.SpeedParticles.GetComponent<ParticleSystem>();
            if (SpeedDirection.magnitude > 20)
            {
                var e = particules.emission;
                e.enabled = true;
            }
            else
            {
                var e = particules.emission;
                e.enabled = false;
            }
            player.SpeedParticles.GetComponent<ParticleSystem>().transform.LookAt(player.transform.position + SpeedDirection);
            var lifemin = 0.2f;
            var lifemax = 0.6f;
            var speedmin = 20f;
            var speedmax = 100f;
            var partmain = particules.main;
            partmain.startLifetime = Mathf.Lerp(lifemin, lifemax, Mathf.Clamp01((SpeedDirection.magnitude - speedmin) / (speedmax - speedmin)));


        }
    }

    public class PlayerUpdatePhysics : FSMAction
    {
        public CarController player;
        public override void Execute(FSMBase machine)
        {
            base.Execute(machine);

            player.UpdateSprings();

            player.ResolveAxle(ref player.FrontAxle);
            player.ResolveAxle(ref player.RearAxle);
        }
    }

    void Awake()
    {
        if (stateMachine.currentState == deadState)
            return;

        // FSM Condition

        FSMCondition playerIsAlive = new PlayerIsAlive();
        PlayerAircraftCondition playerIsAircraft = new PlayerAircraftCondition();
        playerIsAircraft.player = this;
        PlayerBoatCondition playerIsBoat = new PlayerBoatCondition();
        playerIsBoat.player = this;
        PlayerCarCondition playerIsCar = new PlayerCarCondition();
        playerIsCar.player = this;

        // FSM Transition
        //
        FSMTransition aircraftTrans = new FSMTransition();
        aircraftTrans.condition = playerIsAircraft;
        aircraftTrans.trueState = aircraftState;

        FSMTransition waterTrans = new FSMTransition();
        waterTrans.condition = playerIsBoat;
        waterTrans.trueState = boatState;

        FSMTransition groundTrans = new FSMTransition();
        groundTrans.condition = playerIsCar;
        groundTrans.trueState = groundState;

        PlayerDieTransition aliveToDead = new PlayerDieTransition();
        aliveToDead.player = this;
        aliveToDead.condition = playerIsAlive;
        aliveToDead.falseState = deadState;

        // FSM Actions
        //
        PlayerUpdateRenderer updateRendererAction = new PlayerUpdateRenderer();
        updateRendererAction.player = this;

        PlayerSpeedEffect speedEffectAction = new PlayerSpeedEffect();
        speedEffectAction.player = this;

        PlayerUpdatePhysics updatePhysicsAction = new PlayerUpdatePhysics();
        updatePhysicsAction.player = this;

        // FSM Define graph

        aliveState.name = "alive";
        aliveState.actions.Add(new PrintStateName());
        aliveState.actions.Add(updateRendererAction);
        aliveState.actions.Add(speedEffectAction);

        aliveState.fixedActions.Add(updatePhysicsAction);

        aliveState.transitions.Add(aliveToDead);
        aliveState.transitions.Add(aircraftTrans);
        aliveState.transitions.Add(groundTrans);
        aliveState.transitions.Add(waterTrans);

        groundState.name = "ground";
        groundState.player = this;
        groundState.transitions = aliveState.transitions;
        groundState.actions = aliveState.actions;
        groundState.fixedActions = aliveState.fixedActions;

        boatState.name = "boat";
        boatState.player = this;
        boatState.transitions = aliveState.transitions;
        boatState.actions = aliveState.actions;
        boatState.fixedActions = aliveState.fixedActions;

        aircraftState.name = "aircraft";
        aircraftState.player = this;
        aircraftState.transitions = aliveState.transitions;
        aircraftState.actions = aliveState.actions;
        aircraftState.fixedActions = aliveState.fixedActions;

        deadState.name = "dead";
        deadState.actions.Add(new PrintStateName());
        deadState.transitions.Add(aliveToDead);

        // only force this state
        frozenState.name = "frozen";

        invulState.name = "invul";
        invulState.actions = aliveState.actions;
        invulState.fixedActions = aliveState.fixedActions;
        FSMTimedCondition invulTime = new FSMTimedCondition();
        invulTime.time = 1f;
        FSMTransition invulTrans = new FSMTransition();
        invulTrans.condition = invulTime;
        invulTrans.falseState = invulState;
        invulTrans.trueState = aliveState;
        invulState.transitions.Add(invulTrans);

        stateMachine.currentState = aliveState;

        // Avoid instantiating again wheels and such
        // when the Player is duplicated for garage tests
        // > We could also just remove certain awake calls
        // but i want to check that wit u toff
        if (!!transform.parent && transform.parent.GetComponent<UIGarageTestManager>())
        { clonedForTestInit(); return; }

        if (UseRefs)
        {
            RefAxle.Right = RefSuspension;
            RefAxle.Left = RefSuspension;
            // copy axle in front and left
            FrontAxle = RefAxle;
            RearAxle = RefAxle;
            RearAxle.Length *= -1;
        }

        RB.centerOfMass = CenterOfMass.transform.localPosition;

        SpawnAxles();
        DrawDebugWheels(Color.yellow);
        SetMode(CurrentMode);

        Utils.attachControllable<CarController>(this);
    }

    void Update()
    {
        stateMachine.Update();
        FixedUpdateDone = false;

        DrawDebugAxles(Color.blue, Color.red);
        DrawDebugWheels(Color.yellow);

        if (IsAircraft)
        {
            // auto acceleration for now
            // TODO / IMPORTANT toffa : WE REALLY NEED TO FIX THE INPOUTS GOD DAMN IT
            // We need :
            // - button accelerate
            // - button break
            // - up/down for aircraft, might control suspension on car
            // - right/left
            CarMotor.CurrentRPM = CarMotor.MaxTorque;
        }

        debugTorque.SetCurve(TORQUE);
    }

    void FixedUpdate()
    {
        stateMachine.FixedUpdate();

        if (IsAircraft)
        {
            // try to keep the plane leveled
            RB.AddTorque(-Vector3.Dot(transform.right, Vector3.up) * transform.forward * TorqueForce, ForceMode.VelocityChange);
        }

        if (IsHooked)
        {
            var HookDirY = grapin.GetComponent<grapintest>().D.normalized;
            var Rot = grapin.GetComponent<grapintest>().grapin.transform.localRotation;
            var HookDirZ = Rot * Vector3.forward;
            var HookDirX = Rot * Vector3.right;
            //RB.velocity = Vector3.Scale(RB.velocity, HookDirX) + Vector3.Scale(RB.velocity, HookDirY) - Vector3.Scale(RB.velocity, HookDirY);
            var VProj = AddGravityStep(RB.velocity);
            var VProjX = Vector3.Project(VProj, -HookDirX);
            var VProjZ = Vector3.Project(VProj, HookDirZ);
            Debug.Log("Hooked");
            RB.velocity = VProjX + VProjZ;
        }

        FixedUpdateDone = true;
        ApplyForceMultiplier = false;
    }

    /// ================ Controls ===================

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        if (stateMachine.currentState == deadState)
            return;

        var Turn = Entry.Inputs["Turn"].AxisValue;
        var Acceleration = Entry.Inputs["Accelerator"].AxisValue;
        // CarMotor.CurrentRPM = 0;
        CarMotor.CurrentRPM = Acceleration;

        // Accelration
        if (!IsAircraft)
        {
            // if (Acceleration != 0)
            // {
            //     CurrentAccelerationTime += Time.deltaTime;
            //     CarMotor.CurrentRPM = Acceleration * TORQUE.Evaluate(CurrentAccelerationTime);
            // }
            // else
            // {
            //     CurrentAccelerationTime = 0;
            // }

            FrontAxle.Right.Wheel.Direction = transform.forward;
            FrontAxle.Left.Wheel.Direction = transform.forward;
        }

        // direction
        RearAxle.Right.Wheel.Direction = transform.forward;
        RearAxle.Left.Wheel.Direction = transform.forward;

        if (RearAxle.IsDirection)
        {
            RearAxle.Right.Wheel.Direction = Quaternion.AngleAxis(SteeringAngle * (RearAxle.IsReversedDirection ? -Turn : Turn), transform.up) * transform.forward;
            RearAxle.Left.Wheel.Direction = Quaternion.AngleAxis(SteeringAngle * (RearAxle.IsReversedDirection ? -Turn : Turn), transform.up) * transform.forward;
        }

        if (FrontAxle.IsDirection)
        {
            FrontAxle.Right.Wheel.Direction = Quaternion.AngleAxis(SteeringAngle * (FrontAxle.IsReversedDirection ? -Turn : Turn), transform.up) * transform.forward;
            FrontAxle.Left.Wheel.Direction = Quaternion.AngleAxis(SteeringAngle * (FrontAxle.IsReversedDirection ? -Turn : Turn), transform.up) * transform.forward;
            if (IsAircraft)
            {
                FrontAxle.Right.Wheel.Direction = Quaternion.AngleAxis(SteeringAngle * (FrontAxle.IsReversedDirection ? Acceleration : -Acceleration), transform.right) * transform.forward;
                FrontAxle.Left.Wheel.Direction = Quaternion.AngleAxis(SteeringAngle * (FrontAxle.IsReversedDirection ? Acceleration : -Acceleration), transform.right) * transform.forward;
            }
        }

        // jump
        if (Entry.Inputs["Jump"].Down)
        {
            SetSpringSizeMinAndLock();
        }
        else
        {
            if (Entry.Inputs["Jump"].IsUp)
            {
                ApplyForceMultiplier = true;
            }
            ResetSpringSizeMinAndUnlock();
        }
        if (Input.GetKeyDown(KeyCode.E))
            GetComponent<DeathController>().Activate();

        if (Entry.Inputs["Turbo"].Down)
        {
            useTurbo();
        }
    }
}
