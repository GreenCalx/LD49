using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Internals")]
    public Transform cameraFocusable;

    public override void OnStartServer()
    {
        // disable client stuff
        if (isServer)
        {
            self_carMeshHandle.gameObject.SetActive(true);
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

    [Command]
    public void CmdBreakObject(OnlineBreakableObject iOBO)
    {
        iOBO.BreakObject(this);
    }

    [ClientRpc]
    public void RpcRelocate(Vector3 iNewPos, Transform iFacingPoint)
    {
        Relocate(iNewPos, iFacingPoint);
    }

    [Client]
    public void Relocate(Vector3 iNewPos, Transform iFacingPoint)
    {
        //self_PlayerController.Freeze();

        transform.position = iNewPos;
        transform.rotation = Quaternion.identity;

        if (iFacingPoint != null)
        {
            self_PlayerController.transform.LookAt(iFacingPoint);
        }

        self_PlayerController.rb.velocity = Vector3.zero;
        self_PlayerController.rb.angularVelocity = Vector3.zero;

        //self_PlayerController.UnFreeze();
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
        
        while(self_PlayerController.vehicleStates==null)
        {
            yield return null;
        }

        while(self_PlayerController.inputMgr==null)
        {
            yield return null;
        }
        
        var states = self_PlayerController.vehicleStates;
        states.SetState(states.states[(int)PlayerVehicleStates.States.Car]);

         CmdInformPlayerHasLoaded();
    }
}