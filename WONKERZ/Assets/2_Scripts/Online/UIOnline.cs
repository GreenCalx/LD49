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

public class UIOnline : UIPanelTabbed
{
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
        Count,
    };

    // :Sync:
    string[] stateMessage = new string[] {
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
    };

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

    public void SetState(States toState)
    {
        switch (toState)
        {
            case States.ConnectingToLobbyServer:
                {
                    // hide menu until we are connected.
                    uiMainMenu        .SetActive(false);
                    uiRoomCreationMenu.SetActive(false);
                    uiRoom.gameObject .SetActive(false);

                    StartCoroutineWithBarrier(CoroShowStateMessageWithMinTime(stateMessage[(int)toState], 1.0f));
                } break;

            case States.ConnectingToRoom:
                {
                    uiMainMenu        .SetActive(false);
                    uiRoomCreationMenu.SetActive(false);
                    uiRoom.gameObject .SetActive(false);

                    StartCoroutineWithBarrier(CoroShowStateMessageWithMinTime(stateMessage[(int)toState], 1.0f));
                } break;

            case States.CreatingRoom:
                {
                    uiMainMenu        .SetActive(false);
                    uiRoom.gameObject .SetActive(false);
                    uiRoomCreationMenu.SetActive(true );
                } break;

            case States.MainMenu:
                {
                    uiMainMenu        .SetActive(true );
                    uiRoom.gameObject .SetActive(false);
                    uiRoomCreationMenu.SetActive(false);
                } break;

            case States.InRoom:
                {
                    uiMainMenu        .SetActive(false);
                    uiRoom.gameObject .SetActive(true );
                    uiRoomCreationMenu.SetActive(false);
                } break;
            case States.Deactivated:
                {
                    // Deactivate all the UX
                    // Make sure
                    // - all transient objects are destroyed.
                    // - all callbacks are removed.
                    // - all inputs are released.
                    uiMainMenu        .SetActive(false);
                    uiRoom.gameObject .SetActive(false);
                    uiRoomCreationMenu.SetActive(false);

                    stateMessageText.enabled = false;
                    stateMessageText.gameObject.SetActive(false);
                }break;
            case States.Exit:
                {
                    // Go back to last menu
                    // Carefull we need to move back the network manager to the active scene so that it is deleted when
                    // loading back the title scene.
                    SceneManager.MoveGameObjectToScene(roomServer.gameObject.transform.root.gameObject, SceneManager.GetActiveScene());
                    SceneManager.LoadScene(Constants.SN_TITLE, LoadSceneMode.Single);
                } break;
        }
        state = toState;
    }

    // What happens in current state if an error occurs
    public void StateError(int errorCode = 0)
    {
        switch (state)
        {
            case States.ConnectingToLobbyServer:
                {
                    stateMessageText.text = stateMessage[(int)Errors.LobbyServerNotFound];

                    StartCoroutine(Schnibble.Utils.CoroutineChain(
                        CoroWait(1.0f),
                        CoroSetState(States.Exit)));
                }
                break;
        }
    }

    IEnumerator CoroWaitAndGoToState(float waitTime, States toState)
    {
        yield return new WaitForSeconds(waitTime);

        SetState(toState);
    }

    // What happens in current state if a success occurs
    public void StateSuccess(int successCode = 0)
    {
        switch (state)
        {
            case States.ConnectingToLobbyServer:
                {
                    // Show menu
                    uiMainMenu.SetActive(true);
                    // Show success message
                    stateMessageText.enabled = true;
                    stateMessageText.text = stateMessage[(int)Success.ConnectedToServer];

                    SetState(States.MainMenu);
                }
                break;
            case States.ConnectingToRoom:
                {
                    uiMainMenu.SetActive(false);
                    uiRoomCreationMenu.SetActive(false);
                    uiRoom.gameObject.SetActive(false);

                    stateMessageText.enabled = true;
                    stateMessageText.text = stateMessage[(int)Success.ConnectedToRoom];

                    SetState(States.InRoom);
                }
                break;
            case States.CreatingRoom:
                {
                    uiMainMenu.SetActive(false);
                    uiRoomCreationMenu.SetActive(false);
                    uiRoom.gameObject.SetActive(true);

                    stateMessageText.enabled = true;
                    stateMessageText.text = stateMessage[(int)Success.ConnectedToRoom];

                    SetState(States.InRoom);
                }break;
        }
    }


    // only used in debug mode as a way to create a local lobby server.
    SchLobbyServer localServer;
    // client to access the lobby server, this server will send us our clientID, the room list, etc...
    public SchLobbyClient client = new SchLobbyClient(0);

    public NetworkRoomManagerExt roomServer;

    public TextMeshProUGUI stateMessageText;


    public GameObject        uiMainMenu;
    public GameObject        uiRoomCreationMenu;
    public UIRoom            uiRoom;
    public UILobbyServerList uiServerList;

    public IPEndPoint lastNATPunch;
    // We connat do anything Unity related on another thread than the main thread.
    // We therefor queue commands to execute on Update();
    // TODO: thread safe queue.
    public Queue<MainThreadCommand> pendingCommands = new Queue<MainThreadCommand>();

    public abstract class MainThreadCommand
    {
        protected Action OnCmdSuccess;
        protected Action OnCmdError;
        public abstract void Do();
        public virtual void Undo() { }
    };

    public class UIOnlineMainThreadCommand : MainThreadCommand
    {
        public UIOnline uiOnline;
        public UIOnlineMainThreadCommand(UIOnline uiOnline)
        {
            this.uiOnline = uiOnline;
        }

        override public void Do() { }
    };

    public class CreateRoomCmd : UIOnlineMainThreadCommand
    {
        public CreateRoomCmd(UIOnline uiOnline) : base(uiOnline) { }
        override public void Do()
        {
            // start room as a server and client locally
            uiOnline.roomServer.networkAddress = Schnibble.Online.Utils.GetLocalIPAddress().ToString();

            var port = (ushort)UnityEngine.Random.RandomRange(10000, 65000);
            (uiOnline.roomServer.transport as PortTransport).Port = port;



            uiOnline.roomServer.StartHost();

            uiOnline.client.OnStartNATPunch += uiOnline.OnNATPunch;

            OnCmdSuccess?.Invoke();
        }
    };

    public class NATPunchServerCmd : UIOnlineMainThreadCommand
    {
        IPEndPoint ep;
        SchCustomRelayKcpTransport transport;

        public NATPunchServerCmd(SchCustomRelayKcpTransport transport, IPEndPoint ep, UIOnline uiOnline) : base(uiOnline)
        {
            this.ep = ep;
            this.transport = transport;
        }

        override public void Do()
        {
            uiOnline.lastNATPunch = ep;
            uiOnline.StartCoroutine(transport.TrySendNATPunch(ep));

            OnCmdSuccess?.Invoke();
        }
    };

    public class OnConnectedCmd : UIOnlineMainThreadCommand
    {
        UIOnline.States state;
        int code;
        public OnConnectedCmd(UIOnline.States state, int code, UIOnline uiOnline) : base(uiOnline)
        {
            this.state = state;
            this.code = code;
        }

        public override void Do()
        {
            uiOnline.StateSuccess();

            OnCmdSuccess?.Invoke();
        }
    };

    public class OnErrorCmd : UIOnlineMainThreadCommand
    {
        UIOnline.States state;
        Errors code;
        public OnErrorCmd(UIOnline.States state, UIOnline.Errors code, UIOnline uiOnline) : base(uiOnline)
        {
            this.state = state;
            this.code = code;
        }

        public override void Do()
        {
            uiOnline.StateError((int)code);

            OnCmdSuccess?.Invoke();
        }
    };

    #if false
    public void OnGUI() {
        int textHeight = 50;
        int textStartY = 100;
        int textStartX = 1000;
        if (lobbyServer) {
            GUI.Label(new Rect(textStartX, textStartY, 400, 100), "LobbyClient IP:" + lobbyServer.clientIP + ":" + lobbyServer.clientPort);
            textStartY += textHeight;
            GUI.Label(new Rect(textStartX, textStartY, 400, 100), "LobbyServer IP:" + lobbyServer.serverIP + ":" + lobbyServer.serverPort);
            textStartY += textHeight;
        }
        if (roomServer)
        {
            var transport = (roomServer.transport as SchCustomRelayKcpTransport);
            GUI.Label(new Rect(textStartX, textStartY, 400, 100), "RoomServer  IP:" + transport.GetServerLocalEndPoint().ToString());
            textStartY += textHeight;
            GUI.Label(new Rect(textStartX, textStartY, 400, 100), "RoomClient  IP:" + transport.GetClientLocalEndPoint().ToString());
            textStartY += textHeight;
            if (NetworkServer.active && NetworkClient.active)
            {
                // host mode
                GUI.Label(new Rect(textStartX, textStartY, 400, 100), "Room is Host and running.");
            }
            else if (NetworkServer.active)
            {
                // server only
                GUI.Label(new Rect(textStartX, textStartY, 400, 100), "Room is server only and running.");
            }
            else if (NetworkClient.isConnected)
            {
                // client only
                GUI.Label(new Rect(textStartX, textStartY, 400, 100), "Room is client only and connected to " + NetworkManager.singleton.networkAddress + ":" + transport.Port);
            }
            textStartY += textHeight;
            GUI.Label(new Rect(textStartX, textStartY, 400, 100), "NetworkManager config for connection to : " + NetworkManager.singleton.networkAddress + ":" + transport.Port);
        }
    }
    #endif

    public void RegisterLobbyServerCallbacks()
    {
        if (client == null) return;

        client.OnConnected += OnConnected;
        client.OnError += OnError;
        client.OnRoomCreated += OnRoomCreated;
    }

    public void RemoveLobbyServerCallbacks()
    {
        if (client == null) return;

        client.OnConnected -= OnConnected;
        client.OnError -= OnError;
        client.OnRoomCreated -= OnRoomCreated;
    }

    public void RegisterRoomServerCallbacks() {
        roomServer.OnRoomStartHostCB   += OnRoomHostCreated;
        roomServer.OnRoomClientEnterCB += uiRoom.OnPlayerConnected;
        roomServer.OnRoomClientExitCB  += uiRoom.OnPlayerDisconnected;

        roomServer.OnRoomStartClientCB += uiRoom.OnPlayerConnected;
        roomServer.OnRoomStopClientCB  += uiRoom.OnPlayerDisconnected;
    }

    public void RemoveRoomServerCallbacks() {
        roomServer.OnRoomStartHostCB   -= OnRoomHostCreated;
        roomServer.OnRoomClientEnterCB -= uiRoom.OnPlayerConnected;
        roomServer.OnRoomClientExitCB  -= uiRoom.OnPlayerDisconnected;

        roomServer.OnRoomStartClientCB -= uiRoom.OnPlayerConnected;
        roomServer.OnRoomStopClientCB  -= uiRoom.OnPlayerDisconnected;
    }

    public void Start()
    {
        inputMgr = Access.PlayerInputsManager().player1;

        inputActivate = PlayerInputs.InputCode.UIValidate.ToString();
        inputCancel = PlayerInputs.InputCode.UICancel.ToString();
        inputDown = PlayerInputs.InputCode.UIDown.ToString();
        inputUp = PlayerInputs.InputCode.UIUp.ToString();
        inputLeft = PlayerInputs.InputCode.UILeft.ToString();
        inputRight = PlayerInputs.InputCode.UIRight.ToString();

        client.roomManager = roomServer;

        // IMPORTANT: must be done before calling anything on lobby server
        // or else we will never have any answers.
        RegisterLobbyServerCallbacks();
        RegisterRoomServerCallbacks();

        activate();
    }

    IEnumerator CoroTryConnect(IPEndPoint serverEP, int loopCount, float loopWait)
    {
        UnityEngine.Debug.Log("CoroTryConnect");

        SetState(States.ConnectingToLobbyServer);

        for (int i = 0; i < loopCount && !client.IsConnected(); ++i)
        {
            client.Connect(serverEP);
            yield return new WaitForSeconds(loopWait);
        }

        yield return new WaitForSeconds(loopWait);

        if (!client.IsConnected())
        {
            OnError(0);
        }
    }


    public void Update()
    {
        while (pendingCommands.Count != 0)
        {
            var cmd = pendingCommands.Dequeue();
            if (cmd == null)
            {
                UnityEngine.Debug.LogError("Command is null => very weird!");
            }
            else
            {
                cmd.Do();
            }
        }
    }

    public void OnDestroy()
    {
        RemoveLobbyServerCallbacks();

        StopLocalServer();

        deactivate();
    }

    public override void activate()
    {
        base.activate();

        init();
        // On activation we try to connect
        // Will call back OnConnected, or OnError
        if (Access.GameSettings().isLocal)
        {
            StartLocalServer();
            JoinLocalServer();
        }
        else
            StartCoroutine(CoroTryConnect(SchLobbyClient.defaultRemoteEP, 5, 0.5f));
    }

    public override void deactivate()
    {
    }

    public override void init()
    {
        base.init();
        // Init childrens.
        uiServerList.init();
        uiMainMenu.GetComponent<UISelectableElement>().init();
        uiRoom.init();
        uiRoomCreationMenu.GetComponent<UISelectableElement>().init();
    }

    // Following finctions are called by UX, so on the main thread and are safe.

    public void StartLocalServer()
    {
        UnityEngine.Debug.Log("Start local server.");

        var serverAddress = "localhost";
        localServer = new SchLobbyServer(serverAddress, SchLobbyServer.defaultPort);
    }

    public void JoinLocalServer()
    {
        UnityEngine.Debug.Log("Join local server.");

        IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, SchLobbyServer.defaultPort);
        client.Connect(serverEP);
    }

    public void StopLocalServer()
    {
        UnityEngine.Debug.Log("Stop local server.");

        if (localServer == null) return;

        localServer.Close();
        localServer = null;
    }

    public void OpenCreateRoomUI()
    {
        if (roomServer.isNetworkActive)
        {
            UnityEngine.Debug.Log("Room already created and running.");
        }

        SetState(States.CreatingRoom);
    }

    void CreateRoom()
    {
        client.CreateLobby("test name for now");
    }

    public void OpenLobbyServerList(UISelectableElement activator) {
        uiServerList.activator = activator;
        uiServerList.gameObject.SetActive(true);

        uiMainMenu.SetActive(false);
    }

    public void OnRoomHostCreated()
    {
        StateSuccess();
    }

    // Following function can be called from threads.
    // Basically all Network callbacks.
    // Not thread safe

    // TODO: is queue thread safe? should be right?
    public void OnRoomCreated(SchLobbyClient.RoomCreatedData data)
    {
        pendingCommands.Enqueue(new CreateRoomCmd(this));
    }

    // ask server to ping client.
    public void OnNATPunch(IPEndPoint ep)
    {
        pendingCommands.Enqueue(new NATPunchServerCmd((roomServer.transport as SchCustomRelayKcpTransport), ep, this));
    }

    void OnConnected(int id)
    {
        UnityEngine.Debug.Log("OnConnected");
        pendingCommands.Enqueue(new OnConnectedCmd(state, id, this));
    }

    void OnError(int errorCode)
    {
        UnityEngine.Debug.Log("OnError");
        pendingCommands.Enqueue(new OnErrorCmd(state, (Errors)errorCode, this));
    }
}
