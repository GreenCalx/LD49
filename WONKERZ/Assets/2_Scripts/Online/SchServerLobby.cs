#if !SCHSERVER
#define SCH_NOT_SERVER
#endif

using Mirror;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using UnityEngine;

namespace Schnibble.Online
{

    public struct CreateLobbySettings
    {
        public string name;
    };

    public struct Lobby
    {
        public string name;
        public int hostID;
        public int roomID;
    };

    public class SchServerLobby : MonoBehaviour
    {
        public NetworkRoomManager roomManager;
        bool isLocal = false;
        SchLobbyServer localServer;
        // just for editor introspection
        public string serverIP;
        public int serverPort;

        public string clientIP;
        public int clientPort;

        public int clientID = -1;
        public SchLobbyClient client { get; private set; }

        [Conditional("SCH_NOT_SERVER")]
        public void StartLocalServer()
        {
            var serverAddress = "localhost";
            localServer = new SchLobbyServer(serverAddress, SchLobbyServer.defaultPort);
            isLocal = true;

            Init();
        }

        [Conditional("SCH_NOT_SERVER")]
        public void JoinLocalServer()
        {
            isLocal = true;

            Init();
        }

        [Conditional("SCH_NOT_SERVER")]
        public void StopLocalServer()
        {
            if (localServer != null) localServer.Close();
            isLocal = false;
        }

        void Start()
        {
#if !SCHSERVER

            Init();
#else
            localServer = new SchLobbyServer(SchLobbyServer.defaultPort);
#endif
        }

#if DEVELOPMENT_BUILD
        bool isPrintLogsToggled = false;

        struct ReceivedLogs
        {
            public string log;
            public float timeDisplayed;
        };

        List<ReceivedLogs> receivedLogs = new List<ReceivedLogs>();

        [Conditional("SCH_NOT_SERVER")]
        void DisplayNextString(int i, string log)
        {
            GUI.Label(new Rect(25, 25 * i, 1000, 100), log);
        }

        [Conditional("SCH_NOT_SERVER")]
        void OnGUI()
        {
            int i = 0;
            foreach (var l in receivedLogs)
            {
                DisplayNextString(i, l.log);
                ++i;
            }
        }

        // Cannot use conditional on function that coul be delegate?
        //[Conditional("SCH_NOT_SERVER")]
        void OnLogReceived(string logString, string stackTrace, LogType type)
        {
#if !SCHSERVER
            receivedLogs.Add(new ReceivedLogs { log = logString, timeDisplayed = 0 });
            #endif
        }
        #endif

        [Conditional("SCH_NOT_SERVER")]
        void Update()
        {
#if DEVELOPMENT_BUILD
            // Press L to show logs.
            if (Input.GetKeyDown(KeyCode.L))
            {
                if (!isPrintLogsToggled)
                {
                    Application.logMessageReceivedThreaded += OnLogReceived;
                    UnityEngine.Debug.developerConsoleVisible = true;
                    isPrintLogsToggled = true;
                }
                else
                {
                    Application.logMessageReceivedThreaded -= OnLogReceived;
                    UnityEngine.Debug.developerConsoleVisible = true;
                    isPrintLogsToggled = false;
                }
            }


            if (isPrintLogsToggled)
            {
                for (int i = 0; i < receivedLogs.Count; ++i)
                {
                    var l = receivedLogs[i];
                    l.timeDisplayed += Time.deltaTime;
                    receivedLogs[i] = l;
                }

                for (int i = 0; i < receivedLogs.Count; ++i)
                {
                    var l = receivedLogs[i];
                    if (l.timeDisplayed > 3)
                    {
                        receivedLogs.RemoveAt(i);
                    }
                    ++i;
                }
            }
#endif
        }

        [Conditional("SCH_NOT_SERVER")]
        public void Init()
        {
            // TODO: connection error checks.
            IPAddress serverAddress = IPAddress.Loopback;
            if (isLocal)
            {
                serverAddress = IPAddress.Loopback;
            }
            else
            {
                serverAddress = Utils.GetAddress(SchLobbyClient.defaultServerAddress);
            }
            client = new SchLobbyClient();
            client.OnConnected += OnConnected;
            client.Connect(new IPEndPoint(serverAddress, SchLobbyServer.defaultPort));

            // TODO: remove this.
            client.roomManager = roomManager;

            // TODO: remove this
            serverIP = serverAddress.ToString();
            serverPort = SchLobbyServer.defaultPort;

            clientIP = client.GetAddress();
            clientPort = client.GetPort();
        }

        void OnDestroy()
        {
#if !SCHSERVER
            client.Close();
            #else
            if(localServer != null) localServer.Close();
            #endif
        }

        // Cannat use conditional on function that can be a delegate?
        //[Conditional("SCH_NOT_SERVER")]
        void OnConnected(int newClientID)
        {
#if !SCHSERVER
            clientID = newClientID;
#endif
        }

    }
}



//START ARCHIVE FOR LATER
#if false

    // use to NATPunch a certain port 
    // and pass it to another ip:port
    public class SocketProxy {
        UdpClient client;
        // proxied IP
        IPEndPoint remoteEndPoint;
        // relay IP to send data to
        IPEndPoint relayEndPoint;

        public Action<IPEndPoint> OnRelayConnected;

        public SocketProxy(IPEndPoint remoteEndPoint) {
            client = new UdpClient();
            client.BeginReceive(new AsyncCallback(Recv), client);
            this.remoteEndPoint = remoteEndPoint;
        }

        public int GetPort() => (client.Client.LocalEndPoint as IPEndPoint).Port;

        public void StartNATPunch(IPEndPoint toNATPunch) {
            Utils.StartNATPunch(client, toNATPunch);
        }

        void Recv(IAsyncResult result)
        {
            UnityEngine.Debug.Log("SocketProxy : Received data.");
            IPEndPoint clientIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = client.EndReceive(result, ref clientIP);
            client.Send(data, data.Length, remoteEndPoint);
            client.BeginReceive(new AsyncCallback(Recv), client);

            NetworkReader reader = NetworkReaderPool.Get(data);
            if ((SchLobbyClient.OpCode)(reader.ReadByte()) == SchLobbyClient.OpCode.NATPunch)
            {
                // Send back received to initiate connection.
                client.Send(new byte[] { (byte)SchLobbyClient.OpCode.NATPunchReceived }, 1, clientIP);
            }
            else if ((SchLobbyClient.OpCode)(reader.ReadByte()) == SchLobbyClient.OpCode.NATPunchReceived)
            {
                // We received a NATPunch validation, lets connect the proxied server/client to the relay.
                OnRelayConnected?.Invoke(clientIP);
            }
        }

        public void OnProxySend(IPEndPoint remoteEndPoint, ArraySegment<byte> data) {
            client.Send(data.Array, data.Count, remoteEndPoint);
        }
    }

// Room client/server stubs for debugging without using Mirror roomManager
// Not used anymore
public class SchRoomClient
{
    public readonly static int defaultPort = 2222;

    UdpClient socket;

    public int port { get; private set; }

    public SchRoomClient()
    {
        try
        {
            socket = new UdpClient(defaultPort);
        }
        catch (Exception ex)
        {
            try
            {
                socket = new UdpClient(0);
            }
            catch (Exception ex2)
            {

            }
        }
        port = (socket.Client.LocalEndPoint as IPEndPoint).Port;

        socket.BeginReceive(new AsyncCallback(Recv), socket);
    }

    void Recv(IAsyncResult result)
    {
        UnityEngine.Debug.Log("room client: recv.");
        IPEndPoint clientIP = new IPEndPoint(IPAddress.Any, 0);
        byte[] data = socket.EndReceive(result, ref clientIP);
        socket.BeginReceive(new AsyncCallback(Recv), socket);

        ProcessData(data, clientIP);
    }

    void ProcessData(byte[] data, IPEndPoint clientIP)
    {
    }

    public void Close()
    {
        if (socket != null) socket.Close();
    }
}

public class SchRoomServer
{
    public readonly static int defaultPort = 2222;

    public enum OpCode
    {
        NoOp = 0,
        NATPunch,
        Connect,
        Disconnect,
    };

    UdpClient socket;

    public int port { get; private set; }

    public void StartNATPunch(IPEndPoint client)
    {
        Utils.StartNATPunch(socket, client);
    }

    public void Close()
    {
        if (socket != null) socket.Close();
    }

    public SchRoomServer()
    {
        try
        {
            socket = new UdpClient(defaultPort);
        }
        catch (Exception ex)
        {
            try
            {
                socket = new UdpClient(0);
            }
            catch (Exception ex2)
            {
                UnityEngine.Debug.LogError("Failed to create RoomServer.");
            }
        }
        port = (socket.Client.LocalEndPoint as IPEndPoint).Port;
        socket.BeginReceive(new AsyncCallback(Recv), socket);
    }

    void Recv(IAsyncResult result)
    {
        UnityEngine.Debug.Log("room server: recv.");
        IPEndPoint clientIP = new IPEndPoint(IPAddress.Any, 0);
        byte[] data = socket.EndReceive(result, ref clientIP);
        socket.BeginReceive(new AsyncCallback(Recv), socket);

        ProcessData(data, clientIP);
    }

    void ProcessData(byte[] data, IPEndPoint clientIP)
    {
        NetworkReader reader = NetworkReaderPool.Get(data);
        OpCode opCode = (OpCode)reader.ReadByte();
        switch (opCode)
        {
            case OpCode.Connect:
                {
                    UnityEngine.Debug.Log("Client {0} connected.");
                    break;
                }
            case OpCode.Disconnect:
                {
                    UnityEngine.Debug.Log("Client {0} disconnected.");
                    break;
                }
        }
    }
};

#endif

