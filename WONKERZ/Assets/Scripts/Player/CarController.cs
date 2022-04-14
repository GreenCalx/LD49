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

    /// ----------------------------------------------------
    /// BlueCalx for Toffo : put those shit where u want ---
    [Header("Curves")]
    public AnimationCurve TORQUE; // curve itself
    public int torque_movable_keyframe; // the keyframe we want to move in garage w slider
    public AnimationCurve WEIGHT;
    public int weight_movable_keyframe;

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
#if false
        FrontAxle.Right.Wheel.Renderer = GameObject.Instantiate(FrontAxle.Right.Wheel.Renderer, GetEnd(FrontAxle.Right), FrontAxle.Right.Wheel.Renderer.transform.rotation, transform);
        FrontAxle.Left.Wheel.Renderer = GameObject.Instantiate(FrontAxle.Left.Wheel.Renderer,GetEnd(FrontAxle.Left), FrontAxle.Left.Wheel.Renderer.transform.rotation, transform);
        RearAxle.Right.Wheel.Renderer = GameObject.Instantiate(RearAxle.Right.Wheel.Renderer,GetEnd(RearAxle.Right), RearAxle.Right.Wheel.Renderer.transform.rotation, transform);
        RearAxle.Left.Wheel.Renderer = GameObject.Instantiate(RearAxle.Left.Wheel.Renderer, GetEnd(RearAxle.Left), RearAxle.Left.Wheel.Renderer.transform.rotation, transform);
#endif
    }

    void SpawnAxles()
    {
        SpawnSuspensions();
    }

    void UpdateWheelRenderer(Suspension S)
    {
        S.Wheel.Renderer.transform.position = GetEnd(S);
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
        return A + Physics.gravity * Time.fixedDeltaTime;
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
        // IMPORTANT toffa : the physic step might be too high and miss a collision!
        // Therefore we detect the collision by taking into account the next velocity application in the ray
        // as we are doing the detection by hand.
        // NOTE toffa first we compute the real velocity that will be applied this frame to the point
        // and we project it along the suspension.
        var WheelPosition = GetEnd(S);
        var NextWheelPosition = GetNextPointPosition(WheelPosition);
        var Epsilon = S.Spring.CurrentLength + S.Wheel.Radius + ProjectOnSuspension(WheelPosition - NextWheelPosition).magnitude;

        RaycastHit Hit;
        var SpringAnchor = S.Spring.Anchor.transform.position;
        var SpringDirection = -transform.up;
        S.Wheel.IsGrounded = Physics.Raycast(SpringAnchor, SpringDirection, out Hit, Epsilon);

        // If we hit something 
        if (S.Wheel.IsGrounded)
        {
            Debug.DrawLine(SpringAnchor, Hit.point, Color.white);
            // something has been hitten, we compute the distance
            S.Spring.CurrentLength = Mathf.Clamp(Hit.distance - S.Wheel.Radius, S.Spring.MinLength, S.Spring.MaxLength);

            Vector3 CurrentMinimalForcePossible = Vector3.zero;
            if (Hit.distance < S.Spring.MinLength + S.Wheel.Radius)
            {
                // We are trying to say that the next position of the wheel should be inside the ground,
                // so for now we compute the new values
                Debug.Log("Boundaries penetration");
                // say that we need the position to be corrected to NOT be inside the ground, and apply full body velocity.
                // we do this by computing the force needed to do so, it would be the minimum possible force to apply!
                var DistanceToCorrect = Hit.distance - (S.Spring.MinLength + S.Wheel.Radius);
                //var WheelVelocityy = AddGravityStep(GetWheelVelocity(WheelPosition));
                var VelocityCorrection = -(DistanceToCorrect * SpringDirection);
                CurrentMinimalForcePossible = VelocityCorrection *4 *VelocityCorrectionMultiplier;
                //RB.AddForceAtPosition(VelocityCorrection, SpringAnchor, ForceMode.VelocityChange);
            }

            var SpringVelocity = AddGravityStep(GetWheelVelocity(SpringAnchor));
            var SpringVelocityProjected = ProjectOnSuspension(SpringVelocity);
            var SpringLengthError = S.Spring.RestLength - S.Spring.CurrentLength;
            var SpringForce = -(S.Spring.Stiffness * SpringLengthError) - (S.Spring.DampValue * Vector3.Dot(SpringVelocityProjected, SpringDirection));

            // If suspension is fully compressed then we are hard hitting the ground
            var IsSpringFullyCompressed = (S.Spring.CurrentLength == S.Spring.MinLength ? -1 : 0);
            var ForceToApply = IsSpringFullyCompressed * SpringVelocityProjected *VelocityCorrectionMultiplier + SpringForce * SpringDirection;
            ForceToApply = -CurrentMinimalForcePossible + ForceToApply;
            RB.AddForceAtPosition(ForceToApply, SpringAnchor, ForceMode.VelocityChange);

            // Direction
            var WheelVelocity = GetWheelVelocity(WheelPosition);
            var ForceDirection = -Vector3.Cross(Hit.normal, Vector3.Cross(Hit.normal, S.Wheel.Direction));
            Debug.DrawLine(Hit.point, Hit.point + ForceDirection.normalized, Color.green);
            var WheelVelocityX = Vector3.Project(WheelVelocity, S.Wheel.Direction);
            var MotorVelocity = CarMotor.CurrentRPM * S.Wheel.Direction;
            var f = Vector3.Cross(transform.up, S.Wheel.Direction);
            var WheelVelocityY = Vector3.Project(WheelVelocity, f);
            var T = -WheelVelocityY + MotorVelocity;
            T *= Mathf.Pow(Vector3.Dot(transform.up, Vector3.up), 3);
            RB.AddForceAtPosition(T, SpringAnchor, ForceMode.VelocityChange);

            // NOTE toffa : This is a test to apply physics to the ground object if a rigid body existst
            var Collider = Hit.collider.GetComponent<Rigidbody>();
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
        DrawDebugWheels(Color.yellow);
    }

    void Update()
    {
        // TODO toiffa : remove this, it is only for testing the hub
        if (transform.localPosition.z < -300)
        {
            GameObject.Find("SoundManager").GetComponent<SoundManager>().SwitchClip("desert");
        }
        else
        {
            if (GameObject.Find("SoundManager").GetComponent<SoundManager>().CurrentClip.Name == "desert")
                GameObject.Find("SoundManager").GetComponent<SoundManager>().SwitchClip("theme");
        }
        // end TODO

        DrawDebugAxles(Color.blue, Color.red);
        UpdateWheelsRenderer();

        if (Input.GetKey(KeyCode.J))
        {
            // test jump
            SetSpringSizeMin();
        }

        float Y = Input.GetAxis("Vertical");
        float X = Input.GetAxis("Horizontal");

        CarMotor.CurrentRPM = Y * CarMotor.MaxTorque;

        RearAxle.Right.Wheel.Direction = transform.forward;
        RearAxle.Left.Wheel.Direction = transform.forward;
        FrontAxle.Right.Wheel.Direction = Quaternion.AngleAxis(45 * X, transform.up) * transform.forward;
        FrontAxle.Left.Wheel.Direction = Quaternion.AngleAxis(45 * X, transform.up) * transform.forward;
    }

    private void SetSpringSizeMin()
    {
        FrontAxle.Right.Spring.CurrentLength = FrontAxle.Right.Spring.MinLength;
        FrontAxle.Left.Spring.CurrentLength = FrontAxle.Left.Spring.MinLength;
        RearAxle.Right.Spring.CurrentLength = RearAxle.Right.Spring.MinLength;
        RearAxle.Left.Spring.CurrentLength = RearAxle.Left.Spring.MinLength;
    }

    public Vector2 MouseLastPosition = Vector2.zero;
    // Update is called once per frame
    void FixedUpdate()
    {
        ResolveAxle(ref FrontAxle);
        ResolveAxle(ref RearAxle);
    }
}
