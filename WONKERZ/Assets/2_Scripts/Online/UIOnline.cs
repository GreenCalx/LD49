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
public abstract class MainThreadCommand
{
    protected Action OnCmdSuccess;
    protected Action OnCmdError  ;
    public virtual void Do() { }
    public virtual void Undo() { }
};

public class UIOnline : UIPanel
{
    public static UIOnline singleton;
    // Externals

    public class UIOnlineMainThreadCommand : MainThreadCommand
    {
        protected UIOnline uiOnline;
        public UIOnlineMainThreadCommand(UIOnline uiOnline)
        {
            this.uiOnline = uiOnline;
        }
    };

    // :Sync:
    // IMPORTANT: Do not modify order of enums, unless you also
    // modify the order in stateMessage variable!

    // State machine 
    // :Sync:
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

    // :Sync:
    public enum Success
    {
        ConnectedToServer = States.Count,
        ConnectedToRoom,
        Count,
    }

    // :Sync:
    public enum Errors
    {
        LobbyServerNotFound = Success.Count,
        LobbyServerMaintenance,

        RoomNotFound,
        RoomCannotConnect,
        IncorrectPassword,

        Unknown,

        Count,
    };

    // :Sync:
    public string[] stateMessage = new string[] {
        // States messages
        "Connecting to server list...", // ConnectingToLobbyServer
        "Connecting to room...", // ConnectingToRoom
        "Creating room...", // CreatingRoom
        "In main menu...", // MainMenu => not used
        "In room", // InRoom => not used
        "Deactivated", // Deactivated => not used
        "Exit", // Exit => not used
        // Success messages
        "Connected to server list!", // ConnectedToServer
        "Connected to room!", // ConnectedToRoom
        // Error messages
        "Cannot connect to server list, please try again later.", // LobbyServerNotFound
       "Server in maintenance, please try again later.", // LobbyServerMaintenance
        "Room does not exists, please refresh server list.", // RoomNotFound
        "Cannot connect to room, please try again.", // RoomCannotConnect 
        "Password is incorrect! Please try again.", // IncorrectPassword
        "Sorry, something went wrong! Please try again.", // Unknown
    };

    public static readonly string goBackHintStr = " BACK";

    // client to access the lobby server, this server will send us our clientID, the room list, etc...
    public SchLobbyClient client = new SchLobbyClient(0);

    public NetworkRoomManagerExt roomServer;

    public TextMeshProUGUI     stateMessageText;

    public UISelectableElement uiMainMenu;
    public UISelectableElement uiRoomCreationMenu;
    public UIRoom              uiRoom;
    public UILobbyServerList   uiServerList;
    public UIButtonHint        uiCancelButtonHint;

    public void SetState(States toState)
    {
        this.Log("SetState : " + toState.ToString());
        switch (toState)
        {
            case States.ConnectingToLobbyServer:
                {
                    // hide menu until we are connected.
                    uiMainMenu        .Hide();
                    uiRoomCreationMenu.Hide();
                    uiRoom            .Hide();
                    uiCancelButtonHint.Hide();

                    StartCoroutineWithBarrier(CoroShowStateMessageWithMinTime(stateMessage[(int)toState], 1.0f));
                } break;

            case States.ConnectingToRoom:
                {
                    uiMainMenu        .Hide();
                    uiRoomCreationMenu.Hide();
                    uiRoom            .Hide();
                    uiCancelButtonHint.Hide();

                    StartCoroutineWithBarrier(CoroShowStateMessageWithMinTime(stateMessage[(int)toState], 1.0f));
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
            case States.Deactivated:
                {
                    // Deactivate all the UX
                    // Make sure
                    // - all transient objects are destroyed.
                    // - all callbacks are removed.
                    // - all inputs are released.
                    uiMainMenu.Hide();
                    uiMainMenu.deactivate();
                    uiMainMenu.deinit();
                    
                    uiRoom.Hide();
                    uiMainMenu.deactivate();
                    uiMainMenu.deinit();

                    uiRoomCreationMenu.Hide();
                    uiRoomCreationMenu.deactivate();
                    uiRoomCreationMenu.deinit();

                    stateMessageText.enabled = false;
                    stateMessageText.gameObject.SetActive(false);

                    uiCancelButtonHint.Hide();
                }break;
            case States.Exit:
                {
                    // Carefull we need to move back the network manager to the active scene so that it is deleted when
                    var sceneLoader = Access.SceneLoader();
                    sceneLoader.loadScene(Constants.SN_TITLE, new SceneLoader.SceneLoaderParams{
                        useTransitionIn = true,
                        useTransitionOut = true,
                        onEndTransition = delegate
                        {
                            uiCancelButtonHint.Hide();
                            deactivate();
                            // Remove room if need be
                            client.Close();

                            sceneLoader.ResetDontDestroyOnLoad();
                        }
                    });
        } break;
        }
        state = toState;
    }

    // What happens in current state if an error occurs
    public void StateError(int errorCode = 0)
    {
        this.Log("StateError");
        switch (state)
        {
            case States.ConnectingToLobbyServer:
                {
                    stateMessageText.enabled = true;
                    stateMessageText.text = stateMessage[(int)Errors.LobbyServerNotFound];

                    StartCoroutine(Schnibble.Utils.CoroutineChain(
                        CoroWait(1.0f),
                        CoroSetState(States.Exit)));
                }
                break;
            case States.MainMenu:
                {
                    stateMessageText.enabled = true;
                    stateMessageText.text = stateMessage[(int)Errors.Unknown];

                    StartCoroutine(Schnibble.Utils.CoroutineChain(
                        CoroWait(1.0f),
                        CoroSetState(States.Exit)));
                }
                break;
            case States.CreatingRoom:
                {
                    stateMessageText.enabled = true;
                    stateMessageText.text = stateMessage[(int)Errors.Unknown];

                    StartCoroutine(Schnibble.Utils.CoroutineChain(
                        CoroWait(1.0f),
                        CoroSetState(States.MainMenu)));
                }
                break;
            case States.InRoom:
                {
                    if (NetworkServer.activeHost) {
                        roomServer.StopHost();
                    }

                    stateMessageText.enabled = true;
                    stateMessageText.text = stateMessage[(int)Errors.Unknown];

                    StartCoroutine(Schnibble.Utils.CoroutineChain(
                        CoroWait(1.0f),
                        CoroSetState(States.CreatingRoom)));
                }
                break;
            case States.ConnectingToRoom:
                {
                    stateMessageText.enabled = true;
                    stateMessageText.text = stateMessage[(int)Errors.Unknown];

                    StartCoroutine(Schnibble.Utils.CoroutineChain(
                        CoroWait(1.0f),
                        CoroSetState(States.CreatingRoom)));
                }break;
            default:
                {
                    stateMessageText.enabled = true;
                    stateMessageText.text = stateMessage[(int)Errors.Unknown];
                } break;
        }
    }

    // What happens in current state if a success occurs
    public void StateSuccess(int successCode = 0)
    {
        switch (state)
        {
            case States.ConnectingToLobbyServer:
                {
                    // Show success message
                    stateMessageText.enabled = true;
                    stateMessageText.text = stateMessage[(int)Success.ConnectedToServer];

                    StartCoroutine(Schnibble.Utils.CoroutineChain(
                        CoroWait(1.0f),
                        CoroSetState(States.MainMenu)));
                }
                break;
            case States.ConnectingToRoom:
                {
                    stateMessageText.enabled = true;
                    stateMessageText.text = stateMessage[(int)Success.ConnectedToRoom];

                    SetState(States.InRoom);
                }
                break;
            case States.CreatingRoom:
                {
                    stateMessageText.enabled = true;
                    stateMessageText.text = stateMessage[(int)Success.ConnectedToRoom];

                    SetState(States.InRoom);
                }break;
            default:
                {
                } break;
        }
    }

    public override void cancel()
    {
        base.cancel();

        Hide();
    }

    public void Exit() {
        SetState(States.Exit);
    }

    public void CreateRoom()
    {
        client.CreateLobby("test name for now");
    }

    public void StartLocalServer()
    {
        this.Log("Start local server.");

        var serverAddress = "localhost";
        localServer = new SchLobbyServer(serverAddress, SchLobbyServer.defaultPort);
    }

    public void JoinLocalServer()
    {
        this.Log("Join local server.");

        IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, SchLobbyServer.defaultPort);
        client.Connect(serverEP);
    }

    public void StopLocalServer()
    {
        this.Log("Stop local server.");

        if (localServer == null) return;

        localServer.Close();
        localServer = null;
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
        uiServerList.Show();

        uiMainMenu.Hide();
    }

    public void ConnectToRoom(IPEndPoint roomEP, int roomHostID) {
        // Setup client remotePoint as host.
        var transport = (roomServer.transport as SchCustomRelayKcpTransport);
        if (transport == null) {
            this.LogError("Should use CustomRelayKcpTransport.");
            return;
        }

        transport .Port           = (ushort)roomEP.Port;
        roomServer.networkAddress = roomEP.Address.ToString();

        if (!NetworkClient.active) {
            // need to startClient to get back localEndPoint.
            // Mirror should send several messages to try to connect, almost like a NATPunch \o/
            roomServer.StartClient();
            // now that client is started, send to server the port.
            NetworkWriter writer = NetworkWriterPool.Get();

            writer.WriteByte((byte)SchLobbyServer.OpCode.NATPunchIP);

            SchLobbyServer.NATPunchIP data = new SchLobbyServer.NATPunchIP();
            data.senderID = client.id;
            data.port     = transport.GetClientLocalEndPoint().Port;
            data.receiverID = roomHostID;

            writer.Write(data);

            client.Send(writer);
        }
    }

    override public void init()
    {
        // Init childrens.
        uiServerList.init();
        uiMainMenu.GetComponent<UISelectableElement>().init();
        uiRoom.init();
        uiRoomCreationMenu.GetComponent<UISelectableElement>().init();
    }

    override public void deinit()
    {
        uiServerList.deinit();
        uiMainMenu.deinit();
        uiRoom.deinit();
        uiRoomCreationMenu.deinit();

        StopLocalServer();
    }

    override public void activate()
    {
        base.activate();

        init();
        // On activation we try to connect
        // Will call back OnConnected, or OnError
        if (Access.GameSettings().isLocal)
        {
            StartLocalServer();
            JoinLocalServer();
            SetState(States.MainMenu);
        }
        else
        StartCoroutine(CoroTryConnect(SchLobbyClient.defaultRemoteEP, 5, 0.5f));
    }

    override public void deactivate()
    {
        base.deactivate();

        RemoveLobbyServerCallbacks();
        RemoveRoomServerCallbacks();
        
        SetState(States.Deactivated);
    }

    // Internals


    // only used in debug mode as a way to create a local lobby server.
    SchLobbyServer localServer;

    // Current state to know what to shaw/hide, also how to treat errors and success.
    States state = States.ConnectingToLobbyServer;

    IEnumerator CoroWaitForTimer(float timer) {
        while(timer > 0.0f) yield return null;

        yield break;
    }

    IEnumerator CoroShowStateMessageWithMinTime(string message, float minTime) {
        stateMessageText.gameObject.SetActive(true);
        stateMessageText.enabled = true;
        stateMessageText.text = message;

        yield return new WaitForSeconds(minTime);
    }

    IEnumerator CoroShowStateMessage(string message) {
        stateMessageText.gameObject.SetActive(true);
        stateMessageText.enabled = true;
        stateMessageText.text = message;
        yield break;
    }

    IEnumerator CoroSetState(States state) {
        SetState(state);
        yield break;
    }

    IEnumerator CoroWait(float seconds) {
        yield return new WaitForSeconds(seconds);
        yield break;
    }

    IEnumerator coroBarrier;
    void StartCoroutineWithBarrier(IEnumerator routine) {
        StartCoroutine(Schnibble.Utils.CoroutineChain(routine, CoroReleaseBarrier()));
    }

    IEnumerator CoroWaitForBarrier() {
        while(coroBarrier != null) yield return null;
        yield break;
    }

    IEnumerator CoroReleaseBarrier() {
        coroBarrier = null;
        yield break;
    }

    IEnumerator CoroWaitAndGoToState(float waitTime, States toState)
    {
        yield return new WaitForSeconds(waitTime);

        SetState(toState);
    }


    // We connat do anything Unity related on another thread than the main thread.
    // We therefor queue commands to execute on Update();
    // TODO: thread safe queue.
    Queue<MainThreadCommand> pendingCommands = new Queue<MainThreadCommand>();

    void RegisterLobbyServerCallbacks()
    {
        if (client == null) return;

        client.OnConnected   += OnConnected;
        client.OnDisconnected += OnDisconnected;
        client.OnRoomCreated += OnRoomCreated;
        client.OnRoomRemoved += OnRoomRemoved;
        client.OnRoomJoined += OnRoomJoined;
        client.OnError       += OnLobbyClientError;
    }

    void RemoveLobbyServerCallbacks()
    {
        if (client == null) return;

        client.OnConnected   -= OnConnected;
        client.OnDisconnected -= OnDisconnected;
        client.OnRoomCreated -= OnRoomCreated;
        client.OnRoomRemoved -= OnRoomRemoved;
        client.OnRoomJoined -= OnRoomJoined;
        client.OnError       -= OnLobbyClientError;
    }

    void RegisterRoomServerCallbacks() {
        roomServer.OnRoomStartHostCB   += OnRoomHostCreated;

        roomServer.OnRoomClientEnterCB += OnRoomClientEnter;
        roomServer.OnRoomClientExitCB  += OnRoomClientExit;

        roomServer.OnRoomStartClientCB += OnRoomStartClient;
        roomServer.OnRoomStopClientCB  += OnRoomStopClient;

        roomServer.OnRoomStopServerCB += OnRoomStopServer;

        roomServer.OnRoomClientSceneChangedCB += OnRoomSceneChanged;

        roomServer.OnClientErrorCB += OnRoomClientError;

        // OnNetworkManagerChange is static.
        // It means we could add ourselves again.
        NetworkRoomManagerExt.OnNetworkManagerChange -= OnNetworkManagerChange;
        NetworkRoomManagerExt.OnNetworkManagerChange += OnNetworkManagerChange;
    }

    void RemoveRoomServerCallbacks() {
        roomServer.OnRoomStartHostCB   -= OnRoomHostCreated;

        roomServer.OnRoomClientEnterCB -= OnRoomClientEnter;
        roomServer.OnRoomClientExitCB  -= OnRoomClientExit;

        roomServer.OnRoomStartClientCB -= OnRoomStartClient;
        roomServer.OnRoomStopClientCB  -= OnRoomStopClient;

        roomServer.OnRoomStopServerCB -= OnRoomStopServer;

        roomServer.OnRoomClientSceneChangedCB -= OnRoomSceneChanged;

        roomServer.OnClientErrorCB -= OnRoomClientError;

        NetworkRoomManagerExt.OnNetworkManagerChange -= OnNetworkManagerChange;
    }

    void OnNetworkManagerChange() {
        this.Log("OnNetworkManagerChange");
        roomServer = NetworkRoomManagerExt.singleton;
        client.roomManager = roomServer;
        RegisterRoomServerCallbacks();
    }

    void OnRoomClientEnter() {
        this.Log("OnRoomClientEnter");

        if (uiRoom.isActivated) uiRoom.UpdatePlayerSlots(roomServer.roomSlots);
    }

    void OnRoomClientExit() {
        this.Log("OnRoomClientExit");

        if (uiRoom.isActivated) uiRoom.UpdatePlayerSlots(roomServer.roomSlots);
    }

    void OnRoomStartClient() {
        this.Log("OnRoomStartClient");
    }

    void OnRoomStopClient() {
        this.Log("OnRoomStopClient");
    }

    void OnRoomStopServer() {
        this.Log("OnRoomStopServer");

        client.RemoveLobby();
    }

    override protected void Awake()
    {
        if (singleton == null) {
            singleton = this;
            Access.SceneLoader().SetDontDestroyOnLoad(singleton.gameObject);
        } else {
            DestroyImmediate(this.gameObject);
            return;
        }

        base.Awake();

        inputMgr      = Access.PlayerInputsManager().player1;

        inputActivate = PlayerInputs.InputCode.UIValidate.ToString();
        inputCancel   = PlayerInputs.InputCode.UICancel.ToString();
        inputDown     = PlayerInputs.InputCode.UIDown.ToString();
        inputUp       = PlayerInputs.InputCode.UIUp.ToString();
        inputLeft     = PlayerInputs.InputCode.UILeft.ToString();
        inputRight    = PlayerInputs.InputCode.UIRight.ToString();

        client.roomManager = roomServer;

        // IMPORTANT: must be done before calling anything on lobby server
        // or else we will never have any answers.
        RegisterLobbyServerCallbacks();
        RegisterRoomServerCallbacks();
    }

    IEnumerator CoroTryConnect(IPEndPoint serverEP, int loopCount, float loopWait)
    {
        SetState(States.ConnectingToLobbyServer);

        for (int i = 0; i < loopCount && !client.IsConnected(); ++i)
        {
            client.Connect(serverEP);
            yield return new WaitForSeconds(loopWait);
        }

        yield return new WaitForSeconds(loopWait);

        if (!client.IsConnected())
        {
            OnLobbyClientError(SchLobbyClient.ErrorCode.EKO);
        }
    }

    override protected void OnDestroy()
    {
        base.OnDestroy();

        RemoveLobbyServerCallbacks();

        StopLocalServer();

        deactivate();
    }

    override protected void Update()
    {
        base.Update();
        while (pendingCommands.Count != 0)
        {
            var cmd = pendingCommands.Dequeue();
            if (cmd == null)
            {
                this.LogError("Command is null => very weird!");
            }
            else
            {
                cmd.Do();
            }
        }
    }

    // Following finctions are called by UX, so on the main thread and are safe.


    void OnRoomHostCreated()
    {
        StateSuccess();
    }

    void OnRoomSceneChanged() {
        this.Log("OnRoomSceneChanged");
    }

    // Following function can be called from threads.
    // Basically all Network callbacks.
    // Not thread safe


    class CreateRoomCmd : UIOnlineMainThreadCommand
    {
        public CreateRoomCmd(UIOnline uiOnline) : base(uiOnline) { }
        override public void Do()
        {
            this.Log("CreateRoomCmd");
            // start room as a server and client locally
            uiOnline.roomServer.networkAddress = Schnibble.Online.Utils.GetLocalIPAddress().ToString();
            var port = (ushort)UnityEngine.Random.RandomRange(10000, 65000);
            // use port 0 to use any available port from the OS.
            (uiOnline.roomServer.transport as PortTransport).Port = port;
            //(uiOnline.roomServer.transport as PortTransport).Port = 0;

            // make sure that there is not a room already running somewhere...
            if (NetworkServer.active) {
                NetworkServer.Shutdown();
            }
            if (NetworkClient.active) {
                NetworkClient.Shutdown();
            }
            uiOnline.roomServer.StopHost();
            uiOnline.roomServer.StartHost();

            // We now listen when the server wants our NATPunch data.
            // It means a client tries to connect to the room.
            uiOnline.client.OnStartNATPunch += uiOnline.OnNATPunch;

            OnCmdSuccess?.Invoke();
        }
    };
    // TODO: is queue thread safe? should be right?
    void OnRoomCreated(SchLobbyClient.RoomCreatedData data)
    {
        this.Log("OnRoomCreated");
        pendingCommands.Enqueue(new CreateRoomCmd(this));
    }

    void OnRoomRemoved() {
        this.Log("OnRoomRemoved");
    }

    void OnRoomJoined() {
        this.Log("OnRoomJoined");
    }

    class OnNATPunchCmd : UIOnlineMainThreadCommand
    {
        IPEndPoint ep;
        SchCustomRelayKcpTransport transport;

        public OnNATPunchCmd(UIOnline uiOnline, SchCustomRelayKcpTransport transport, IPEndPoint ep) : base(uiOnline)
        {
            this.ep        = ep;
            this.transport = transport;
        }

        override public void Do()
        {
            uiOnline.StartCoroutine(transport.TrySendNATPunch(ep));
        }
    };
    // ask server to ping client.
    void OnNATPunch(IPEndPoint ep)
    {
        pendingCommands.Enqueue(new OnNATPunchCmd(this, (roomServer.transport as SchCustomRelayKcpTransport), ep));
    }

    class OnConnectedCmd : UIOnlineMainThreadCommand
    {
        UIOnline.States state;
        public OnConnectedCmd(UIOnline uiOnline, UIOnline.States state) : base(uiOnline)
        {
            this.state = state;
        }

        public override void Do() { uiOnline.StateSuccess(); }
    };

    void OnConnected(int id)
    {
        this.Log("OnConnected");
        pendingCommands.Enqueue(new OnConnectedCmd(this, state));
    }

    class OnDisconnectedCmd : UIOnlineMainThreadCommand
    {
        UIOnline.States state;
        public OnDisconnectedCmd(UIOnline uiOnline, UIOnline.States state) : base(uiOnline)
        {
            this.state = state;
        }

        public override void Do() {
            // TODO: what to do?
        }
    };

    void OnDisconnected()
    {
        this.Log("OnDisconnected");
        pendingCommands.Enqueue(new OnDisconnectedCmd(this, state));
    }

    class OnLobbyClientErrorCmd : UIOnlineMainThreadCommand
    {
        UIOnline.States      state;
        SchLobbyClient.ErrorCode code;

        public OnLobbyClientErrorCmd(UIOnline uiOnline, UIOnline.States currentState, SchLobbyClient.ErrorCode error) : base(uiOnline)
        {
            this.state = currentState;
            this.code  = error;
        }

        public override void Do()
        {
            uiOnline.StateError((int)code);
        }
    };

    void OnLobbyClientError(SchLobbyClient.ErrorCode errorCode)
    {
        this.LogError("Lobby error : " + errorCode);
        pendingCommands.Enqueue(new OnLobbyClientErrorCmd(this, state, errorCode));
    }

    void OnRoomClientError(TransportError error, string reason) {
        this.LogError("Room error:" + reason);
        Exit();
    }
}
