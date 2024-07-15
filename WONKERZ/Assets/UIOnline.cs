using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Schnibble.UI;
using Schnibble;
using Schnibble.Online;
using System.Net.Sockets;
using System.Net;
using System;

using Wonkerz;

public class UIOnline : UIPanelTabbed
{
    public SchServerLobby     lobbyServer;
    public NetworkRoomManager roomServer;

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
        public virtual void Undo() {}
    };

    public class CreateRoomCmd : MainThreadCommand
    {
        UIOnline cmdObj;
        public CreateRoomCmd(UIOnline uiOnline)
        {
            cmdObj = uiOnline;
        }

        override public void Do()
        {
            // start room as a server and client locally
            cmdObj.roomServer.networkAddress = Schnibble.Online.Utils.GetLocalIPAddress().ToString();

            var port = (ushort)UnityEngine.Random.RandomRange(10000, 65000);
            (cmdObj.roomServer.transport as PortTransport).Port = port;

            cmdObj.roomServer.StartHost();
            cmdObj.lobbyServer.client.OnStartNATPunch += cmdObj.OnNATPunch;

            OnCmdSuccess?.Invoke();
        }
    };

    public class NATPunchServerCmd : MainThreadCommand
    {
        IPEndPoint ep;
        SchCustomRelayKcpTransport transport;
        UIOnline uiOnline;
        public NATPunchServerCmd(SchCustomRelayKcpTransport transport, IPEndPoint ep, UIOnline uiOnline)
        {
            this.ep = ep;
            this.transport = transport;
            this.uiOnline = uiOnline;
        }

        override public void Do()
        {
            uiOnline.lastNATPunch = ep;
            uiOnline.StartCoroutine(transport.TrySendNATPunch(ep));
            //transport.TrySendNATPunch(ep);

            OnCmdSuccess?.Invoke();
        }
    };

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

    public void Start() {
        inputMgr = Access.PlayerInputsManager().player1;

        inputActivate = PlayerInputs.InputCode.UIValidate.ToString();
        inputCancel   = PlayerInputs.InputCode.UICancel.ToString();
        inputDown     = PlayerInputs.InputCode.UIDown.ToString();
        inputUp       = PlayerInputs.InputCode.UIUp.ToString();
        inputLeft     = PlayerInputs.InputCode.UILeft.ToString();
        inputRight    = PlayerInputs.InputCode.UIRight.ToString();

        activate();

        lobbyServer.client.OnRoomCreated += OnRoomCreated;
    }


    public void Update()
    {
        while (pendingCommands.Count != 0)
        {
            var cmd = pendingCommands.Dequeue();
            if (cmd == null) {
                UnityEngine.Debug.LogError("Command is null => very weird!");
            } else {
                cmd.Do();
            }
        }
    }

    public void OnDestroy() {
        deactivate();
    }

    // Following finctions are called by UX, so on the main thread and are safe.

    public void StartLocalServer() {
        lobbyServer.StartLocalServer();
        lobbyServer.client.OnRoomCreated += OnRoomCreated;
    }

    public void JoinLocalServer() {
        lobbyServer.JoinLocalServer();
        lobbyServer.client.OnRoomCreated += OnRoomCreated;
    }

    public void CreateRoom() {
        if (roomServer.isNetworkActive) {
            UnityEngine.Debug.Log("Room already created and running.");
        }
        lobbyServer.client.CreateLobby("this is a test lobby, please connect to me I am lonely :(.");
    }

    // Following function can be called from threads.
    // Basically all Network callbacks.
    // Not thread safe

    public void OnRoomCreated(SchLobbyClient.RoomCreatedData data) {
        pendingCommands.Enqueue(new CreateRoomCmd(this));
    }

    // ask server to ping client.
    public void OnNATPunch(IPEndPoint ep) {
        pendingCommands.Enqueue(new NATPunchServerCmd((roomServer.transport as SchCustomRelayKcpTransport), ep, this));
    }

}
