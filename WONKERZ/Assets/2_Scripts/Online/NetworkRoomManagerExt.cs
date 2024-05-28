using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Mirror;

    public enum ONLINE_GAME_STATE {
        NONE = 0,
        PREGAME = 1,
        MAIN = 2,
        TRIAL = 3,
        POSTGAME = 4
    }

    public class NetworkRoomManagerExt : NetworkRoomManager
    {
        public static new NetworkRoomManagerExt singleton => NetworkManager.singleton as NetworkRoomManagerExt;

        public Dictionary<NetworkRoomPlayerExt, OnlinePlayerController> roomplayersToGameplayersDict;
        public OnlineGameManager onlineGameManager;
        
        public ONLINE_GAME_STATE gameState = ONLINE_GAME_STATE.NONE;

        public bool subsceneLoaded;
        public bool subsceneUnloaded;

    public string selectedTrial = "";
        
        


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
                StartCoroutine(ServerLoadOpenCourse());
                
                // do stuff
                Access.GameSettings().IsOnline = true;
                roomplayersToGameplayersDict = new Dictionary<NetworkRoomPlayerExt, OnlinePlayerController>();
                StartCoroutine(SeekOnlineGameManager());
            }
        }

        public IEnumerator ServerLoadOpenCourse()
        {
            subsceneLoaded = false;
            yield return SceneManager.LoadSceneAsync(Constants.SN_OPENCOURSE, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive
            });
            subsceneLoaded = true;
        }

        public void unloadOpenCourse()
        {
            StartCoroutine(ServerUnloadOpenCourse());
        }

        IEnumerator ServerUnloadOpenCourse()
        {
            subsceneUnloaded = false;
            yield return SceneManager.UnloadSceneAsync(Constants.SN_OPENCOURSE);
            subsceneUnloaded = true;
        }
        
        public void loadSelectedTrial()
        {
            StartCoroutine(ServerLoadTrial());
        }

        IEnumerator ServerLoadTrial()
        {
            subsceneLoaded = false;
            yield return SceneManager.LoadSceneAsync(selectedTrial, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive
            });
            
            subsceneLoaded = true;
        }

        public override void OnClientChangeScene(string sceneName, SceneOperation sceneOperation, bool customHandling)
        {
            if (sceneOperation == SceneOperation.UnloadAdditive)
                StartCoroutine(UnloadAdditive(sceneName));

            if (sceneOperation == SceneOperation.LoadAdditive)
                StartCoroutine(LoadAdditive(sceneName));
        }



        // public override void OnRoomClientSceneChanged()
        // {
        //     if (sceneOperation == SceneOperation.UnloadAdditive)
        //         StartCoroutine(UnloadAdditive(sceneName));

        //     if (sceneOperation == SceneOperation.LoadAdditive)
        //         StartCoroutine(LoadAdditive(sceneName));
        // }

        IEnumerator LoadAdditive(string sceneName)
        {
            // host client is on server...don't load the additive scene again
            if (mode == NetworkManagerMode.ClientOnly)
            {
                subsceneLoaded = false;
                // Start loading the additive subscene
                loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                while (loadingSceneAsync != null && !loadingSceneAsync.isDone)
                    yield return null;
                subsceneLoaded = true;
            }

            // Reset these to false when ready to proceed
            NetworkClient.isLoadingScene = false;
        }

        IEnumerator UnloadAdditive(string sceneName)
        {
            // host client is on server...don't unload the additive scene here.
            if (mode == NetworkManagerMode.ClientOnly)
            {
                subsceneLoaded = false;

                yield return SceneManager.UnloadSceneAsync(sceneName);
                yield return Resources.UnloadUnusedAssets();

                subsceneLoaded = true;
            }

            // Reset these to false when ready to proceed
            NetworkClient.isLoadingScene = false;
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

            conn.Send(new SceneMessage { sceneName = Constants.SN_OPENCOURSE, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });
            
            return true;
        }

    public void clientLoadSelectedTrial()
    {

        foreach (OnlinePlayerController opc in roomplayersToGameplayersDict.Values)
        {
            NetworkConnectionToClient conn = opc.netIdentity.connectionToClient;
            if (conn == null) 
                return;

            // Tell client to unload previous subscene with custom handling (see NetworkManager::OnClientChangeScene).
            conn.Send(new SceneMessage { sceneName = Constants.SN_OPENCOURSE, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });
            conn.Send(new SceneMessage { sceneName = selectedTrial, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });
        }

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
