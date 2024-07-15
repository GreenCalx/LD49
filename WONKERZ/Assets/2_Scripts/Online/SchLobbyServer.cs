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
    // NOTE: toffa: probably not necessary if the weaver is functionnal?
    // It is error prone, but very clear.
    public static partial class WriterReaderExtensions
    {
        public static void Write(this NetworkWriter writer, IPEndPoint ip)
        {
            if (ip != null)
            {
                writer.WriteString(ip.Address.ToString());
                writer.WriteInt(ip.Port);
            }
            else
            {
                writer.WriteString("0.0.0.0");
                writer.WriteInt(0);
            }
        }

        public static IPEndPoint ReadIPEndPoint(this NetworkReader reader)
        {
            var address = IPAddress.Parse(reader.ReadString());
            var port = reader.ReadInt();
            return new IPEndPoint(address, port);
        }

        public static void Write(this NetworkWriter writer, SchLobbyServer.CreateRoomData data)
        {
            writer.WriteInt(data.hostID);
            writer.WriteString(data.name);
        }

        public static SchLobbyServer.CreateRoomData ReadCreateRoomData(this NetworkReader reader)
        {
            SchLobbyServer.CreateRoomData ret = new SchLobbyServer.CreateRoomData();
            ret.hostID = reader.ReadInt();
            ret.name = reader.ReadString();
            return ret;
        }

        public static void Write(this NetworkWriter writer, SchLobbyServer.RemoveRoomData data)
        {
            writer.WriteInt(data.roomID);
        }

        public static SchLobbyServer.RemoveRoomData ReadRemoveRoomData(this NetworkReader reader)
        {
            SchLobbyServer.RemoveRoomData ret = new SchLobbyServer.RemoveRoomData();
            ret.roomID = reader.ReadInt();
            return ret;
        }

        public static void Write(this NetworkWriter writer, SchLobbyServer.NATPunchIP data)
        {
            writer.WriteInt(data.port);
            writer.WriteInt(data.clientID);
            writer.WriteInt(data.roomID);
        }

        public static SchLobbyServer.NATPunchIP ReadNATPunchIP(this NetworkReader reader)
        {
            SchLobbyServer.NATPunchIP ret = new SchLobbyServer.NATPunchIP();
            ret.port = reader.ReadInt();
            ret.clientID = reader.ReadInt();
            ret.roomID = reader.ReadInt();
            return ret;
        }

        public static void Write(this NetworkWriter writer, Lobby data)
        {
            writer.WriteString(data.name);
            writer.WriteInt(data.hostID);
        }

        public static Lobby ReadLobby(this NetworkReader reader)
        {
            Lobby res = new Lobby();
            res.name = reader.ReadString();
            res.hostID = reader.ReadInt();
            return res;
        }

        public static void Write(this NetworkWriter writer, List<Lobby> data)
        {
            writer.WriteInt(data.Count);
            foreach (var l in data)
            {
                writer.Write(l);
            }
        }

        public static List<Lobby> ReadLobbyList(this NetworkReader reader)
        {
            int count = reader.ReadInt();
            List<Lobby> res = new List<Lobby>(count);
            for (int i = 0; i < count; ++i)
            {
                res.Add(reader.ReadLobby());
            }
            return res;
        }

        public static void Write(this NetworkWriter writer, SchLobbyServer.JoinRoomData data)
        {
            writer.Write(data.roomID);
        }


        public static SchLobbyServer.JoinRoomData ReadJoinRoomData(this NetworkReader reader)
        {
            SchLobbyServer.JoinRoomData res = new SchLobbyServer.JoinRoomData();
            res.roomID = reader.ReadInt();
            return res;
        }
    }

    public class SchLobbyServer
    {
            public readonly static int    defaultPort = 7777;

            public bool isLocal = false;

            Dictionary<int, int> clientHashToServerHash = new Dictionary<int, int>();
            List<IPEndPoint> clients = new List<IPEndPoint>();

            Dictionary<int, int> clientHashToLobbyHash = new Dictionary<int, int>();
            List<Lobby> lobbies = new List<Lobby>();

            UdpClient socket;

            // Each OpCode is an available message from the client.
            public enum OpCode
            {
                NoOp = 0,
                // Lobby client OpCode
                LobbyClientAuthRequest,
                LobbyClientConnect,
                LobbyClientDisconnect,
                // Lobby API
                CreateRoom,
                RemoveRoom,
                ListRooms,
                JoinRoom,
                // NATPunch API
                NATPunchIP,
            };

            // Each OpCode has an available payload sent with the message. 
            struct LobbyClientAuthRequestData
            {
                int authKey;
            };

            struct LobbyClientConnectData { };
            struct LobbyClientDisconnectData { };

            public struct CreateRoomData
            {
                public string name;
                public int hostID;
            };

            public struct RemoveRoomData
            {
                public int roomID;
            };

            struct ListRoomsData { };

            public struct JoinRoomData
            {
                public int roomID;
            };

            public struct NATPunchIP
            {
                public int clientID;
                public int roomID;
                public int port;
            };

            public IPEndPoint GetIPEndPoint()
            {
                var localEP = (socket.Client.LocalEndPoint as IPEndPoint);
                if (localEP.Address.Equals(IPAddress.Any))
                {
                    localEP.Address = Utils.GetLocalIPAddress();
                }
                return localEP;
            }

            // Start with default port.
            public SchLobbyServer()
            {
                SetupSocket(new IPEndPoint(IPAddress.Any, defaultPort));
            }

            // Start with user defined port
            // NOTE: set to 0 to let the OS choose an available port.
            // IMPORTANT: if you set it to 0, the actual used port will be set only after first Send
            public SchLobbyServer(int port)
            {
                SetupSocket(new IPEndPoint(IPAddress.Any, port));
            }

            // Start with user defined address on default port.
            // Address is a local end-point.
            public SchLobbyServer(string address)
            {
                SetupSocket(new IPEndPoint(Utils.GetAddress(address), defaultPort));
            }

            // Start with fully user defined local end-point.
            public SchLobbyServer(string address, int port)
            {
                SetupSocket( new IPEndPoint(Utils.GetAddress(address), port));
            }

            void SetupSocket(IPEndPoint localEP)
            {
                UnityEngine.Debug.LogFormat("lobby server: setup server with ip {0}:{1}.", localEP.Address, localEP.Port);

                socket = new UdpClient(localEP);

                //Needed to avoid bugs where UdpClient close without reasons.
                #if PLATFORM_STANDALONE_WIN
                const int SIO_UDP_CONNRESET = -1744830452;
                byte[] inValue = new byte[] {0};
                byte[] outValue = new byte[] {0};
                socket.Client.IOControl(SIO_UDP_CONNRESET, inValue, outValue);
                #endif

                // Start to receive packets.
                socket.BeginReceive(new AsyncCallback(Recv), socket);
            }

            public void Close()
            {
                socket.Close();
            }

            // cache to avoid new
            byte[] oneByte = new byte[1];
            void Send(SchLobbyClient.OpCode code, IPEndPoint client)
            {
                oneByte[0] = (byte)code;
                socket.Send(oneByte, 1, client);
            }

            void Send(NetworkWriter writer, IPEndPoint client)
            {
                var arr = writer.ToArraySegment();
                socket.Send(arr.Array, arr.Count, client);
            }

            void SendToAll(NetworkWriter writer) {
                foreach (var client in clients) {
                    Send(writer, client);
                }
            }


            // Client management.

            bool AddClient(IPEndPoint client, ref int clientID)
            {
                var hash = client.GetHashCode();
                if (clientHashToServerHash.ContainsKey(hash))
                {
                    var id = clientHashToServerHash[hash];
                    UnityEngine.Debug.LogWarning("Lobby server: Trying to add already existing client with clientID=" + id);
                    return false;
                }

                clients.Add(client);
                clientID = clients.Count - 1;
                clientHashToServerHash.Add(hash, clientID);

                UnityEngine.Debug.Log("Lobby server: Client added with clientID=" + clientID);
                return true;
            }

            bool RemoveClient(IPEndPoint client)
            {
                var hash = client.GetHashCode();
                if (clientHashToServerHash.ContainsKey(hash))
                {
                    var idx = clientHashToServerHash[hash];
                    if (idx >= 0 && idx < clients.Count)
                    {
                        clients.RemoveAt(idx);
                        clientHashToServerHash.Remove(hash);
                        UnityEngine.Debug.Log("Lobby Server removed clientID=" + idx);
                        return true;
                    }
                }
                UnityEngine.Debug.LogError("Lobby Server failed to remove clientIP=" + client.ToString());
                return false;
            }

            // end Client management.

            // Room management.

            bool CreateRoom(CreateRoomData data, ref int roomID)
            {
                Lobby l = new Lobby();
                l.hostID = data.hostID;
                l.name = data.name;

                l.roomID = lobbies.Count;
                roomID = l.roomID;

                lobbies.Add(l);

                UnityEngine.Debug.Log("Lobby server : Created room with roomID=" + roomID + " and hostID=" + l.hostID);

                return true;
            }

            bool RemoveRoom(RemoveRoomData data)
            {
                var id = data.roomID;
                if (id < 0 || id > lobbies.Count)
                {
                    UnityEngine.Debug.LogError("Lobby server : Failed to remove room with received roomID=" + id);
                    return false;
                }

                lobbies.RemoveAt(id);

                UnityEngine.Debug.Log("Lobby server : Removed room with roomID=" + id);
                return true;
            }

            bool ListRooms(ref SchLobbyClient.RoomListData data)
            {
                data.lobbies = lobbies;
                return true;
            }

            // End Room management.

            // Dispatch received bytes.
            void Recv(IAsyncResult result)
            {
                IPEndPoint clientIP = new IPEndPoint(IPAddress.Any, 0);
                // EndReceive will put in clientIP the end-point from which we received the packet.
                // It is a remote end-point, or a local end-point in case of localhost.
                byte[] data = socket.EndReceive(result, ref clientIP);
                // Immediately begin a new receive for next packets.
                socket.BeginReceive(new AsyncCallback(Recv), socket);

                UnityEngine.Debug.Log("Lobby server Recv from " + clientIP.ToString());

                ProcessData(data, clientIP);
            }

            void ProcessData(byte[] data, IPEndPoint clientIP)
            {
                NetworkReader reader = NetworkReaderPool.Get(data);
                OpCode opCode = (OpCode)reader.ReadByte();
                switch (opCode)
                {
                    case OpCode.NoOp:
                        {
                            UnityEngine.Debug.Log("Lobby server received [NoOp] from " + clientIP.ToString());
                            break;
                        }
                    case OpCode.LobbyClientAuthRequest:
                        {
                            // TODO: implement authentication.
                            UnityEngine.Debug.Log("lobby server received : [LobbyClientAuthRequest] from " + clientIP.ToString());
                            UnityEngine.Debug.Log("Lobby server sent :  [LobbyClientAuthentified] to " + clientIP.ToString());
                            Send(SchLobbyClient.OpCode.Authentified, clientIP);
                            break;
                        }
                    case OpCode.LobbyClientConnect:
                        {
                            UnityEngine.Debug.Log("Lobby server received : [LobbyClientConnect] from " + clientIP.ToString());
                            int clientID = -1;
                            if (AddClient(clientIP, ref clientID))
                            {
                                SchLobbyClient.ConnectedData requestData = new SchLobbyClient.ConnectedData();
                                requestData.clientID = clientID;

                                NetworkWriter writer = NetworkWriterPool.Get();
                                writer.WriteByte((byte)SchLobbyClient.OpCode.Connected);
                                writer.Write(requestData);

                                UnityEngine.Debug.Log("Lobby server sent : [LobbyClientConnected] with clientID=" + clientID + " to " + clientIP.ToString());
                                Send(writer, clientIP);
                            }
                            else
                            {
                                UnityEngine.Debug.LogError("Lobby server: failed to connect client because we already know it.");

                                // send error
                                // NetworkWriter writer = NetworkWriterPool.Get();
                                // writer.WriteByte((byte)SchLobbyClient.OpCode.Error);
                                // writer.WriteByte((byte)SchLobbyClient.ErrorCode.EClientAlreadyExists);
                                // writer.WriteInt(clientHash);

                                // var arr = writer.ToArraySegment();
                                // serverConnection.Send(arr.Array, arr.Count, clientIP);
                            }
                            break;
                        }
                    case OpCode.LobbyClientDisconnect:
                        {
                            UnityEngine.Debug.Log("Lobby server received : [LobbyClientDisconnect] from " + clientIP.ToString());
                            if (RemoveClient(clientIP))
                            {
                                UnityEngine.Debug.Log("Lobby server sent : [LobbyClientDisconnected] to " + clientIP.ToString());
                                Send(SchLobbyClient.OpCode.Disconnected, clientIP);
                            }
                            break;
                        }
                    case OpCode.CreateRoom:
                        {
                            UnityEngine.Debug.Log("Lobby server received : [CreateRoom] from " + clientIP.ToString());
                            CreateRoomData requestData = reader.ReadCreateRoomData();
                            int roomID = -1;
                            if (CreateRoom(requestData, ref roomID))
                            {
                                SchLobbyClient.RoomCreatedData answerData = new SchLobbyClient.RoomCreatedData();
                                answerData.roomID = roomID;

                                NetworkWriter writer = NetworkWriterPool.Get();
                                writer.WriteByte((byte)SchLobbyClient.OpCode.RoomCreated);
                                writer.Write(answerData);

                                UnityEngine.Debug.Log("Lobby server sent : [RoomCreated] to " + clientIP.ToString());
                                Send(writer, clientIP);
                            }
                            break;
                        }
                    case OpCode.RemoveRoom:
                        {
                            UnityEngine.Debug.Log("Lobby server received : [RemoveRoom] from " + clientIP.ToString());
                            RemoveRoomData requestData = reader.ReadRemoveRoomData();
                            if (RemoveRoom(requestData))
                            {
                                UnityEngine.Debug.Log("Lobby server: sent [RoomRemoved] with roomID=" + requestData.roomID);
                                // TODO: send validation message;
                            }
                            break;
                        }
                    case OpCode.ListRooms:
                        {
                            UnityEngine.Debug.Log("Lobby server received : [ListRooms] from " + clientIP.ToString());
                            SchLobbyClient.RoomListData requestData = new SchLobbyClient.RoomListData();
                            if (ListRooms(ref requestData))
                            {
                                NetworkWriter writer = NetworkWriterPool.Get();
                                writer.WriteByte((byte)SchLobbyClient.OpCode.RoomList);
                                writer.Write(requestData);

                                UnityEngine.Debug.Log("Lobby server sent : [RoomList] of " + requestData.lobbies.Count + " rooms to " + clientIP.ToString());

                                Send(writer, clientIP);
                            }
                            break;
                        }
                    case OpCode.JoinRoom:
                        {
                            // ask room host for its port.
                            // Ask only the room, because we have to have started the seclient to know its port.
                            SchLobbyServer.JoinRoomData requestData = reader.ReadJoinRoomData();
                            SchLobbyClient.NATPunchIPRequestedData answerData = new SchLobbyClient.NATPunchIPRequestedData();

                            UnityEngine.Debug.Log("Lobby server received : [JoinRoom] from " + clientIP.ToString());

                            answerData.roomID = requestData.roomID;
                            if (answerData.roomID < 0 || answerData.roomID >= lobbies.Count)
                            {
                                UnityEngine.Debug.LogError("Lobby server : [JoinRoom] Wrong roomID=" + answerData.roomID);
                                return;
                            }

                            var hostID = lobbies[requestData.roomID].hostID;
                            if (hostID < 0 || hostID >= clients.Count)
                            {
                                UnityEngine.Debug.LogError("Lobby server : [JoinRoom] Wrong hostID=" + hostID);
                                return;
                            }

                            IPEndPoint roomHostIP = clients[hostID];
                            // Get client infos to pass to hast.
                            var clientHash = clientIP.GetHashCode();
                            if (!clientHashToServerHash.ContainsKey(clientHash))
                            {
                                UnityEngine.Debug.LogError("Lobby server : [JoinRoom] Client hash not in dic from IP=" + clientIP.ToString());
                                return;
                            }
                            // Host will have to contact clientID.
                            answerData.NATPunchID = clientHashToServerHash[clientHash];
                            if (answerData.NATPunchID < 0 || answerData.NATPunchID >= clients.Count)
                            {
                                UnityEngine.Debug.LogError("Lobby server : [JoinRoom] Wrong hostID=" + answerData.NATPunchID);
                                return;
                            }

                            NetworkWriter writerHost = NetworkWriterPool.Get();
                            writerHost.WriteByte((byte)SchLobbyClient.OpCode.NATPunchIPRequested);
                            writerHost.Write(answerData);

                            UnityEngine.Debug.Log("Lobby server sent : [NATPunchIPRequested] with NATPunchID=" + answerData.NATPunchID + " to " + roomHostIP.ToString());

                            Send(writerHost, roomHostIP);

                            UnityEngine.Debug.LogFormat("Lobby server : Initiated NATPunch between clientID {0} and clientID {1}.", answerData.NATPunchID, answerData.NATPunchID);

                            #if false
                            SchLobbyServer.JoinRoomData requestData = reader.ReadJoinRoomData();
                            UnityEngine.Debug.Log("Lobby server received : [JoinRoom] from " + clientIP.ToString());
                            // ask client to contact room host.
                            // roomID used to differentiate host/client.
                            // NATPunchID used to remember desired client id.
                            SchLobbyClient.NATPunchIPRequestedData answerData = new SchLobbyClient.NATPunchIPRequestedData();

                            answerData.roomID = requestData.roomID;
                            if (answerData.roomID < 0 || answerData.roomID >= lobbies.Count)
                            {
                                UnityEngine.Debug.LogError("Lobby server : [JoinRoom] Wrong roomID=" + answerData.roomID);
                                return;
                            }

                            answerData.NATPunchID = lobbies[requestData.roomID].hostID;
                            if (answerData.NATPunchID < 0 || answerData.NATPunchID >= clients.Count)
                            {
                                UnityEngine.Debug.LogError("Lobby server : [JoinRoom] Wrong hostID=" + answerData.NATPunchID);
                                return;
                            }

                            NetworkWriter writer = NetworkWriterPool.Get();
                            writer.WriteByte((byte)SchLobbyClient.OpCode.NATPunchIPRequested);
                            writer.Write(answerData);

                            UnityEngine.Debug.Log("Lobby server sent : [NATPunchIPRequested] with roomID=" + answerData.roomID + " and NATPunchID=" + answerData.NATPunchID+ " to " + clientIP.ToString());

                            Send(writer, clientIP);

                            // ask host to contact client.
                            IPEndPoint roomHostIP = clients[answerData.NATPunchID];

                            var clientHash = clientIP.GetHashCode();
                            if (!clientHashToServerHash.ContainsKey(clientHash))
                            {
                                UnityEngine.Debug.LogError("Lobby server : [JoinRoom] Client hash not in dic from IP="+clientIP.ToString());
                                return;
                            }

                            SchLobbyClient.NATPunchIPRequestedData answerData2 = new SchLobbyClient.NATPunchIPRequestedData();
                            answerData2.roomID = requestData.roomID;
                            answerData2.NATPunchID = clientHashToServerHash[clientIP.GetHashCode()];

                            if (answerData2.NATPunchID < 0 || answerData2.NATPunchID >= clients.Count)
                            {
                                UnityEngine.Debug.LogError("Lobby server : [JoinRoom] Wrong hostID="+answerData2.NATPunchID);
                                return;
                            }


                            NetworkWriter writerHost = NetworkWriterPool.Get();
                            writerHost.WriteByte((byte)SchLobbyClient.OpCode.NATPunchIPRequested);
                            writerHost.Write(answerData2);

                            UnityEngine.Debug.Log("Lobby server sent : [NATPunchIPRequested] with NATPunchID="+answerData2.NATPunchID +" to " + roomHostIP.ToString());

                            Send(writerHost, roomHostIP);

                            UnityEngine.Debug.LogFormat("Lobby server : Initiated NATPunch between clientID {0} and clientID {1}.", answerData.NATPunchID, answerData2.NATPunchID);
                            #endif
                            break;
                        }
                    case OpCode.NATPunchIP:
                        {
                            UnityEngine.Debug.Log("Lobby server received : [NATPunchIP] from " + clientIP.ToString());
                            NATPunchIP requestData = reader.ReadNATPunchIP();
                            int clientPort = requestData.port;
                            int NATPunchID = requestData.clientID;

                            if (NATPunchID < 0 || NATPunchID >= clients.Count)
                            {
                                UnityEngine.Debug.LogError("Lobby server : [NATPunchIP] Wrong NATPunchID=" + NATPunchID);
                                return;
                            }

                            int clientID = clientHashToServerHash[clientIP.GetHashCode()];
                            IPEndPoint NATPunchIP = clients[NATPunchID];
                            IPEndPoint clientNATPunchIP = new IPEndPoint(clientIP.Address, clientPort);

                            UnityEngine.Debug.LogFormat("Lobby server: [NATPunchIP] Ask clientid {0} with ip {1}:{2} to natpunch clientid {3} with ip {4}:{5}.", NATPunchID, NATPunchIP.Address, NATPunchIP.Port, clientID, clientNATPunchIP.Address, clientNATPunchIP.Port);

                            SchLobbyClient.StartNATPunchData answerData = new SchLobbyClient.StartNATPunchData();
                            answerData.ip = clientNATPunchIP;

                            NetworkWriter writer = NetworkWriterPool.Get();
                            writer.WriteByte((byte)SchLobbyClient.OpCode.StartNATPunch);
                            writer.Write(answerData);

                            UnityEngine.Debug.Log("Lobby server sent : [StartNATPunch] to " + NATPunchIP.ToString());

                            Send(writer, NATPunchIP);
                            break;
                        }
                }
            }


        };
}
