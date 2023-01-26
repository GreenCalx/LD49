using System;
using System.Collections.Generic;
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

        public WheelHitInfo HitInfo;
    }

    public List<Suspension> suspensions;

    [System.Serializable]
    /// NOTE toffa : This is a spring damper system based on Hooke's law of elasticity.
    /// The inputs are a Frequency(F) and a Damping ratio(Dr). The spring Stiffness(k) and Damping value(Dv) will be deduced from it.
    /// IMPORTANT toffa : We dont take into account mass in our calculations. => m=1
    /// Dr = Dv / 2*sqr(m*k) <=> DV = 2*Dr*sqr(k)
    /// F = sqr(k/m) / 2*pi <=> k = (2*pi*F)�
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
    public class Suspension : ICloneable
    {
        public Spring Spring;
        public Wheel Wheel;

        public Suspension(Suspension S)
        {
            Spring = S.Spring;
            Wheel = S.Wheel;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
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
    public struct Axle : ICloneable
    {
        public float Width;
        public float Length;
        public Suspension Right;
        public Suspension Left;
        public bool IsTraction;
        public bool IsDirection;
        public bool IsReversedDirection;

        public Axle(Axle a)
        {
            this = (Axle)a.Clone();
            this.Right = new Suspension(a.Right);
            this.Left = new Suspension(a.Left);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
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

    private bool IsInJumpStart = false;
    private bool IsInJump = false;
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
    public PlayerBallState ballState = new PlayerBallState();
    public PlayerSpiderState spiderState = new PlayerSpiderState();

    /// =============== Cache ===============
    public Rigidbody RB;
    private bool modifierCalled = false;

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

    void DrawDebugWheel(Suspension S, Color C)
    {
        DebugDrawCircle(S.Wheel.Radius, GetEnd(S), transform.right, 8, S.Wheel.IsGrounded ? Color.magenta : C);
        DebugDrawCircle(S.Wheel.Radius, GetEnd(S) + transform.right * S.Wheel.Width, transform.right, 8, S.Wheel.IsGrounded ? Color.magenta : C);
        DebugDrawCircle(S.Wheel.Radius, GetEnd(S) - transform.right * S.Wheel.Width, transform.right, 8, S.Wheel.IsGrounded ? Color.magenta : C);
    }

    void DrawDebugWheels(Color C)
    {
        DrawDebugWheel(FrontAxle.Right, C);
        DrawDebugWheel(FrontAxle.Left, C);
        DrawDebugWheel(RearAxle.Right, C);
        DrawDebugWheel( RearAxle.Left, C);
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

        suspensions = new List<Suspension>();
        suspensions.Add(FrontAxle.Right);
        suspensions.Add(FrontAxle.Left);
        suspensions.Add(RearAxle.Right);
        suspensions.Add(RearAxle.Left);
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
        return RB.GetPointVelocity(Position) / 4;
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

    public struct WheelHitInfo
    {
        public Ground.GroundInfos GroundI;
        public float Distance;
        public Vector3 Normal;
        public bool Hit;
        public Vector3 Point;
        public Vector3 PenetrationCorrectionDirection;
        public float PenetrationCorrectionDistance;
        public GameObject Collider;

        public Matrix4x4 ContactToWorld;
        public Vector3 LocalVelocity;

        public Vector3 RelativeContactPosition;
        public Vector3 RelativeContactVelocity;
        public float DesiredDeltaVelocity;
    }

    public float PhysicsBias = .1f;
    void GetWheelHitInfo(Suspension S)
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
        //MaxDistance = S.Spring.MinLength + PhysicsBias;
        var MinDistance = S.Spring.MinLength;
        //S.Spring.CurrentLength = S.Spring.MinLength;

        var TestSweep = S.Spring.Anchor.GetComponentInChildren<Rigidbody>().SweepTestAll(SpringDirection, MaxDistance, QueryTriggerInteraction.Ignore);
        Array.Sort(TestSweep, (a, b) => a.distance < b.distance ? -1 : 1);

        Result.Hit = TestSweep.Length != 0;
        if (Result.Hit)
        {
            Result.Collider = TestSweep[0].collider.gameObject;
            Result.Distance = TestSweep[0].distance;
            Result.Normal = TestSweep[0].normal;
            Result.Point = TestSweep[0].point;
              // check for overlap?
            var overlap = Physics.OverlapBox(WheelPosition, new Vector3(S.Wheel.Width, S.Wheel.Radius, S.Wheel.Radius),
                                             Quaternion.LookRotation(S.Wheel.Direction, -SpringDirection),
                                             ~(1 << LayerMask.NameToLayer("No Player Collision"))
                                               , QueryTriggerInteraction.Ignore);
            if (overlap.Length != 0)
            {
                for (int x=0;x<overlap.Length;++x){
                    if (overlap[x].gameObject == Result.Collider) continue;
                    var wheelCollider = S.Spring.Anchor.GetComponentInChildren<MeshCollider>();
                    // IMPORTANT : Physx treat mesh collider as hollow surface : if center of overlap is under, triangle will be culled and
                    // no penetration will be found........
                    float Pene = 0;
                    Vector3 PeneDir = Vector3.zero;
                    if (Physics.ComputePenetration(wheelCollider,
                                                WheelPosition,
                                                wheelCollider.transform.rotation,
                                                overlap[x],
                                                overlap[x].transform.position,
                                                overlap[x].transform.rotation,
                                                out PeneDir, out Pene))
                    //    BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                    //    bc.center = transform.InverseTransformPoint(WheelPosition);
                    //    bc.size = new Vector3(S.Wheel.Width * 2, S.Wheel.Radius * 2, S.Wheel.Radius * 2);
                    //    Debug.DrawLine(WheelPosition, WheelPosition + transform.up * 2);
                    //    if (Physics.ComputePenetration(bc, WheelPosition, transform.rotation,
                    //                                   overlap[0], overlap[0].transform.position, overlap[0].transform.rotation,
                    //                                   out Result.PenetrationCorrectionDirection, out Result.PenetrationCorrectionDistance))
                    {
                        if (Pene > Result.PenetrationCorrectionDistance) {
                            Result.Hit = true;
                            Result.PenetrationCorrectionDirection = PeneDir;
                            Result.PenetrationCorrectionDistance = Pene;
                            Result.Normal = (Result.PenetrationCorrectionDirection * Result.PenetrationCorrectionDistance).normalized;
                            Result.Collider = overlap[x].gameObject;
                            Result.Distance = S.Spring.CurrentLength;
                            Result.Point = WheelPosition - (S.Wheel.Radius*Result.PenetrationCorrectionDirection) - (Result.PenetrationCorrectionDirection * Result.PenetrationCorrectionDistance);
                            Debug.Log("overlap and penetration   " + Result.Normal + "  " + Result.PenetrationCorrectionDistance);
                        }
                    }
                    else
                    {

                        Debug.Log("overlap but no penetration?" + "    " + overlap.Length);
                    }
                }
            }


            var diff = Result.Distance - MinDistance;
            if (diff < 0)
            {
                Result.Distance = 0;
                var wheelCollider = S.Spring.Anchor.GetComponentInChildren<MeshCollider>();
                if (Physics.ComputePenetration(wheelCollider,
                                              SpringAnchor + SpringDirection * S.Spring.MinLength,
                                              wheelCollider.transform.rotation,
                                              TestSweep[0].collider,
                                              Result.Collider.transform.position,
                                              Result.Collider.transform.rotation,
                                              out Result.PenetrationCorrectionDirection, out Result.PenetrationCorrectionDistance))
                {
                    Debug.Log("penetration found" + Result.Normal + "   " + Result.PenetrationCorrectionDistance);
                }
                else
                {
                    Debug.Log("dist < min but no penetration?");
                }
            }
            else
            {
                Debug.Log("Hit");
            }
        }
        else
        {
            // check for overlap?
            var overlap = Physics.OverlapBox(WheelPosition, new Vector3(S.Wheel.Width, S.Wheel.Radius, S.Wheel.Radius),
                                             Quaternion.LookRotation(S.Wheel.Direction, -SpringDirection),
                                             ~(1 << LayerMask.NameToLayer("No Player Collision"))
                                               , QueryTriggerInteraction.Ignore);
            if (overlap.Length != 0)
            {
                Result.Distance = 0;
                var wheelCollider = S.Spring.Anchor.GetComponentInChildren<MeshCollider>();
                // IMPORTANT : Physx treat mesh collider as hollow surface : if center of overlap is under, triangle will be culled and
                // no penetration will be found........
                if (Physics.ComputePenetration(wheelCollider,
                                              WheelPosition,
                                              wheelCollider.transform.rotation,
                                              overlap[0],
                                              overlap[0].transform.position,
                                              overlap[0].transform.rotation,
                                              out Result.PenetrationCorrectionDirection, out Result.PenetrationCorrectionDistance))
                //    BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                //    bc.center = transform.InverseTransformPoint(WheelPosition);
                //    bc.size = new Vector3(S.Wheel.Width * 2, S.Wheel.Radius * 2, S.Wheel.Radius * 2);
                //    Debug.DrawLine(WheelPosition, WheelPosition + transform.up * 2);
                //    if (Physics.ComputePenetration(bc, WheelPosition, transform.rotation,
                //                                   overlap[0], overlap[0].transform.position, overlap[0].transform.rotation,
                //                                   out Result.PenetrationCorrectionDirection, out Result.PenetrationCorrectionDistance))
                {
                    Result.Hit = true;
                    Result.Normal = (Result.PenetrationCorrectionDirection * Result.PenetrationCorrectionDistance).normalized;
                    Debug.Log("overlap and penetration   " + Result.Normal + "  " + Result.PenetrationCorrectionDistance);
                    Result.PenetrationCorrectionDistance = Result.PenetrationCorrectionDistance;
                }
                else
                {

                    Debug.Log("overlap but no penetration?" + "    " + overlap.Length);
                }
                //GameObject.DestroyImmediate(bc);

            }
        }

        //DrawDebugWheels(Color.yellow);

        if (Result.Collider && Result.Collider.GetComponent<Ground>())
        {
            Result.GroundI = Result.Collider.GetComponent<Ground>().GetGroundInfos(Result.Point);
        }
        else
        {
            Result.GroundI = new Ground.GroundInfos();
        }

        S.Wheel.Velocity = GetWheelVelocity(GetEnd(S));
        S.Wheel.HitInfo = Result;

        ComputeContactBasis(ref S.Wheel);
        
        Vector3 contactVelocity = S.Wheel.HitInfo.ContactToWorld.transpose * S.Wheel.Velocity;
        Vector3 accVelocity = LastVelocity;
        accVelocity = S.Wheel.HitInfo.ContactToWorld.transpose * accVelocity;
        accVelocity.x = 0;
        contactVelocity += accVelocity;

        S.Wheel.HitInfo.LocalVelocity = S.Wheel.Velocity;
        S.Wheel.HitInfo.LocalVelocity = contactVelocity;

        if (S.Wheel.HitInfo.Point == Vector3.zero) {
            S.Wheel.HitInfo.Point = S.Spring.Anchor.transform.position;
        }

        S.Wheel.HitInfo.RelativeContactPosition = S.Wheel.HitInfo.Point - RB.position;

        ComputeDesiredVelocity(ref S.Wheel);
    }

    float ScalarProduct(Vector3 a, Vector3 b) {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    void ComputeDesiredVelocity(ref Wheel W) {
       var velocityLimit = 0.25f;

        // Calculate the acceleration induced velocity accumulated this frame
        float velocityFromAcc = 0;
	    velocityFromAcc += ScalarProduct(LastVelocity, W.HitInfo.Normal) * Time.fixedDeltaTime;
    
        float thisRestitution = Bounciness;
        if (Mathf.Abs(W.HitInfo.LocalVelocity.x) < velocityLimit)
        {
            thisRestitution = 0.0f;
        }

        // Combine the bounce velocity with the removed
        // acceleration velocity.
        W.HitInfo.DesiredDeltaVelocity = -W.HitInfo.LocalVelocity.x - thisRestitution * (W.HitInfo.LocalVelocity.x - velocityFromAcc);
    }

    void ComputeContactBasis(ref Wheel w)
    {
        var Normal = w.HitInfo.Normal;
        var Tangent = Vector3.zero;
        var BiTangent = Vector3.zero;

        if (Mathf.Abs(Normal.x) > Mathf.Abs(Normal.y))
        {
            // The new X-axis is at right angles to the world Y-axis.
            Tangent = new Vector3(Normal.z, 0, -Normal.x);
            // The new Y-axis is at right angles to the new X- and Z-axes.
            BiTangent = new Vector3(Normal.y * Tangent.x,
                                    Normal.z * Tangent.x - Normal.x * Tangent.z,
                                    -Normal.y * Tangent.x);
        }
        else
        {
            // The new X-axis is at right angles to the world X-axis.
            Tangent = new Vector3(0, -Normal.z, Normal.y);
            // The new Y-axis is at right angles to the new X- and Z-axes.
            BiTangent = new Vector3(Normal.y * Tangent.z - Normal.z * Tangent.y,
                                     -Normal.x * Tangent.z,
                                     Normal.x * Tangent.y);
        }

        w.HitInfo.ContactToWorld = new Matrix4x4(   new Vector4(Normal.x,    Normal.y,    Normal.z),
                                                    new Vector4(Tangent.x,   Tangent.y,   Tangent.z),
                                                    new Vector4(BiTangent.x, BiTangent.y, BiTangent.z),
                                                    Vector4.zero);
    }

    [Range(0f, 1f)]
    public float Bounciness = 0;
    Vector3 ComputeSuspensionForce(Suspension S, WheelHitInfo HitInfo)
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

    Vector3 LastVelocity = Vector3.zero;
    float ComputeWheelLoad(bool front)
    {
        //for now lets split by 2 in front and rear
        var FrontWheelBase = FrontAxle.Length;
        var RearWheelBase = -RearAxle.Length;
        var TotalWheelBase = FrontWheelBase + RearWheelBase;

        var G = Physics.gravity;

        var TotalMass = RB.mass;
        var TotalWeight = TotalMass * G;

        // compute diff in X with center of mass
        var front_rear_left = GetEnd(FrontAxle.Left) - GetEnd(RearAxle.Left);
        var front_rear_right = GetEnd(FrontAxle.Right) - GetEnd(RearAxle.Right);
        var com_projected_front_left = Vector3.Project(RB.worldCenterOfMass - GetEnd(FrontAxle.Left), front_rear_left).magnitude / front_rear_left.magnitude;
        var com_projected_rear_left = Vector3.Project(RB.worldCenterOfMass - GetEnd(RearAxle.Left), front_rear_left).magnitude / front_rear_left.magnitude;

        var com_projected_front_right = Vector3.Project(RB.worldCenterOfMass - GetEnd(FrontAxle.Right), front_rear_left).magnitude / front_rear_left.magnitude;
        var com_projected_rear_right = Vector3.Project(RB.worldCenterOfMass - GetEnd(RearAxle.Right), front_rear_left).magnitude / front_rear_left.magnitude;

        var front_left_right = GetEnd(FrontAxle.Left) - GetEnd(FrontAxle.Right);
        var com_projected_x_front_right = Vector3.Project(RB.worldCenterOfMass - GetEnd(FrontAxle.Left), front_left_right).magnitude / front_left_right.magnitude;
        var com_projected_x_front_left = Vector3.Project(RB.worldCenterOfMass - GetEnd(FrontAxle.Right), front_left_right).magnitude / front_left_right.magnitude;

        var rear_left_right = GetEnd(RearAxle.Left) - GetEnd(RearAxle.Right);
        var com_projected_x_rear_right = Vector3.Project(RB.worldCenterOfMass - GetEnd(RearAxle.Left), rear_left_right).magnitude / rear_left_right.magnitude;
        var com_projected_x_rear_left = Vector3.Project(RB.worldCenterOfMass - GetEnd(RearAxle.Right), rear_left_right).magnitude / rear_left_right.magnitude;

        var front_left = com_projected_front_left * 0.25f + com_projected_x_front_left * 0.25f;
        var front_right = com_projected_front_right * 0.25f + com_projected_x_front_right * 0.25f;
        var rear_left = com_projected_rear_left * 0.25f + com_projected_x_rear_left * 0.25f;
        var rear_right = com_projected_rear_right * 0.25f + com_projected_x_rear_right * 0.25f;

        //Debug.Log(front_left + "  " + front_right + "  " + rear_left + "  " + rear_right + "    " + (front_left + front_right + rear_left + rear_right) );

        return front_left;
    }

    Vector3 ComputeWheelForce(Wheel W, WheelHitInfo HitInfo)
    {
        var WheelVelocity = W.Velocity;
        // Compute according to the ground normal where the forward direction would be
        // it means the ground normal act as if it was the up vector of the car.

        // NOTE toffa : we forst do everything as if we were touching the ground from above at 90 angle.
        var WheelForward = Quaternion.FromToRotation(transform.up, HitInfo.Normal) * W.Direction;
        var ForceDirection = Quaternion.FromToRotation(transform.up, HitInfo.Normal) * W.Direction;
        // note toffa : trying to apply contact point information
        // for now acting as if we only have one
        //var ForceDirection = Quaternion.AngleAxis(
        //    Vector3.SignedAngle(transform.up, HitInfo.Normal, W.Renderer.transform.right),
        //    W.Renderer.transform.right) * WheelForward;

        //var ForceDirection = Vector3.ProjectOnPlane(WheelForward, HitInfo.Normal);
        //var ForceDirection = Vector3.ProjectOnPlane(WheelForward, HitInfo.Normal);
        //var ForceDirection = -Vector3.Cross(HitInfo.Normal, Vector3.Cross(HitInfo.Normal, WheelForward));
        Debug.DrawLine(W.Renderer.transform.position, W.Renderer.transform.position + ForceDirection);

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
        var RollingResistance = .01f;

        var torque = Mathf.Clamp01(Mathf.Abs(Speed) / MaxSpeed);
        var GripX = TORQUE.Evaluate(torque);

        if (debugTorque)
            debugTorque.currentValue = torque;

        var Force = -WheelVelocityY * GripY;
        var ForceX = WheelForward * GripX * CarMotor.CurrentRPM;
        var deltaForceX = WheelVelocityX - ForceX;
        if (deltaForceX.magnitude - RollingResistance < 0)
            ForceX = Vector3.zero;
        else 
            ForceX -= WheelVelocityX.normalized * RollingResistance;
        Force += ForceX;
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

    public void AddTorque(Vector3 torque, bool useMass = false)
    {
        RB.angularVelocity += (useMass ? RB.inertiaTensorRotation : Quaternion.identity) * torque;
    }

    public void AddForce(Vector3 force, bool useMass = false)
    {
        RB.velocity += (useMass ? 1 / RB.mass : 1) * force;
    }

    public void AddForceAtPos(Vector3 force, Vector3 position, bool useMass = false)
    {
        AddForce (force                                             , useMass);
        AddTorque(Vector3.Cross((position - RB.centerOfMass), force), useMass);
    }

    void ApplyWheelDepen(Wheel w)
    {
        if (w.HitInfo.PenetrationCorrectionDistance > 0)
        {
            // we are hitting the ground
            // apply at COM the depenetration velocity : depen force do 0 work
            var depenForce = w.HitInfo.PenetrationCorrectionDistance * w.HitInfo.PenetrationCorrectionDirection;
            var depenForceClamped = (depenForce.magnitude > Physics.defaultMaxDepenetrationVelocity ? depenForce.normalized * Physics.defaultMaxDepenetrationVelocity : depenForce);
            CurrentDepenAcc.x = Mathf.Sign(depenForceClamped.x) != Mathf.Sign(CurrentDepenAcc.x) ?
                CurrentDepenAcc.x + depenForceClamped.x :
                Mathf.Max(depenForceClamped.x, CurrentDepenAcc.x);
            CurrentDepenAcc.y = Mathf.Sign(depenForceClamped.y) != Mathf.Sign(CurrentDepenAcc.y) ?
                                           CurrentDepenAcc.y + depenForceClamped.y :
                                           Mathf.Max(depenForceClamped.y, CurrentDepenAcc.y);
            CurrentDepenAcc.z = Mathf.Sign(depenForceClamped.z) != Mathf.Sign(CurrentDepenAcc.z) ?
                                           CurrentDepenAcc.z + depenForceClamped.z :
                                           Mathf.Max(depenForceClamped.z, CurrentDepenAcc.z);
        }
    }

    void ApplyWheelConstraints(Wheel w)
    {
        var wheelPosition = w.Renderer.transform.position;
        var currentAngVelocity = w.Velocity - CurrentRBVelocity;
        var currentVelocity = CurrentRBVelocity;

        if (w.HitInfo.Hit)
        {
            var N = w.HitInfo.Normal;
            Bounciness = 0;
            var V = -(1 + Bounciness) * Vector3.Dot(currentVelocity, N);
            var R = (wheelPosition - RB.worldCenterOfMass);
            var AV = Vector3.Cross(Vector3.Cross(R, N), R);
            var ang = Vector3.Dot(AV, N);

            var impulse = N * (V / ang);

            NextRBAngVelocity += Vel2AngularVel(impulse, RB.worldCenterOfMass, wheelPosition);
            NextRBVelocity += impulse;
        }
    }

    Vector3 AngularVel2Vel(Vector3 angVel, Vector3 rotOrigin, Vector3 pt)
    {
        return Vector3.Cross(angVel, (pt - rotOrigin));
    }

    Vector3 Vel2AngularVel(Vector3 vel, Vector3 rotOrigin, Vector3 pt)
    {
        var torque = Vector3.Cross(vel, (pt - rotOrigin));
        return torque;
    }

    Matrix4x4 Inverse(Matrix4x4 M)
    {
        return M.transpose;
    }

    void UpdateContactPoints(Vector3 velocityChange, Vector3 angularChange) {
            // update other contacts that could be impacted by the change
            for (int j = 0; j < suspensions.Count; ++j)
            {
                if (suspensions[j].Wheel.HitInfo.Hit)
                {
                    ref var body_b = ref suspensions[j].Wheel.HitInfo;
                    var deltaPos = velocityChange + Vector3.Cross(angularChange, body_b.RelativeContactPosition);
                    var new_pene = body_b.PenetrationCorrectionDistance - ScalarProduct(deltaPos, body_b.Normal);
                    body_b.PenetrationCorrectionDistance = new_pene;
                }
            }
    }

    // Inertia Tensor Matrix can be decomposed in M = transpose(R)*D*R
    // M is the original matrix
    // R is a rotation matrix, stored in the rigidbody as a quaternion in inertiaTensorRotation
    // D is a diagonal matrix, stored in the rigidbody as a vector3 in inertiaTensor
    // D are the eigenvalues and R are the eigenvectors
    // Inertia Tensor Matrix is a 3x3 Matrix, so it will appear in the first 3x3 positions of the 4x4 Unity Matrix used here
    public static Matrix4x4 CalculateInertiaTensorMatrix(Vector3 inertiaTensor, Quaternion inertiaTensorRotation)
    {
        Matrix4x4 R = Matrix4x4.Rotate(inertiaTensorRotation); //rotation matrix created
        Matrix4x4 S = Matrix4x4.Scale(inertiaTensor); // diagonal matrix created
        return R * S * R.transpose; // R is orthogonal, so R.transpose == R.inverse
    }

    Quaternion QuatAddVec(Quaternion Q, Vector3 V) {
        Quaternion q = new Quaternion(V.x, V.y, V.z, 0);
        q *= Q;

        Q.x += q.x * 0.5f;
        Q.y += q.y * 0.5f;
        Q.z += q.z * 0.5f;
        Q.w += q.w * 0.5f;

        return Q;
    }


    void ApplyDepen(ref WheelHitInfo body, out Vector3 linearChange, out Vector3 angularChange) {
        // Get current inertia ob the objects aat the contact point
        var InverseInertiaTensorWorld = GetInverseInertiaTensorWorld();
        
        Vector3 angularInertiaWorld = Vector3.Cross(body.RelativeContactPosition, body.Normal);
        angularInertiaWorld = InverseInertiaTensorWorld * angularInertiaWorld;
        angularInertiaWorld = Vector3.Cross(angularInertiaWorld, body.RelativeContactPosition);
        var angularInertia = ScalarProduct(angularInertiaWorld, body.Normal);

        var linearInertia = 1/RB.mass;

        var totalInertia = linearInertia + angularInertia;

        // The linear and angular movements required are in proportion to
        // the two inverse inertias.
        var angularMove = body.PenetrationCorrectionDistance * (angularInertia / totalInertia);
        var linearMove =  body.PenetrationCorrectionDistance * (linearInertia / totalInertia);

        // To avoid angular projections that are too great (when mass is large
        // but inertia tensor is small) limit the angular move.
        Vector3 projection = body.RelativeContactPosition;
        projection += body.Normal * ScalarProduct(-body.RelativeContactPosition, body.Normal);

        // Use the small angle approximation for the sine of the angle (i.e.
        // the magnitude would be sine(angularLimit) * projection.magnitude
        // but we approximate sine(angularLimit) to angularLimit).
        var angularLimit = 0.2f;
        var maxMagnitude = angularLimit * projection.magnitude;

        if (angularMove < -maxMagnitude)
        {
            float totalMove = angularMove + linearMove;
            angularMove = -maxMagnitude;
            linearMove = totalMove - angularMove;
        }
        else if (angularMove > maxMagnitude)
        {
            float totalMove = angularMove + linearMove;
            angularMove = maxMagnitude;
            linearMove = totalMove - angularMove;
        }

        // We have the linear amount of movement required by turning
        // the rigid body (in angularMove). We now need to
        // calculate the desired rotation to achieve that.
        angularChange = Vector3.zero;
        if (angularMove != 0)
        {
            // Work out the direction we'd like to rotate in.
            Vector3 targetAngularDirection = Vector3.Cross(body.RelativeContactPosition, body.Normal);  
            // Work out the direction we'd need to rotate to achieve that
            angularChange = InverseInertiaTensorWorld * targetAngularDirection * (angularMove / angularInertia);
        }

        // Velocity change is easier - it is just the linear movement
        // along the contact normal.
        linearChange = body.Normal * linearMove;

        // Now we can start to apply the values we've calculated.
        // Apply the linear movement
        RB.position += linearChange;
        // And the change in orientation
        RB.rotation = QuatAddVec(RB.rotation, angularChange);
    }

    void DepenWheels(int maxIter)
    {
        // See at least once each contact point to avoid missing collisions
        for (int i = 0; i < suspensions.Count; ++i)
        {
            var body_a = suspensions[i].Wheel.HitInfo;
            if (!body_a.Hit) continue;

            Vector3 linearChange = Vector3.zero;
            Vector3 angularChange = Vector3.zero;
            ApplyDepen(ref body_a, out linearChange, out angularChange);
            //UpdateContactPoints(linearChange, angularChange);
        }
        // Now do the iterative algo trying to take care of biggest pene first if any still there
        for (int i = 0; i < maxIter; ++i)
        {
            // sort according to biggest penetration
            suspensions.Sort((a, b) => (a.Wheel.HitInfo.PenetrationCorrectionDistance > b.Wheel.HitInfo.PenetrationCorrectionDistance) ? -1 : 1);
            // always look at first object
            var body_a = suspensions[0].Wheel.HitInfo;
            if (!body_a.Hit) break;

            Vector3 linearChange = Vector3.zero;
            Vector3 angularChange = Vector3.zero;
            ApplyDepen(ref body_a, out linearChange, out angularChange);
            UpdateContactPoints(linearChange, angularChange );
        }
    }

    Matrix4x4 GetInverseInertiaTensor() {
        var M = CalculateInertiaTensorMatrix(RB.inertiaTensor, RB.inertiaTensorRotation);
        return M.inverse;
    }

    Matrix4x4 GetInverseInertiaTensorWorld() {
        var InertiaTensor = CalculateInertiaTensorMatrix(RB.inertiaTensor, RB.inertiaTensorRotation);
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(RB.transform.rotation);
        Matrix4x4 InertiaTensorWorld = rotationMatrix * InertiaTensor * rotationMatrix.transpose;
        return InertiaTensorWorld.inverse;
    }

    void ApplyCollisions(int maxIter)
    {
        var CurrentDepenForceVel = Vector3.zero;
        var CurrentDepenForceAngVel = Vector3.zero;
        for (int i = 0; i < maxIter; ++i)
        {
            // sort according to biggest update to do by delta velocity
            suspensions.Sort((a, b) => ( a.Wheel.HitInfo.DesiredDeltaVelocity > b.Wheel.HitInfo.DesiredDeltaVelocity) ? -1 : 1);

            ref var body_a = ref suspensions[0].Wheel.HitInfo;

            if (body_a.Hit && body_a.Distance < suspensions[0].Spring.MinLength)
            {
                // compute forces to depenetrate
                Vector3 Impulse = CalculateFrictionlessImpulse(body_a, GetInverseInertiaTensor());
                Impulse = body_a.ContactToWorld * Impulse;

                 // Split in the impulse into linear and rotational components
                Vector3 ImpulsiveTorque = Vector3.Cross(body_a.RelativeContactPosition, Impulse);
                Vector3 rotationChange = GetInverseInertiaTensor() * ImpulsiveTorque;
                var velocityChange = Impulse * (1/RB.mass);

                // Apply the changes
                RB.velocity += velocityChange;
                RB.angularVelocity += rotationChange;

                // update other contacts that could be impacted by the change
                for (int j = 0; j < suspensions.Count; ++j)
                {
                    if (suspensions[j].Wheel.HitInfo.Hit)
                    {
                        ref var body_b = ref suspensions[j].Wheel.HitInfo;
                        var deltaVel = velocityChange + Vector3.Cross(rotationChange, body_b.RelativeContactPosition);

                        Vector3 v = body_b.ContactToWorld.transpose * (deltaVel);
                        body_b.LocalVelocity += v;
                        ComputeDesiredVelocity(ref suspensions[j].Wheel);
                    }
                }
            }
        }
    }

Vector3 CalculateFrictionlessImpulse(WheelHitInfo W, Matrix4x4 inverseInertiaTensor)
{
    Vector3 impulseContact;

    // Build a vector that shows the change in velocity in
    // world space for a unit impulse in the direction of the contact
    // normal.
    Vector3 deltaVelWorld = Vector3.Cross(W.RelativeContactPosition, W.Normal);
    deltaVelWorld = inverseInertiaTensor * deltaVelWorld;
    deltaVelWorld = Vector3.Cross(deltaVelWorld, W.RelativeContactPosition);

    // Work out the change in velocity in contact coordinates.
    float deltaVelocity = ScalarProduct(deltaVelWorld, W.Normal);

    // Add the linear component of velocity change
    deltaVelocity += 1 / RB.mass;

    // Calculate the required size of the impulse
    impulseContact.x = W.DesiredDeltaVelocity / deltaVelocity;
    impulseContact.y = 0;
    impulseContact.z = 0;
    return impulseContact;
}

    Vector3 CurrentRBVelocity = Vector3.zero;
    Vector3 NextRBVelocity = Vector3.zero;
    Vector3 CurrentRBAngVelocity = Vector3.zero;
    Vector3 NextRBAngVelocity = Vector3.zero;
    Vector3 CurrentDepenAcc = Vector3.zero;
    void ApplyAllWheelConstraints()
    {
        Debug.Log("ApplyAllWheelConstraints START : vel : " + RB.velocity + "   angVel : " + RB.angularVelocity + "   pos : " + RB.position + "   rot: " + RB.rotation);
        
        // first compute everything that we might need to resolve collisions
        foreach (var s in suspensions)
        {
            GetWheelHitInfo(s);
        }
        // apply position and rotation changes to avoid penetration
        // this will not do work from forces!
        DepenWheels(4);
        Debug.Log("ApplyAllWheelConstraints DepenWheels : vel : " + RB.velocity + "   angVel : " + RB.angularVelocity + "   pos : " + RB.position + "   rot: " + RB.rotation);
        // really compute an answer to the collisions : remove velocity, add torque, according to contact point
        ApplyCollisions(4);
        //Debug.Log("ApplyAllWheelConstraints ApplyCollisions : vel : " + RB.velocity + "   angVel : " + RB.angularVelocity + "   pos : " + RB.position + "   rot: " + RB.rotation);
        // after everything is sorted out about depenetration and hard hits, we apply suspension force
        for(int i=0; i < suspensions.Count; ++i)
        {
            Debug.Log("Resoleve Suspension : " + i);
            ResolveSuspension(suspensions[i]);
        }

        Debug.Log("CurrentDepenAcc   " + CurrentDepenAcc + "   LastVel   " + CurrentRBVelocity + "  " + CurrentRBAngVelocity + "    NewVel   " + NextRBVelocity + "   " + NextRBAngVelocity + "    Final    " + RB.velocity + "    " + RB.angularVelocity);
    }

    void ApplyConstraints(Suspension S, WheelHitInfo HitInfo)
    {
        ApplyAllWheelConstraints();
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

    void ResolveSuspension(Suspension S)
    {
        //GetWheelHitInfo(S);
        var HitInfo = S.Wheel.HitInfo;
        Debug.Log(HitInfo);

        S.Wheel.IsGrounded = HitInfo.Hit;

        var SpringAnchor = S.Spring.Anchor.transform.position;
        var SpringDirection = -transform.up;

        if (!SuspensionLock)
            S.Spring.CurrentLength = S.Spring.MinLength;

        if (S.Wheel.IsGrounded || IsAircraft)
        {
            SetModeFromGround(HitInfo.GroundI.Type);

            //ApplyConstraints(S, HitInfo) 
            if (HitInfo.Distance != 0f)
            {
                var F = ComputeSuspensionForce(S, HitInfo);
                RB.AddForceAtPosition(F, GetEnd(S), ForceMode.VelocityChange);
                Debug.Log("ResolveSuspension : " + F);
            }

            S.Wheel.Velocity = GetWheelVelocity(GetEnd(S));
            var WF = ComputeWheelForce(S.Wheel, HitInfo);
            Debug.Log("Wheel Force : " + WF);
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
        ResolveSuspension(A.Right);
        //Physics.SyncTransforms();
        ResolveSuspension( A.Left);
        //Physics.SyncTransforms();
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
        BALL,
        SPIDER,
        NONE
    };

    public void SetModeFromGround(Ground.EType Mode)
    {
        // Manual toggle through wheel powers , obsolete?

        //if (Mode == Ground.EType.NONE) return;
        //SetMode(CarMode.GROUND);
        //if (Mode == Ground.EType.WATER) SetMode(CarMode.WATER);
    }

    public class PlayerGroudState : FSMState
    {
        public CarController player;
        public override void OnEnter(FSMBase machine)
        {
            base.OnEnter(machine);
            Debug.Log("GroundState");
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

    public class PlayerBallState : FSMState
    {
        public CarController player;
        public override void OnEnter(FSMBase machine)
        {
            base.OnEnter(machine);
            Debug.Log("BallState");
            player.FrontAxle.IsTraction = false;
            player.FrontAxle.IsDirection = false;
            player.FrontAxle.IsReversedDirection = false;

            player.RearAxle.IsDirection = false;
            player.RearAxle.IsTraction = false;
            player.RearAxle.IsReversedDirection = false;
        }
    }

    public class PlayerSpiderState : FSMState
    {
        public CarController player;
        public override void OnEnter(FSMBase machine)
        {
            base.OnEnter(machine);
            player.FrontAxle.IsTraction = false;
            player.FrontAxle.IsDirection = false;
            player.FrontAxle.IsReversedDirection = false;

            player.RearAxle.IsDirection = false;
            player.RearAxle.IsTraction = false;
            player.RearAxle.IsReversedDirection = false;
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

    public class PlayerBallCondition : FSMCondition
    {
        public CarController player;
        public override bool Check(FSMBase machine)
        {
            return player.CurrentMode == CarMode.BALL;
        }
    }

    public class PlayerSpiderCondition : FSMCondition
    {
        public CarController player;
        public override bool Check(FSMBase machine)
        {
            return player.CurrentMode == CarMode.SPIDER;
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
    public void takeDamage(int iDamage, Vector3 iDamageSourcePoint, Vector3 iDamageSourceNormal, float iRepulsionForce = 5f)
    {
        if (stateMachine.currentState == invulState || stateMachine.currentState == deadState || stateMachine.currentState == frozenState)
            return;

        // lose nuts
        CollectiblesManager cm = Access.CollectiblesManager();
        int n_nuts = cm.getCollectedNuts();

        Vector3 contactNormal = iDamageSourceNormal;
        Vector3 contactPoint = iDamageSourcePoint;
        Debug.DrawRay(contactPoint, contactNormal * 5, Color.red, 5, false);

        Vector3 repulseDir = contactPoint + contactNormal;
        repulseForce = -repulseDir * iRepulsionForce;

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

    public bool GetAndUpdateIsInJump()
    {
        // Jump Start over when 4wheels touching ground
        if ( IsInJumpStart )
        {
            List<Axle> axles = new List<Axle>(){ FrontAxle, RearAxle};
            foreach( Axle a in axles)
            {
                if ( a.Left.Wheel.IsGrounded && a.Right.Wheel.IsGrounded)
                {
                    Debug.Log("Still starting to jump");
                    return false; // early return
                }
            }
            // no more wheels grounded
            IsInJumpStart   = false;
            IsInJump        = true;
            return false;
        } else if (IsInJump) { // In actual Jump airtime
            List<Axle> axles = new List<Axle>(){ FrontAxle, RearAxle};
            foreach( Axle a in axles)
            {
                if (a.Left.Wheel.IsGrounded || a.Right.Wheel.IsGrounded)
                {
                    Debug.Log("IsInJump over : " + a);
                    IsInJump = false;
                    return false;
                }
            }
            return true;
        }
        IsInJumpStart   = false;
        IsInJump        = false;
        return false;
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
            int n_nuts = cm != null ? cm.getCollectedNuts() : 0;
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
                if (CamMgr?.active_camera is PlayerCamera)
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

            Debug.Log("PlyerUpdatePhysics START : " + player.RB.velocity + "   " + player.RB.angularVelocity );
            player.UpdateSprings();
            Debug.Log("PlyerUpdatePhysics Post UpdatSprings : " + player.RB.velocity + "   " + player.RB.angularVelocity );
            player.ApplyAllWheelConstraints();
            Debug.Log("PlyerUpdatePhysics Post ApplyAllWheelConstraints : " + player.RB.velocity + "   " + player.RB.angularVelocity );
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
        PlayerBallCondition playerIsBall = new PlayerBallCondition();
        playerIsBall.player = this;
        PlayerSpiderCondition playerIsSpider = new PlayerSpiderCondition();
        playerIsSpider.player = this;

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

        FSMTransition ballTrans = new FSMTransition();
        ballTrans.condition = playerIsBall;
        ballTrans.trueState = ballState;

        FSMTransition spiderTrans = new FSMTransition();
        spiderTrans.condition = playerIsSpider;
        spiderTrans.trueState = spiderState;

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
        aliveState.transitions.Add(ballTrans);
        aliveState.transitions.Add(spiderTrans);

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

        ballState.name = "ball";
        ballState.player = this;
        ballState.transitions   = aliveState.transitions;
        ballState.actions       = aliveState.actions;
        ballState.fixedActions  = aliveState.fixedActions;

        spiderState.name = "spider";
        spiderState.player = this;
        spiderState.transitions   = aliveState.transitions;
        spiderState.actions       = aliveState.actions;
        spiderState.fixedActions  = aliveState.fixedActions;

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
            RefAxle.Right = RefSuspension.Clone() as Suspension;
            RefAxle.Left = RefSuspension.Clone() as Suspension;
            // copy axle in front and left
            FrontAxle = new Axle(RefAxle);
            RearAxle = new Axle(RefAxle);
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

        debugTorque?.SetCurve(TORQUE);
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

        // power controller update
        PowerController pc = GetComponent<PowerController>();
        if (!!pc)
        {
            // modifier inputs
            if (Entry.Inputs["Modifier"].Down)
            {
                pc.showUI(true);
                if (Entry.Inputs["Power1"].Down) {
                    pc.showIndicator(PowerController.PowerWheelPlacement.LEFT);
                    pc.setNextPower(1);
                } else if (Entry.Inputs["Power2"].Down) {
                    pc.setNextPower(2);
                    pc.showIndicator(PowerController.PowerWheelPlacement.DOWN);
                } else if (Entry.Inputs["Power3"].Down) {
                    pc.showIndicator(PowerController.PowerWheelPlacement.UP);
                    pc.setNextPower(3);
                } else if (Entry.Inputs["Power4"].Down) {
                    pc.showIndicator(PowerController.PowerWheelPlacement.RIGHT);
                    pc.setNextPower(4);
                } else {
                    pc.showIndicator(PowerController.PowerWheelPlacement.NEUTRAL);
                    pc.setNextPower(0);
                }
                modifierCalled = true;
            } else if (modifierCalled && !Entry.Inputs["Modifier"].Down) {
                pc.hideIndicators();
                pc.showUI(false);
                
                pc.tryTriggerPower();
                modifierCalled = false;
            }

            pc.applyPowerEffectInInputs(Entry,this);
        }


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
                IsInJumpStart = true;
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
