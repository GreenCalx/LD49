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
            player.ActivateMode(player.boat.gameObject, player.boat.boat.rb);
        }

        public override void OnExit(FSMBase fsm)
        {
            var player = (fsm as PlayerFSM).GetPlayer();
            player.DeactivateMode(player.boat.gameObject);
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
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
                        player.vehicleStates.SetState(player.vehicleStates.states[(int)PlayerVehicleStates.States.Car]);
                    }
                }

                player.boat.ProcessInputs(currentMgr, Entry);
            }
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
            if (!player.IsInMenu())
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

                player.plane.ProcessInputs(currentMgr, Entry);
            }
        }
    }

    // FSM when in ground mode
    public class GroundState : PlayerState, IControllable
    {
        public class GeneralGroundAction : FSMAction
        {
            float groundAerialLatencyCurrent = 0.0f;
            public override void Execute(FSMBase fsm)
            {
                var player = (fsm as PlayerFSM).GetPlayer();
                var car = player.car.GetCar();

                var isInAir = !car.IsTouchingGround();
                if (player.flags[PlayerController.FJump] != isInAir)
                {
                    // start latency between air/ground.
                    groundAerialLatencyCurrent = (car as WkzCar).wkzDef.groundAerialSwitchLatency;
                }
                player.flags[PlayerController.FJump] = isInAir;

                if (groundAerialLatencyCurrent > 0.0f) {
                    groundAerialLatencyCurrent -= Time.fixedDeltaTime;
                }
                else
                {
                    if (isInAir)
                    {
                        player.car.GetCar().ResetCarCenterOfMass();
                        player.car.GetCar().SetCarAerialTorque(Time.fixedDeltaTime);
                    }
                    else
                    {
                        player.car.GetCar().SetCarCenterOfMass(Time.fixedDeltaTime);
                    }
                }
            }
        }

        private class SpeedEffect : FSMAction
        {
            public override void Execute(FSMBase fsm)
            {
                var player = (fsm as PlayerFSM).GetPlayer();

                var playerCarController = player.car;
                var playerCar           = playerCarController.car;
                var effectRatio = (playerCar as WkzCar).GetSpeedEffectRatio();
                // Speed effect on camera
                // update camera FOV/DIST if a PlayerCamer
                CameraManager CamMgr = Access.CameraManager();
                if (CamMgr?.active_camera is PlayerCamera)
                {
                    PlayerCamera pc = (PlayerCamera)CamMgr.active_camera;
                    pc.applySpeedEffect(effectRatio);
                }

                var particles = (playerCar as WkzCar).speedEffect.particles;

                var e = particles.emission;
                e.enabled = effectRatio != 0.0f;

                var relativeWindDir = playerCar.GetChassis().GetBody().velocity;
                particles.transform.LookAt(playerCar.GetChassis().GetBody().position + relativeWindDir);

                var lifemin  = 0.2f;
                var lifemax  = 0.6f;
                var partmain = particles.main;
                partmain.startLifetime = Mathf.Lerp(lifemin, lifemax, effectRatio);
            }
        }

        public GroundState(PlayerController player) : base(player, "ground")
        {
            this.actions.Add(new SpeedEffect());
            this.fixedActions.Add(new GeneralGroundAction());
        }

        public override void OnEnter(FSMBase fsm)
        {
            base.OnEnter(fsm);
            var player = (fsm as PlayerFSM).GetPlayer();
            player.ActivateMode(player.car.gameObject, player.car.car.GetChassis().GetBody());
            // car also need to reset wheels velocity.
            player.car.GetCar().ResetWheels();
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

                // powers
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
        public WkzCarController car;
        public WkzBoatController boat;
        public WkzGliderController plane;
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
            //car.car.constraintSolver.solver.bodyInterface.Init();
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
        }

        public WonkerDecal jumpDecal;

        #if false
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

        private bool isTouchingWater = false;
        public void SetTouchingWater(bool state)
        {
            isTouchingWater = state;
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
                rb.velocity           = current.rb.velocity;
                rb.angularVelocity    = current.rb.angularVelocity;
                rb.isKinematic        = current.rb.isKinematic;
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

        public void ForceTransform(Vector3 position, Quaternion rotation) {
            var t = GetTransform();
            if (t != null)
            {
                t.position = position;
                t.rotation = rotation;

                if (vehicleStates.GetState() == vehicleStates.states[(int)PlayerVehicleStates.States.Car])
                {
                    car.GetCar().ResetWheels();
                }
            }
        }

        public void ForceVelocity(Vector3 linear, Vector3 angular) {
            var r = GetRigidbody();
            if (r != null)
            {
                r.velocity = linear;
                r.angularVelocity = angular;

                if (vehicleStates.GetState() == vehicleStates.states[(int)PlayerVehicleStates.States.Car])
                {
                    car.GetCar().ResetWheels();
                }
            }
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
