using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble.UI;
using Schnibble;
using Schnibble.Online;
using Mirror;
using System.Net;
using System.Net.Sockets;


public class UILobbyServerTab : UITextTab 
{
    public Lobby lobby;

    UILobbyServerList serverList;

    public class NATPunchClientCmd : UIOnline.MainThreadCommand
    {
        UILobbyServerTab tab;
        IPEndPoint ep;
        public NATPunchClientCmd(UILobbyServerTab tab, IPEndPoint ep) {
            this.tab = tab;
            this.ep = ep;
        }

        public override void Do()
        {
            // Setup client remotePoint as host.
            (tab.serverList.online.roomServer.transport as PortTransport).Port = (ushort)ep.Port;
            tab.serverList.online.roomServer.networkAddress = ep.Address.ToString();
            // try to connect to the server directly.
            //tab.StartCoroutine(tab.TryConnectToServer());
            // Mirror should send several messages to try to connect.
            if (!NetworkClient.active) {
                    UnityEngine.Debug.Log("coroutine tryconnectserver.");
                    // need to startClient to get back localEndPoint.
                    tab.serverList.online.roomServer.StartClient();
                    // now that client is started, send to server the port.
                    NetworkWriter writer = NetworkWriterPool.Get();
                writer.WriteByte((byte)SchLobbyServer.OpCode.NATPunchIP);
                SchLobbyServer.NATPunchIP data = new SchLobbyServer.NATPunchIP();
                data.clientID = tab.lobby.roomID ;
                data.port = (tab.serverList.online.roomServer.transport as SchCustomRelayKcpTransport).GetClientLocalEndPoint().Port;

                UnityEngine.Debug.Log("Sending NATPUnchIP to " + data.clientID + " with port=" + data.port);

                writer.Write(data);

                tab.serverList.online.lobbyServer.client.Send(writer);
            }
        }
    }
    // avaid init before having parent set.
    protected override void Awake()
    {
    }

    override public void init() {
        base.init();

        serverList = Parent as UILobbyServerList;
        if (serverList == null) {
            UnityEngine.Debug.LogError("Please connect a Parent of type UILobbyServerList to UILobbyServerTab.");
        }

        text.text = lobby.name;
    }

    public void OnRoomJoined(SchLobbyClient.RoomJoinedData data) {
        UnityEngine.Debug.Log("Room wit id " + data.roomID + " joined.");
        serverList.online.roomServer.StartClient();
    }

    public IEnumerator TryConnectToServer() {
        for (int i = 0; i < 10; ++i) {
            if (!NetworkClient.active) {
                UnityEngine.Debug.Log("coroutine tryconnectserver.");
                serverList.online.roomServer.StartClient();
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void OnActivated() {
        #if false
        if (lobby.hostID != serverList.online.lobbyServer.clientID)
        {
            (serverList.online.roomServer.transport as PortTransport).Port = (ushort)Random.RandomRange(10000, 65000);
        }
        // try to joint lobby.
        serverList.online.lobbyServer.client.OnStartNATPunch += OnNATPunch;
        // Update RoomServer as client to use a new port.
        serverList.online.lobbyServer.client.JoinLobby(lobby.roomID);
        #endif

        serverList.online.lobbyServer.client.OnStartNATPunch += OnNATPunch;
        serverList.online.lobbyServer.client.JoinLobby(lobby.roomID);
    }

    // Nedd to be on main thread (cf UIOnline)
    
    public void OnNATPunch(IPEndPoint toNATPunch) {
        serverList.online.pendingCommands.Enqueue(new NATPunchClientCmd(this, toNATPunch));
        //SchLobbyClient NATPunchClient = new SchLobbyClient(port);
        //NATPunchClient.OnRoomJoined += OnRoomJoined;
        //Utils.StartNATPunch(NATPunchClient, toNATPunch);
    }
}
