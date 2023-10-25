using Schnibble;
using static Schnibble.SchPhysics;
using static Schnibble.SchMathf;
using System.Collections.Specialized;
using UnityEngine;

// FSM when in ground mode
public class GroundState : FSMState, IControllable
{
    private PlayerController player;

    private class GeneralGroundAction : FSMAction
    {
        public override void Execute(FSMBase fsm)
        {
            var player = (fsm as PlayerFSM).GetPlayer();

            player.TryJump();
            player.flags[PlayerController.FJump] = !player.TouchGround();
            // apply jump correction
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

    public GroundState(PlayerController player) : base("Car")
    {
        this.player = player;
        this.actions.Add(new UpdateWheelBasis());
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

    void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        var jumpButton = Entry[(int)PlayerInputs.InputCode.Jump] as GameInputButton;
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

        var turboButton = Entry[(int)PlayerInputs.InputCode.Turbo] as GameInputButton;
        if (turboButton != null)
        {
            if (turboButton.GetState().heldDown)
            {
                player.useTurbo();
            }
        }

        var handbrakeButton = Entry[(int)PlayerInputs.InputCode.Handbrake] as GameInputButton;
        if (handbrakeButton != null)
        {
            if (handbrakeButton.GetState().heldDown)
            {
                player.SetHandbrake(true);
            }
        }

        // makes car torque control a power

        var weightXAxis = Entry[(int)PlayerInputs.InputCode.WeightX] as GameInputAxis;
        var weightYAxis = Entry[(int)PlayerInputs.InputCode.WeightY] as GameInputAxis;
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

        var accelerationAxis = Entry[(int)PlayerInputs.InputCode.Accelerator] as GameInputAxis;
        if (accelerationAxis != null)
        {
            var acceleration = Mathf.Clamp01(accelerationAxis.GetState().valueSmooth);
            if (acceleration != 0f)
            player.car.currentRPM.Add(acceleration);
        }

        var breakAxis = Entry[(int)PlayerInputs.InputCode.Break] as GameInputAxis;
        if (breakAxis != null)
        {
            var breaks = Mathf.Clamp01(breakAxis.GetState().valueSmooth);
            if (breaks != 0f)
            player.car.currentRPM.Add(-breaks);
        }
        // todo toffa : this is very weird...check this at some point

        var turnAxis = Entry[(int)PlayerInputs.InputCode.Turn] as GameInputAxis;
        if (turnAxis != null)
        {
            if (turnAxis.GetState().valueSmooth != 0f)
            player.turn.Add(turnAxis.GetState().valueSmooth);
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

        var initState = states[(int)States.Init];
        var carState = states[(int)States.Car];

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
    private GameObject planeInstance;

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

        generalStates = new PlayerGeneralStates(this);
        vehicleStates = new PlayerVehicleStates(this);

        Freeze();
    }

    void Start()
    {
        if (inputMgr == null)
        {
            inputMgr = Access.PlayerInputsManager().player1;
        }

        inputMgr.Attach(this);
    }

    void OnDestroy()
    {
        inputMgr.Detach(this);
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
    public void useTurbo()
    {
        turbo.intervalElapsedTime += Time.deltaTime;
        if (turbo.intervalElapsedTime < turbo.timeInterval)
        return;

        var nextTurboValue = turbo.current - (turbo.infinite ? 0 : turbo.consumptionPerTick);
        if (nextTurboValue < 0)
        {
            return;
        }

        turbo.current = Mathf.Clamp(nextTurboValue, 0f, turbo.max);

        Access.UITurboAndSaves().updateTurboBar();

        Vector3 turboDir = transform.forward.normalized;
        Debug.DrawRay(transform.position, turboDir, Color.yellow, 4, false);
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
    Access.Player().inputMgr.Attach(car as IControllable);
}

public void DeactivateCar()
{
    carInstance.SetActive(false);
    Access.Player().inputMgr.Detach(car as IControllable);
}

public bool IsInMenu() { return isInMenu; }
public void Freeze() { isInMenu = true; rb.isKinematic = true; MuteSound(); }
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
    Debug.DrawRay(contactPoint, contactNormal * 5, Color.red, 5, false);

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


void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
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



    // to remove

    if (Input.GetKeyDown(KeyCode.Space))
    {
        PlayerController pc = Instantiate(playerInst, transform.position, transform.rotation);
        pc.inputMgr = Access.PlayerInputsManager().dualPlayers;

        var states = pc.vehicleStates;
        states.SetState(states.states[(int)PlayerVehicleStates.States.Car]);
        Access.Player().UnFreeze();
    }
}
}
