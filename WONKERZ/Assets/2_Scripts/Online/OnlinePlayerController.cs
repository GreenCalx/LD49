using Mirror;
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
    [Header("OnlinePlayerController")]
    public string onlinePlayerName;
    public OnlineCollectibleBag bag;

    [Header("Mand Refs")]
    public PlayerController self_PlayerController;
    public Transform self_carMeshHandle;
    public Transform self_weightMeshHandle;
    public GameObject prefabCameraFocusable;
    public List<OnlineDamager> self_oDamagers;
    public OnlineDamageable self_oDamageable;
    [Header("Online Tweaks")]
    public float minSpeedToDoDamage = 30f;
    [Header("Internals")]
    // Client has authority on this,
    // It will not modify it directly but send a cmd to send it.
    // When the server sets it, the client listen for the change and react to it.
    [SyncVar]
    bool isReady = false;
    [SyncVar]
    bool isLoaded = false;
    [SyncVar]
    bool isSpawned = false;

    public Transform cameraFocusable;

    public bool IsSpawned() => isSpawned;
    public bool IsReady()   => isReady;
    public bool IsLoaded()  => isLoaded;
    public bool IsLockAndLoaded() => IsReady() && IsLoaded();


    void FixedUpdate()
    {
        if (self_oDamagers!=null)
            UpdatePlayerDamagers();
    }

    private void UpdatePlayerDamagers()
    {
        int damage = 0;
        WkzCar cc = self_PlayerController.car.GetCar();
        if (cc.GetCurrentSpeedInKmH() > minSpeedToDoDamage)
        { 
            damage = (int) Mathf.Abs((float)cc.GetCurrentSpeedInKmH());
            damage +=(int) Mathf.Floor((self_PlayerController.GetRigidbody().mass * 0.01f));
        }

        foreach( OnlineDamager d in self_oDamagers )
        {
            d.damage = damage;
        }
    }

    public void InitPlayerDamagers()
    {
        self_oDamagers = new List<OnlineDamager>(5);

        WkzCar cc = self_PlayerController.car.GetCar();

        // body damager
        GameObject bodyRef = self_PlayerController.GetRigidbody().gameObject;
        OnlineDamager body_dmgr = bodyRef.GetComponent<OnlineDamager>();
        if (body_dmgr==null)
            body_dmgr = bodyRef.AddComponent<OnlineDamager>();
        body_dmgr.owner = gameObject;
        self_oDamagers.Add(body_dmgr);

        // wheel damagers
        List<WkzWheelCollider> wheels = new List<WkzWheelCollider>(self_PlayerController.gameObject.GetComponentsInChildren<WkzWheelCollider>());
        foreach( WkzWheelCollider w in wheels)
        {
            OnlineDamager wheel_dmgr = w.gameObject.GetComponent<OnlineDamager>();
            if (wheel_dmgr==null)
                wheel_dmgr = w.gameObject.AddComponent<OnlineDamager>();
            wheel_dmgr.owner = gameObject;
            self_oDamagers.Add(wheel_dmgr);
        }
    }

    public void InitPlayerDamageable()
    {
        self_oDamageable = null;

        GameObject bodyRef = self_PlayerController.GetRigidbody().gameObject;
        OnlineDamageable body_dmgbl = bodyRef.GetComponent<OnlineDamageable>();
        if (body_dmgbl==null)
            body_dmgbl = bodyRef.AddComponent<OnlineDamageable>();
        self_oDamageable = body_dmgbl;
        self_oDamageable.owner = gameObject;
    }

    /* ----------------------------------
    Server
    ------------------------------------ */

    public override void OnStartServer()
    {
        // Server only or host but not the localplayer
        //means we need on online stub with a few extras: no rendering, etc...I
        if (!isClient || (isClient && !isLocalPlayer))
        {
            UnityEngine.Debug.LogError("OnlinePlayerInit : server.");
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
            OnStartServerInit();
        }
    }

    void OnStartServerInit()
    {
        // TODO: remove anything linked to input poll, rendering, etc...
        self_PlayerController.OnStateChange        += OnStateChange;
        self_PlayerController.OnVehicleStateChange += OnVehicleStateChange;
    }

    // Update clients about their states.
    void OnVehicleStateChange(PlayerController.PlayerVehicleStates state) {
        RpcSetCameraFocus(self_PlayerController.GetTransform());
        RpcSetVehicleState(state);
    }

    void OnStateChange(PlayerController.PlayerStates state)
    {
        RpcSetState(state);
    }


    [Command]
    public void CmdModifyReadyState(bool state) {
        isReady = state;
    }

    [Command]
    public void CmdModifyLoadedState(bool state) {
        isLoaded = state;
    }

    [Command]
    public void CmdModifySpawnedState(bool state) {
        if (state)
        {
            Debug.LogError("Server received client spawned.");

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
    public void FreezePlayer(bool state) {
        if (self_PlayerController)
        {
            if (state) self_PlayerController.Freeze();
            else       self_PlayerController.UnFreeze();
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
    }

    /* ----------------------------------
    Client 
    ------------------------------------ */

    IEnumerator WaitForDependencies()
    {
        Debug.LogError("Start OnlinePlayerController wait for dependencies.");

        // Wait for OnlineGameManager to setup.
        while (NetworkRoomManagerExt.singleton == null) { yield return null; }
        while (NetworkRoomManagerExt.singleton.onlineGameManager == null) { yield return null; }

        // Wait for any scene to load.
        while (!NetworkRoomManagerExt.singleton.subsceneLoaded) { yield return null; }

        // Wait for objcet that we need to access at init time.
        while (Access.UIPlayerOnline() == null) { yield return null; }
        while (Access.CameraManager() == null) { yield return null; }

        Debug.LogError("End OnlinePlayerController wait for dependencies.");
    }


    public override void OnStartClient()
    {
        // OnStartuserver will setup the OnlineStub if need be, so we init here
        // only if we are client only, and not the local player.
        if (!isServer && !isLocalPlayer)
        {
            // add a camera focusable
            // NOTE:
            // For now it is local only, but in the future we might want to have it spawned on the server
            // And sync the focus state, for things like fog of war (server only knows what can be focus by the player or not)
            cameraFocusable = Instantiate(prefabCameraFocusable, transform).transform;
            // We do not set isSpawnde because we are not the local player that will receive the commands from the server.
            // We were asked to spawn BY the server.
        }
    }

    public override void OnStartLocalPlayer()
    {
        StartCoroutine(InitLocalPlayer());
    }

    public override void OnStopLocalPlayer()
    {
        OnlineGameManager.Get().localPlayer = null;

        Destroy(gameObject);
    }

    IEnumerator InitLocalPlayer()
    {
        UnityEngine.Debug.LogError("OnlinePlayerInit : client local player.");
        // client but local: need to wait dependencies as some things might not be loaded.
        // It is probably a wrong statement: every dependencies should be loaded when this object is calling Start() ?
        yield return StartCoroutine(WaitForDependencies());

        if (self_PlayerController == null) self_PlayerController = GetComponent<PlayerController>();
        self_PlayerController.Init();



        // Set underlying player controller to not apply any inputs.
        // Instead send inputs to the server and wait for server answer that will
        // update states.
        self_PlayerController.inputMode = PlayerController.InputMode.Online;

        OnlineGameManager.Get().localPlayer = this;
        gameObject.name = Constants.GO_PLAYER;
        // What is the purpose of this boolean?
        Access.GameSettings().IsOnline = true;



        // We tell the server that we spawned, we are ready to communicate and init.
        CmdModifySpawnedState(true);

        // init damagers/damageables
        while(self_PlayerController.GetRigidbody() == null)
        {
            yield return null;
        }
        while (self_PlayerController.car.GetCar() ==null)
        {
            yield return null;
        }


        
    }

    [TargetRpc]
    void RpcSetCameraFocus(Transform t)
    {
        if (t != null)
        {
            // Update camera focus.
            Debug.LogError("RpcSetCameraFocus." + t.gameObject.name);
            Access.CameraManager()?.OnTargetChange(t);
        }
    }

    [ClientRpc]
    void RpcSetState(PlayerController.PlayerStates state) {
        UnityEngine.Debug.LogError("RpcSetState : " + state.ToString());
        if(!isServer) self_PlayerController.TransitionFromTo(self_PlayerController.playerState, state);
    }

    [ClientRpc]
    void RpcSetVehicleState(PlayerController.PlayerVehicleStates state) {
        UnityEngine.Debug.LogError("RpcSetVehicleState : " + state.ToString());
        if(!isServer) self_PlayerController.TransitionTo(state);
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

        Access.UIPlayerOnline().LinkToPlayer(this);
        Access.CameraManager()?.changeCamera(GameCamera.CAM_TYPE.ORBIT, false);
        
        InitPlayerDamageable();
        InitPlayerDamagers();

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
