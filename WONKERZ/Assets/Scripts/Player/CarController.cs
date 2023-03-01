using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using static Schnibble.SchMathf;
using static Schnibble.SchPhysics;
public class CarController : MonoBehaviour, IControllable
{
    [Header("Inertia")]
    public bool overrideInertia;
    public float linearInertia;
    public Vector3 inertiaTensor;
    public Vector3 inertiaTensorRotation;

    public GameObject centerOfMass;

    [Header("Aerodynamics")]
    public float dragCoeff;
    public float downForceCoeff;

    [Header("Steering")]
    [Range(0, 90)]
    public float maxSteeringAngle_deg;
    [Range(-20, 20)]
    public float toeAngle;
    public bool ackerman;
    public GameObject ackermanReferential;

    [Header("Drive line")]
    public Differential differential;

    [Header("Engine")]
    public float maxTorque;
    // curve between 0 and 1 (= maxTorque)
    public AnimationCurve torqueCurve;
    public float currentRPM;

#if false
      [System.Serializable]
    /// NOTE toffa : A motor has a currentRPM defining its torque
    /// It also has a MaxTorque that it cannot generat more of, and a maxbreak.
    /// The way it works is that breaking is actually applying negative torque.
    /// IMPORTANT toffa : The motor is the breaking system too!!
    public struct Engine
    {
        public float Stroke;
        public float Bore;

        public int Strokes; // 2 or 4 moswt of the time
        public float Displacement; // litres
        public int Cylinders; // V6, V8, V12, etc

        public float IdleRPM;
        public float IdleTorque;

        public float PeakRPM;
        public float PeakTorque;

        public float MaxRPM;
        public float MaxTorque;
        public float MaxPower;
        // friction
        public float BoundaryFriction; // Heywood suggest 0.97
        public float ViscousFriction; // Heywood suggest 0.15
        public float TurbulentDissipation; // Heywood suggest 0.05
        public float GetEngineFriction(float RPM)
        {
            // var AngVel = RPM * Mathf.PI / 30;
            // return BoundaryFriction + AngVel*ViscousFriction + (AngVel*AngVel)*TurbulentDissipation;

            // FMEP in bar
            var FMEP_Bar = BoundaryFriction + ViscousFriction * (RPM / 1000) + TurbulentDissipation * (RPM / 1000) * (RPM / 1000);
            // FMEP in Pascal => 1 Pa = 1 N/m2
            var FMEP_kPa = FMEP_Bar * 100;
            return ( /* * Cylinders */ Displacement * FMEP_kPa) / (2 * Mathf.PI * (Strokes / 2));

        }

        public float GetDisplacement()
        {
            if (Displacement == 0)
            {
                return 4 * (Mathf.PI / 4) * (Bore * Bore) * Stroke;
            }

            return Displacement;
        }

        public float CurrentRPM;
    }
       (Vector3, Vector3) ComputeWheelForce(ref Wheel W, WheelHitInfo HitInfo)
    {
        var WheelVelocity = W.Velocity;
        var WheelLongitudinal = Quaternion.FromToRotation(transform.up, HitInfo.Normal) * W.Direction;
        var WheelVelocityX = Vector3.Project(WheelVelocity, WheelLongitudinal);
        var WheelVelocityXSigned = Vector3.Dot(WheelLongitudinal, WheelVelocityX);

        //if (W.IsTraction)

        var WheelSpeed = CarMotor.CurrentRPM == 0f ? WheelVelocityXSigned : CarMotor.CurrentRPM;

        var WheelTransversal = Vector3.Cross(transform.up, W.Direction);
        var WheelVelocityY = Vector3.Project(WheelVelocity, WheelTransversal);
        // How much the wheel resist to being turned, meaning you need a certain force to actually move the wheel.
        var RollingResistance = -0.01f;
        // Friction of the wheel
        // Slip ratio is the ratio in longitutinal forces : how much the wheel is trying to turn, against how much it is travelling at
        var LongitudinalSlip = (WheelSpeed - WheelVelocityX.magnitude);
        var SlipRatio = LongitudinalSlip <= 0 ? LongitudinalSlip / WheelVelocityX.magnitude : LongitudinalSlip / WheelSpeed;
        W.Slip.x = SlipRatio;
        // slipangle is cloisely related to SlipRatio, but instead mesure how much of an angle there is between the wheel forward plan and the travelling direction
        var WheelDirectionInUpPlane = Vector3.ProjectOnPlane(W.Direction, transform.up);
        var WheelVelocityInUpPlane = Vector3.ProjectOnPlane(WheelVelocity, transform.up);
        var SlipAngle = Vector3.SignedAngle(Vector3.ProjectOnPlane(W.Direction, transform.up), Vector3.ProjectOnPlane(WheelVelocity, transform.up), transform.up);
        W.Slip.y = SlipAngle;
        // Longitudinal friction is not related to velocity (actually it is a little bit, the slippage rate for which MuMax is reached decreases as the velocity increases) but to the deceleration : this is how much is affected by breaking
        // We approximate mu accroding to the slipping ratio as two linear motion between mu0, muMax and then muSlip
        var SlipRatioMuMax = 0.2f;
        var muMax = 1;
        var muBlocked = 0.5f;
        var SlipRatioAbs = Mathf.Abs(SlipRatio);
        var mu = SlipRatioAbs < SlipRatioMuMax ? Mathf.Lerp(RollingResistance, muMax, Mathf.Clamp01(SlipRatioAbs / SlipRatioMuMax)) : Mathf.Lerp(muMax, muBlocked, Mathf.Clamp01(SlipRatioAbs - SlipRatioMuMax));
        mu *= Mathf.Sign(SlipRatio);
        var Fx = mu;
        // Transversal friction is heavily linked to the weight applied on the wheel. For now we dont take this into account.
        // We will mimick this by using transversal velocity as weight. The faster you go the harder it is to turn hard.
        var tauMax = muMax;
        var tauBlocked = muBlocked;
        var SlipAngleAbs = Mathf.Abs(SlipAngle / 180f);
        // for now tau = mu
        var tau = SlipAngleAbs < SlipRatioMuMax ? Mathf.Lerp(RollingResistance, tauMax, Mathf.Clamp01(SlipAngleAbs / SlipRatioMuMax)) : Mathf.Lerp(muMax, tauBlocked, Mathf.Clamp01(SlipAngleAbs - SlipRatioMuMax));
        var SlipRatioY = (WheelVelocityX.magnitude / WheelSpeed) * Mathf.Tan(SlipAngle * Mathf.Deg2Rad);
        var TotalGrip = Mathf.Sqrt(SlipRatio * SlipRatio + SlipRatioY * SlipRatioY);
        tau *= Mathf.Sign(SlipAngle);
        // trasnversal friction drops dramatically when slip ratio increases
        var Fy = tau * (1 - Mathf.Abs(SlipRatio));

        var Vx = WheelSpeed * Fx;
        var Vy = -WheelVelocityY.magnitude * Fy;

        var ForceX = Vx * WheelLongitudinal;
        var ForceY = Vy * WheelTransversal;
        ForceX *= Mathf.Pow(Mathf.Clamp01(Vector3.Dot(HitInfo.Normal, Vector3.up)), 3);
        ForceY *= Mathf.Pow(Mathf.Clamp01(Vector3.Dot(HitInfo.Normal, Vector3.up)), 3);

        var ValidationValueX = ValidateForce(ForceX);
        if (!ValidationValueX.Item2)
        {
            this.Log("FAILED TO VALIDATE FORCE");
        }

        var ValidationValueY = ValidateForce(ForceY);
        if (!ValidationValueY.Item2)
        {
            this.Log("FAILED TO VALIDATE FORCE");
        }

        return (ValidationValueX.Item1, ValidationValueY.Item1);
    }
#endif

    [Header("Brakes")]
    public float maxBreak;
    public AnimationCurve breakCurve;
    public float handbreakTorque;
    [Range(0, 1)]
    public float handbreakBias;

    [Header("Axles")]
    public List<Axle> axles;

    private ContactResolver wheelContactsResolver = new ContactResolver();

    [Header("Effects")]
    public GameObject trailsMark;
    /// ================== Variables ===================
    [System.Serializable]
    public struct SpeedTrailEffect
    {
        public GameObject particles;
        [Range(0, 1)]
        public float threshold;
    }
    public SpeedTrailEffect speedEffect;
    public Vector3 repulseForce;
    [Header("Prefab Refs(MAND)")]
    public GameObject onDeathClone;

    [System.Serializable]
    public struct Jump
    {
        public bool isStarting;
        public bool isJumping;
        public bool suspensionLock;
        public bool applyForceMultiplier;
        public float value;
    }
    [Header("Jump")]
    public Jump jump;

    [System.Serializable]
    public struct Turbo
    {
        public bool infinite;
        public float strength;
        public float timeInterval;
        public float intervalElapsedTime;
        public float consumptionPerTick;
        public float current;
        public float max;
        public Turbo(float strength
                     , float timeInterval
                     , float intervalElapsedTime
                     , float consumptionPerTick
                     , bool infinite
                     , float current
                     , float max)
        {
            this.max = max;
            this.current = current;
            this.infinite = infinite;
            this.strength = strength;
            this.timeInterval = timeInterval;
            this.intervalElapsedTime = intervalElapsedTime;
            this.consumptionPerTick = consumptionPerTick;
        }
    }
    [Header("Turbo")]
    public Turbo turbo = new Turbo(5, .2f, 99, .02f, false, 0, 1);

    [Header("Grappin")]
    public bool IsHooked;
    // TODO toffa : remove this hardcoded object
    public GameObject grapin;

    /// States ==========================
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

    /// Cache ============================
    public Rigidbody rb;
    private bool modifierCalled = false;
    public bool fixedUpdateDone = false;

    [Header("Debug")]
    public float springStiffness;
    public float springDamper;
    public float springMax;
    public float springMin;
    public float springRestPercent;
    public float wheelRadius;
    public float wheelWidth;
    public bool overrideMaxSpring;
    public AnimationCurve friction;
    public UIGraphView debugTorque;

    /// =================== Updates =================
    void UpdateSprings()
    {
        if (overrideMaxSpring) return;
        foreach (var axle in axles)
        {
            axle.left.suspension.SetSpringValues(springMin, springMax, springRestPercent, springStiffness, springDamper);
            axle.right.suspension.SetSpringValues(springMin, springMax, springRestPercent, springStiffness, springDamper);
        }
    }

    /// ================== Draw =================
    void DrawDebug(Color C, Color C2)
    {
        foreach (var axle in axles)
            axle.DrawDebug(C, C2);
    }

    /// =============== Resolve =============

    void ComputeWheelLoad()
    {
        var frontAxle = axles[(int)AxleType.front];
        var rearAxle = axles[(int)AxleType.rear];
        //for now lets split by 2 in front and rear
        // note : rear length is negative
        var frameLength = frontAxle.length + Mathf.Abs(rearAxle.length);

        var gravity = UnityEngine.Physics.gravity;

        var totalMass = rb.mass;
        var totalWeight = totalMass * gravity;

        // compute diff in X with center of mass
        var front_rear_left = frontAxle.left.centerPosition - rearAxle.left.centerPosition;
        var front_rear_right = frontAxle.right.centerPosition - rearAxle.right.centerPosition;
        var com_projected_front_left = Vector3.Project(rb.worldCenterOfMass - (frontAxle.left.centerPosition), front_rear_left).magnitude / front_rear_left.magnitude;
        var com_projected_rear_left = Vector3.Project(rb.worldCenterOfMass - (rearAxle.left.centerPosition), front_rear_left).magnitude / front_rear_left.magnitude;

        var com_projected_front_right = Vector3.Project(rb.worldCenterOfMass - (frontAxle.right.centerPosition), front_rear_left).magnitude / front_rear_left.magnitude;
        var com_projected_rear_right = Vector3.Project(rb.worldCenterOfMass - (rearAxle.right.centerPosition), front_rear_left).magnitude / front_rear_left.magnitude;

        var front_left_right = (frontAxle.left.centerPosition) - (frontAxle.right.centerPosition);
        var com_projected_x_front_right = Vector3.Project(rb.worldCenterOfMass - (frontAxle.left.centerPosition), front_left_right).magnitude / front_left_right.magnitude;
        var com_projected_x_front_left = Vector3.Project(rb.worldCenterOfMass - (frontAxle.right.centerPosition), front_left_right).magnitude / front_left_right.magnitude;

        var rear_left_right = (rearAxle.left.centerPosition) - (rearAxle.right.centerPosition);
        var com_projected_x_rear_right = Vector3.Project(rb.worldCenterOfMass - (rearAxle.left.centerPosition), rear_left_right).magnitude / rear_left_right.magnitude;
        var com_projected_x_rear_left = Vector3.Project(rb.worldCenterOfMass - (rearAxle.right.centerPosition), rear_left_right).magnitude / rear_left_right.magnitude;

        var front_left = com_projected_front_left * 0.25f + com_projected_x_front_left * 0.25f;
        var front_right = com_projected_front_right * 0.25f + com_projected_x_front_right * 0.25f;
        var rear_left = com_projected_rear_left * 0.25f + com_projected_x_rear_left * 0.25f;
        var rear_right = com_projected_rear_right * 0.25f + com_projected_x_rear_right * 0.25f;

        frontAxle.left.load = front_left;
        frontAxle.right.load = front_right;
        rearAxle.right.load = rear_right;
        rearAxle.left.load = rear_left;
    }

    void ApplyAllWheelConstraints()
    {
        //this.Log("ApplyAllWheelConstraints START : vel : " + rb.velocity + "   angVel : " + rb.angularVelocity + "   pos : " + rb.position + "   rot: " + rb.rotation);

        // first compute everything that we might need to resolve collisions
        wheelContactsResolver.contacts.Clear();
        foreach (var axle in axles)
        {
            axle.right.GetContacts(wheelContactsResolver.contacts);
            axle.left.GetContacts(wheelContactsResolver.contacts);
        }

        if (wheelContactsResolver.contacts.Count > 0)
        {
            wheelContactsResolver.SolvePenetrations(30);
            wheelContactsResolver.SolveCollisions(30);
            ComputeWheelLoad();
            wheelContactsResolver.SolveWheels();
        }
    }

    void ApplyGroundForces(Suspension S, ContactInfo HitInfo)
    {
        // tode toffa : redo this function => should be dealt with in the contact resolver!
#if false
        // NOTE toffa : This is a test to apply physics to the ground object if a rigid body existst
        var SpringAnchor = S.Spring.AnchorPosition.transform.position;
            var VelocityGravity = Vector3.Project(GetWheelVelocity(SpringAnchor), Vector3.up);
            VelocityGravity.y += Physics.gravity.y;
            if (VelocityGravity.y < 0)
                Collider.AddForceAtPosition(VelocityGravity * RB.mass, HitInfo.ContactPoint, ForceMode.Force);

        // NOTE toffa :
        // Add current ground velocity to the RB to be able to sit still on moving plateform for instance.
        var RBVelProjected = Vector3.Project(RB.velocity, HitInfo.ContactGround.Velocity);
        var VelDiff = Vector3.Scale((HitInfo.ContactGround.Velocity - RBVelProjected), new Vector3(1, 0, 1));
        var ValidationValue = ValidateForce(VelDiff);
        if (!ValidationValue.Item2)
        {
            this.Log("FAILED TO VALIDATE FORCE");
        }
        RB.AddForce(ValidationValue.Item1 / 4, ForceMode.VelocityChange);
#endif
    }


    /// =================== Mecanics ===================

    public void TryJump()
    {
        if (jump.applyForceMultiplier)
        {
            foreach (var axle in axles)
            {
                rb.AddForceAtPosition(jump.value * transform.up * (axle.right.isGrounded ? 1 : 0), axle.right.suspension.spring.loadPosition, ForceMode.VelocityChange);
                rb.AddForceAtPosition(jump.value * transform.up * (axle.right.isGrounded ? 1 : 0), axle.left.suspension.spring.loadPosition, ForceMode.VelocityChange);
            }
            jump.applyForceMultiplier = false;
        }
    }
    private Ground.EType currentGround = Ground.EType.NONE;

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
            this.Log("GroundState");
#if false
            player.FrontAxle.IsTraction = false;
            player.FrontAxle.IsDirection = true;
            player.FrontAxle.IsReversedDirection = false;

            player.RearAxle.IsDirection = false;
            player.RearAxle.IsTraction = true;
            player.RearAxle.IsReversedDirection = true;
#endif
        }
    }

    public class PlayerBoatState : FSMState
    {
        public CarController player;
        public override void OnEnter(FSMBase machine)
        {
            base.OnEnter(machine);
#if false
            player.FrontAxle.IsTraction = false;
            player.FrontAxle.IsDirection = false;
            player.FrontAxle.IsReversedDirection = false;

            player.RearAxle.IsDirection = true;
            player.RearAxle.IsTraction = true;
            player.RearAxle.IsReversedDirection = true;
#endif
        }
    }

    public class PlayerAircraftState : FSMState
    {
        public CarController player;
        public override void OnEnter(FSMBase machine)
        {
            base.OnEnter(machine);
#if false
            player.FrontAxle.IsTraction = true;
            player.FrontAxle.IsDirection = true;
            player.FrontAxle.IsReversedDirection = false;

            player.RearAxle.IsDirection = true;
            player.RearAxle.IsTraction = true;
            player.RearAxle.IsReversedDirection = true;
#endif
        }
    }

    public class PlayerBallState : FSMState
    {
        public CarController player;
        public override void OnEnter(FSMBase machine)
        {
            base.OnEnter(machine);
            this.Log(null, "BallState");
#if false
            player.FrontAxle.IsTraction = false;
            player.FrontAxle.IsDirection = false;
            player.FrontAxle.IsReversedDirection = false;

            player.RearAxle.IsDirection = false;
            player.RearAxle.IsTraction = false;
            player.RearAxle.IsReversedDirection = false;
#endif
        }
    }

    public class PlayerSpiderState : FSMState
    {
        public CarController player;
        public override void OnEnter(FSMBase machine)
        {
            base.OnEnter(machine);

#if false
            player.FrontAxle.IsTraction = false;
            player.FrontAxle.IsDirection = false;
            player.FrontAxle.IsReversedDirection = false;

            player.RearAxle.IsDirection = false;
            player.RearAxle.IsTraction = false;
            player.RearAxle.IsReversedDirection = false;
#endif
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
        foreach (var axle in axles)
        {
            axle.right.suspension.spring.SetLengthSettings(springMin, springMax, springRestPercent);
            axle.left.suspension.spring.SetLengthSettings(springMin, springMax, springRestPercent);
        }
        overrideMaxSpring = false;
    }

    private void SetSpringSizeMinAndLock()
    {
        foreach (var axle in axles)
        {
            axle.right.suspension.spring.SetLengthSettings(springMin, springMin + 0.1f, springRestPercent);
            axle.left.suspension.spring.SetLengthSettings(springMin, springMin + 0.1f, springRestPercent);
        }
        overrideMaxSpring = true;
    }

    private void clonedForTestInit()
    {
        rb.centerOfMass = centerOfMass.transform.localPosition;
    }

    public Vector2 MouseLastPosition = Vector2.zero;
    // Update is called once per frame

    public float GetCurrentSpeed()
    {
        if (!rb)
            return 0f;
        var PlayerVelocity = rb.velocity;
        var PlayerForward = rb.transform.forward;
        var currentSpeed = new Vector3(PlayerVelocity.x * PlayerForward.x, PlayerVelocity.y * PlayerForward.y, PlayerVelocity.z * PlayerForward.z).magnitude;
        return currentSpeed;
    }

    /// =============== Game Logic ==================
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
        rb.AddForce(repulseForce, ForceMode.Impulse);

        stateMachine.ForceState(invulState);
    }

    public void useTurbo()
    {
        turbo.intervalElapsedTime += Time.deltaTime;
        if (turbo.intervalElapsedTime < turbo.timeInterval)
            return;

        var nextTurboValue = turbo.current - (turbo.infinite ? 0 : turbo.consumptionPerTick);
        if (nextTurboValue < 0)
            return;

        turbo.current = Mathf.Clamp(0, turbo.max, nextTurboValue);

        Access.UITurboAndLifePool().updateTurboBar(turbo.current);

        Vector3 turboDir = transform.forward.normalized;
        Debug.DrawRay(transform.position, turboDir, Color.yellow, 4, false);
        rb.AddForce(turboDir * turbo.strength, ForceMode.VelocityChange);

        turbo.intervalElapsedTime = 0f;
    }

    public void kill(Vector3 iSteer = default(Vector3))
    {
        GameObject dummy_player = Instantiate(onDeathClone, transform.position, transform.rotation);
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
        PowerController PC = GetComponent<PowerController>();
        if (!!PC && PC.isInNeutralPowerMode())
        {
            jump.isStarting = false;
            jump.isJumping = false;
            return false;
        }

        // Jump Start over when 4wheels touching ground
        if (jump.isStarting)
        {
            foreach (Axle a in axles)
            {
                if (a.left.isGrounded && a.right.isGrounded)
                {
                    this.Log("Still starting to jump");
                    return false; // early return
                }
            }
            // no more wheels grounded
            jump.isStarting = false;
            jump.isJumping = true;
            return false;
        }
        else if (jump.isJumping)
        { // In actual Jump airtime
            foreach (Axle a in axles)
            {
                if (a.left.isGrounded || a.right.isGrounded)
                {
                    this.Log("IsInJump over : " + a);
                    jump.isJumping = false;
                    return false;
                }
            }
            return true;
        }
        jump.isStarting = false;
        jump.isJumping = false;
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
            //this.Log(machine.currentState.name);
        }
    }

    public class PlayerDeadAction : FSMAction
    {
        public override void Execute(FSMBase machine)
        {
            base.Execute(machine);
            // player alive logic
            this.Log("dead");
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
            this.Log("die");
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
            foreach (var axle in player.axles)
            {
                axle.right.UpdatePositions();
                axle.right.UpdateRenderers();
                axle.left.UpdatePositions();
                axle.left.UpdateRenderers(true);
            }
        }
    }

    public class PlayerSpeedEffect : FSMAction
    {
        public CarController player;
        public override void Execute(FSMBase machine)
        {
            base.Execute(machine);
            // Speed effect on camera
            // update camera FOV/DIST if a PlayerCamera
            CameraManager CamMgr = Access.CameraManager();
            if (CamMgr?.active_camera is PlayerCamera)
            {
                PlayerCamera pc = (PlayerCamera)CamMgr.active_camera;
                pc.applySpeedEffect(player.GetCurrentSpeed() / player.maxTorque);
            }

            var SpeedDirection = player.rb.velocity;
            var particules = player.speedEffect.particles.GetComponent<ParticleSystem>();
            if (SpeedDirection.magnitude / player.maxTorque > player.speedEffect.threshold)
            {
                var e = particules.emission;
                e.enabled = true;
            }
            else
            {
                var e = particules.emission;
                e.enabled = false;
            }

            particules.transform.LookAt(player.transform.position + SpeedDirection);
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

            // Ball Power UC : no physx simulation from car
            if (player.CurrentMode == CarController.CarMode.BALL)
            {
                this.LogWarn("Player is ball : no physx update");
                return;
            }

            player.UpdateSprings();
            player.ApplyAllWheelConstraints();
            player.TryJump();
        }
    }

    public class PlayerInPowerMode : FSMAction
    {
        public CarController player;
        public override void Execute(FSMBase machine)
        {
            base.Execute(machine);
            PowerController pc = player.gameObject.GetComponent<PowerController>();
            if (!!pc)
            { pc.refreshPower(); }
        }
    }

    private void CreateFSM()
    {
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

        PlayerInPowerMode playerInPowerModeAction = new PlayerInPowerMode();
        playerInPowerModeAction.player = this;

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
        boatState.fixedActions.Add(playerInPowerModeAction);

        aircraftState.name = "aircraft";
        aircraftState.player = this;
        aircraftState.transitions = aliveState.transitions;
        aircraftState.actions = aliveState.actions;
        aircraftState.fixedActions = aliveState.fixedActions;
        aircraftState.fixedActions.Add(playerInPowerModeAction);

        ballState.name = "ball";
        ballState.player = this;
        ballState.transitions = aliveState.transitions;
        ballState.actions = aliveState.actions;
        ballState.fixedActions = aliveState.fixedActions;
        ballState.fixedActions.Add(playerInPowerModeAction);

        spiderState.name = "spider";
        spiderState.player = this;
        spiderState.transitions = aliveState.transitions;
        spiderState.actions = aliveState.actions;
        spiderState.fixedActions = aliveState.fixedActions;
        spiderState.fixedActions.Add(playerInPowerModeAction);

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
    }

    void Awake()
    {
        if (stateMachine.currentState == deadState)
            return;

        CreateFSM();

        int i = 0;
        foreach (var axle in axles)
        {
            axle.SetSuspensionsLoadPosition();

            axle.right.localToWorld = GetBasis(transform.up, transform.forward);
            axle.left.localToWorld = GetBasis(transform.up, transform.forward);

            axle.right.axle = (AxleType)i;
            axle.right.width = wheelWidth;
            axle.right.radius = wheelRadius;
            axle.right.frictionCurve = friction;
            axle.right.Init();
            axle.left.axle = (AxleType)i;
            axle.left.width = wheelWidth;
            axle.left.radius = wheelRadius;
            axle.left.frictionCurve = friction;
            axle.left.Init();
            i++;
        }

        wheelContactsResolver.axles = axles;
        differential.axles = axles;

        // Avoid instantiating again wheels and such
        // when the Player is duplicated for garage tests
        // > We could also just remove certain awake calls
        // but i want to check that wit u toff
        if (!!transform.parent && transform.parent.GetComponent<UIGarageTestManager>())
        { clonedForTestInit(); return; }

        rb.centerOfMass = centerOfMass.transform.localPosition;
        if (overrideInertia)
        {
            rb.inertiaTensorRotation = Quaternion.Euler(inertiaTensorRotation);
            rb.inertiaTensor = inertiaTensor;
            rb.mass = linearInertia;
        }

        SetMode(CurrentMode);

        Utils.attachControllable<CarController>(this);
    }

    void Update()
    {
        fixedUpdateDone = false;

        stateMachine.Update();
        if (DebugVars.debugAxle)
        {
            foreach (var axle in axles)
            {
                axle.right.debugSlipX.gameObject.SetActive(true);
                axle.right.debugSlipY.gameObject.SetActive(true);
                axle.left.debugSlipX.gameObject.SetActive(true);
                axle.left.debugSlipY.gameObject.SetActive(true);
            }
            DrawDebug(Color.blue, Color.red);
        }
        else
            foreach (var axle in axles)
            {
                axle.right.debugSlipX.gameObject.SetActive(false);
                axle.right.debugSlipY.gameObject.SetActive(false);
                axle.left.debugSlipX.gameObject.SetActive(false);
                axle.left.debugSlipY.gameObject.SetActive(false);
            }
        debugTorque?.SetCurve(torqueCurve);

#if false
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
#endif

    }

    void FixedUpdate()
    {
        stateMachine.FixedUpdate();

#if false
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
            var VProj = AddGravityStep(rb.velocity);
            var VProjX = Vector3.Project(VProj, -HookDirX);
            var VProjZ = Vector3.Project(VProj, HookDirZ);
            this.Log("Hooked");
            rb.velocity = VProjX + VProjZ;
        }

#endif

        fixedUpdateDone = true;
        jump.applyForceMultiplier = false;
    }

    /// ================ Controls ===================

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        if (stateMachine.currentState == deadState)
            return;

        var turn = Entry.Inputs["Turn"].AxisValue;
        var acceleration = Entry.Inputs["Accelerator"].AxisValue;
        // todo toffa : this is very weird...check this at some point
        currentRPM = acceleration;

        // power controller update
        PowerController pc = GetComponent<PowerController>();
        if (!!pc)
        {
            // modifier inputs
            if (Entry.Inputs["Modifier"].Down)
            {
                Vector2 mouse_mod = new Vector2(Entry.Inputs["Power_MouseX"].AxisValue, Entry.Inputs["Power_MouseY"].AxisValue);
                pc.showUI(true);
                if (Entry.Inputs["Power1"].Down || (mouse_mod.x > 0))
                { // BallPower
                    pc.showIndicator(PowerController.PowerWheelPlacement.LEFT);
                    pc.setNextPower(1);
                }
                else if (Entry.Inputs["Power2"].Down || (mouse_mod.y > 0))
                { // WaterPower
                    pc.setNextPower(2);
                    pc.showIndicator(PowerController.PowerWheelPlacement.DOWN);
                }
                else if (Entry.Inputs["Power3"].Down || (mouse_mod.y < 0))
                { // PlanePower
                    pc.showIndicator(PowerController.PowerWheelPlacement.UP);
                    pc.setNextPower(3);
                }
                else if (Entry.Inputs["Power4"].Down || (mouse_mod.x < 0))
                { // SpiderPower
                    pc.showIndicator(PowerController.PowerWheelPlacement.RIGHT);
                    pc.setNextPower(4);
                }
                else
                {
                    pc.showIndicator(PowerController.PowerWheelPlacement.NEUTRAL);
                    pc.setNextPower(0);
                }
                modifierCalled = true;
            }
            else if (modifierCalled && !Entry.Inputs["Modifier"].Down)
            {
                pc.hideIndicators();
                pc.showUI(false);

                pc.tryTriggerPower();
                modifierCalled = false;
            }

            pc.applyPowerEffectInInputs(Entry, this);
        }

        foreach (var axle in axles)
        {
            if (axle.isSteerable)
            {
                axle.right.basis = GetBasis(transform.up,
                                                       Quaternion.AngleAxis(maxSteeringAngle_deg * turn, transform.up) * transform.forward);
            }
            else
            {
                axle.right.basis = GetBasis(transform.up, transform.forward);
            }
            axle.left.basis = axle.right.basis;
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
                jump.applyForceMultiplier = true;
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
