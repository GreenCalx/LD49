using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.SceneManagement;
using Schnibble;
using Schnibble.Managers;
using static Schnibble.Math;
using static Schnibble.Physics;
using static Schnibble.Utils;
using static UnityEngine.Debug;

namespace Wonkerz
{

    public class PlayerState : FSMState
    {
        protected PlayerController player;
        protected PlayerState(PlayerController player, string stateName) : base(stateName)
        {
            this.player = player;
        }
    }

    public class BoatState : PlayerState, IControllable
    {
        public BoatState(PlayerController player) : base(player, "boat")
        {
        }

        public override void OnEnter(FSMBase fsm)
        {
            var player = (fsm as PlayerFSM).GetPlayer();
            player.ActivateMode(player.boat.gameObject, player.boat.rb);
        }
        public override void OnExit(FSMBase fsm)
        {
            var player = (fsm as PlayerFSM).GetPlayer();
            player.DeactivateMode(player.boat.gameObject);
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            var airplaneMode = Entry.Get((int)PlayerInputs.InputCode.AirplaneMode) as GameInputButton;
            if (airplaneMode != null)
            {
                if (airplaneMode.GetState().down)
                {
                    player.vehicleStates.SetState(player.vehicleStates.states[(int)PlayerVehicleStates.States.Plane]);
                }
            }

            var boatMode = Entry.Get((int)PlayerInputs.InputCode.BoatMode) as GameInputButton;
            if (boatMode != null)
            {
                if (boatMode.GetState().down)
                {
                    player.vehicleStates.SetState(player.vehicleStates.states[(int)PlayerVehicleStates.States.Car]);
                }
            }

            player.boat.ProcessInputs(currentMgr, Entry);
        }
    }

    public class AirplaneState : PlayerState, IControllable
    {
        private class GeneralAirAction : GroundState.GeneralGroundAction { }

        public AirplaneState(PlayerController player) : base(player, "airplane")
        {
            this.fixedActions.Add(new GeneralAirAction());
        }

        public override void OnEnter(FSMBase fsm)
        {
            var player = (fsm as PlayerFSM).GetPlayer();
            player.ActivateMode(player.plane.gameObject, player.plane.plane.rb);
        }
        public override void OnExit(FSMBase fsm)
        {
            var player = (fsm as PlayerFSM).GetPlayer();
            player.DeactivateMode(player.plane.gameObject);
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            var airplaneMode = Entry.Get((int)PlayerInputs.InputCode.AirplaneMode) as GameInputButton;
            if (airplaneMode != null)
            {
                if (airplaneMode.GetState().down)
                {
                    player.vehicleStates.SetState(player.vehicleStates.states[(int)PlayerVehicleStates.States.Car]);
                }
            }

            var boatMode = Entry.Get((int)PlayerInputs.InputCode.BoatMode) as GameInputButton;
            if (boatMode != null)
            {
                if (boatMode.GetState().down)
                {
                    player.vehicleStates.SetState(player.vehicleStates.states[(int)PlayerVehicleStates.States.Boat]);
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


            player.plane.ProcessInputs(currentMgr, Entry);
        }
    }

    // FSM when in ground mode
    public class GroundState : PlayerState, IControllable
    {
        public class GeneralGroundAction : FSMAction
        {
            public override void Execute(FSMBase fsm)
            {
                var player = (fsm as PlayerFSM).GetPlayer();

                #if false
                if (player.car_old)
                {
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
                            torque = player.car_old.transform.TransformDirection(torque);
                            player.car_old.rb.AddTorque(torque * Time.fixedDeltaTime, ForceMode.VelocityChange);
                        }
                        else
                        {
                            player.SetCarCenterOfMass();
                        }

                        // very bad design for now.
                        var weightIndPos = player.car_old.centerOfMassInitial + new Vector3(player.jump.diRollUnscaled.average * player.weightControlMaxX,
                            player.weightIndicatorHeight,
                            player.jump.diPitchUnscaled.average * player.weightControlMaxZ);

                        player.weightIndicator.transform.position = player.car_old.centerOfMass.transform.parent.TransformPoint(weightIndPos);
                    }
                }
                #endif
            }
        }

        private class UpdateWheelBasis : FSMAction
        {
            public override void Execute(FSMBase fsm)
            {
                var player = (fsm as PlayerFSM).GetPlayer();

                #if false
                if (player.car_old)
                {
                    foreach (var axle in player.car_old.axles)
                    {
                        if (axle.isSteerable)
                        {
                            axle.right.basis = GetBasis(player.car_old.transform.up,
                                Quaternion.AngleAxis(player.car_old.maxSteeringAngle_deg * player.turn.average, player.car_old.transform.up) * player.car_old.transform.forward);
                        }
                        else
                        {
                            axle.right.basis = GetBasis(player.car_old.transform.up, player.car_old.transform.forward);
                        }
                        axle.left.basis = axle.right.basis;
                    }
                }
                #endif
            }
        }

        private class SpeedEffect : FSMAction
        {
            public override void Execute(FSMBase fsm)
            {
                var player = (fsm as PlayerFSM).GetPlayer();

                #if false
                if (player.car_old)
                {
                    var playerCar = player.car_old
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
                #endif

                var playerCarController = player.car;
                var playerCar           = playerCarController.car;
                var effectRatio         = Mathf.Clamp(((float)playerCar.GetCurrentSpeedInKmH() - playerCarController.speedEffect.thresholdSpeedInKmH) / playerCarController.speedEffect.maxSpeedInKmH, 0.0f, 1.0f);
                // Speed effect on camera
                // update camera FOV/DIST if a PlayerCamer
                CameraManager CamMgr = Access.CameraManager();
                if (CamMgr?.active_camera is PlayerCamera)
                {
                    PlayerCamera pc = (PlayerCamera)CamMgr.active_camera;
                    pc.applySpeedEffect(effectRatio);
                }

                var particles = playerCarController.speedEffect.particles;

                var e = particles.emission;
                e.enabled = effectRatio != 0.0f;

                var relativeWindDir = playerCar.chassis.rb.GetLinearVelocity();
                particles.transform.LookAt(playerCar.chassis.rb.GetPosition() + relativeWindDir);

                var lifemin  = 0.2f;
                var lifemax  = 0.6f;
                var partmain = particles.main;
                partmain.startLifetime = Mathf.Lerp(lifemin, lifemax, effectRatio);
            }
        }

        public GroundState(PlayerController player) : base(player, "ground")
        {
            this.actions.Add(new UpdateWheelBasis());
            this.actions.Add(new SpeedEffect());
            this.fixedActions.Add(new GeneralGroundAction());
        }

        public override void OnEnter(FSMBase fsm)
        {
            base.OnEnter(fsm);
            var player = (fsm as PlayerFSM).GetPlayer();
            player.ActivateMode(player.car.gameObject, player.car.car.chassis.rb.GetPhysXBody());
        }

        public override void OnExit(FSMBase fsm)
        {
            base.OnExit(fsm);
            (fsm as PlayerFSM).GetPlayer().DeactivateMode(player.car.gameObject);
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            // paraglider
            if (!player.IsInMenu())
            {
                var airplaneMode = Entry.Get((int)PlayerInputs.InputCode.AirplaneMode) as GameInputButton;
                if (airplaneMode != null)
                {
                    if (airplaneMode.GetState().down)
                    {
                        player.vehicleStates.SetState(player.vehicleStates.states[(int)PlayerVehicleStates.States.Plane]);
                    }
                }

                var boatMode = Entry.Get((int)PlayerInputs.InputCode.BoatMode) as GameInputButton;
                if (boatMode != null)
                {
                    if (boatMode.GetState().down)
                    {
                        player.vehicleStates.SetState(player.vehicleStates.states[(int)PlayerVehicleStates.States.Boat]);
                    }
                }

                player.car.ProcessInputs(currentMgr, Entry);
            }

            #if false
            if (player.car_old)
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
                    player.car_old.currentRPM.Add(acceleration);
                }

                var breakAxis = Entry.Get((int)PlayerInputs.InputCode.Break) as GameInputAxis;
                if (breakAxis != null)
                {
                    var breaks = Mathf.Clamp01(breakAxis.GetState().valueSmooth);
                    if (breaks != 0f)
                    player.car_old.currentRPM.Add(-breaks);
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
                    if (spinAtk != null)
                    {
                        if (spinAtk.GetState().down)
                        {
                            player.self_PowerController.setNextPower(1);
                            player.self_PowerController.tryTriggerPower();
                        }
                    }
                }

            }
            #endif
        }
    };

    public class PlayerFSM : FSMBase
    {
        protected PlayerController player;

        public PlayerFSM(PlayerController player, string baseName) : base(baseName)
        {
            this.player = player;
        }

        public PlayerController GetPlayer() { return player; }
    }

    public class PlayerVehicleStates : PlayerFSM
    {
        public PlayerVehicleStates(PlayerController p) : base(p, "vehicle") { CreateFSM(); }

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
            states[(int)States.Boat] = new BoatState(player);

            var initState = states[(int)States.Init];
            var carState = states[(int)States.Car];
            var airplaneState = states[(int)States.Plane];
            var boatState = states[(int)States.Plane];

            globalTransitions.Add(new FSMNullTransition(initState));

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

        public PlayerGeneralStates(PlayerController player) : base(player, "general") { CreateFSM(); }

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

        //private GameObject carPrefab;
        //private GameObject carInstance;

        [Header("States")]
        public SchCarController car;
        public SchBoatController boat;
        public SchPlaneController plane;
        public Accumulator turn;

        public AudioSource audioSource;
        public AudioClip damageSound;

        public InputManager inputMgr;

        // what is currently controlled by the player?
        // it might be different depending on states.
        // the playercontroller does not have relevant data when it comes
        // to rigidbody, transform, etc...
        struct controlled
        {
            public Rigidbody rb;
        };
        controlled current;


        // do not use directly transform, rigidbody, etc...
        public Transform GetTransform()
        {
            return current.rb ? current.rb.transform : transform;
        }
        // transform is readonly
        //public void SetTransform(Transform t)

        // Rigidbody can be different depending on which transform we are using.
        public Rigidbody GetRigidbody()
        {
            return current.rb;
        }

        void Awake()
        {
            // SchCarController can't be null
            if (car == null)
            {
                this.LogError("car property cannot be null! Please assign an gameobject to car.");
            }

            // nocheckin
            //if (car_old == null && car_new == null)
            //{
            //    if (carInstance == null)
            //    {
            //        carInstance = Instantiate(carPrefab);
            //        carInstance.transform.parent = gameObject.transform;
            //    }
            //    car_new = carInstance.GetComponent<SchCarController>();
            //    car_old = carInstance.GetComponent<CarController>();
            //}
            //carInstance.SetActive(false);

            //if (planeInstance == null) {
            //    planeInstance = Instantiate(planePrefab);
            //    planeInstance.transform.parent = gameObject.transform;
            //}

            // create states if nuul
            if (generalStates == null)
            generalStates = new PlayerGeneralStates(this);
            if (vehicleStates == null)
            vehicleStates = new PlayerVehicleStates(this);

            if (self_PowerController == null)
            self_PowerController = GetComponent<PowerController>();

            if (self_PowerController == null)
            {
                this.LogError("No powercontroller, please assign one or add a PowerController script to this object.");
            }

            // Always freeze when starting.
            Freeze();
        }

        // ----- Scene listeners
        void InitSceneListeners()
        {
            var scene_loader = Access.SceneLoader();
            if (scene_loader == null)
            {
                this.LogError("No scene loader available, world might be broken!");
            }

            scene_loader.beforeLoadScene.AddListener(OnBeforeLoadScene);
            scene_loader.beforeEnableScene.AddListener(OnBeforeEnableScene);
            scene_loader.afterLoadScene.AddListener(OnAfterLoadScene);
        }

        void OnBeforeLoadScene()
        {
            vehicleStates.SetState(vehicleStates.states[(int)PlayerVehicleStates.States.Init]);
            Freeze();
        }

        void OnBeforeEnableScene(Scene toBeEnabled)
        {

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

        void OnAfterLoadScene()
        {
            Access.invalidate();
            car.car.constraintSolver.solver.bodyInterface.Init();
        }
        //------ end scene listeners

        void Start()
        {
            if (inputMgr == null)
            {
                this.LogWarn("Input manager is null. Using default player 1.");
                inputMgr = Access.PlayerInputsManager().player1;
            }


            if (inputMgr == null)
            {
                this.LogError("Input manager is null even after init.");
            }

            inputMgr.Attach(this);

            InitSceneListeners();
        }

        void OnDestroy()
        {
            inputMgr.Detach(this);

            var sceneLoader = Access.SceneLoader();
            if (sceneLoader)
            {

                sceneLoader.beforeLoadScene.RemoveListener(OnBeforeLoadScene);
                sceneLoader.afterLoadScene.RemoveListener(OnAfterLoadScene);
                sceneLoader.beforeEnableScene.RemoveListener(OnBeforeEnableScene);
            }
        }


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

        #if false
        public void ResetSpringSizeMinAndUnlock()
        {
            foreach (var axle in car_old.axles)
            {
                var suspensionRight = axle.right.suspension;
                suspensionRight.SetMinLength(car_old.springMin);
                suspensionRight.SetMaxLength(car_old.springMax);
                suspensionRight.SetRestLength(car_old.springRestPercent);


                var suspensionLeft = axle.left.suspension;
                suspensionLeft.SetMinLength(car_old.springMin);
                suspensionLeft.SetMaxLength(car_old.springMax);
                suspensionLeft.SetRestLength(car_old.springRestPercent);
            }
            car_old.overrideMaxSpring = false;
        }

        public void SetSpringSizeMinAndLock()
        {
            springElapsedCompression += Time.deltaTime;
            float springCompVal = Mathf.Lerp(car_old.springMax, car_old.springMin + 0.1f, springElapsedCompression / springCompressionTime);
            springCompVal = Mathf.Min(1, springCompVal);


            float springJumpFactor = jumpCompressionOverTime.Evaluate(Mathf.Min(1, springElapsedCompression / springCompressionTime));

            jumpDecal.SetAnimationTime(springJumpFactor);
            foreach (var axle in car_old.axles)
            {
                var suspensionRight = axle.right.suspension;
                suspensionRight.SetMinLength(car_old.springMin);
                suspensionRight.SetMaxLength(springCompVal);
                suspensionRight.SetRestLength(car_old.springRestPercent);


                var suspensionLeft = axle.left.suspension;
                suspensionLeft.SetMinLength(car_old.springMin);
                suspensionLeft.SetMaxLength(springCompVal);
                suspensionLeft.SetRestLength(car_old.springRestPercent);
            }
            car_old.overrideMaxSpring = true;
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

                foreach (var axle in car_old.axles)
                {
                    car_old.rb.AddForceAtPosition(jump.value * springJumpFactor * transform.up * (axle.right.isGrounded ? 1 : 0), axle.right.suspension.GetAnchorA(), ForceMode.VelocityChange);
                    car_old.rb.AddForceAtPosition(jump.value * springJumpFactor * transform.up * (axle.right.isGrounded ? 1 : 0), axle.left.suspension.GetAnchorA(), ForceMode.VelocityChange);
                }
                jump.applyForceMultiplier = false;
                springElapsedCompression = 0f;
                jumpDecal.SetAnimationTime(0f);
            }

        }

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
            car_old.rb.AddForce(turboDir * turbo.strength, ForceMode.VelocityChange);

            turbo.intervalElapsedTime = 0f;
        }
        #endif

        public bool TouchGround()
        {
            #if false
            if (car_old)
            {
                foreach (var a in car_old.axles)
                {
                    if (a.left.isGrounded || a.right.isGrounded)
                    {
                        return true; // early return
                    }
                }
            }
            #endif
            return false;
        }

        private bool isTouchingWater = false;
        public void SetTouchingWater(bool state)
        {
            isTouchingWater = state;
        }

        public bool TouchGroundAll()
        {
            #if false
            if (car_old)
            {
                foreach (var a in car_old.axles)
                {
                    if (!a.left.isGrounded || !a.right.isGrounded)
                    {
                        return false; // early return
                    }
                }
            }
            #endif
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

        public void ActivateMode(GameObject go, Rigidbody rb) {
            go.SetActive(true);

            if (current.rb)
            {
                rb.transform.position = current.rb.transform.position;
                rb.transform.rotation = current.rb.transform.rotation;
                rb.velocity = current.rb.velocity;
                rb.angularVelocity = current.rb.angularVelocity;
                rb.isKinematic = current.rb.isKinematic;
            }

            current.rb = rb;

            Access.CameraManager().OnTargetChange(GetTransform());
        }

        public void DeactivateMode(GameObject go) {
            go.SetActive(false);
            // IMPORTANT: do not set to null as the position/rotation needs to be copied to
            // init the next rb.
            //current.rb = null;
            Access.CameraManager().OnTargetChange(GetTransform());
        }

        public bool IsInMenu() { return isInMenu; }
        public void Freeze() { isInMenu = true; if (current.rb) current.rb.isKinematic = true; MuteSound(); }
        public void UnFreeze() { isInMenu = false; if (current.rb) current.rb.isKinematic = false; UnMuteSound(); }
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
            #if false
            if (car_old)
            {
                var rear = car_old.axles[(int)AxleType.rear];
                rear.left.isHandbraked = v;
                rear.right.isHandbraked = v;

                var front = car_old.axles[(int)AxleType.front];
                front.left.isHandbraked = v;
                front.right.isHandbraked = v;
            }
            #endif
        }

        #if false
        public void SetCarCenterOfMass()
        {
            car_old.centerOfMass.transform.localPosition = car_old.centerOfMassInitial + new Vector3(jump.diRollUnscaled.average * weightControlMaxX, 0f, jump.diPitchUnscaled.average * weightControlMaxZ);
        }
        #endif

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
            GetRigidbody().AddForce(repulseForce, ForceMode.Impulse);

            elapsedTimeSinceLastDamage = 0f;
            //stateMachine.ForceState(invulState);
        }

        bool modifierCalled = false;

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {

            #if false
            if (car_old)
            {
                // dirty fix for respawn when slipping
                foreach (var a in car_old.axles)
                {
                    a.right.slipX = 1;
                    a.right.slipY = 1;

                    a.left.slipX = 1;
                    a.left.slipY = 1;
                }
            }
            #endif


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
}
