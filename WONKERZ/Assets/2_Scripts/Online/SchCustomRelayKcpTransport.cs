using Mirror;
using kcp2k;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using UnityEngine;

// Custom class to have NATPunch and relays.
// For now uses KCP but could use any port based transport in the future.
//
// How it works:
//    - NATPunch :
//        - When a server is launched, we start another client that will proxy it.
//          This way we have a controllable IP:Port ta NATPunch. Every client will try
//          to connect through it instead of directily on the server.
//        - When a client is created, we first try to NATPunch our way into the server
//    The NATPunchers act as man in the middle.
//       Direct connection :
//       ------------           -----------------------             ----------------------            ----------
//      | RoomServer | <------> | RoomServerNATPuncher |  <======>  | RoomClientNATPuncher| <------> | RoomClient|
//      ------------            ----------------------             ----------------------             ---------
//       NATPunched connection :
//       ------------           -----------------------              ---------------------            -----------
//      | RoomServer | <------> | RoomServerNATPuncher |  <======>  | RoomClientNATPuncher| <------> | RoomClient|
//      ------------            ----------------------               ---------------------            -----------
//
//     Note: We could use NATPunching to have direct connection between RoomServer and RoomClient that are behing NAT
//           But in our case we are using different Lobby/Room server and clients, meaning we might create a
//
//  NATPunch Client/Server datagram:
//
//   LobbyClient    Client NAT      Lobby Server         ServerNAT       LobbyClient
//    =======================================================================
//    JoinRoom ------------------- 
//                                 Request NAT IP --------------------->  
//          <--------------------- Request NAT IP   
//      GetPort -->                                                      
//                                                          <------  GetPort
//                                                    SendPort -------->
//             <-- SendPort                                   
//                                           <----------------------- NATIP
//      NATIP  -------------------->   
//                                   StartNATPunch ----------------> 
//          <------------------------ StartNATPunch
//StrartNATPunch ---> 
//                                                           <------ StartNATPunch
//                 <----------------------------------- NATPunch 
//              NATPunch ---------------------------------->
//          OnRelayConnected <---------------------  NATPunchReceived
//              NATPUnchReceived ---------------------> OnRelayConnected 
//
//     - Relays :
//        - It is the same thing as NATPuncher but is using known server as man in the middle.
//
//       Direct connection :
//       ------------           -----------------------             ----------------------           ----------
//      | RoomServer | <------> | RoomServerNATPuncher |  <======>  | RoomClientNATPuncher| <------> | RoomClient|
//      ------------            ----------------------             ----------------------            ---------
//       Relayed connection :
//       ------------            -------------            ------------
//      | RoomServer | <------> | RelayServer | <------> | RoomClient |
//      ------------            --------------            ------------
//


public class SchCustomRelayKcpTransport : KcpTransport
{
    SocketProxy NATRelay;

    Dictionary<int, int> connectionIDToRelayID = new Dictionary<int, int>();
    List<IPEndPoint> relayClients = new List<IPEndPoint>();

    int NATPunchCount = 10;
    void DoNATPunch() {
        NetworkWriter writer = NetworkWriterPool.Get();
        writer.WriteByte((byte)SchLobbyClient.OpCode.NATPunch);
        for (int i = 0; i < NATPunchCount; ++i) {
            ClientSend(writer.ToArraySegment(), 0);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        //OnServerDataReceived += OnDataReceived;
    }

    void OnDataReceived(int connectionID, ArraySegment<byte> data, int channel) {
        NetworkReader reader = NetworkReaderPool.Get(data);
        var opcode = reader.ReadByte();
        if (opcode == (byte)SchLobbyClient.OpCode.NATPunch) {
            NetworkWriter writer = NetworkWriterPool.Get();
            writer.WriteByte((byte)SchLobbyClient.OpCode.NATPunchReceived);
            ClientSend(writer, 0);
        } else if (opcode == (byte)SchLobbyClient.OpCode.NATPunchReceived) {
            UnityEngine.Debug.Log("nat punch received.");
        }
    }

    public IEnumerator TrySendNATPunch(IPEndPoint ep) {
        UnityEngine.Debug.LogError("TrySendNATPunch to client " + ep.ToString());
        for (int i = 0; i < 10; ++i) {
            SendNATPunchToClient(ep);
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void SendNATPunchToClient(IPEndPoint ep) {
        NetworkWriter writer = NetworkWriterPool.Get();
        writer.WriteByte((byte)SchLobbyClient.OpCode.NATPunch);
        var arr = writer.ToArraySegment();

        if (server.socket.AddressFamily == AddressFamily.InterNetworkV6) {
            if (ep.AddressFamily == AddressFamily.InterNetwork) {
                ep.Address = ep.Address.MapToIPv6();
            }
        }
        server.socket.SendToNonBlocking(arr, ep);
    }

    public IPEndPoint GetClientLocalEndPoint() {
        if (client != null && client.LocalEndPoint != null) return client.LocalEndPoint as IPEndPoint;
        return new IPEndPoint(IPAddress.None, 0);
    }

    public IPEndPoint GetServerLocalEndPoint() {
        if (server != null && server.LocalEndPoint != null) return server.LocalEndPoint as IPEndPoint;
        return new IPEndPoint(IPAddress.None, 0);
    }

    public void ConnectToRelay(IPEndPoint relayIP) {
        Port = (ushort)relayIP.Port;
        ClientConnect(relayIP.Address.ToString());
    }

    public void OnSendHook(int connectionId, ArraySegment<byte> data, int channel) {
        var remoteEP = server.connections[connectionId].remoteEndPoint;
        NATRelay.OnProxySend(remoteEP as IPEndPoint, data);
    }

    public void StartNATPunch(IPEndPoint toNATPunch) {
        NATRelay.OnRelayConnected += ConnectToRelay;
        NATRelay.StartNATPunch(toNATPunch);
    }

    public void SetupClientProxy() {
        NATRelay = new SocketProxy(client.LocalEndPoint as IPEndPoint);
        OnServerDataSent += OnSendHook;
    }

    // Setup proxy to send/receive data when NATPunched or relayed.
    public void SetupServerProxy() {
        NATRelay = new SocketProxy(server.LocalEndPoint as IPEndPoint);
        OnServerDataSent += OnSendHook;
    }
};
