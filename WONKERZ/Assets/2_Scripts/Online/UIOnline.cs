using Mirror;
using Schnibble.Online;
using Schnibble.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wonkerz;
using Schnibble.Managers;
using Schnibble;

// TODO: move outside this file.
// probably into Utils?
// For now used as wrapper to a delegate to execute on main thread.
public class MainThreadCommand
{
    public static Queue<MainThreadCommand> pendingCommands = new Queue<MainThreadCommand>();

    protected Action OnCmdSuccess;
    protected Action OnCmdError  ;

    public MainThreadCommand() {}
    public MainThreadCommand(Action ex) {OnCmdSuccess = ex;}

    public virtual void Do() { OnCmdSuccess?.Invoke(); }
    public virtual void Undo() {}
};

namespace Wonkerz.UI
{


    public class UIOnline : UIPanel
    {
        public static UIOnline singleton;

        // State machine
        public enum States
        {
            ConnectingToLobbyServer,
            ConnectingToRoom,
            CreatingRoom,

            MainMenu,
            InRoom,

            Deactivated,
            Exit,

            Count,
        };

        public static readonly string goBackHintStr = " BACK";

        public NetworkRoomManagerExt roomServer;

        public TextMeshProUGUI     stateMessageText;

        public UISelectableElement uiMainMenu;
        public UICreateRoomMenu    uiRoomCreationMenu;
        public UIRoom              uiRoom;
        public UILobbyServerList   uiServerList;
        public UIButtonHint        uiCancelButtonHint;
        public UIElement uiBackground;

        override public void Hide() {
            base.Hide();

            uiMainMenu        .Hide();
            uiRoomCreationMenu.Hide();
            uiRoom            .Hide();
            uiCancelButtonHint.Hide();
            uiBackground      .Hide();
        }

        public void SetState(States toState)
        {
            this.Log("SetState : " + toState.ToString());
            switch (toState)
            {
                case States.ConnectingToLobbyServer:
                    case States.ConnectingToRoom:
                    case States.Deactivated:
                    {
                        Hide();
                    } break;

                case States.CreatingRoom:
                    {
                        uiMainMenu.Hide();
                        uiRoom    .Hide();

                        uiRoomCreationMenu.Show();
                        uiCancelButtonHint.Show();
                        uiCancelButtonHint.SetButton((uiRoomCreationMenu as UIPanel).inputCancel, goBackHintStr);
                    } break;

                case States.MainMenu:
                    {
                        uiBackground.Show();
                        uiRoom            .Hide();
                        uiRoomCreationMenu.Hide();

                        uiMainMenu.Show();
                        uiCancelButtonHint.Show();
                        uiCancelButtonHint.SetButton((uiMainMenu as UIPanel).inputCancel, goBackHintStr);
                    } break;

                case States.InRoom:
                    {
                        uiRoomCreationMenu.Hide();
                        uiMainMenu        .Hide();

                        uiRoom.Show();
                        uiCancelButtonHint.Show();
                        uiCancelButtonHint.SetButton((uiRoom as UIPanel).inputCancel, goBackHintStr);
                    } break;
                case States.Exit:
                    {
                        // Carefull we need to move back the network manager to the active scene so that it is deleted when
                        var sceneLoader = Access.managers.sceneMgr;
                        sceneLoader.loadScene(Constants.SN_TITLE, new SceneLoader.LoadParams{
                            useTransitionIn = true,
                            useTransitionOut = true,
                            onEndTransition = delegate
                        {
                            if (uiCancelButtonHint) uiCancelButtonHint.Hide();

                            Deactivate();
                            sceneLoader.ResetDontDestroyOnLoad();
                        }
                        });
                    } break;
            }
            state = toState;
        }

        void ShowInfoMessage(string message) {
            stateMessageText.enabled = true;
            stateMessageText.text    = message;
        }

        void ShowErrorMessage(string message) {
            stateMessageText.enabled = true;
            stateMessageText.text    = message;
        }

        public override void Cancel()
        {
            base.Cancel();

            SetState(States.Exit);
        }

        public void CreateRoom()
        {
            NetworkRoomManagerExt.singleton.CreateRoom(new Lobby{
                maxPlayerCount = 4,
                //cf :hastName: SchLobbyServer
                hostName = Access.managers.gameProgressSaveMgr.activeProfile,
                name     = uiRoomCreationMenu.GetRoomName(),
            });
        }

        public void StartLocalServer()
        {
            this.Log("Start local server.");
            NetworkRoomManagerExt.singleton.SetupLocalSocket();
        }

        public void JoinLocalServer()
        {
            this.Log("Join local server.");
            NetworkRoomManagerExt.singleton.JoinLobbyServer();
        }

        public void OpenCreateRoomUI()
        {
            if (roomServer.isNetworkActive)
            {
                this.Log("Room already created and running.");
            }

            SetState(States.CreatingRoom);
        }

        public void CloseCreateRoomUI()
        {
            SetState(States.MainMenu);
        }


        public void OpenLobbyServerList(UISelectableElement activator) {
            uiServerList.activator = activator;
            // uiServerList Shaw will init+activate.
            uiServerList.Show();
        }

        override public void Init()
        {
            // IMPORTANT: must be done before calling anything on lobby server
            // or else we will never have any answers.
            RegisterLobbyServerCallbacks();
            RegisterRoomServerCallbacks();
            // Init childrens.
            // NOTE: do not init uiServerList, will be init at Show.
            //uiServerList.Init();
            uiMainMenu.GetComponent<UISelectableElement>().Init();
            uiRoom.Init();
            uiRoomCreationMenu.GetComponent<UISelectableElement>().Init();
        }

        override public void Deinit()
        {
            // ux
            uiServerList      .Deinit();
            uiMainMenu        .Deinit();
            uiRoom            .Deinit();
            uiRoomCreationMenu.Deinit();

            if (stateMessageText) {
                stateMessageText.enabled = false;
                stateMessageText.gameObject.SetActive(false);
            }

            // lobby server
            RemoveLobbyServerCallbacks();
            // room server
            RemoveRoomServerCallbacks();
        }

        override protected void OnEnable() {
            Activate();
        }

        override public void Activate()
        {
            base.Activate();

            Init();
            Show();

            OnSceneChanged();
        }

        override public void Deactivate()
        {
            base.Deactivate();

            Hide();
            Deinit();
        }

        // Internals

        States state = States.Deactivated;

        void RegisterLobbyServerCallbacks()
        {
            // Try to remove if already exists in InvocationList.
            RemoveLobbyServerCallbacks();

            var lobbyClient = roomServer.lobbyClient;
            if (lobbyClient != null) {
                lobbyClient.OnConnected    += OnConnected;
                lobbyClient.OnDisconnected += OnDisconnected;
                lobbyClient.OnError        += OnLobbyClientError;
            }
        }

        void RemoveLobbyServerCallbacks()
        {
            var lobbyClient = roomServer.lobbyClient;
            if (lobbyClient != null) {
                lobbyClient.OnConnected    -= OnConnected;
                lobbyClient.OnDisconnected -= OnDisconnected;
                lobbyClient.OnError        -= OnLobbyClientError;
            }
        }

        void RegisterRoomServerCallbacks() {
            // Try to remove if already exists in InvocationList.
            RemoveRoomServerCallbacks();

            roomServer.OnRoomStartHostCB          += OnRoomHostCreated;
            roomServer.OnClientErrorCB            += OnRoomClientError;
            roomServer.OnRoomServerSceneChangedCB += OnRoomSceneChanged;
            roomServer.OnRoomClientSceneChangedCB += OnRoomClientSceneChanged;

            NetworkRoomManagerExt.OnNetworkManagerChange += OnNetworkManagerChange;
        }

        void RemoveRoomServerCallbacks() {
            roomServer.OnRoomStartHostCB          -= OnRoomHostCreated;
            roomServer.OnClientErrorCB            -= OnRoomClientError;
            roomServer.OnRoomServerSceneChangedCB -= OnRoomSceneChanged;
            roomServer.OnRoomClientSceneChangedCB -= OnRoomClientSceneChanged;

            NetworkRoomManagerExt.OnNetworkManagerChange -= OnNetworkManagerChange;
        }

        void OnNetworkManagerChange() {
            this.Log("OnNetworkManagerChange");
            roomServer = NetworkRoomManagerExt.singleton;

            RegisterRoomServerCallbacks ();
            RegisterLobbyServerCallbacks();
        }

        void OnSceneChanged() {
            if      (Mirror.Utils.IsSceneActive(roomServer.RoomScene    )) {Show(); SetState(States.InRoom);}
            else if (Mirror.Utils.IsSceneActive(roomServer.GameplayScene)) {Hide();}
            else                                                           {Show(); SetState(States.MainMenu);}
        }

        void OnRoomSceneChanged(string sceneName) {
            this.Log("OnRoomSceneChanged : " + sceneName);

            OnSceneChanged();
        }

        void OnRoomClientSceneChanged() {
            this.Log("OnRoomClientSceneChanged");

            OnSceneChanged();
        }

        override protected void Awake()
        {
            if (singleton == null) {
                singleton = this;
                Access.managers.sceneMgr.SetDontDestroyOnLoad(singleton.gameObject);
            } else {
                DestroyImmediate(this.gameObject);
                return;
            }

            base.Awake();

            inputMgr      = Access.managers.playerInputsMgr.player1;

            inputActivate = PlayerInputs.InputCode.UIValidate.ToString();
            inputCancel   = PlayerInputs.InputCode.UICancel.ToString();
            inputDown     = PlayerInputs.InputCode.UIDown.ToString();
            inputUp       = PlayerInputs.InputCode.UIUp.ToString();
            inputLeft     = PlayerInputs.InputCode.UILeft.ToString();
            inputRight    = PlayerInputs.InputCode.UIRight.ToString();
        }

        override protected void OnDestroy()
        {
            if (singleton == this) {
                base.OnDestroy();
                Deactivate();
            }
        }

        override protected void Update()
        {
            base.Update();
        }

        // Following finctions are called by UX, so on the main thread and are safe.

        IEnumerator CoroutineWrapper(Action fun) {
            fun.Invoke();
            yield break;
        }

        void OnRoomHostCreated()
        {
            this.Log("Room host created");
            SetState(States.InRoom);
        }

        void OnRoomClientError(TransportError error, string reason) {
            this.LogError("Room error:" + reason);
            StartCoroutine(Schnibble.Utils.CoroutineChain(
                CoroutineWrapper(delegate() {ShowErrorMessage(reason);}),
                Schnibble.Utils.CoroWait(3.0f),
                CoroutineWrapper(delegate() {SetState(States.Exit);})));
        }

        // Following function can be called from threads.
        // Basically all Network callbacks.
        // Not thread safe

        void OnConnected(int id)
        {
            this.Log("OnConnected");
            MainThreadCommand.pendingCommands.Enqueue(new MainThreadCommand(
                delegate() {
                    SetState(States.MainMenu);
                }
            ));
        }

        void OnDisconnected()
        {
            this.Log("OnDisconnected");
            MainThreadCommand.pendingCommands.Enqueue(new MainThreadCommand(
                delegate() {
                    SetState(States.Exit);
                }
            ));
        }

        void OnLobbyClientError(SchLobbyClient.ErrorCode errorCode, string reason)
        {
            this.LogError("Lobby error : " + reason );
            MainThreadCommand.pendingCommands.Enqueue(new MainThreadCommand(
                delegate() {
                    ShowErrorMessage(reason);
                }
            ));
        }

    }

}
