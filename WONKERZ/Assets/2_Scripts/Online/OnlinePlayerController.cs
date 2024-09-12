using Mirror;
using Schnibble;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wonkerz;

// Network object specs
//
// Client side:
//
// Server side:

public class OnlinePlayerController : NetworkBehaviour
{
    public static OnlinePlayerController localPlayer = null;

    [Header("OnlinePlayerController")]
    string _onlinePlayerName;
    public string onlinePlayerName { get => _onlinePlayerName; set { _onlinePlayerName = value; onAnyChange?.Invoke(); } }

    public OnlineCollectibleBag bag;

    [Header("Mand Refs")]
    public PlayerController self_PlayerController;
    public PowerController self_powerController;
    public Transform self_weightMeshHandle;
    public GameObject prefabCameraFocusable;
    public List<OnlineDamager> self_oDamagers;
    public OnlineDamageable self_oDamageable;

    [Header("Online Tweaks")]
    public float minSpeedToDoDamage = 30f;
    [Header("Internals")]
    // Client has authority on this,
    // It will not modify it directly but send a cmd to try to modify it.
    // When the server sets it, the client listen for the change and react to it.
    [SyncVar]
    bool _isReady = false;
    public bool isReady { get => _isReady; private set { _isReady = value; onAnyChange?.Invoke(); } }
    [SyncVar]
    bool _isLoaded = false;
    public bool isLoaded { get => _isLoaded; private set { _isLoaded = value; onAnyChange?.Invoke(); } }
    [SyncVar]
    bool _isSpawned = false;
    public bool isSpawned { get => _isSpawned; private set { _isSpawned = value; onAnyChange?.Invoke(); } }

    // TODO: Remove this hack and do a proper online object.
    [SyncVar(hook = nameof(OnPlayerStateChanged))]
    PlayerController.PlayerStates playerState;
    [SyncVar(hook = nameof(OnPlayerVehicleStateChanged))]
    PlayerController.PlayerVehicleStates playerVehicleState;
    // wheelOmega needs to be updated by the server.
    SyncList<float> omegas = new SyncList<float>();

    public void OnUpdateOmegas(SyncList<float>.Operation op, int itemIndex, float oldItem, float newItem)
    {
        this.Log("OnUpdateOmegas");
        if (omegas.Count != 4) {
            this.Log("omegas is not yet ready, count is " + omegas.Count);
        }
        else
        {
            if (self_PlayerController == null)
            {
                this.LogError("OnUpdateOmegas: player is null");
                return;
            }
            if (self_PlayerController.car == null)
            {
                this.LogError("OnUpdateOmegas: car is null");
                return;
            }
            if (self_PlayerController.car.GetCar() == null)
            {
                this.LogError("OnUpdateOmegas: car is null");
                return;
            }
            if (self_PlayerController.car.GetCar().chassis == null)
            {
                this.LogError("OnUpdateOmegas: chassis is null");
                return;
            }

            var chassis = self_PlayerController.car.GetCar().chassis;
            var axle = chassis.axles[0];

            axle.right.SetAngularVelocity(omegas[0]);
            axle.left.SetAngularVelocity(omegas[1]);

            axle = chassis.axles[1];
            axle.right.SetAngularVelocity(omegas[2]);
            axle.left.SetAngularVelocity(omegas[3]);
        }
    }

    public Transform cameraFocusable;

        // Will be called when anything changes on this online player.
        // To do this we removed any public variable to have getter/setter
        // that call this callback if anyone is listening.
        public Action onAnyChange;

        public bool IsSpawned()       => isSpawned;
        public bool IsReady()         => isReady;
        public bool IsLoaded()        => isLoaded;
        public bool IsLockAndLoaded() => IsReady() && IsLoaded();

        void FixedUpdate()
        {
            if ((self_oDamagers != null) && (self_oDamagers.Count > 0))
            UpdatePlayerDamagers();

            if (isServer)
            {
                if (IsLoaded())
                {
                    if (self_PlayerController.IsCar())
                    {
                        this.LogError("FixedUpdate : UpdateOmegas");
                        var chassis = self_PlayerController.car.GetCar().chassis;
                        var axle = chassis.axles[0];
                        omegas[0] = axle.right.GetAngularVelocity();
                        omegas[1] = axle.left.GetAngularVelocity();

                        axle = chassis.axles[1];
                        omegas[2] = axle.right.GetAngularVelocity();
                        omegas[3] = axle.left.GetAngularVelocity();
                    }
                }
            }
        }

        private void UpdatePlayerDamagers()
        {
            int damage = 0;
            WkzCar cc = self_PlayerController.car.GetCar();
            if (cc.GetCurrentSpeedInKmH() > minSpeedToDoDamage)
            {
                damage = (int)Mathf.Abs((float)cc.GetCurrentSpeedInKmH());
                damage += (int)Mathf.Floor((self_PlayerController.GetRigidbody().mass * 0.01f));
            }

            foreach (OnlineDamager d in self_oDamagers)
            {
                d.damage = damage;
            }
        }

        public void InitPlayerDamagers()
        {
            if (!isLoaded && !isSpawned)
            {
                this.LogError("InitPlayerDamegeable : not loaded or spawned.");
                return;
            }

            self_oDamageable = null;

            // Safely get the current player's rigidbody as a Car.
            // It might be null, for instance if the state is not yet a rigidbody compliant state.
            if (self_PlayerController == null) {
                this.LogError("InitPlayerDameger : not loaded or spawned.");
                return;
            }

            if (self_PlayerController.vehicleState != PlayerController.PlayerVehicleStates.Car) {
                this.LogError("InitPlayerDameger : only implemented for car.");
                return;
            }

            if (!self_PlayerController.car || !self_PlayerController.car.GetCar()) {
                this.LogError("InitPlayerDameger : cannot get car");
                return;
            }

            if (self_PlayerController.GetRigidbody() == null) {
                this.LogError("InitPlayerDameger : rigidbody is null");
                return;
            }
            if (self_PlayerController.GetRigidbody().gameObject == null) {
                this.LogError("InitPlayerDameger : rigidbody's gameobject is null");
                return;
            }

            self_oDamagers = new List<OnlineDamager>(5);

            // body damager
            GameObject       bodyRef     = self_PlayerController.GetRigidbody().gameObject;
            OnlineDamager    bodyDmgr;
            if (!bodyRef.TryGetComponent<OnlineDamager>(out bodyDmgr)) {
                this.LogWarn("InitPlayerDamager : cannot retrieve OnlineDamager on body => trying to create one.");
                bodyDmgr = bodyRef.AddComponent<OnlineDamager>();
            }
            if (bodyDmgr == null) {
                this.LogError("InitPlayerDamager : cannot retrieve OnlineDamager on body or create one => catastrophique failure.");
                return;
            }

            // Finally set references.
            bodyDmgr.owner = gameObject;
            self_oDamagers.Add(bodyDmgr);

            // wheel damagers
            List<WkzWheelCollider> wheels = new List<WkzWheelCollider>(self_PlayerController.gameObject.GetComponentsInChildren<WkzWheelCollider>());
            if (wheels.Count == 0) {
                this.LogError("InitPlayerDamager : cannot retrieve wheelColliders.");
                return;
            }

            foreach (WkzWheelCollider w in wheels)
            {
                OnlineDamager wheelDmgr;
                if (!w.gameObject.TryGetComponent<OnlineDamager>(out wheelDmgr)){
                    this.LogWarn("InitPlayerDamager : cannot retrieve OnlineDamager on wheel => trying to create one.");
                    wheelDmgr = w.gameObject.AddComponent<OnlineDamager>();
                }

                if (wheelDmgr == null) {
                    this.LogError("InitPlayerDamager : cannot retrieve OnlineDamager on wheel or create one => catastrophique failure.");
                    continue;
                }

                wheelDmgr.owner = gameObject;
                self_oDamagers.Add(wheelDmgr);
            }
            this.Log("InitPlayerDamagers : success.");
        }

        public void InitPlayerDamageable()
        {
            if (!isLoaded && !isSpawned)
            {
                this.LogError("InitPlayerDamegeable : not loaded or spawned.");
                return;
            }

            self_oDamageable = null;

            // Safely get the current player's rigidbody.
            // It might be null, for instance if the state is not yet a rigidbody compliant state.
            if (self_PlayerController == null) {
                this.LogError("InitPlayerDamegeable : playerController is null");
                return;
            }

            if (self_PlayerController.vehicleState != PlayerController.PlayerVehicleStates.Car) {
                this.LogError("InitPlayerDameger : only implemented for car.");
                return;
            }

            if (self_PlayerController.GetRigidbody() == null) {
                this.LogError("InitPlayerDamegeable : rigidbody is null");
                return;
            }
            if (self_PlayerController.GetRigidbody().gameObject == null) {
                this.LogError("InitPlayerDamegeable : rigidbody's gameobject is null");
                return;
            }

            // Safely get/create the OnlineDamagable.
            GameObject       bodyRef     = self_PlayerController.GetRigidbody().gameObject;
            OnlineDamageable bodyDmgble;
            if (!bodyRef.TryGetComponent<OnlineDamageable>(out bodyDmgble)) {
                this.LogWarn("InitPlayerDamageable : cannot retrieve OnlineDamageable => trying to create one.");
                bodyDmgble = bodyRef.AddComponent<OnlineDamageable>();
            }

            if (bodyDmgble == null) {
                this.LogError("InitPlayerDamageable : cannot retrieve OnlineDamageable or create one => catastrophique failure.");
                return;
            }

            // Finally set to the right references.
            self_oDamageable       = bodyDmgble;
            self_oDamageable.owner = gameObject;

            this.Log("InitPlayerDamageable : success.");
        }

        /* ----------------------------------
        Server
        ------------------------------------ */

        public override void OnStartServer()
        {
            this.Log("OnStartServer");

            if (omegas == null) {
                this.LogError("omegas list is null.");
            } else {
                omegas.AddRange(new float[]{0,0,0,0});
                this.Log("omegas count is " + omegas.Count);
            }
            // Server only or host but not the localplayer
            //means we need on online stub with a few extras: no rendering, etc...I
            if (!isClient || (isClient && !isLocalPlayer))
            {
                this.Log("OnlinePlayerInit : server.");
                // server : no need to wait for dependencies => they should be local and loaded asap locally.
                if (self_PlayerController == null) self_PlayerController = GetComponent<PlayerController>();
                self_PlayerController.InitOnServer();
                // set name to whatever, cannot be "Player" => this is the name of the local player.
                onlinePlayerName = Constants.GO_PLAYER + this.netId.ToString();
                gameObject.name = onlinePlayerName;
                // NOTE: do we want to do this?
                // for now we dont, but 
                // AudioListener AL = GetComponentInChildren<AudioListener>();
                // if (!!AL) { Destroy(AL); }
            }
            OnStartServerInit();
        }

        void OnStartServerInit()
        {
            // TODO: remove anything linked to input poll, rendering, etc...
            self_PlayerController.OnStateChange += OnStateChange;
            self_PlayerController.OnVehicleStateChange += OnVehicleStateChange;
        }

        // Update clients about their states.
        void OnVehicleStateChange(PlayerController.PlayerVehicleStates state)
        {
            this.Log("OnVehicleStateChange : " + state.ToString());

            RpcSetCameraFocus(self_PlayerController.GetTransform());

            playerVehicleState = state;
            //server
            InitPlayerDamageable();
            InitPlayerDamagers();
        }

        void OnStateChange(PlayerController.PlayerStates state)
        {
            this.Log("OnStateChange : " + state.ToString());
            playerState = state;
        }

        [Command]
        public void CmdModifyReadyState(bool state)
        {
            isReady = state;
        }

        [Command]
        public void CmdModifyLoadedState(bool state)
        {
            isLoaded = state;
        }

        [Command]
        public void CmdModifySpawnedState(bool state)
        {
            if (state)
            {
                this.Log("Server received client spawned.");

                OnlineGameManager.Get().AddPlayer(this);

                self_PlayerController.TransitionTo(PlayerController.PlayerVehicleStates.Car);

                isSpawned = state;

                // Ask this client to load.
                RpcLoad();
            }
            else
            {
                isSpawned = state;
            }

        }

        [Server]
        public void FreezePlayer(bool state)
        {
            if (self_PlayerController)
            {
                if (state) self_PlayerController.Freeze();
                else self_PlayerController.UnFreeze();
            }
        }

        [Command]
        public void CmdFreezePlayer(bool state)
        {
            FreezePlayer(state);
        }

        [Command]
        public void CmdSendPlayerInputs(byte[] playerInputs)
        {
            // Deserialize inputs.
            Schnibble.Managers.GameController gc = Schnibble.Managers.GameController.Deserialize((int)PlayerInputs.InputCode.Count, playerInputs);
            // Execute inputs.
            (self_PlayerController as Schnibble.Managers.IControllable).ProcessInputs(null, gc);
            // Normally any results should be broadcasted back to clients.
        }

        [Command]
        public void CmdNotifyPlayerFinishedTrial()
        {
            if (NetworkRoomManagerExt.singleton.onlineGameManager.trialManager == null)
            {
                // that would be a very bad error
                return;
            }
            NetworkRoomManagerExt.singleton.onlineGameManager.trialManager.NotifyPlayerHasFinished(this);

        }

        // [Command]
        // public void CmdBreakObject(OnlineBreakableObject iOBO)
        // {
        //     iOBO.BreakObject(this);
        // }

        [Server]
        public void Relocate(Vector3 iNewPos, Quaternion iNewRot)
        {
            self_PlayerController.ForceTransform(iNewPos, iNewRot);
            self_PlayerController.ForceVelocity(Vector3.zero, Vector3.zero);

            // self_PlayerController.ForceTransform(Vector3.zero, Quaternion.identity);
            // self_PlayerController.ForceVelocity(Vector3.zero, Vector3.zero);

            // //// relocates player root and seems to work
            // transform.position = iNewPos;
            // transform.rotation = iNewRot;
        }

        /* ----------------------------------
        Client 
        ------------------------------------ */

        IEnumerator WaitForDependencies()
        {
            this.Log("Start OnlinePlayerController wait for dependencies.");

            // Wait for OnlineGameManager to setup.
            while (NetworkRoomManagerExt.singleton == null) { yield return null; }
            while (NetworkRoomManagerExt.singleton.onlineGameManager == null) { yield return null; }

            // Wait for any scene to load.
            while (!NetworkRoomManagerExt.singleton.subsceneLoaded) { yield return null; }

            // Wait for objcet that we need to access at init time.
            //while (Access.UIPlayerOnline() == null) { yield return null; }
            while (Access.CameraManager() == null) { yield return null; }

            this.Log("End OnlinePlayerController wait for dependencies.");
        }

    public override void OnStartClient()
    {
        this.Log("OnStartClient");
        // OnStartuserver will setup the OnlineStub if need be, so we init here
        // only if we are client only, and not the local player.
        if (!isServer && !isLocalPlayer)
        {
            this.Log("OnlinePlayerInit : client.");
            omegas.Callback += OnUpdateOmegas;

            if (self_PlayerController == null) self_PlayerController = GetComponent<PlayerController>();

            self_PlayerController.InitAsOnlineStub();

            // might have already changed

            // add a camera focusable
            // NOTE:
            // For now it is local only, but in the future we might want to have it spawned on the server
            // And sync the focus state, for things like fog of war (server only knows what can be focus by the player or not)
            cameraFocusable = Instantiate(prefabCameraFocusable, transform).transform;
            // We do not set isSpawnde because we are not the local player that will receive the commands from the server.
            // We were asked to spawn BY the server.
        }

        // self_PlayerController might already have been set to car?
        InitPlayerDamageable();
        InitPlayerDamagers();
    }

    public override void OnStartLocalPlayer()
    {
        this.Log("OnStartLocalPlayer");

        if (!isServer) omegas.Callback += OnUpdateOmegas;

        StartCoroutine(InitLocalPlayer());
    }

    public override void OnStopLocalPlayer()
    {
        OnlineGameManager.Get().localPlayer = null;

        Destroy(gameObject);
    }

    IEnumerator InitLocalPlayer()
    {
        this.Log("OnlinePlayerInit : client local player.");
        // client but local: need to wait dependencies as some things might not be loaded.
        // It is probably a wrong statement: every dependencies should be loaded when this object is calling Start() ?
        yield return StartCoroutine(WaitForDependencies());

        if (self_PlayerController == null) self_PlayerController = GetComponent<PlayerController>();

        // Updated by server.
        //self_PlayerController.OnStateChange += OnStateChange;
        //self_PlayerController.OnVehicleStateChange += OnVehicleStateChange;

        self_PlayerController.Init();

        // Set underlying player controller to not apply any inputs.
        // Instead send inputs to the server and wait for server answer that will
        // update states.
        self_PlayerController.inputMode = PlayerController.InputMode.Online;

        OnlineGameManager.Get().localPlayer = this;
        gameObject.name = Constants.GO_PLAYER;
        // What is the purpose of this boolean?
        Access.GameSettings().isOnline = true;

        // We tell the server that we spawned, we are ready to communicate and init.
        if (!isSpawned)
        CmdModifySpawnedState(true);

        // init damagers/damageables
        while (self_PlayerController.GetRigidbody() == null)
        {
            yield return null;
        }
        while (self_PlayerController.car.GetCar() == null)
        {
            yield return null;
        }
    }

    [TargetRpc]
    public void RpcLoadSubScene()
    {
        StartCoroutine(WaitSubSceneToLoad());
    }
    IEnumerator WaitSubSceneToLoad()
    {
        while (!NetworkRoomManagerExt.singleton.subsceneLoaded)
        {
            yield return null;
        }
        Access.CameraManager()?.changeCamera(GameCamera.CAM_TYPE.ORBIT, false);
        CmdModifyLoadedState(true);
    }

    [TargetRpc]
    void RpcSetCameraFocus(Transform t)
    {
        if (t != null)
        {
            // Update camera focus.
            this.Log("RpcSetCameraFocus." + t.gameObject.name);
            Access.CameraManager()?.OnTargetChange(t);
        }
    }

    void OnPlayerStateChanged(PlayerController.PlayerStates oldState, PlayerController.PlayerStates newState)
    {
        if (!isServer) self_PlayerController.TransitionFromTo(oldState, newState);
    }

    void OnPlayerVehicleStateChanged(PlayerController.PlayerVehicleStates oldState, PlayerController.PlayerVehicleStates newState)
    {
        this.Log("OnPlayerVehicleStateChanged : " + newState.ToString());

        if (!isServer) self_PlayerController.TransitionTo(newState);

        // init damager/damageable
        InitPlayerDamageable();
        InitPlayerDamagers();
    }

    [TargetRpc]
    void RpcLoad()
    {
        Load();
    }

    [Client]
    void Load()
    {
        CmdFreezePlayer(true);

        // might have been updated to a new state.
        InitPlayerDamageable();
        InitPlayerDamagers();

        //Access.CameraManager()?.changeCamera(GameCamera.CAM_TYPE.ORBIT, false);

        CmdModifyLoadedState(true);
    }

    void Update()
    {
        if (NetworkClient.ready && isLocalPlayer && isLoaded)
        {
            if (self_PlayerController.lastInputs != null)
            {
                // Send inputs to server.
                CmdSendPlayerInputs(self_PlayerController.lastInputs);
            }
        }
    }

    public void TakeDamage(OnlineDamageSnapshot iDamageSnap)
    {
        self_PlayerController.takeDamage(
            iDamageSnap.damage,
            iDamageSnap.worldOrigin,
            iDamageSnap.ownerVelocity,
            iDamageSnap.repulsionForce
        );
    }
}
