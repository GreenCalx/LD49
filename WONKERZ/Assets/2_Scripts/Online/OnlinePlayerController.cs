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

    public override void OnStartServer()
    {
        // disable client stuff
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
        }
    }

    public override void OnStartLocalPlayer()
    {
        if (isLocalPlayer)
        {
            gameObject.name = Constants.GO_PLAYER;
            Access.OfflineGameManager().localPlayer = this;

            if (self_PlayerController==null)
                self_PlayerController = GetComponent<PlayerController>();

            StartCoroutine(initCo());
        } 
    }

    [Command]
    public void CmdInformPlayerHasLoaded()
    {
        NetworkRoomManagerExt.singleton.onlineGameManager.AddPlayer(this);
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

        //  var states = self_PlayerController.vehicleStates;
        //  states.SetState(states.states[(int)PlayerVehicleStates.States.Car]);

        while(Access.UIPlayerOnline()==null)
        {
            yield return null;
        }
        Access.UIPlayerOnline().LinkToPlayer(this);

        
        while(Access.CameraManager()==null)
        {
            yield return null;
        }
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
