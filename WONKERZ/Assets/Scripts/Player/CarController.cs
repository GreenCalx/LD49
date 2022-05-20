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
        public Vector3 GroundVelocity;
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

        RaycastHit Hit, Hit1, Hit2;
        var SpringDirection = -transform.up;

        bool ResultHit = Physics.SphereCast(SpringAnchor + transform.right * S.Wheel.Width, S.Wheel.Radius, SpringDirection, out Hit, Epsilon, ~(1 << LayerMask.NameToLayer("No Player Collision")));
        bool ResultHit1 = Physics.SphereCast(SpringAnchor - transform.right * S.Wheel.Width, S.Wheel.Radius, SpringDirection, out Hit1, Epsilon, ~(1 << LayerMask.NameToLayer("No Player Collision")));
        bool ResultHit2 = Physics.SphereCast(SpringAnchor, S.Wheel.Radius, SpringDirection, out Hit2, Epsilon, ~(1 << LayerMask.NameToLayer("No Player Collision")));

        // Chose closest hit possible
        if (ResultHit)
        {
            if (ResultHit1 && Hit.distance < Hit1.distance)
            {
                if (ResultHit2 && Hit.distance < Hit2.distance)
                {
                    Hit = Hit;
                }
            }
        }
        else
        {
            if (ResultHit1 && !ResultHit2)
            {
                Hit = Hit1;
            }
            if (!ResultHit1 && ResultHit2)
            {
                Hit = Hit2;
            }
            if (ResultHit1 && ResultHit2)
            {
                Hit = (Hit1.distance < Hit2.distance) ? Hit1 : Hit2;
            }
        }

        S.Wheel.IsGrounded = ResultHit1 || ResultHit2 || ResultHit;

        if (!SuspensionLock)
            S.Spring.CurrentLength = S.Spring.MaxLength;

        // NOTE toffa :
        // We are using su stepping, meaning we are dividing the real deltatime by chunks
        // in order to avoid problem with euler integration over time.
        // This way we should be able to avoid slamming into the ground and jittering.
        if (S.Wheel.IsGrounded || IsAircraft)
        {
            // NOTE toffa :
            // This piece of code needs to be removed once we are using the groundinfos struct
            // to get back all infos we need.
            // For now it is only disable, we might need to still do this as a safetynet?
#if false
            // disgusting...
            if (NeedCheckMaterial)
            {
                if (Hit.collider)
                {
                    Renderer renderer = Hit.collider.GetComponent<MeshRenderer>();
                    Material M = null;
                    if (renderer.materials.Length == 1 || Hit.triangleIndex < 0)
                    {
                        M = renderer.materials[0];
                    }
                    else
                    {

                        MeshCollider meshCollider = Hit.collider as MeshCollider;
                        Mesh mesh = meshCollider.sharedMesh;

                        int[] hitTriangle = new int[]
                        {
                        mesh.triangles[Hit.triangleIndex * 3 + 0],
                        mesh.triangles[Hit.triangleIndex * 3 + 1],
                        mesh.triangles[Hit.triangleIndex * 3 + 2]
                        };
                        for (int i = 0; i < mesh.subMeshCount && M == null; i++)
                        {
                            int[] subMeshTris = mesh.GetTriangles(i);
                            for (int j = 0; j < subMeshTris.Length && M == null; j += 3)
                            {
                                if (subMeshTris[j] == hitTriangle[0] &&
                                    subMeshTris[j + 1] == hitTriangle[1] &&
                                    subMeshTris[j + 2] == hitTriangle[2])
                                {
                                    M = renderer.materials[i];
                                }
                            }
                        }
                    }

                    string Suffix = " (Instance)";
                    if (M.name == "SandNew" + Suffix)
                    {
                        SetMode(CarMode.DESERT);
                    }
                    else if (M.name == "WATER2" + Suffix)
                    {
                        SetMode(CarMode.WATER);
                    }
                    else
                    {
                        SetMode(CarMode.ROAD);
                    }
                }
                NeedCheckMaterial = false;
            }
            //end disguting...
#endif

            var G = Hit.collider?.gameObject?.GetComponent<Ground>();
            var GroundInfos = G ? G.GetGroundInfos(Hit.point) : new Ground.GroundInfos();
            SetModeFromGround(GroundInfos.Type);

            var SubStepDeltaTime = Time.fixedDeltaTime / PhysicsSubSteps;
            var TotalProcessedTime = 0f;
            var TraveledDistance = 0f;
            var ForceToApply = Vector3.zero;
            var InitialSpringVelocity = GetWheelVelocity(SpringAnchor);
            var SpringVelocity = InitialSpringVelocity;

            // TODO toffa :
            // this is done because SphereCast Hit.normal DOES NOT RETURN THE SURFACE NORMAL, but the diff between sphere center and hit point.
            // therefor I m doing a small raycast to get the right Hit.normal
            RaycastHit HitRay;
            Hit.collider.Raycast(new Ray(SpringAnchor, SpringDirection), out HitRay, Epsilon);
            Debug.DrawLine(SpringAnchor, Hit.point, Color.white);


            while (TotalProcessedTime < Time.fixedDeltaTime)
            {
                // Add gravity acceleration to velocity
                SpringVelocity += Physics.gravity * SubStepDeltaTime;
                var SpringVelocityProjected = ProjectOnSuspension(SpringVelocity);

                var StepForce = Vector3.zero;

                // Compute what is the current distance to hit point now
                var StepDistance = Hit.distance + GroundInfos.DepthPerturbation.y - TraveledDistance;
                if (!SuspensionLock)
                    S.Spring.CurrentLength = Mathf.Clamp(StepDistance, S.Spring.MinLength, S.Spring.MaxLength);

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
                        //var FCollider = Vector3.Reflect(SpringVelocity * VelocityCorrectionMultiplier, Hit.normal);
                        var FCollider = Vector3.Dot(SpringVelocity.normalized, HitRay.normal) < 0 ? Vector3.Reflect(SpringVelocity * VelocityCorrectionMultiplier, HitRay.normal) : SpringVelocity;
                        // NOTE toffa : StepForce is actually the diff between what wehave and what we want!
                        // and right now this is a perfect bounce, we can probably compute bouciness factor if needed, according to mass?.
                        // NOTE toffa : We can probably get a sticky effect by removing anny part that is not directly linked to the SpringDirection.
                        StepForce += (FCollider - SpringVelocity);
                        Debug.Log("HARD HIT");
                    }
                }

                TotalProcessedTime += SubStepDeltaTime;
                var SuspensionForce = ProjectOnSuspension(StepForce + SpringVelocityProjected * SubStepDeltaTime);
                TraveledDistance += SuspensionForce.magnitude * Vector3.Dot(SuspensionForce.normalized, SpringDirection);

                ForceToApply += StepForce;
                SpringVelocity += StepForce;

            }
            RB.AddForceAtPosition(ForceToApply, GetEnd(S), ForceMode.VelocityChange);

            // Direction
            var WheelVelocity = GetWheelVelocity(WheelPosition);
            var ForceDirection = -Vector3.Cross(Hit.normal, Vector3.Cross(Hit.normal, S.Wheel.Direction));
            Debug.DrawLine(Hit.point, Hit.point + ForceDirection.normalized, Color.green);
            var WheelVelocityX = Vector3.Project(WheelVelocity, S.Wheel.Direction);
            var MotorVelocity = CarMotor.CurrentRPM * S.Wheel.Direction;
            var f = Vector3.Cross(transform.up, S.Wheel.Direction);
            var WheelVelocityY = Vector3.Project(WheelVelocity, f);
            var T = -WheelVelocityX * GroundInfos.Friction.x;
            T -= WheelVelocityY * GroundInfos.Friction.y;
            T += MotorVelocity;
            T *= Mathf.Pow(Vector3.Dot(transform.up, Vector3.up), 3);
            if (IsAircraft)
            {
                // apply at centerofmass height
                RB.AddForceAtPosition(T, SpringAnchor + Vector3.Project(CenterOfMass.transform.position - SpringAnchor, transform.up), ForceMode.VelocityChange);
            }
            else
            {
                if (GroundInfos.Type == Ground.EType.DESERT)
                    S.Wheel.Trails.GetComponent<ParticleSystem>().Play();

                RB.AddForceAtPosition(T, GetEnd(S), ForceMode.VelocityChange);
            }

            // NOTE toffa : This is a test to apply physics to the ground object if a rigid body existst
            var Collider = Hit.collider?.GetComponent<Rigidbody>();
            if (Collider != null)
            {
                var VelocityGravity = Vector3.Project(GetWheelVelocity(SpringAnchor), Vector3.up);
                VelocityGravity.y += Physics.gravity.y;
                if (VelocityGravity.y < 0)
                    Collider.AddForceAtPosition(VelocityGravity * RB.mass, Hit.point, ForceMode.Force);
            }

            // NOTE toffa :
            // Add current ground velocity to the RB to be able to sit still on moving plateform for instance.
            var RBVelProjected = Vector3.Project(RB.velocity, GroundInfos.Velocity);
            var VelDiff = Vector3.Scale((GroundInfos.Velocity - RBVelProjected), new Vector3(1, 0, 1));
            RB.AddForce(VelDiff / 4, ForceMode.VelocityChange);
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
    }

    void Update()
    {
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
