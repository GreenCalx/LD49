using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Mirror;


    public class NetworkRoomManagerExt : NetworkRoomManager
    {
        public static new NetworkRoomManagerExt singleton => NetworkManager.singleton as NetworkRoomManagerExt;

        public Dictionary<NetworkRoomPlayerExt, OnlinePlayerController> roomplayersToGameplayersDict;
        public OnlineGameManager onlineGameManager;

        IEnumerator SeekOnlineGameManager()
        {
            while (onlineGameManager==null)
            {
                yield return null;
            }

        }

        /// <summary>
        /// This is called on the server when a networked scene finishes loading.
        /// </summary>
        /// <param name="sceneName">Name of the new scene.</param>
        public override void OnRoomServerSceneChanged(string sceneName)
        {
            // spawn the initial batch of Rewards
            if (sceneName == GameplayScene)
            { 
                // do stuff
                Access.GameSettings().IsOnline = true;
                roomplayersToGameplayersDict = new Dictionary<NetworkRoomPlayerExt, OnlinePlayerController>();
                StartCoroutine(SeekOnlineGameManager());
                //Access.OnlineGameManager().expectedPlayersFromLobby = pendingPlayers.Count;
            }
        }

        public override void OnRoomClientSceneChanged()
        {
        }

        public override void OnRoomStopClient()
        {
            base.OnRoomStopClient();
            Access.GameSettings().IsOnline = false;
        }

        public override void OnRoomStopServer()
        {
            base.OnRoomStopServer();
            Access.GameSettings().IsOnline = false;
        }

        public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
        {
            OnlinePlayerController OPC = gamePlayer.GetComponent<OnlinePlayerController>();
            OPC.connectionToClient = conn;
            NetworkRoomPlayerExt nrp = roomPlayer.GetComponent<NetworkRoomPlayerExt>();
            roomplayersToGameplayersDict.Add(nrp, OPC);
            
            return true;
        }



        // public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
        // {

        // }

        bool showStartButton;

        public override void OnRoomServerPlayersReady()
        {
            // calling the base method calls ServerChangeScene as soon as all players are in Ready state.
            if (Mirror.Utils.IsHeadless())
            {
                base.OnRoomServerPlayersReady();
            }
            else
            {
                showStartButton = true;
            }
        }

        public override void OnGUI()
        {
            base.OnGUI();

            if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
            {
                // set to false to hide it in the game scene
                showStartButton = false;

                ServerChangeScene(GameplayScene);
            }
        }
    }
