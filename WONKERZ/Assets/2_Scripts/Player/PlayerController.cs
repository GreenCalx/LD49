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
    public class PlayerController : MonoBehaviour, IControllable
    {
        // Simple State Machine
        public enum PlayerStates
        {
            None,
            Init,
            InMenu,
            Frozen,
            Alive,
            Dead,
            Count,
        };
        public PlayerStates playerState {
            get;
            private set;
        }

        public enum PlayerVehicleStates
        {
            None,
            Car,
            Boat,
            Plane,
            Count,
        };
        public PlayerVehicleStates vehicleState {
            get;
            private set;
        }

        // Flags
        public static readonly int FJump = 1;
        public BitVector32 flags = new BitVector32(0);

        public void TransitionTo(PlayerVehicleStates to) {
            TransitionFromTo(vehicleState, to);
        }

        private void TransitionFromTo(PlayerStates from, PlayerStates to) {
            // TODO: fix me, only temporary
            // should be done only if possible.
            playerState = to;

            // glabal states.
            if (from == PlayerStates.Frozen) {
                if (current.rb) current.rb.isKinematic = false;
                UnMuteSound();
                return;
            }
            if (to == PlayerStates.Frozen) {
                if (current.rb) current.rb.isKinematic = true;
                MuteSound();
                return;
            }

            if (to == PlayerStates.Dead) {
                Kill();
                Access.CheckPointManager().loadLastCP(true);
                return;
            }

            switch (from)
            {
                case PlayerStates.None:
                    {
                        switch (to)
                        {
                            case PlayerStates.None:
                                case PlayerStates.Init:
                                case PlayerStates.InMenu:
                                case PlayerStates.Frozen:
                                case PlayerStates.Alive:
                                case PlayerStates.Dead:
                                case PlayerStates.Count:
                                {
                                    break;
                                }
                        }
                        break;
                    }
                case PlayerStates.Init:
                    {

                        switch (to)
                        {
                            case PlayerStates.None:
                                case PlayerStates.Init:
                                case PlayerStates.InMenu:
                                case PlayerStates.Frozen:
                                case PlayerStates.Alive:
                                case PlayerStates.Dead:
                                case PlayerStates.Count:
                                {
                                    break;
                                }
                        }
                        break;
                    }

                case PlayerStates.InMenu: {
                    switch (to)
                    {
                        case PlayerStates.None:
                            case PlayerStates.Init:
                            case PlayerStates.InMenu:
                            case PlayerStates.Frozen:
                            case PlayerStates.Alive:
                            case PlayerStates.Dead:
                            case PlayerStates.Count:
                            {
                                break;
                            }
                    }
                    break;
                }
                case PlayerStates.Frozen: {
                    switch (to)
                    {
                        case PlayerStates.None:
                            case PlayerStates.Init:
                            case PlayerStates.InMenu:
                            case PlayerStates.Frozen:
                            case PlayerStates.Alive:
                            case PlayerStates.Dead:
                            case PlayerStates.Count:
                            {
                                break;
                            }
                    }
                    break;
                }
                case PlayerStates.Alive: {
                    switch (to)
                    {
                        case PlayerStates.None:
                            case PlayerStates.Init:
                            case PlayerStates.InMenu:
                            case PlayerStates.Frozen:
                            case PlayerStates.Alive:
                            case PlayerStates.Dead:
                            case PlayerStates.Count:
                            {
                                break;
                            }
                    }
                    break;
                }
                case PlayerStates.Dead: {
                    switch (to)
                    {
                        case PlayerStates.None:
                            case PlayerStates.Init:
                            case PlayerStates.InMenu:
                            case PlayerStates.Frozen:
                            case PlayerStates.Alive:
                            case PlayerStates.Dead:
                            case PlayerStates.Count:
                            {
                                break;
                            }
                    }
                    break;
                }
                case PlayerStates.Count: {
                    break;
                }
            }
        }

        private void TransitionFromTo(PlayerVehicleStates from, PlayerVehicleStates to) {
            // TODO: fix me, only temporary
            // should be done only if possible.
            vehicleState = to;

            switch (from)
            {
                case PlayerVehicleStates.None:
                    {
                        switch (to)
                        {
                            case PlayerVehicleStates.None:
                                case PlayerVehicleStates.Count:
                                {
                                    break;
                                }

                            case PlayerVehicleStates.Car:
                                {
                                    // if (!car.init) init();
                                    ActivateMode(car.gameObject, car.car.chassis.GetBody());
                                    break;
                                }
                            case PlayerVehicleStates.Plane:
                                {
                                    ActivateMode(plane.gameObject, plane.plane.rb);
                                    break;
                                }
                            case PlayerVehicleStates.Boat:
                                {
                                    ActivateMode(boat.gameObject, boat.boat.rb);
                                    break;
                                }
                        }
                        break;
                    }

                case PlayerVehicleStates.Car:
                    case PlayerVehicleStates.Boat:
                    case PlayerVehicleStates.Plane:
                    {
                        DeactivateMode(from);
                        switch (to)
                        {
                            case PlayerVehicleStates.Car:
                                {
                                    ActivateMode(car.gameObject, car.car.chassis.GetBody());
                                    break;
                                }
                            case PlayerVehicleStates.Plane:
                                {
                                    ActivateMode(plane.gameObject, plane.plane.rb);
                                    break;
                                }
                            case PlayerVehicleStates.Boat:
                                {
                                    ActivateMode(boat.gameObject, boat.boat.rb);
                                    break;
                                }
                            case PlayerVehicleStates.Count:
                                {
                                    break;
                                }
                        }
                        break;
                    }
            }
        }

        public PowerController self_PowerController;

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
        public struct controlled
        {
            public Rigidbody rb;
        };
        public controlled current;

        float jumpTimeCurrent    = 0.0f;
        float jumpLatencyCurrent = 0.0f;
        bool  jumpLock           = true;

        bool IsInJumpLatency() => jumpLatencyCurrent > 0.0f;

        public bool IsInAir() {
            switch (vehicleState) {
                case PlayerVehicleStates.Car:
                    return !(car.car as WkzCar).IsTouchingGround();
                case PlayerVehicleStates.Boat:
                    return (boat.boat as WkzBoat).IsInAir();
            }
            return false;
        }

        public float GetGroundAerialLatency()
        {
            return (car.car as WkzCar).wkzDef.groundAerialSwitchLatency;
        }

        public float GetAerialMaxForce()
        {
            return (car.car as WkzCar).wkzDef.aerialMaxForce;
        }

        public Vector3 GetWeightContralMaxValues()
        {
            var wkzCar = (car.car as WkzCar);
            return new Vector3(wkzCar.wkzDef.weightControlMaxX, wkzCar.wkzDef.weightControlMaxY, wkzCar.wkzDef.weightControlMaxZ);
        }

        public float GetWeightControlSpeed()
        {
            return (car.car as WkzCar).wkzDef.weightControlSpeed;
        }

        public ParticleSystem GetSpeedParticles() {
            return (car.car as WkzCar).speedEffect.particles;
        }

        public void ApplySpeedEffect() {
            // Apply camera effect.
            // Will change FoV, distance, etc.

            // TODO: makethis work 
            var effectRatio = 0.0f;

            CameraManager CamMgr = Access.CameraManager();
            if (CamMgr?.active_camera is PlayerCamera)
            {
                PlayerCamera pc = (PlayerCamera)CamMgr.active_camera;
                pc.applySpeedEffect(effectRatio);
            }
            // Apply particles effect.
            // Will display speed trails.
            var particles = GetSpeedParticles();
            if (particles) {
                var e = particles.emission;
                e.enabled = effectRatio != 0.0f;
                var rb = GetRigidbody();
                var relativeWindDir = rb.velocity;
                particles.transform.LookAt(rb.position + relativeWindDir);

                var lifemin  = 0.2f;
                var lifemax  = 0.6f;
                var partmain = particles.main;
                partmain.startLifetime = Mathf.Lerp(lifemin, lifemax, effectRatio);
            }
        }

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

            playerState  = PlayerStates.None;
            vehicleState = PlayerVehicleStates.None;

            if (self_PowerController == null)
            self_PowerController = GetComponent<PowerController>();

            if (self_PowerController == null)
            {
                this.LogError("No powercontroller, please assign one or add a PowerController script to this object.");
            }

            TransitionFromTo(playerState, PlayerStates.Frozen);
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
            TransitionFromTo(playerState, PlayerStates.Frozen);
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

        // Jump logic
        void OnJumpPressed() {
            switch (vehicleState) {
                case PlayerVehicleStates.Plane:
                    case PlayerVehicleStates.Car: {
                        (car.car as WkzCar).SetSuspensionTargetPosition();
                        break;
                    }

                case PlayerVehicleStates.Boat: {
                    (boat.boat as WkzBoat).SetFloatersSize(GetJumpCompressionRatio());
                    break;
                }
            }

            jumpDecal.SetJumpTime(GetJumpCompressionRatio());
        }

        void OnJumpReleased() {

        }

        void OnJumpLatency() {
            jumpDecal.SetLatencyTime(Mathf.Clamp01(jumpLatencyCurrent / (car.car as WkzCar).wkzDef.jumpDef.latency));
        }

        float GetJumpCompressionRatio() => 1.0f - Mathf.Clamp01(jumpTimeCurrent / (car.car as WkzCar).wkzDef.jumpDef.time);

        void UpdateJump(float dt) {
            if (jumpTimeCurrent > 0.0f)
            {
                jumpTimeCurrent -= dt;

                OnJumpPressed();
            }
            else if (jumpLatencyCurrent > 0.0f)
            {
                jumpLatencyCurrent -= Time.deltaTime;

                OnJumpLatency();
            }
        }
        // end jump logic.


        // weight logic
        float groundAerialLatencyCurrent = 0.0f;
        [SerializeField]
        Math.AccumulatorV2 weightInput = new Math.AccumulatorV2();

        void UpdateWeight(float dt)
        {
            var rb = GetRigidbody();
            if (rb != null)
            {
                var rbBehaviour = rb.GetComponent<SchRigidbodyBehaviour>();

                if (rbBehaviour)
                {
                    bool isInAir = IsInAir();
                    if (flags[PlayerController.FJump] != isInAir)
                    {
                        // start latency between air/ground.
                        groundAerialLatencyCurrent = GetGroundAerialLatency();
                    }
                    flags[PlayerController.FJump] = isInAir;

                    if (groundAerialLatencyCurrent > 0.0f) groundAerialLatencyCurrent -= dt;
                    else
                    {
                        if (isInAir) rbBehaviour.SetAddedLocalCOMOffset(Vector3.zero);
                        else
                        {
                            var weightControlMax = GetWeightContralMaxValues();

                            var maxVect = new Vector3(weightControlMax.x, .0f, weightControlMax.z);
                            var weightInputValue = weightInput.average;
                            var targetOffset = new Vector3(weightInputValue.x * weightControlMax.x, 0f, weightInputValue.y * weightControlMax.z);
                            // try hemisphere instead of plan.
                            // COM will be lowered the farther out it is.
                            var Y = (targetOffset.magnitude / maxVect.magnitude) * weightControlMax.y;
                            targetOffset.y = Y;

                            var currentOffset = rbBehaviour.GetAddedLocalCOMOffest();
                            var diffOffset = targetOffset - currentOffset;
                            var offset     = currentOffset + (diffOffset * Mathf.Clamp01(GetWeightControlSpeed() * dt));
                            rbBehaviour.SetAddedLocalCOMOffset(offset);
                        }
                    }
                }
            }
        }
        // end weight logic

        void Update()
        {
            elapsedTimeSinceLastDamage += Time.deltaTime;

            // Simple State Machine
            switch (playerState)
            {
                case PlayerStates.None:
                    case PlayerStates.Dead:
                    case PlayerStates.Count:
                    case PlayerStates.Frozen:
                    case PlayerStates.InMenu:
                    {
                        // do nothing;
                        break;
                    }
                case PlayerStates.Init:
                    {
                        break;
                    }

                case PlayerStates.Alive:
                    {
                        switch (vehicleState)
                        {
                            case PlayerVehicleStates.Boat:
                                case PlayerVehicleStates.Plane:
                                case PlayerVehicleStates.Car:
                                {
                                    UpdateJump(Time.deltaTime);
                                    UpdateWeight(Time.deltaTime);
                                }
                                break;
                        }
                        break;
                    }
            }

            weightInput.Reset();
        }

        void FixedUpdate()
        {
            // Simple State Machine
            switch (playerState)
            {
                case PlayerStates.None:
                    case PlayerStates.Dead:
                    case PlayerStates.Count:
                    case PlayerStates.Frozen:
                    case PlayerStates.InMenu:
                    {
                        // do nothing;
                        break;
                    }
                case PlayerStates.Init:
                    {
                        break;
                    }

                case PlayerStates.Alive:
                    {

                        switch (vehicleState) {
                            case PlayerVehicleStates.Car :{
                                var rb     = GetRigidbody();
                                if (rb)
                                {
                                    var aerialMaxForce = GetAerialMaxForce();
                                    var torque = new Vector3(aerialMaxForce * weightInput.average.x,
                                        0,
                                        -aerialMaxForce * weightInput.average.y);

                                    torque = rb.transform.TransformDirection(torque);

                                    rb.AddTorque(torque * Time.fixedDeltaTime, ForceMode.VelocityChange);
                                }
                                break;
                            }
                        }

                        break;
                    }
            }
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

        // helper
        public bool IsBoat() {
            return vehicleState == PlayerVehicleStates.Boat;
        }

        public bool IsCar() {
            return vehicleState == PlayerVehicleStates.Car;
        }

        public bool IsPlane() {
            return vehicleState == PlayerVehicleStates.Plane;
        }
        // end helpers

        public bool IsAlive()
        {
            if (isTouchingWater && !IsBoat())
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

            var lastrb = current.rb;
            current.rb = rb;

            if (lastrb)
            {
                ForceTransform(lastrb.transform.position, lastrb.transform.rotation);
                ForceVelocity(lastrb.velocity, lastrb.angularVelocity);
                rb.isKinematic        = lastrb.isKinematic;
            }

            Access.CameraManager().OnTargetChange(GetTransform());
        }

        public void DeactivateMode(PlayerVehicleStates mode) {
            GameObject go = null;
            switch (mode) {
                case PlayerVehicleStates.Boat:
                    go = boat.gameObject;
                    break;
                case PlayerVehicleStates.Car:
                    go = car.gameObject;
                    break;
                case PlayerVehicleStates.Plane:
                    go = plane.gameObject;
                    break;
            }
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

                if (IsCar())
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

                if (IsCar())
                {
                    car.GetCar().ResetWheels();
                }
            }
        }

        public bool IsInMenu() { return playerState == PlayerStates.InMenu; }
        // TODO: fix me, this is completely wrong!
        public void Freeze() { TransitionFromTo(playerState, PlayerStates.Frozen); }
        public void UnFreeze() {
            if (playerState != PlayerStates.Frozen) {
                this.LogError("Weird : trying to unfreeze an object that is not currently frozen");
            }
            TransitionFromTo(playerState, PlayerStates.Alive);
        }
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

        void IControllable.ProcessInputs(InputManager currentMgr, GameController inputs)
        {
            int weightX = (int)PlayerInputs.InputCode.WeightX;
            int weightY = (int)PlayerInputs.InputCode.WeightY;
            int jump    = (int)PlayerInputs.InputCode.Jump;
            // Simple State Machine
            switch (playerState)
            {
                case PlayerStates.None:
                    case PlayerStates.Dead:
                    case PlayerStates.Count:
                    case PlayerStates.Frozen:
                    case PlayerStates.InMenu:
                    case PlayerStates.Init:
                    {
                        // do nothing;
                        break;
                    }

                case PlayerStates.Alive:
                    {
                        switch (vehicleState) {
                            case PlayerVehicleStates.None:
                                case PlayerVehicleStates.Count:
                                {
                                    break;
                                }
                            case PlayerVehicleStates.Car:
                                {
                                    if (inputs.GetButtonState((int)PlayerInputs.InputCode.AirplaneMode).down) {
                                        TransitionFromTo(vehicleState, PlayerVehicleStates.Plane);
                                    } else if (inputs.GetButtonState((int)PlayerInputs.InputCode.BoatMode).down) {
                                        TransitionFromTo(vehicleState, PlayerVehicleStates.Boat);
                                    } else {
                                        weightInput.Add(inputs.GetAxisState(weightX).valueSmooth, inputs.GetAxisState(weightY).valueSmooth);

                                        var jumpValue = inputs.GetButtonState(jump);
                                        var wkzCar = car.car as WkzCar;
                                        if (jumpValue.up)
                                        {
                                            if (!jumpLock) wkzCar.StopSuspensionCompression();
                                        }
                                        else if (jumpValue.down)
                                        {
                                            if (!IsInJumpLatency()) wkzCar.StartSuspensionCompression();
                                        }

                                        car.ProcessInputs(currentMgr, inputs);
                                    }
                                    break;
                                }
                            case PlayerVehicleStates.Boat:
                                {
                                    if (inputs.GetButtonState((int)PlayerInputs.InputCode.AirplaneMode).down) {
                                        TransitionFromTo(vehicleState, PlayerVehicleStates.Plane);
                                    } else if (inputs.GetButtonState((int)PlayerInputs.InputCode.BoatMode).down) {
                                        TransitionFromTo(vehicleState, PlayerVehicleStates.Car);
                                    } else {
                                        weightInput.Add(inputs.GetAxisState(weightX).valueSmooth, inputs.GetAxisState(weightY).valueSmooth);
                                        boat.ProcessInputs(currentMgr, inputs);
                                    }
                                    break;
                                }
                            case PlayerVehicleStates.Plane:
                                {
                                    if (inputs.GetButtonState((int)PlayerInputs.InputCode.AirplaneMode).down) {
                                        TransitionFromTo(vehicleState, PlayerVehicleStates.Car);
                                    } else if (inputs.GetButtonState((int)PlayerInputs.InputCode.BoatMode).down) {
                                        TransitionFromTo(vehicleState, PlayerVehicleStates.Boat);
                                    } else {
                                        weightInput.Add(inputs.GetAxisState(weightX).valueSmooth, inputs.GetAxisState(weightY).valueSmooth);
                                        plane.ProcessInputs(currentMgr, inputs);
                                    }
                                    break;
                                }
                        } //! switch carMode

                        if (inputs.GetButtonState((int)PlayerInputs.InputCode.UnequipPower).down)
                        {
                            self_PowerController.UnequipPower();
                        }

                        break;
                    } 
            }//! switch playerState


        }
    }
}
