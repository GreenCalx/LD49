using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wonkerz;
using Mirror;

public class OnlinePlayerController : NetworkBehaviour
{
    [Header("OnlinePlayerController")]
    public string onlinePlayerName;
    public NetworkConnectionToClient connectionToClient;
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
    [SyncVar]
    public bool readyToPlay = false;
    public Transform cameraFocusable;

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

    public override void OnStartServer()
    {
        // disable client stuff
        if (isServer)
        {
            self_carMeshHandle.gameObject.SetActive(true);
            if (isServerOnly)
            {
                Access.OfflineGameManager().isServerOnly = true;
            }
        }
    }

    public override void OnStartClient()
    {
        // register client events, enable effects
        onlinePlayerName = Constants.GO_PLAYER + this.netId.ToString();
        gameObject.name = onlinePlayerName;
        if (!isLocalPlayer)
        {
            Destroy(GetComponent<PlayerController>());
            Access.OfflineGameManager().AddToRemotePlayers(this);

            AudioListener AL = GetComponentInChildren<AudioListener>();
            if (!!AL)
            { Destroy(AL); }

            // make car visible
            self_carMeshHandle.gameObject.SetActive(true);
            self_weightMeshHandle.gameObject.SetActive(true);

            // add a camera focusable
            cameraFocusable = Instantiate(prefabCameraFocusable, transform).transform;
        }
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

    [Command]
    public void CmdNotifyOGMPlayerReady()
    {
        NetworkRoomManagerExt.singleton.onlineGameManager.NotifyPlayerIsReady(this, true);
    }

    // [Command]
    // public void CmdBreakObject(OnlineBreakableObject iOBO)
    // {
    //     iOBO.BreakObject(this);
    // }

    [ClientRpc]
    public void RpcRelocate(Vector3 iNewPos, Quaternion iNewRot)
    {
        Relocate(iNewPos, iNewRot);
    }
    [Command]
    public void CmdRelocate(Vector3 iNewPos, Quaternion iNewRot)
    {
        Relocate(iNewPos, iNewRot);
    }

    [Client]
    public void Relocate(Vector3 iNewPos, Quaternion iNewRot)
    {
        self_PlayerController.ForceTransform(iNewPos, iNewRot);
        self_PlayerController.ForceVelocity(Vector3.zero, Vector3.zero);
    }

    public override void OnStartLocalPlayer()
    {
        if (isLocalPlayer)
        {
            gameObject.name = Constants.GO_PLAYER;
            Access.OfflineGameManager().localPlayer = this;
            Access.GameSettings().IsOnline = true;

            if (self_PlayerController==null)
                self_PlayerController = GetComponent<PlayerController>();

            StartCoroutine(initCo());
        } 
    }

    [Command]
    public void CmdInformPlayerHasLoaded()
    {
        InformPlayerHasLoaded();
    }

    public void InformPlayerHasLoaded()
    {
        NetworkRoomManagerExt.singleton.onlineGameManager.AddPlayer(this);
        NetworkRoomManagerExt.singleton.onlineGameManager.NotifyPlayerHasLoaded(this, true);
    }

    public override void OnStopLocalPlayer()
    {
        Destroy(gameObject);
        if (isLocalPlayer)
        {
            Access.OfflineGameManager().localPlayer = null;
        }
        
    }

    IEnumerator initCo()
    {
        self_PlayerController.Freeze();
        readyToPlay = false;

        // Wait additive scenes to be laoded

        while(!NetworkRoomManagerExt.singleton.subsceneLoaded)
        {
            yield return null;
        }

        while(Access.UIPlayerOnline()==null)
        {
            yield return null;
        }
        Access.UIPlayerOnline().LinkToPlayer(this);

        while(Access.CameraManager()==null)
        {
            yield return null;
        }
        // Force Init Camera Init ?
        Access.CameraManager()?.changeCamera(GameCamera.CAM_TYPE.ORBIT, false);
        
        // while(self_PlayerController.vehicleState == PlayerController.PlayerVehicleStates.None)
        // {
        //     yield return null;
        // }

        while(self_PlayerController.inputMgr==null)
        {
            yield return null;
        }

        self_PlayerController.TransitionTo(PlayerController.PlayerVehicleStates.Car);

        while (self_PlayerController.car.GetCar() == null)
        { yield return null;}

        while (self_PlayerController.GetRigidbody() == null)
        {
            // not ideal but RB can be null at first call thus we need to re-init?
            self_PlayerController.TransitionTo(PlayerController.PlayerVehicleStates.Car);
            yield return null; 
        }

        while (self_PlayerController.car.car.GetChassis().GetBody() == null)
        {
            yield return null;
        }

        self_PlayerController.UnFreeze();

        //CmdInformPlayerHasLoaded();
        readyToPlay = true;
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
