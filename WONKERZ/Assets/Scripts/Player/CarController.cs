using System.Collections.Generic; // KeyValuePair
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

    /// ================== Variables ===================

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
    
    [Header("Behaviours")]
    public bool isFrozen;

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

    [Header("Mecanics")]
    public bool IsHooked;
    // TODO toffa : remove this hardcoded object
    public GameObject grapin;

    /// =============== Cache ===============
    private Rigidbody RB;

    /// ================ Curves ================

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
        S.Renderer.transform.position = S.Anchor.transform.position - transform.up * S.CurrentLength / 2;
        S.Renderer.transform.localScale = new Vector3(1, S.CurrentLength / 2, 1);
    }

    void UpdateSuspensionRenderers()
    {
        UpdateSuspensionRenderer(ref FrontAxle.Right.Spring);
        UpdateSuspensionRenderer(ref FrontAxle.Left.Spring);
        UpdateSuspensionRenderer(ref RearAxle.Right.Spring);
        UpdateSuspensionRenderer(ref RearAxle.Left.Spring);
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
        DebugDrawCircle(S.Wheel.Radius, GetEnd(S), transform.right, 10, C);
        DebugDrawCircle(S.Wheel.Radius, GetEnd(S) + transform.right * S.Wheel.Width, transform.right, 10, C);
        DebugDrawCircle(S.Wheel.Radius, GetEnd(S) - transform.right * S.Wheel.Width, transform.right, 10, C);

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
        S.Renderer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        S.Renderer.transform.parent = gameObject.transform;
        S.Renderer.GetComponent<CapsuleCollider>().enabled = false;
    }

    void SpawnSuspensions()
    {
        ComputeSuspensionsAnchor();
        SpawnWheels();

        SpawnSuspensionRenderers();

        UpdateSuspensionRenderers();
    }

    void SpawnWheel(ref Wheel W, Vector3 pos)
    {
        W.Renderer = GameObject.Instantiate(W.Renderer, pos, W.Renderer.transform.rotation, transform);
        W.Renderer.SetActive(true);
        W.Renderer.transform.localScale = new Vector3(W.Radius * 2, W.Width, W.Radius * 2);
        W.Trails = GameObject.Instantiate(W.Trails, pos, W.Trails.transform.rotation, transform);
    }

    void SpawnWheels()
    {
        SpawnWheel(ref FrontAxle.Right.Wheel, GetEnd(FrontAxle.Right));
        SpawnWheel(ref FrontAxle.Left.Wheel, GetEnd(FrontAxle.Left));
        SpawnWheel(ref RearAxle.Right.Wheel, GetEnd(RearAxle.Right));
        SpawnWheel(ref RearAxle.Left.Wheel, GetEnd(RearAxle.Left));
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

        var SpringDirection = -transform.up;
        var SpringAnchor = S.Spring.Anchor.transform.position;
        // IMPORTANT toffa : the physic step might be too high and miss a collision!
        // Therefore we detect the collision by taking into account the next velocity application in the ray
        // as we are doing the detection by hand.
        // NOTE toffa first we compute the real velocity that will be applied this frame to the point
        // and we project it along the suspension.
        var WheelPosition = GetEnd(S);
        var NextWheelPosition = GetNextPointPosition(WheelPosition);
        var Epsilon = S.Spring.MaxLength + ProjectOnSuspension(WheelPosition - NextWheelPosition).magnitude;
        // trying to use 3 capsule colliders to have predictable collisions
        Vector3 CapsuleUpVector = SpringDirection * S.Wheel.Radius;
        Vector3 CapsuleCenter = SpringAnchor;
        // Cast a capsule in the middle of the wheel
        //
        //       + +
        //     +  |  +
        //     +  |  +
        //       + +
        RaycastHit Hit90;
        bool ResultHit90 = Physics.CapsuleCast(CapsuleCenter - CapsuleUpVector,
                                               CapsuleCenter + CapsuleUpVector,
                                              S.Wheel.Width,
                                              SpringDirection,
                                              out Hit90,
                                              Epsilon,
                                              ~(1 << LayerMask.NameToLayer("No Player Collision")));

        //bool A = Physics.BoxCast(CapsuleCenter, transform.up * S.Wheel.Radius + transform.right * S.Wheel.Width + transform.forward * S.Wheel.Width, SpringDirection, out Hit90, Quaternion.identity, Epsilon, ~(1<< LayerMask.NameToLayer("No Player Collision")));

        if (!ResultHit90) Hit90.distance = float.PositiveInfinity;

        Result.Normal = Hit90.normal;
        Result.Collider = Hit90.collider?.gameObject;
        Result.Distance = Hit90.distance;
        Result.Point = Hit90.point;

        // Cast a capsule at +45deg angle
        //
        //       + +
        //     + \   +
        //     +  \  +
        //       + +
        CapsuleUpVector = Quaternion.AngleAxis(45f, transform.right) * CapsuleUpVector;
        RaycastHit Hit135;
        bool ResultHit135 = Physics.CapsuleCast(CapsuleCenter - CapsuleUpVector,
                                              CapsuleCenter + CapsuleUpVector,
                                              S.Wheel.Width,
                                              SpringDirection,
                                              out Hit135,
                                              Epsilon,
                                             ~(1 << LayerMask.NameToLayer("No Player Collision")));
        if (!ResultHit135) Hit135.distance = float.PositiveInfinity;

        if (Hit135.distance < Result.Distance)
        {
            Result.Normal = Hit135.normal;
            Result.Collider = Hit135.collider?.gameObject;
            Result.Distance = Hit135.distance;
            Result.Point = Hit135.point;
        }
        // Cast a capsule at +90deg angle
        //
        //       + +
        //     +     +
        //     +-----+
        //     +     +
        //       + +
        CapsuleUpVector = Quaternion.AngleAxis(45f, transform.right) * CapsuleUpVector;
        RaycastHit Hit0;
        bool ResultHit0 = Physics.CapsuleCast(CapsuleCenter - CapsuleUpVector,
                                              CapsuleCenter + CapsuleUpVector,
                                              S.Wheel.Width,
                                              SpringDirection,
                                              out Hit0,
                                              Epsilon,
                                              ~(1 << LayerMask.NameToLayer("No Player Collision")));
        if (!ResultHit0) Hit0.distance = float.PositiveInfinity;
        if (Hit0.distance < Result.Distance)
        {
            Result.Normal = Hit0.normal;
            Result.Collider = Hit0.collider?.gameObject;
            Result.Distance = Hit0.distance;
            Result.Point = Hit0.point;
        }
        // Cast a capsule at -45deg angle
        //
        //       + +
        //     +   / +
        //     + /   +
        //       + +
        CapsuleUpVector = SpringDirection * S.Wheel.Radius;
        CapsuleUpVector = Quaternion.AngleAxis(-45f, transform.right) * CapsuleUpVector;
        RaycastHit Hit45;
        bool ResultHit45 = Physics.CapsuleCast(CapsuleCenter - CapsuleUpVector,
                                              CapsuleCenter + CapsuleUpVector,
                                              S.Wheel.Width,
                                              SpringDirection,
                                              out Hit45,
                                              Epsilon,
                                              ~(1 << LayerMask.NameToLayer("No Player Collision")));
        if (!ResultHit45) Hit45.distance = float.PositiveInfinity;
        if (Hit45.distance < Result.Distance)
        {
            Result.Normal = Hit45.normal;
            Result.Collider = Hit45.collider?.gameObject;
            Result.Distance = Hit45.distance;
            Result.Point = Hit45.point;
        }

        Result.Hit = ResultHit0 || ResultHit45 || ResultHit90 || ResultHit135;
        // SphereCast and CapsuleCast does not return the right normal so we need to get back this value
        // from a normal raycast.
        if (Result.Hit)
        {
            Ray R = new Ray(SpringAnchor, Result.Point - SpringAnchor);
            RaycastHit Hit;
            if (Result.Collider.GetComponent<Collider>().Raycast(R, out Hit, 100))
                Result.Normal = Hit.normal;
        }

        // In case everything says it does not hit a collider
        // We check if there is not overlap, because unity does not send back a hit
        // when the checker overlap the collider...

        if (!Result.Hit)
        {
            Vector3 TempDir = Vector3.zero;
            float TempDist = 0;
            bool ResultHitOverlap = false;
            CapsuleUpVector = SpringDirection * (S.Wheel.Radius - S.Wheel.Width);
            var OverlappedColliders = Physics.OverlapCapsule(GetEnd(S) - CapsuleUpVector, GetEnd(S) + CapsuleUpVector, S.Wheel.Width, ~(1 << LayerMask.NameToLayer("No Player Collision")));
            if (OverlappedColliders.Length > 0)
            {
                foreach (var C in OverlappedColliders)
                {
                    if (C.GetComponent<Ground>())
                    {
                        ResultHitOverlap = true;
                        CapsuleCollider CC = gameObject.AddComponent<CapsuleCollider>();
                        CC.center = transform.InverseTransformPoint(GetEnd(S));
                        CC.height = S.Wheel.Radius * 2;
                        CC.radius = S.Wheel.Width / 2;

                        Physics.ComputePenetration(CC, transform.position, transform.rotation, C, C.gameObject.transform.position, C.gameObject.transform.rotation, out TempDir, out TempDist);

                        Result.Hit = true;
                        Result.Normal = Vector3.zero;
                        Result.Collider = C.gameObject;
                        Result.Distance = 0;
                        Result.Point = Vector3.zero;


                        GameObject.DestroyImmediate(CC);
                    }
                }
            }

            CapsuleUpVector = Quaternion.AngleAxis(90f, transform.right) * CapsuleUpVector;
            OverlappedColliders = Physics.OverlapCapsule(GetEnd(S) - CapsuleUpVector, GetEnd(S) + CapsuleUpVector, S.Wheel.Width, ~(1 << LayerMask.NameToLayer("No Player Collision")));
            if (OverlappedColliders.Length > 0)
            {
                foreach (var C in OverlappedColliders)
                {
                    if (C.GetComponent<Ground>())
                    {
                        ResultHitOverlap = true;
                        CapsuleCollider CC = gameObject.AddComponent<CapsuleCollider>();
                        CC.center = transform.InverseTransformPoint(GetEnd(S));
                        CC.height = S.Wheel.Radius * 2;
                        CC.radius = S.Wheel.Width / 2;
                        CC.direction = 2;

                        Physics.ComputePenetration(CC, transform.position, transform.rotation, C, C.transform.position, C.transform.rotation, out Result.PenetrationCorrectionDirection, out Result.PenetrationCorrectionDistance);
                        GameObject.DestroyImmediate(CC);
                    }
                }

            }
            if (Result.PenetrationCorrectionDistance > TempDist)
            {
                Result.Hit = true;
                Result.Normal = (Result.PenetrationCorrectionDirection * Result.PenetrationCorrectionDistance).normalized;
                Result.Collider = OverlappedColliders[0].gameObject;
                Result.Distance = 0;
                Result.Point = Vector3.zero;
            }
            else
            {
                Result.PenetrationCorrectionDistance = TempDist;
                Result.PenetrationCorrectionDirection = TempDir;
                Result.Normal = (TempDir * TempDist).normalized;
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


        Debug.DrawLine(GetEnd(S) - S.Wheel.Radius * transform.forward, GetEnd(S) - S.Wheel.Radius * transform.forward + Result.PenetrationCorrectionDistance * Result.PenetrationCorrectionDirection);
        return Result;
    }

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

            S.Wheel.IsGrounded = StepDistance - (S.Spring.CurrentLength) <= 0;
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
                    //var FCollider = Vector3.Reflect(SpringVelocity * VelocityCorrectionMultiplier, Hit.normal);
                    var FCollider = Vector3.Dot(SpringVelocity.normalized, HitInfo.Normal) < 0 ? Vector3.Reflect(SpringVelocity * VelocityCorrectionMultiplier, HitInfo.Normal) : SpringVelocity;
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

        var T = -WheelVelocityX * HitInfo.GroundI.Friction.x;
        T -= WheelVelocityY * HitInfo.GroundI.Friction.y;
        T += MotorVelocity;
        T *= Mathf.Pow(Mathf.Clamp01(Vector3.Dot(HitInfo.Normal, Vector3.up)), 3);

        var ValidationValue = Utils.Math.ValidateForce(T);
        if (!ValidationValue.Item2)
        {
            Debug.Log("FAILED TO VALIDATE FORCE");
        }
        return ValidationValue.Item1;
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
            RB.AddForceAtPosition(FCollider, S.Spring.Anchor.transform.position, ForceMode.VelocityChange);
            Debug.Log("ApplyConstraints : " + FCollider);
            RB.position += HitInfo.PenetrationCorrectionDirection * HitInfo.PenetrationCorrectionDistance;
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

            if (HitInfo.Distance > 0)
            {
                var F = ComputeSuspensionForce(ref S, HitInfo);
                RB.AddForceAtPosition(F, GetEnd(S), ForceMode.VelocityChange);
                //Debug.Log("ResolveSuspension : " + F);
            }

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
        ResolveSuspension(ref A.Left);
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

    public CarMode CurrentMode = CarMode.NONE;
    public void SetMode(CarMode Mode)
    {
        if (Mode == CurrentMode) return;
        CurrentMode = Mode;

        IsAircraft = false;
        RearAxle.IsTraction = true;
        switch (Mode)
        {
            case CarMode.GROUND:
                {
                    FrontAxle.IsTraction = false;
                    FrontAxle.IsDirection = true;
                    FrontAxle.IsReversedDirection = false;

                    RearAxle.IsDirection = false;
                    RearAxle.IsReversedDirection = false;
                }
                break;
            case CarMode.WATER:
                {
                    FrontAxle.IsTraction = false;
                    FrontAxle.IsDirection = false;
                    FrontAxle.IsReversedDirection = false;

                    RearAxle.IsDirection = true;
                    RearAxle.IsReversedDirection = true;
                }
                break;
            case CarMode.DELTA:
                {
                    FrontAxle.IsTraction = true;
                    FrontAxle.IsDirection = true;
                    FrontAxle.IsReversedDirection = false;

                    RearAxle.IsDirection = true;
                    RearAxle.IsReversedDirection = true;

                    IsAircraft = true;
                }
                break;
        }
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
        Debug.Log("Alternative init for cloned car in test scenarios.");
        RB = GetComponent<Rigidbody>();
        RB.centerOfMass = CenterOfMass.transform.localPosition;
    }

    public Vector2 MouseLastPosition = Vector2.zero;
    // Update is called once per frame
    public float TorqueForce;

    /// =============== Unity ==================

    private void OnDestroy()
    {
        Utils.detachControllable<CarController>(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
    }
    // Start is called before the first frame update
    void Awake()
    {
        // Avoid instantiating again wheels and such
        // when the Player is duplicated for garage tests
        // > We could also just remove certain awake calls
        // but i want to check that wit u toff
        if (transform.parent.GetComponent<UIGarageTestManager>())
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

        GetComponent<Rigidbody>().centerOfMass = CenterOfMass.transform.localPosition;
        RB = GetComponent<Rigidbody>();

        SpawnAxles();
        DrawDebugWheels(Color.yellow);
        SetMode(CurrentMode);

        Utils.attachControllable<CarController>(this);
        isFrozen = false;
    }

    void Update()
    {
        if (isFrozen)
        {
            return;
        }

        FixedUpdateDone = false;


        DrawDebugAxles(Color.blue, Color.red);
        DrawDebugWheels(Color.yellow);
        UpdateWheelsRenderer();
        UpdateSuspensionRenderers();

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
    }

    void FixedUpdate()
    {
        if (isFrozen)
        {
            return;
        }
        
        UpdateSprings();

        ResolveAxle(ref FrontAxle);
        ResolveAxle(ref RearAxle);

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
        var Turn = Entry.Inputs["Turn"].AxisValue;
        var Acceleration = Entry.Inputs["Accelerator"].AxisValue;
        CarMotor.CurrentRPM = 0;

        // Accelration
        if (!IsAircraft)
        {
            if (Acceleration != 0)
            {
                CurrentAccelerationTime += Time.deltaTime;
                CarMotor.CurrentRPM = Acceleration * TORQUE.Evaluate(CurrentAccelerationTime);
            }
            else
            {
                CurrentAccelerationTime = 0;
            }

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
    }
}
