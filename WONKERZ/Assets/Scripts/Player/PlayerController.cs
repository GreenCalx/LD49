using Schnibble;
using static Schnibble.SchPhysics;
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
            if (!player.TouchGround()) {
                var torque = new Vector3(player.jump.diMaxForce * player.jump.diPitch, 0, -player.jump.diMaxForce * player.jump.diRoll);
                torque = player.car.transform.TransformDirection(torque);
                player.car.rb.AddTorque(torque, ForceMode.VelocityChange);
            }
            else {
                player.SetCarCenterOfMass();
            }
            // reset jump correction
            player.jump.diRoll = 0;
            player.jump.diPitch = 0;
        }
    }

    public GroundState(PlayerController player) : base("Car") { this.player = player; this.fixedActions.Add(new GeneralGroundAction()); }

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

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        if (Entry.Inputs["Jump"].Down)
        {
            player.SetSpringSizeMinAndLock();
        }
        else
        {
            if (Entry.Inputs["Jump"].IsUp)
            {
                player.jump.applyForceMultiplier = true;
            }
            player.ResetSpringSizeMinAndUnlock();
        }

        if (Entry.Inputs["Turbo"].Down)
        {
            player.useTurbo();
        }

        if (Entry.Inputs["Power1"].Down)
        {
            player.SetHandbrake(true);
        } else
        {
            player.SetHandbrake(false);
        }

        // makes car torque control a power
        //if (player.flags[PlayerController.FJump])
        if (Entry.Inputs["Modifier"].Down)
        {
            var x = Entry.Inputs["Turn"].AxisValue;
            var y = Entry.Inputs["UIUpDown"].AxisValue;

            player.jump.diRoll += x;
            player.jump.diPitch += y;
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

        // we can die from any state
        this.globalTransitions.Add(dieTransition);
        this.globalTransitions.Add(new FSMNullTransition(initState));
        this.globalTransitions.Add(new FSMTransition(new PlayerInMenuCondition(), inMenuState, null));

        aliveState.transitions.Add(new FSMTransition(aliveCondition, null, deadState));
        deadState.transitions.Add(new FSMTransition(aliveCondition, aliveState, null));
        inMenuState.transitions.Add(new FSMTransition(aliveCondition, null, deadState));
        initState.transitions.Add(new FSMTransition(aliveCondition, aliveState, null));

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
        public float diPitch;
        public float diRoll;
        public float diMaxForce;

        public AudioClip[] sounds;
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

    // cant be null
    public GameObject carPrefab;
    public GameObject carInstance;
    public CarController car;

    public GameObject boatPrefab;
    private GameObject boatInstance;
    public GameObject planePrefab;
    private GameObject planeInstance;

    public Rigidbody rb;

    public AudioSource audioSource;

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

        Utils.attachControllable(this);
    }

    void OnDestroy()
    {
        Utils.detachControllable(this);
    }

    // Update is called once per frame
    void Update()
    {
        generalStates.Update();
        vehicleStates.Update();
    }

    void FixedUpdate()
    {
        generalStates.FixedUpdate();
        vehicleStates.FixedUpdate();
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
    private float springElapsedCompression = 0f;
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

        // var nextTurboValue = turbo.current - (turbo.infinite ? 0 : turbo.consumptionPerTick);
        // if (nextTurboValue < 0)
        // return;
        float nextTurboValue = turbo.current - turbo.consumptionPerTick;

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
        CollectiblesManager cm = Access.CollectiblesManager();
        int n_nuts = cm != null ? cm.getCollectedNuts() : 0;
        return (n_nuts >= 0);
    }

    public void Kill(Vector3 iSteer = default(Vector3))
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

    public void ActivateCar()
    {
        carInstance.SetActive(true);
        Utils.attachControllable(car);
    }

    public void DeactivateCar()
    {
        carInstance.SetActive(false);
        Utils.detachControllable(car);
    }

    public bool IsInMenu() { return isInMenu; }
    public void Freeze() { isInMenu = true; rb.isKinematic = true; }
    public void UnFreeze() { isInMenu = false; rb.isKinematic = false; }

    public void SetHandbrake(bool v) {
        var rear = car.axles[(int)AxleType.rear];
        rear.left.isHandbraked = v;
        rear.right.isHandbraked = v;

        var front = car.axles[(int)AxleType.front];
        front.left.isHandbraked = v;
        front.right.isHandbraked = v;
    }

    public void SetCarCenterOfMass() {
        car.centerOfMass.transform.localPosition = car.centerOfMassInitial + new Vector3(jump.diRoll * 2f, 0f, jump.diPitch * 3f);
    }

    /// =============== Game Logic ==================
    public void takeDamage(int iDamage, Vector3 iDamageSourcePoint, Vector3 iDamageSourceNormal, float iRepulsionForce = 5f)
    {
        //
        //if (stateMachine.currentState == invulState || stateMachine.currentState == deadState || stateMachine.currentState == frozenState)
        //return;


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

        //stateMachine.ForceState(invulState);
    }

    bool modifierCalled = false;


    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        // Every states controls

        // power controller update
        PowerController pc = GetComponent<PowerController>();
        if (!!pc)
        {
            // modifier inputs
            // modifier now used for torque control
            //if (Entry.Inputs["Modifier"].Down)
            //{
            //Vector2 mouse_mod = new Vector2(Entry.Inputs["Power_MouseX"].AxisValue, Entry.Inputs["Power_MouseY"].AxisValue);
            //pc.showUI(true);
            //if (Entry.Inputs["Power1"].Down || (mouse_mod.x > 0))
            //{ // BallPower
            //pc.showIndicator(PowerController.PowerWheelPlacement.LEFT);
            //pc.setNextPower(1);
            //}
            //else if (Entry.Inputs["Power2"].Down || (mouse_mod.y > 0))
            //{ // WaterPower
            //pc.setNextPower(2);
            //pc.showIndicator(PowerController.PowerWheelPlacement.DOWN);
            //}
            //else if (Entry.Inputs["Power3"].Down || (mouse_mod.y < 0))
            //{ // PlanePower
            //pc.showIndicator(PowerController.PowerWheelPlacement.UP);
            //pc.setNextPower(3);
            //}
            //else if (Entry.Inputs["Power4"].Down || (mouse_mod.x < 0))
            //{ // SpiderPower
            //pc.showIndicator(PowerController.PowerWheelPlacement.RIGHT);
            //pc.setNextPower(4);
            //}
            //else
            //{
            //pc.showIndicator(PowerController.PowerWheelPlacement.NEUTRAL);
            //pc.setNextPower(0);
            //}
            //modifierCalled = true;
            //}
            //else if (modifierCalled && !Entry.Inputs["Modifier"].Down)
            //{
            //pc.hideIndicators();
            //pc.showUI(false);
            //
            //pc.tryTriggerPower();
            //modifierCalled = false;
            //}


            // Specific states control

            var generalInputs = (generalStates.GetState() as IControllable);
            if (generalInputs != null)
            {
                generalInputs.ProcessInputs(Entry);
            }


            var vehicleInputs = (vehicleStates.GetState() as IControllable);
            if (vehicleInputs != null)
            {
                vehicleInputs.ProcessInputs(Entry);
            }


            pc.applyPowerEffectInInputs(Entry, this);
        }
    }
}
