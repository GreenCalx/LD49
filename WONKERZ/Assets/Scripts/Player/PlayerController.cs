using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;


// FSM when in ground mode
public class GroundFSM : PlayerFSM
{
   public override void OnEnter(FSMBase fsm){
        (fsm as PlayerFSM).player.ActivateCar();
    }

    public override void OnExit(FSMBase fsm){
        (fsm as PlayerFSM).player.DeactivateCar();
    }

    public void CreateFSM()
    {
        name = "Ground";

        FSMState invulState = new FSMState();
        states[(int)States.Invulnerable] = invulState;


        invulState.name = "Invulnerable";

        FSMTimedCondition invulTime = new FSMTimedCondition();
        invulTime.time = 1f;

        FSMTransition invulTrans = new FSMTransition();
        invulTrans.condition = invulTime;
        invulTrans.falseState = invulState;
        invulTrans.trueState = this;

        invulState.transitions.Add(invulTrans);
    }
};


public class PlayerFSM : FSMBase
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
        Jumping,
        Invulnerable,
        // vehicle types
        Ground,
        Water,
        Air,
        // power
        Ball,
        Count,
    };
    public FSMState[] states = new FSMState[(int)States.Count];

    public PlayerController player;

    private class AliveCondition : FSMCondition {
        public override bool Check(FSMBase fsm) {
            return (fsm as PlayerFSM).player.IsAlive();
        }
    }
    FSMCondition aliveCondition = new AliveCondition();

    public class GroundTransition : FSMTransition {
        public override void OnTransition(FSMBase fsm, FSMState toState){
            (fsm as PlayerFSM).player.ActivateCar();
        }
    };
    public class WaterTransition : FSMTransition {};
    public class AirTransition : FSMTransition {};

    private class CarCondition : FSMCondition {
        public override bool Check(FSMBase fsm) {
            return (fsm as PlayerFSM).player.playerQueryTransitionToCarController;
        }
    }
    FSMCondition carCondition = new CarCondition();

    public class DieTransition : FSMTransition
    {
        public override void OnTransition(FSMBase fsm, FSMState toState)
        {
            (fsm as PlayerFSM).player.Kill();

            Access.CheckPointManager().loadLastCP(true);

            base.OnTransition(fsm, toState);
        }
    }

    private class WaterState : FSMState {
    }

    private class AirState : FSMState {
    }

    private FSMState stateBeforeFreeze;
    public void FreezeState()
    {
        var currentStateFSM = currentState as PlayerFSM;
        if (currentStateFSM != null)
        {
            // try set freeze state
            var currentStateFSMFreezeState = currentStateFSM.states[(int)States.Invulnerable];
            if (currentStateFSMFreezeState != null)
            {
                stateBeforeFreeze = currentStateFSM.currentState;
                currentStateFSM.ForceState(currentStateFSMFreezeState);
                return;
            }
        }

        stateBeforeFreeze = currentState;
        ForceState(states[(int)States.Invulnerable]);
    }

    public void UnFreezeState()
    {
        ForceState(stateBeforeFreeze);
        stateBeforeFreeze = null;
    }

    public void CreateFSM(PlayerController player)
    {
        this.player = player;

        // init states
        states[(int)States.Init] = new FSMState();
        states[(int)States.Alive] = new FSMState();
        states[(int)States.Dead] = new FSMState();
        states[(int)States.Frozen] = new FSMState();
        states[(int)States.Ground] = new GroundFSM();

        // Transitions

        // Global Transitions that every states can go to all the time
        // -> dead
        FSMTransition dieTransition = new DieTransition();
        dieTransition.condition = aliveCondition;
        dieTransition.falseState = states[(int)States.Dead];

        // we can die from any state
        this.globalTransitions.Add(dieTransition);

        // -> ground
        FSMTransition groundTransition = new GroundTransition();
        groundTransition.condition = carCondition;
        groundTransition.trueState = states[(int)States.Ground];

        // -> water
        //FSMTransition waterTransition = new WaterTransition();
        //waterTransition.condition = boatCondition;
        //waterTransition.trueState = waterState;

        // -> air
        //FSMTransition airTransition = new AirTransition();
        //airTransition.condition = airCondition;
        //airTransition.trueState = airState;

        // -> ball
        //FSMTransition ballTransition = new BallTransition();

        //
        // States
        //

        // AliveState transitions

        var aliveState = states[(int)States.Alive];
        aliveState.name = "Alive";
        //aliveState.transitions.Add(airTransition);
        aliveState.transitions.Add(groundTransition);
        //aliveState.transitions.Add(waterTransition);

        // Ground state
        var groundState = states[(int)States.Ground] as GroundFSM;
        groundState.CreateFSM();
        //    ground -> air
        //groundState.transitions.Add(airTransition);
        //    ground -> water
        //groundState.transitions.Add(waterTransition);
        //    ground -> invulnerableGround

        ForceState(states[(int)States.Init]);
        SetState(states[(int)States.Ground]);

        #if false
        // Water state
        waterState.name = "Water";
        //    water -> air
        waterState.transitions.Add(airTransition);
        //    water -> ground
        waterState.transitions.Add(groundTransition);

        // Air state
        airState.name = "Air";
        //    air -> ground
        airState.transitions.Add(groundTransition);
        //    air -> water
        airState.transitions.Add(waterTransition);
        #endif
    }
};


public class PlayerController : MonoBehaviour, IControllable
{
    public PlayerFSM fsm = new PlayerFSM();

    public bool playerQueryTransitionToCarController = false;
    public bool playerQueryTransitionToBoatController = false;
    public bool playerQueryTransitionToAircraftController = false;

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

    // cant be null
    public GameObject carPrefab;
    public GameObject carInstance;
    public CarController car;

    public GameObject boatPrefab;
    private GameObject boatInstance;
    public GameObject planePrefab;
    private GameObject planeInstance;

    public Rigidbody rb;

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

        fsm.CreateFSM(this);

        Utils.attachControllable(this);
    }

    void OnDestroy()
    {
        Utils.detachControllable(this);
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        fsm.Update();
        TryJump();
    }

    void FixedUpdate()
    {
        fsm.FixedUpdate();
        jump.applyForceMultiplier = false;
    }

    public WonkerDecal jumpDecal;
    private void ResetSpringSizeMinAndUnlock()
    {
        foreach (var axle in car.axles)
        {
            axle.right.suspension.spring.SetLengthSettings(car.springMin, car.springMax, car.springRestPercent);
            axle.left.suspension.spring.SetLengthSettings(car.springMin, car.springMax, car.springRestPercent);
        }
        car.overrideMaxSpring = false;
    }

    private void SetSpringSizeMinAndLock()
    {
        springElapsedCompression += Time.deltaTime;
        float springCompVal = Mathf.Lerp(car.springMax, car.springMin + 0.1f, springElapsedCompression/springCompressionTime);
        //jumpDecal.SetAnimationTime(springElapsedCompression/springCompressionTime);
        foreach (var axle in car.axles)
        {
            axle.right.suspension.spring.SetLengthSettings(car.springMin, springCompVal, car.springRestPercent);
            axle.left.suspension.spring.SetLengthSettings(car.springMin, springCompVal, car.springRestPercent);
        }
        car.overrideMaxSpring = true;
    }

    public float springCompressionTime = 0.5f;
    private float springElapsedCompression = 0f;
    public void TryJump()
    {
        if (jump.applyForceMultiplier)
        {
            float springCompVal =  springElapsedCompression / springCompressionTime;

            springCompVal = Mathf.Min(1, springCompVal);
            foreach (var axle in car.axles)
            {
                car.rb.AddForceAtPosition(jump.value * springCompVal * transform.up * (axle.right.isGrounded ? 1 : 0), axle.right.suspension.spring.loadPosition, ForceMode.VelocityChange);
                car.rb.AddForceAtPosition(jump.value * springCompVal * transform.up * (axle.right.isGrounded ? 1 : 0), axle.left.suspension.spring.loadPosition, ForceMode.VelocityChange);
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
        return;

        turbo.current = Mathf.Clamp(0, turbo.max, nextTurboValue);

        Access.UITurboAndLifePool().updateTurboBar(turbo.current);

        Vector3 turboDir = transform.forward.normalized;
        Debug.DrawRay(transform.position, turboDir, Color.yellow, 4, false);
        car.rb.AddForce(turboDir * turbo.strength, ForceMode.VelocityChange);

        turbo.intervalElapsedTime = 0f;
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
            foreach (var a in car.axles)
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
            foreach (var a in car.axles)
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

    public bool IsAlive() {
        CollectiblesManager cm = Access.CollectiblesManager();
        int n_nuts = cm != null ? cm.getCollectedNuts() : 0;
        return (n_nuts >= 0);
    }

    public void Kill(Vector3 iSteer = default(Vector3)) {
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

    public void ActivateCar(){
        carInstance.SetActive(true);
        Utils.attachControllable(car);
    }

    public void DeactivateCar(){
        carInstance.SetActive(false);
        Utils.detachControllable(car);
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

        for (int i = 0; i < n_nuts; i++)
        {
            GameObject nutFromDamage = Instantiate(cm.nutCollectibleRef);
            nutFromDamage.GetComponent<CollectibleNut>().setSpawnedFromDamage(transform.position);
        }
        cm.loseNuts(iDamage);
        rb.AddForce(repulseForce, ForceMode.Impulse);

        //stateMachine.ForceState(invulState);
    }

    bool modifierCalled =false;


    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        // Every states controls

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


            // Specific states control

            if (fsm.currentState == fsm.states[(int)PlayerFSM.States.Ground])
            {
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


                if (Entry.Inputs["Turbo"].Down)
                {
                    useTurbo();
                }
            }

            pc.applyPowerEffectInInputs(Entry, this);
        }
    }


    public void Freeze()
    {
        fsm.FreezeState();
    }

    public void UnFreeze() {
        fsm.UnFreezeState();
    }
}
