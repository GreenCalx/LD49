using Mirror;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using UnityEngine;

namespace Schnibble.Online {

    // NOTE: toffa: probably not necessary if the weaver is functionnal?
    // It is error prone, but very clear.
    public static partial class WriterReaderExtensions
    {
        public static void Write(this NetworkWriter writer, SchLobbyClient.ConnectedData data)
        {
            writer.WriteInt(data.clientID);
        }

        public static SchLobbyClient.ConnectedData ReadConnectedData(this NetworkReader reader)
        {
            SchLobbyClient.ConnectedData ret = new SchLobbyClient.ConnectedData();
            ret.clientID = reader.ReadInt();
            return ret;
        }

        public static void Write(this NetworkWriter writer, SchLobbyClient.RoomCreatedData data)
        {
            writer.WriteInt(data.roomID);
        }

        public static SchLobbyClient.RoomCreatedData ReadRoomCreatedData(this NetworkReader reader)
        {
            SchLobbyClient.RoomCreatedData ret = new SchLobbyClient.RoomCreatedData();
            ret.roomID = reader.ReadInt();
            return ret;
        }

        public static void Write(this NetworkWriter writer, SchLobbyClient.RoomListData data)
        {
            writer.Write(data.lobbies);
        }

        public static SchLobbyClient.RoomListData ReadRoomListData(this NetworkReader reader)
        {
            SchLobbyClient.RoomListData res = new SchLobbyClient.RoomListData();
            res.lobbies = reader.ReadLobbyList();
            return res;
        }

        public static void Write(this NetworkWriter writer, SchLobbyClient.NATPunchIPRequestedData data)
        {
            writer.Write(data.roomID);
            writer.Write(data.NATPunchID);
        }

        public static SchLobbyClient.NATPunchIPRequestedData ReadNATPunchIPRequestedData(this NetworkReader reader)
        {
            SchLobbyClient.NATPunchIPRequestedData res = new SchLobbyClient.NATPunchIPRequestedData();
            res.roomID = reader.ReadInt();
            res.NATPunchID = reader.ReadInt();
            return res;
        }

        public static void Write(this NetworkWriter writer, SchLobbyClient.StartNATPunchData data)
        {
            writer.Write(data.ip);
        }

        public static SchLobbyClient.StartNATPunchData ReadStartNATPunchData(this NetworkReader reader)
        {
            SchLobbyClient.StartNATPunchData res = new SchLobbyClient.StartNATPunchData();
            res.ip = reader.ReadIPEndPoint();
            return res;
        }
    }

    public class SchLobbyClient
    {
        public static readonly int    defaultPort          = 8888;
        public static readonly string defaultServerAddress = "toffaraspberry.ddns.net";

        public static readonly IPEndPoint defaultLocalEP  = new IPEndPoint(IPAddress.Any, defaultPort);
        public static readonly IPEndPoint defaultRemoteEP = new IPEndPoint(Utils.GetAddress(defaultServerAddress), SchLobbyServer.defaultPort);

        // Each OpCode is an available message from the server.
        public enum OpCode
        {
            Authentified,
            Connected,
            Disconnected,
            RoomList,
            RoomCreated,
            RoomJoined,
            NATPunchIPRequested,
            StartNATPunch,
            NATPunch,
            NATPunchReceived,
        };

        // Each OpCode has an available payload sent with the message. 
        public struct AuthenticatedData
        {
            public int authToken;
        };

        public struct ConnectedData
        {
            public int clientID;
        };

        public struct RoomListData
        {
            public List<Lobby> lobbies;
        };

        public struct RoomCreatedData
        {
            public int roomID;
        };

        public struct RoomJoinedData
        {
            public int roomID;
        };

        public struct NATPunchIPRequestedData
        {
            public int roomID;
            public int NATPunchID;
        };

        public struct StartNATPunchData
        {
            public IPEndPoint ip;
        };

        public struct NATPunchData { };

        public enum ErrorCode
        {
            ELobbyExists,
            ELobbyDontExists,
            EClientAlreadyExists,
            EClientNotFound,
        };

        public enum SuccessCode
        {
            SLobbyCreated,
            SLobbyDeleted,
            SJoinLobby,
            SConnected,
        };

        // Client identification server-side.
        bool isAuthentified = false;
        int  id             = -1;

        // Room identification server-side.
        int roomID = -1;

        // Needed to get the room client port.
        public NetworkRoomManager roomManager;

        // Callbacks for server answers.
        public Action<RoomListData>    OnLobbyListRefreshed;
        public Action<RoomJoinedData>  OnRoomJoined;
        public Action<RoomCreatedData> OnRoomCreated;
        public Action<int>             OnConnected;
        public Action<IPEndPoint>      OnStartNATPunch;

        UdpClient clientConnection = null;

        public IPEndPoint remoteEndPoint = null;

        public int    GetPort   () => (clientConnection.Client.LocalEndPoint as IPEndPoint).Port;
        public string GetAddress() => (clientConnection.Client.LocalEndPoint as IPEndPoint).Address.ToString();

        // Start a client with default port an any local address.
        public SchLobbyClient()
        {
            SetupClient(defaultLocalEP);
        }

        // Start a client on user-defined port => this can fail if the port is already used.
        public SchLobbyClient(int port)
        {
            SetupClient(new IPEndPoint(IPAddress.Any, port));
        }

        void SetupClient(IPEndPoint localEP) {
            clientConnection = new UdpClient(localEP);

            //Needed to avoid bugs where UdpClient close without reasons.
            #if PLATFORM_STANDALONE_WIN
            const int SIO_UDP_CONNRESET = -1744830452;
            byte[] inValue = new byte[] {0};
            byte[] outValue = new byte[] {0};
            clientConnection.Client.IOControl(SIO_UDP_CONNRESET, inValue, outValue);
            #endif

            clientConnection.BeginReceive(new AsyncCallback(Recv), clientConnection);
        }

        public bool IsSetup()
        {
            return clientConnection != null;
        }

        public bool IsConnected()
        {
            return remoteEndPoint != null;
        }

        public void Connect(IPEndPoint serverEP)
        {
            UnityEngine.Debug.Log("client: connect to " + serverEP.ToString());
            // NOTE: in an ideal world we would be connected to only the serevr
            // but at some point we need to send data to an arbitrary client for NATPunching.
            // We dont connect, so we have to set the serverIP every time we send data, it is less efficient but fine in our case.
            //clientConnection.Connect(remoteEndPoint);
            remoteEndPoint = serverEP;
            Send(SchLobbyServer.OpCode.LobbyClientConnect);
        }

        public void Disconnect()
        {
            UnityEngine.Debug.Log("client: disconnect.");

            Send(SchLobbyServer.OpCode.LobbyClientDisconnect);
            remoteEndPoint = null;
        }

        public void Close()
        {
            UnityEngine.Debug.Log("client: close.");

            // TODO: bugfix the message that we are using a disposed object.
            // just need to wrap in try/catch 
            Disconnect();
            clientConnection.Close();
        }

        public void Send(SchLobbyServer.OpCode opCode)
        {
            clientConnection.Send(new byte[] { (byte)opCode }, 1, remoteEndPoint);
        }


        public void Send(NetworkWriter writer)
        {
            Send(writer.ToArraySegment());
        }

        public void Send(ArraySegment<byte> data)
        {
            clientConnection.Send(data.Array, data.Count, remoteEndPoint);
        }

        // Send to specific client.
        public void SendTo(NetworkWriter writer, IPEndPoint client) {
            var arr = writer.ToArraySegment();
            clientConnection.Send(arr.Array, arr.Count, client);
        }

        // Lobby management.

        public void CreateLobby(string name)
        {
            UnityEngine.Debug.Log("client: send create lobby.");

            NetworkWriter writer = NetworkWriterPool.Get();
            writer.WriteByte((byte)SchLobbyServer.OpCode.CreateRoom);

            SchLobbyServer.CreateRoomData data = new SchLobbyServer.CreateRoomData();
            data.hostID = id;
            data.name = name;

            writer.Write(data);

            Send(writer);
        }

        public void RemoveLobby()
        {
            UnityEngine.Debug.Log("client: remove lobby.");

            Send(SchLobbyServer.OpCode.RemoveRoom);
        }

        public void GetLobbies()
        {
            UnityEngine.Debug.Log("client: get lobbies.");

            NetworkWriter writer = NetworkWriterPool.Get();
            writer.WriteByte((byte)SchLobbyServer.OpCode.ListRooms);

            Send(writer);
        }

        public void JoinLobby(int lobbyID)
        {
            UnityEngine.Debug.Log("client: join lobby.");

            NetworkWriter writer = NetworkWriterPool.Get();
            writer.WriteByte((byte)SchLobbyServer.OpCode.JoinRoom);
            writer.WriteInt(lobbyID);

            Send(writer);
        }

        void Recv(IAsyncResult result)
        {

            IPEndPoint serverIP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = clientConnection.EndReceive(result, ref serverIP);
            clientConnection.BeginReceive(new AsyncCallback(Recv), clientConnection);

            UnityEngine.Debug.Log("Lobby client [Recv] from " + serverIP.ToString());

            ProcessData(data, serverIP);
        }

        void ProcessData(byte[] data, IPEndPoint clientIP)
        {
            NetworkReader reader = NetworkReaderPool.Get(data);
            OpCode opCode = (OpCode)reader.ReadByte();
            switch (opCode)
            {
                case OpCode.Authentified:
                    {
                        UnityEngine.Debug.Log("Lobby client received : [Authentified] from " + clientIP.ToString());
                        isAuthentified = true;
                        break;
                    }
                case OpCode.Connected:
                    {
                        ConnectedData requestData = reader.ReadConnectedData();
                        id = requestData.clientID;
                        UnityEngine.Debug.Log("Lobby client received : [Connected] with ID="+id+" from " + clientIP.ToString());
                        OnConnected?.Invoke(id);
                        break;
                    }
                case OpCode.Disconnected:
                    {
                        UnityEngine.Debug.Log("Lobby client received : [Disconnected] from "+clientIP.ToString());
                        id = -1;
                        break;
                    }
                case OpCode.RoomCreated:
                    {
                        RoomCreatedData requestData = reader.ReadRoomCreatedData();
                        roomID = requestData.roomID;

                        UnityEngine.Debug.Log("Lobby client received : [RoomCreated] with roomID="+roomID+" from "+clientIP.ToString());

                        OnRoomCreated?.Invoke(requestData);
                        break;
                    }
                case OpCode.RoomJoined:
                    {
                        UnityEngine.Debug.Log("Lobby client received : [RoomJoined] from " + clientIP.ToString());

                        OnRoomJoined?.Invoke(new RoomJoinedData());
                        break;
                    }
                case OpCode.RoomList:
                    {
                        RoomListData requestData = reader.ReadRoomListData();

                        UnityEngine.Debug.Log("Lobby client received : [RoomList] with "+requestData.lobbies.Count+ " rooms from " + clientIP.ToString());
                        int idx = 0;
                        foreach (var lobby in requestData.lobbies)
                        {
                            UnityEngine.Debug.LogFormat("Lobby client : [RoomList]["+idx+"] Room {0} with hostID {1}", lobby.name, lobby.hostID);
                            idx++;
                        }
                        OnLobbyListRefreshed?.Invoke(requestData);
                        break;
                    }
                case OpCode.NATPunchIPRequested:
                    {
                        UnityEngine.Debug.LogError("Lobby client: [NATPunchIPRequested] rec."); 
                        NATPunchIPRequestedData requestData = reader.ReadNATPunchIPRequestedData();
                        UnityEngine.Debug.Log("Lobby client received : [NATPunchIPRequested] with roomID="+requestData.roomID+" isHost="+ (roomID == requestData.roomID) +" from " + clientIP.ToString());

                        SchLobbyServer.NATPunchIP answerData = new SchLobbyServer.NATPunchIP();
                        if (roomID == requestData.roomID)
                        {
                            UnityEngine.Debug.LogError("Lobby client: [NATPunchIPRequested] host."); 
                            // // is host return server port.
                            // if (roomServer == null)
                            // {
                            //     UnityEngine.Debug.LogError("lobby client: asked to give NATPunchIP as host, but no room server is found.");
                            //     return;
                            // }
                            // answerData.port = roomServer.port;
                            // answerData.clientID = requestData.NATPunchID;

                            // UnityEngine.Debug.LogFormat("lobby client: NATPunchIPRequested on room host to client id {0}.", answerData.clientID);

                            // is host return server port.
                            if (roomManager == null)
                            {
                                UnityEngine.Debug.LogError("Lobby client : [NATPunchIPRequested] Asked to give NATPunchIP as host, but no roomManager is found.");
                                return;
                            }
                            //host has to send back server ip => aka networkmanager remote end point.
                            answerData.port = (roomManager.transport as PortTransport).Port;
                            answerData.clientID = requestData.NATPunchID;

                            UnityEngine.Debug.Log("Lobby client sent: [NATPunchIP] with port=" + answerData.port + " on roomID="+requestData.roomID+" with hostID="+requestData.NATPunchID+" to clientid=" + answerData.clientID);
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("Lobby client: [NATPunchIPRequested] client."); 
                            // // is client return client port.
                            // if (roomClient == null)
                            // {
                            //     // create a new client.
                            //     roomClient = new SchRoomClient();
                            // }
                            // answerData.port = roomClient.port;
                            // answerData.clientID = requestData.NATPunchID;
                            // UnityEngine.Debug.LogFormat("lobby client: NATPunchIPRequested on client to room host id {0}.", answerData.clientID);

                            // is client return client port.
                            if (roomManager == null)
                            {
                                UnityEngine.Debug.LogError("Lobby client: [NATPunchIPRequested] RoomManager is null.");
                            }

                            // Client has to send back its local end point port.

                            answerData.port = (roomManager.transport as SchCustomRelayKcpTransport).GetClientLocalEndPoint().Port;
                            answerData.clientID = requestData.NATPunchID;
                            UnityEngine.Debug.Log("Lobby client sent: [NATPunchIP] with port=" + answerData.port + " on roomID="+requestData.roomID+" with hostID="+requestData.NATPunchID+" to clientid=" + answerData.clientID);
                        }

                        NetworkWriter writer = NetworkWriterPool.Get();
                        writer.WriteByte((byte)SchLobbyServer.OpCode.NATPunchIP);
                        writer.Write(answerData);

                        Send(writer);
                        break;
                    }
                case OpCode.NATPunch:
                    {
                        UnityEngine.Debug.Log("Lobby client received : [NATPunch] from " + clientIP.ToString());
                        // someone is contacting us from nat punching => connection established.
                        // send back that we received the message.
                        NetworkWriter writer = NetworkWriterPool.Get();
                        writer.WriteByte((byte)SchLobbyClient.OpCode.NATPunchReceived);

                        UnityEngine.Debug.Log("Lobby client sent : [NATPunchReceived] to " + clientIP.ToString());
                        SendTo(writer, clientIP);
                        break;
                    }
                case OpCode.NATPunchReceived:
                    {
                        UnityEngine.Debug.Log("lobby client received : [NATPunchReceived] from " + clientIP.ToString());

                        if (NetworkClient.active) {
                            UnityEngine.Debug.Log("Create player.");
                            // create a player and cannect to room.
                            var mgr = NetworkManager.singleton;
                            mgr.networkAddress = clientIP.Address.ToString();
                            (mgr.transport as PortTransport).Port = (ushort)clientIP.Port;
                            mgr.StartClient();
                        }

                        break;
                    }
                case OpCode.StartNATPunch:
                    {
                        StartNATPunchData requestData = reader.ReadStartNATPunchData();
                        UnityEngine.Debug.Log("Lobby client received : [StartNATPunch] to IP="+requestData.ip.ToString() + " from " + clientIP.ToString());
                        OnStartNATPunch?.Invoke(requestData.ip);
                        break;
                    }
            }
        }
    };
}
