using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.SceneManagement;
using Schnibble;
using Schnibble.Managers;
using static Schnibble.Math;
using static Schnibble.Physics;
using static Schnibble.Utils;
using static UnityEngine.Debug;

public class AirplaneState : FSMState, IControllable 
{
    private PlayerController player;
    private class GeneralAirAction : GroundState.GeneralGroundAction {}

    public override void OnEnter(FSMBase fsm) {
        var player = (fsm as PlayerFSM  ).GetPlayer();
        player.ActivateAirplane();
    }
    public override void  OnExit(FSMBase fsm) {
        var player = (fsm as PlayerFSM).GetPlayer();
        player.DeactivateAirplane();
    }
    void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry) {
        var airplaneMode = Entry.Get((int)PlayerInputs.InputCode.AirplaneMode) as GameInputButton;
        if (airplaneMode != null) {
            if (airplaneMode.GetState().down) {
                player.vehicleStates.SetState(player.vehicleStates.states[(int)PlayerVehicleStates.States.Car]);
            }
        }

        var weightXAxis = Entry.Get((int)PlayerInputs.InputCode.WeightX) as GameInputAxis;
        var weightYAxis = Entry.Get((int)PlayerInputs.InputCode.WeightY) as GameInputAxis;
        if (weightXAxis != null)
        {
            player.jump.diRollUnscaled.Add(weightXAxis.GetState().valueSmooth);
            player.jump.diRoll.Add(weightXAxis.GetState().valueSmooth); //* Time.deltaTime;
        }
        if (weightYAxis != null)
        {
            player.jump.diPitchUnscaled.Add(weightYAxis.GetState().valueSmooth);
            player.jump.diPitch.Add(weightYAxis.GetState().valueSmooth); //* Time.deltaTime;
        }
    }

    public AirplaneState(PlayerController player) : base("Airplane")
    {
        this.player = player;
        this.fixedActions.Add(new GeneralAirAction());
    }
}

// FSM when in ground mode
public class GroundState : FSMState, IControllable
{
    private PlayerController player;

    public class GeneralGroundAction : FSMAction
    {
        public override void Execute(FSMBase fsm)
        {
            var player = (fsm as PlayerFSM).GetPlayer();

            player.TryJump();
            player.flags[PlayerController.FJump] = !player.TouchGround();
            // apply jump correction

            // IMPORTANT toffa : sometimes fixedUpdate could be called multiple times if the framerate dips.
            // but it would take a value of 0 that is wrong because we reset at the end of the first fixedUpdate frame.
            if (player.jump.diPitchUnscaled.count != 0 && player.jump.diRollUnscaled.count != 0)
            {
                if (!player.TouchGround())
                {
                    var torque = new Vector3(player.jump.diMaxForce * player.jump.diPitchUnscaled.average,
                        0,
                        -player.jump.diMaxForce * player.jump.diRollUnscaled.average);
                    torque = player.car.transform.TransformDirection(torque);
                    player.car.rb.AddTorque(torque * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
                else
                {
                    player.SetCarCenterOfMass();
                }

                // very bad design for now.
                var weightIndPos = player.car.centerOfMassInitial + new Vector3(player.jump.diRollUnscaled.average * player.weightControlMaxX,
                    player.weightIndicatorHeight,
                    player.jump.diPitchUnscaled.average * player.weightControlMaxZ);

                player.weightIndicator.transform.position = player.car.centerOfMass.transform.parent.TransformPoint(weightIndPos);
            }
        }
    }

    private class UpdateWheelBasis : FSMAction
    {
        public override void Execute(FSMBase fsm)
        {
            var player = (fsm as PlayerFSM).GetPlayer();

            foreach (var axle in player.car.axles)
            {
                if (axle.isSteerable)
                {
                    axle.right.basis = GetBasis(player.car.transform.up,
                        Quaternion.AngleAxis(player.car.maxSteeringAngle_deg * player.turn.average, player.car.transform.up) * player.car.transform.forward);
                }
                else
                {
                    axle.right.basis = GetBasis(player.car.transform.up, player.car.transform.forward);
                }
                axle.left.basis = axle.right.basis;
            }
        }
    }

    private class SpeedEffect : FSMAction
    {
        public override void Execute(FSMBase fsm){
            var player = (fsm as PlayerFSM).GetPlayer();
            var playerCar = player.car;
            // Speed effect on camera
            // update camera FOV/DIST if a PlayerCamera
            CameraManager CamMgr = Access.CameraManager();
            if (CamMgr?.active_camera is PlayerCamera)
            {
                PlayerCamera pc = (PlayerCamera)CamMgr.active_camera;
                pc.applySpeedEffect(playerCar.GetCurrentSpeed() / playerCar.maxTorque);
            }

            var SpeedDirection = playerCar.rb.velocity;
            var particules = playerCar.speedEffect.particles.GetComponent<ParticleSystem>();
            if (SpeedDirection.magnitude / playerCar.maxTorque > playerCar.speedEffect.threshold)
            {
                var e = particules.emission;
                e.enabled = true;
            }
            else
            {
                var e = particules.emission;
                e.enabled = false;
            }

            particules.transform.LookAt(playerCar.transform.position + SpeedDirection);
            var lifemin = 0.2f;
            var lifemax = 0.6f;
            var speedmin = 20f;
            var speedmax = 100f;
            var partmain = particules.main;
            partmain.startLifetime = Mathf.Lerp(lifemin, lifemax, Mathf.Clamp01((SpeedDirection.magnitude - speedmin) / (speedmax - speedmin)));
        }
    }

    public GroundState(PlayerController player) : base("Car")
    {
        this.player = player;
        this.actions.Add(new UpdateWheelBasis());
        this.actions.Add(new SpeedEffect());
        this.fixedActions.Add(new GeneralGroundAction());
    }

    public override void OnEnter(FSMBase fsm)
    {
        base.OnEnter(fsm);
        (fsm as PlayerFSM).GetPlayer().ActivateCar();
    }

    public override void OnExit(FSMBase fsm)
    {
        base.OnExit(fsm);
        (fsm as PlayerFSM).GetPlayer().DeactivateCar();
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        var jumpButton = Entry.Get((int)PlayerInputs.InputCode.Jump) as GameInputButton;
        if (jumpButton != null)
        {
            var jumpButtonState = jumpButton.GetState();
            if (jumpButtonState.heldDown)
            {
                player.SetSpringSizeMinAndLock();
            }
            else
            {
                if (jumpButtonState.up)
                {
                    player.jump.applyForceMultiplier = true;
                }
                player.ResetSpringSizeMinAndUnlock();
            }
        }

        var turboButton = Entry.Get((int)PlayerInputs.InputCode.Turbo) as GameInputButton;
        if (turboButton != null)
        {
            if (turboButton.GetState().heldDown)
            {
                player.useTurbo();
            }
        }

        var handbrakeButton = Entry.Get((int)PlayerInputs.InputCode.Handbrake) as GameInputButton;
        if (handbrakeButton != null)
        {
            if (handbrakeButton.GetState().heldDown)
            {
                player.SetHandbrake(true);
            }
        }

        // makes car torque control a power
        //var weightControlButton = (Entry.Get((int)PlayerInputs.InputCode.WeightControl) as GameInputButton);
        //if (weightControlButton != null)
        {
            //if (weightControlButton.GetState().heldDown)
            {
                var weightXAxis = Entry.Get((int)PlayerInputs.InputCode.WeightX) as GameInputAxis;
                var weightYAxis = Entry.Get((int)PlayerInputs.InputCode.WeightY) as GameInputAxis;

                if (weightXAxis != null)
                {
                    player.jump.diRollUnscaled.Add(weightXAxis.GetState().valueSmooth);
                    player.jump.diRoll.Add(weightXAxis.GetState().valueSmooth); //* Time.deltaTime;
                }

                if (weightYAxis != null)
                {
                    player.jump.diPitchUnscaled.Add(weightYAxis.GetState().valueSmooth);
                    player.jump.diPitch.Add(weightYAxis.GetState().valueSmooth); //* Time.deltaTime;
                }
            }


        }

        var accelerationAxis = Entry.Get((int)PlayerInputs.InputCode.Accelerator) as GameInputAxis;
        if (accelerationAxis != null)
        {
            var acceleration = Mathf.Clamp01(accelerationAxis.GetState().valueSmooth);
            if (acceleration != 0f)
            player.car.currentRPM.Add(acceleration);
        }

        var breakAxis = Entry.Get((int)PlayerInputs.InputCode.Break) as GameInputAxis;
        if (breakAxis != null)
        {
            var breaks = Mathf.Clamp01(breakAxis.GetState().valueSmooth);
            if (breaks != 0f)
            player.car.currentRPM.Add(-breaks);
        }
        // todo toffa : this is very weird...check this at some point

        var turnAxis = Entry.Get((int)PlayerInputs.InputCode.Turn) as GameInputAxis;
        if (turnAxis != null)
        {
            if (turnAxis.GetState().valueSmooth != 0f)
            player.turn.Add(turnAxis.GetState().valueSmooth);
        }

        // powers
        // spin
        if (!player.IsInMenu())
        {
            var spinAtk = Entry.Get((int)PlayerInputs.InputCode.SpinAttack) as GameInputButton;
            if (spinAtk!=null)
            {
                if (spinAtk.GetState().down)
                {
                    player.self_PowerController.setNextPower(1);
                    player.self_PowerController.tryTriggerPower();
                }
            }
        }

        // paraglider
        if (!player.IsInMenu())
        {
            var airplaneMode = Entry.Get((int)PlayerInputs.InputCode.AirplaneMode) as GameInputButton;
            if (airplaneMode != null) {
                if (airplaneMode.GetState().down) {
                    player.vehicleStates.SetState(player.vehicleStates.states[(int)PlayerVehicleStates.States.Plane]);
                }
            }
        }

    }
};

public class PlayerFSM : FSMBase
{
    protected PlayerController player;

    public PlayerFSM(PlayerController player) : base("PlayerFSM")
    {
        this.player = player;
    }

    public PlayerController GetPlayer() { return player; }
}

public class PlayerVehicleStates : PlayerFSM
{

    public PlayerVehicleStates(PlayerController p) : base(p) { CreateFSM(); }

    // this is used to commonly refers to some states
    // for instance setting the "Frozen" state, might be the one from
    // the current FSM, or maybe from the currentState FSM, etc
    // As sometimes we need to force a certain state it is better to have a
    // common enum too. We dont need to know what exact state object "Init" refers too.
    public enum States
    {
        // common
        Init,
        Car,
        Boat,
        Plane,
        Spider,
        Count,
    };
    public FSMState[] states = new FSMState[(int)States.Count];

    public void CreateFSM()
    {
        states[(int)States.Init] = new FSMState("Init");
        states[(int)States.Car] = new GroundState(player);
        states[(int)States.Plane] = new AirplaneState(player);

        var initState = states[(int)States.Init];
        var carState = states[(int)States.Car];
        var aitplaneState = states[(int)States.Plane];

        globalTransitions.Add(new FSMNullTransition(initState));

        //initState.transitions.Add(new GroundTransition(carState));

        ForceState(initState);
    }

}

public class PlayerGeneralStates : PlayerFSM
{
    // this is used to commonly refers to some states
    // for instance setting the "Frozen" state, might be the one from
    // the current FSM, or maybe from the currentState FSM, etc
    // As sometimes we need to force a certain state it is better to have a
    // common enum too. We dont need to know what exact state object "Init" refers too.
    public enum States
    {
        // common
        Init,
        Idle,
        Alive,
        Dead,
        Frozen,
        InMenu,
        Count,
    };
    public FSMState[] states = new FSMState[(int)States.Count];

    public PlayerGeneralStates(PlayerController player) : base(player) { CreateFSM(); }

    private class PlayerAliveCondition : FSMCondition
    {
        public override bool Check(FSMBase fsm)
        {
            return (fsm as PlayerFSM).GetPlayer().IsAlive();
        }
    }

    private class PlayerInMenuCondition : FSMCondition
    {
        public override bool Check(FSMBase fsm)
        {
            return (fsm as PlayerFSM).GetPlayer().IsInMenu();
        }
    }

    public class DieTransition : FSMTransition
    {
        public DieTransition(FSMCondition condition, FSMState tstate, FSMState fstate) : base(condition, tstate, fstate) { }

        public override void OnTransition(FSMBase fsm, FSMState toState)
        {
            (fsm as PlayerFSM).GetPlayer().Kill();

            Access.CheckPointManager().loadLastCP(true);

            base.OnTransition(fsm, toState);
        }
    }

    private void CreateFSM()
    {
        // init states
        states[(int)States.Init] = new FSMState("Init");
        states[(int)States.Alive] = new FSMState("Alive");
        states[(int)States.Dead] = new FSMState("Dead");
        states[(int)States.Frozen] = new FSMState("Frozen");
        states[(int)States.InMenu] = new FSMState("InMenu");

        var aliveState = states[(int)States.Alive];
        var deadState = states[(int)States.Dead];
        var inMenuState = states[(int)States.InMenu];
        var initState = states[(int)States.Init];

        // Transitions

        // Global Transitions that every states can go to all the time
        // -> dead
        FSMCondition aliveCondition = new PlayerAliveCondition();
        FSMTransition dieTransition = new DieTransition(aliveCondition, null, deadState);
        FSMTransition aliveTransition = new FSMTransition(aliveCondition, aliveState, null);

        // we can die from any state
        this.globalTransitions.Add(dieTransition);
        this.globalTransitions.Add(new FSMNullTransition(initState));
        this.globalTransitions.Add(new FSMTransition(new PlayerInMenuCondition(), inMenuState, null));

        deadState.transitions.Add(aliveTransition);
        initState.transitions.Add(aliveTransition);

        ForceState(states[(int)States.Init]);
    }
}

public class PlayerController : MonoBehaviour, IControllable
{
    public PlayerGeneralStates generalStates;
    public PlayerVehicleStates vehicleStates;
    public PowerController self_PowerController;
    private bool isInMenu = false;

    // Flags
    public static readonly int FJump = 1;
    public BitVector32 flags = new BitVector32(0);

    public Vector3 repulseForce;
    [Header("Prefab Refs(MAND)")]
    public GameObject onDeathClone;

    [System.Serializable]
    public struct Jump
    {
        public bool isStarting;
        public bool suspensionLock;
        public bool applyForceMultiplier;
        public float value;
        // di
        public Accumulator diPitch;
        public Accumulator diRoll;
        public Accumulator diPitchUnscaled;
        public Accumulator diRollUnscaled;
        public float diMaxForce;

        public AudioClip[] sounds;
    }
    [Header("Jump")]
    public Jump jump;

    public float weightControlMaxX = 1;
    public float weightControlMaxZ = 2;
    public GameObject weightIndicator;
    public float weightIndicatorHeight;

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

    [Header("Generics")]
    [Range(0f, 2f)]
    public float invulnerabilityTimeAfterDamage;
    private float elapsedTimeSinceLastDamage = 999;

    [Header("Grappin")]
    public bool IsHooked;
    // TODO toffa : remove this hardcoded object
    public GameObject grapin;

    // cant be null
    public GameObject carPrefab;
    public GameObject carInstance;
    public CarController car;
    public Accumulator turn;

    public GameObject boatPrefab;
    private GameObject boatInstance;
    public GameObject planePrefab;
    public GameObject planeInstance;

    public Rigidbody rb;

    public AudioSource audioSource;
    public AudioClip damageSound;

    public InputManager inputMgr;
    public PlayerController playerInst;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (car == null)
        {
            if (carInstance == null)
            {
                carInstance = Instantiate(carPrefab);
                carInstance.transform.parent = gameObject.transform;
            }
            car = carInstance.GetComponent<CarController>();
        }
        carInstance.SetActive(false);

        if (planeInstance == null) {
            planeInstance = Instantiate(planePrefab);
            planeInstance.transform.parent = gameObject.transform;
        }

        generalStates = new PlayerGeneralStates(this);
        vehicleStates = new PlayerVehicleStates(this);

        self_PowerController = GetComponent<PowerController>();

        Freeze();

    }

    void Start()
    {
        if (inputMgr == null)
        {
            inputMgr = Access.PlayerInputsManager().player1;
        }

        inputMgr.Attach(this);

        Access.SceneLoader().beforeLoadScene.AddListener(OnBeforeLoadScene);
        Access.SceneLoader().beforeEnableScene.AddListener(OnBeforeEnableScene);
        Access.SceneLoader().afterLoadScene.AddListener(OnAfterLoadScene);

    }

    void OnDestroy()
    {
        inputMgr.Detach(this);

        var sceneLoader = Access.SceneLoader();
        if (sceneLoader) {

            sceneLoader.beforeLoadScene.RemoveListener(OnBeforeLoadScene);
            sceneLoader.afterLoadScene.RemoveListener(OnAfterLoadScene);
            sceneLoader.beforeEnableScene.RemoveListener(OnBeforeEnableScene);
        }
    }

    void OnBeforeLoadScene(){
        vehicleStates.SetState(vehicleStates.states[(int)PlayerVehicleStates.States.Init]);
        Freeze();
    }

    void OnBeforeEnableScene(Scene toBeEnabled) {

        //
        //private void RemovePreviousPlayerIfExists(Scene sceneToLoad)
        {
            foreach (var go in toBeEnabled.GetRootGameObjects())
            {
                if (go.name == Constants.GO_PLAYER)
                {
                    Destroy(go);
                }
            }
        }
        SceneManager.MoveGameObjectToScene(Access.Player().transform.root.gameObject,
            toBeEnabled);
    }

    void OnAfterLoadScene() {
        Access.invalidate();
    }

    public Transform GetCurrentTransform() {
        return currentRB == null ? transform : currentRB.transform;
    }

    // Update is called once per frame
    void Update()
    {
        generalStates.Update();
        vehicleStates.Update();

        elapsedTimeSinceLastDamage += Time.deltaTime;

    }

    void FixedUpdate()
    {
        generalStates.FixedUpdate();
        vehicleStates.FixedUpdate();


        SetHandbrake(false);
        turn.Reset();
        jump.diRollUnscaled.Reset();
        jump.diPitchUnscaled.Reset();
    }

    public WonkerDecal jumpDecal;

    #if SCH_SUSPENSION_V2
    public void ResetSpringSizeMinAndUnlock()
    {
        foreach (var axle in car.axles)
        {
            var suspensionRight = axle.right.suspension;
            suspensionRight.SetMinLength(car.springMin);
            suspensionRight.SetMaxLength(car.springMax);
            suspensionRight.SetRestLength(car.springRestPercent);


            var suspensionLeft = axle.left.suspension;
            suspensionLeft.SetMinLength(car.springMin);
            suspensionLeft.SetMaxLength(car.springMax);
            suspensionLeft.SetRestLength(car.springRestPercent);
        }
        car.overrideMaxSpring = false;
    }

    public void SetSpringSizeMinAndLock()
    {
        springElapsedCompression += Time.deltaTime;
        float springCompVal = Mathf.Lerp(car.springMax, car.springMin + 0.1f, springElapsedCompression / springCompressionTime);
        springCompVal = Mathf.Min(1, springCompVal);


        float springJumpFactor = jumpCompressionOverTime.Evaluate(Mathf.Min(1, springElapsedCompression / springCompressionTime));

        jumpDecal.SetAnimationTime(springJumpFactor);
        foreach (var axle in car.axles)
        {
            var suspensionRight = axle.right.suspension;
            suspensionRight.SetMinLength(car.springMin);
            suspensionRight.SetMaxLength(springCompVal);
            suspensionRight.SetRestLength(car.springRestPercent);


            var suspensionLeft = axle.left.suspension;
            suspensionLeft.SetMinLength(car.springMin);
            suspensionLeft.SetMaxLength(springCompVal);
            suspensionLeft.SetRestLength(car.springRestPercent);
        }
        car.overrideMaxSpring = true;
    }

    public float springCompressionTime = 0.5f;
    public AnimationCurve jumpCompressionOverTime;
    public float springElapsedCompression = 0f;
    public void TryJump()
    {
        if (jump.applyForceMultiplier)
        {
            float springCompVal = springElapsedCompression / springCompressionTime;
            springCompVal = Mathf.Min(1, springCompVal);
            if (springCompVal > 0.5f)
            {
                audioSource.clip = jump.sounds[0];
                audioSource.Play(0);
            }
            float springJumpFactor = jumpCompressionOverTime.Evaluate(springCompVal);

            foreach (var axle in car.axles)
            {
                car.rb.AddForceAtPosition(jump.value * springJumpFactor * transform.up * (axle.right.isGrounded ? 1 : 0), axle.right.suspension.GetAnchorA(), ForceMode.VelocityChange);
                car.rb.AddForceAtPosition(jump.value * springJumpFactor * transform.up * (axle.right.isGrounded ? 1 : 0), axle.left.suspension.GetAnchorA(), ForceMode.VelocityChange);
            }
            jump.applyForceMultiplier = false;
            springElapsedCompression = 0f;
            jumpDecal.SetAnimationTime(0f);
        }

    }
    #else

    public void ResetSpringSizeMinAndUnlock()
    {
        foreach (var axle in car.axles)
        {
            axle.right.suspension.spring.SetLengthSettings(car.springMin, car.springMax, car.springRestPercent);
            axle.left.suspension.spring.SetLengthSettings(car.springMin, car.springMax, car.springRestPercent);
        }
        car.overrideMaxSpring = false;
    }

    public void SetSpringSizeMinAndLock()
    {
        springElapsedCompression += Time.deltaTime;
        float springCompVal = Mathf.Lerp(car.springMax, car.springMin + 0.1f, springElapsedCompression / springCompressionTime);
        springCompVal = Mathf.Min(1, springCompVal);


        float springJumpFactor = jumpCompressionOverTime.Evaluate(Mathf.Min(1, springElapsedCompression / springCompressionTime));

        jumpDecal.SetAnimationTime(springJumpFactor);
        foreach (var axle in car.axles)
        {
            axle.right.suspension.spring.SetLengthSettings(car.springMin, springCompVal, car.springRestPercent);
            axle.left.suspension.spring.SetLengthSettings(car.springMin, springCompVal, car.springRestPercent);
        }
        car.overrideMaxSpring = true;
    }

    public float springCompressionTime = 0.5f;
    public AnimationCurve jumpCompressionOverTime;
    public float springElapsedCompression = 0f;
    public void TryJump()
    {
        if (jump.applyForceMultiplier)
        {
            float springCompVal = springElapsedCompression / springCompressionTime;
            springCompVal = Mathf.Min(1, springCompVal);
            if (springCompVal > 0.5f)
            {
                audioSource.clip = jump.sounds[0];
                audioSource.Play(0);
            }
            float springJumpFactor = jumpCompressionOverTime.Evaluate(springCompVal);

            foreach (var axle in car.axles)
            {
                car.rb.AddForceAtPosition(jump.value * springJumpFactor * transform.up * (axle.right.isGrounded ? 1 : 0), axle.right.suspension.spring.loadPosition, ForceMode.VelocityChange);
                car.rb.AddForceAtPosition(jump.value * springJumpFactor * transform.up * (axle.right.isGrounded ? 1 : 0), axle.left.suspension.spring.loadPosition, ForceMode.VelocityChange);
            }
            jump.applyForceMultiplier = false;
            springElapsedCompression = 0f;
            jumpDecal.SetAnimationTime(0f);
        }

    }
    #endif
    public void useTurbo()
    {
        // no turbo atm
        return;

        turbo.intervalElapsedTime += Time.deltaTime;
        if (turbo.intervalElapsedTime < turbo.timeInterval)
        return;

        var nextTurboValue = turbo.current - (turbo.infinite ? 0 : turbo.consumptionPerTick);
        if (nextTurboValue < 0)
        {
            return;
        }

        turbo.current = Mathf.Clamp(nextTurboValue, 0f, turbo.max);

        //Access.UITurboAndSaves().updateTurboBar();

        Vector3 turboDir = transform.forward.normalized;
        DrawRay(transform.position, turboDir, Color.yellow, 4, false);
        car.rb.AddForce(turboDir * turbo.strength, ForceMode.VelocityChange);

        turbo.intervalElapsedTime = 0f;
    }

    public bool TouchGround()
    {
        foreach (var a in car.axles)
        {
            if (a.left.isGrounded || a.right.isGrounded)
            {
                return true; // early return
            }
        }
        return false;
    }

    private bool isTouchingWater = false;
    public void SetTouchingWater(bool state)
    {
        isTouchingWater = state;
    }

    public bool TouchGroundAll()
    {
        foreach (var a in car.axles)
        {
            if (!a.left.isGrounded || !a.right.isGrounded)
            {
                return false; // early return
            }
        }
        return true;
    }

    public bool IsAlive()
    {
        if (isTouchingWater && vehicleStates.GetState() != vehicleStates.states[(int)PlayerVehicleStates.States.Boat])
        {
            return false;
        }


        CollectiblesManager cm = Access.CollectiblesManager();
        if (cm == null)
        {
            return true;
        }

        return (cm.getCollectedNuts() >= 0);
    }

    public void Kill(Vector3 iSteer = default(Vector3))
    {
        GameObject dummy_player = Instantiate(onDeathClone, transform.position, transform.rotation);
        //Destroy(dummy_player.GetComponent<CarController>());

        // add dummy's suspension/ wheels to DC objects
        DeathController dc = dummy_player.GetComponent<DeathController>();
        // dc.objects.Clear();
        // foreach (Transform child in dummy_player.transform)
        // {
        //     Rigidbody rb = child.GetComponent<Rigidbody>();
        //     if (!!rb)
        //     dc.objects.Add(rb);
        // }
        dc.Activate(iSteer);
        Access.CameraManager().launchDeathCam();
    }

    public void ActivateCar()
    {
        carInstance.SetActive(true);
        inputMgr.Attach(car as IControllable);

        foreach (var axle in car.axles)
        {
            if (axle.right != null) {
                var go = axle.right.AsGameObject();
                if (go) go.SetActive(true);
            }

            if (axle.left != null) {
                var go = axle.left.AsGameObject();
                if (go) go.SetActive(true);
            }
        }
    }

    public void DeactivateCar()
    {
        carInstance.SetActive(false);
        inputMgr.Detach(car as IControllable);

        // enable object that might not be part of us, aka the wheel colliders
        foreach (var axle in car.axles)
        {
            if (axle.right != null) {
                var go = axle.right.AsGameObject();
                if (go) go.SetActive(false);
            }

            if (axle.left != null) {
                var go = axle.left.AsGameObject();
                if (go) go.SetActive(false);
            }
        }
    }

    public void ActivateAirplane()
    {
        planeInstance.SetActive(true);
        rb.useGravity = false;
    }

    public void DeactivateAirplane()
    {
        planeInstance.SetActive(false);
        rb.useGravity = true;
    }

    public bool IsInMenu() { return isInMenu; }
    public void Freeze() { isInMenu = true; rb.isKinematic = true; MuteSound();  }
    public void UnFreeze() { isInMenu = false; rb.isKinematic = false; UnMuteSound(); }
    private void MuteSound()
    {
        foreach (var source in GetComponentsInChildren<AudioSource>())
        {
            source.mute = true;
        }
    }

    private void UnMuteSound()
    {
        foreach (var source in GetComponentsInChildren<AudioSource>())
        {
            source.mute = false;
        }
    }

    public void SetHandbrake(bool v)
    {
        var rear = car.axles[(int)AxleType.rear];
        rear.left.isHandbraked = v;
        rear.right.isHandbraked = v;

        var front = car.axles[(int)AxleType.front];
        front.left.isHandbraked = v;
        front.right.isHandbraked = v;
    }

    public void SetCarCenterOfMass()
    {
        car.centerOfMass.transform.localPosition = car.centerOfMassInitial + new Vector3(jump.diRollUnscaled.average * weightControlMaxX, 0f, jump.diPitchUnscaled.average * weightControlMaxZ);
    }

    /// =============== Game Logic ==================
    public void takeDamage(int iDamage, Vector3 iDamageSourcePoint, Vector3 iDamageSourceNormal, float iRepulsionForce = 5f)
    {
        //
        //if (stateMachine.currentState == invulState || stateMachine.currentState == deadState || stateMachine.currentState == frozenState)
        //return;
        if (elapsedTimeSinceLastDamage <= invulnerabilityTimeAfterDamage)
        return;

        audioSource.clip = damageSound;
        audioSource.Play();

        // lose nuts
        CollectiblesManager cm = Access.CollectiblesManager();
        int n_nuts = cm.getCollectedNuts();

        Vector3 contactNormal = iDamageSourceNormal;
        Vector3 contactPoint = iDamageSourcePoint;
        DrawRay(contactPoint, contactNormal * 5, Color.red, 5, false);

        Vector3 repulseDir = contactPoint + contactNormal;
        repulseForce = -repulseDir * iRepulsionForce;

        int availableNuts = (n_nuts >= iDamage) ? iDamage : n_nuts;
        for (int i = 0; i < availableNuts; i++)
        {
            GameObject nutFromDamage = Instantiate(cm.nutCollectibleRef);
            nutFromDamage.GetComponent<CollectibleNut>().setSpawnedFromDamage(transform.position);
        }
        cm.loseNuts(iDamage);
        rb.AddForce(repulseForce, ForceMode.Impulse);

        elapsedTimeSinceLastDamage = 0f;
        //stateMachine.ForceState(invulState);
    }

    bool modifierCalled = false;

    void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        // dirty fix for respawn when slipping
        foreach (var a in car.axles)
        {
            a.right.slipX = 1;
            a.right.slipY = 1;

            a.left.slipX = 1;
            a.left.slipY = 1;
        }

        // Specific states control
        var generalInputs = (generalStates.GetState() as IControllable);
        if (generalInputs != null)
        {
            generalInputs.ProcessInputs(currentMgr, Entry);
        }

        var vehicleInputs = (vehicleStates.GetState() as IControllable);
        if (vehicleInputs != null)
        {
            vehicleInputs.ProcessInputs(currentMgr, Entry);
        }
    }
}
