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

public static class WriterReaderExtensions
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

public static class Utils
{
    public static IPAddress GetAddress(string ip)
    {
        IPAddress[] addresses = Dns.GetHostAddresses(ip);
        foreach (var add in addresses)
        {
            if (add.AddressFamily == AddressFamily.InterNetwork)
            {
                return add;
            }
        }
        return IPAddress.Loopback;
    }

    public static IPAddress GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    public class NATPunchThread
    {
        public UdpClient client;
        public IPEndPoint server;

        public void NATPunch()
        {
            var clientIP = (client.Client.LocalEndPoint as IPEndPoint);
            for (int i = 0; i < 10; i++)
            {
                UnityEngine.Debug.LogFormat("nat punch from {0}:{1} to {2}:{3}", clientIP.Address, clientIP.Port, server.Address, server.Port);
                NetworkWriter writer = NetworkWriterPool.Get();
                writer.WriteByte((byte)SchLobbyClient.OpCode.NATPunch);

                client.Send(writer.ToArraySegment().Array, writer.ToArraySegment().Count, server);

                Thread.Sleep(25);
            }
        }

    }

    public static void StartNATPunch(UdpClient client, IPEndPoint server)
    {
        NATPunchThread nt = new NATPunchThread();
        nt.client = client;
        nt.server = server;

        Thread t = new Thread(nt.NATPunch);
        t.Start();
    }
}

#if false
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
#endif

#if false
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

public class SchLobbyServer
{
    public readonly static int defaultPort = 7777;

    public bool isLocal = false;

    Dictionary<int, int> clientHashToServerHash = new Dictionary<int, int>();
    List<IPEndPoint> clients = new List<IPEndPoint>();

    Dictionary<int, int> clientHashToLobbyHash = new Dictionary<int, int>();
    List<Lobby> lobbies = new List<Lobby>();

    UdpClient socket;

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

    void Send(SchLobbyClient.OpCode code, IPEndPoint client)
    {
        socket.Send(new byte[] { (byte)code }, 1, client);
    }

    void Send(NetworkWriter writer, IPEndPoint client)
    {
        var arr = writer.ToArraySegment();
        socket.Send(arr.Array, arr.Count, client);
    }

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

                        UnityEngine.Debug.Log("Lobby server sent : [LobbyClientConnected] with clientID="+clientID+" to " + clientIP.ToString());
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
                    SchLobbyServer.JoinRoomData            requestData = reader.ReadJoinRoomData();
                    SchLobbyClient.NATPunchIPRequestedData answerData  = new SchLobbyClient.NATPunchIPRequestedData();

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
                        UnityEngine.Debug.LogError("Lobby server : [JoinRoom] Client hash not in dic from IP="+clientIP.ToString());
                        return;
                    }
                    // Host will have to contact clientID.
                    answerData.NATPunchID = clientHashToServerHash[clientHash];
                    if (answerData.NATPunchID < 0 || answerData.NATPunchID >= clients.Count)
                    {
                        UnityEngine.Debug.LogError("Lobby server : [JoinRoom] Wrong hostID="+answerData.NATPunchID);
                        return;
                    }

                    NetworkWriter writerHost = NetworkWriterPool.Get();
                    writerHost.WriteByte((byte)SchLobbyClient.OpCode.NATPunchIPRequested);
                    writerHost.Write(answerData);

                    UnityEngine.Debug.Log("Lobby server sent : [NATPunchIPRequested] with NATPunchID="+answerData.NATPunchID +" to " + roomHostIP.ToString());

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

    IPEndPoint ep;

    public IPEndPoint GetIP()
    {
        var localEP = (socket.Client.LocalEndPoint as IPEndPoint);
        if (localEP.Address.Equals(IPAddress.Any))
        {
            localEP.Address = Utils.GetLocalIPAddress();
        }
        return localEP;
    }

    public SchLobbyServer()
    {
        ep = new IPEndPoint(IPAddress.Any, 0);
        SetupSocket();
    }

    public SchLobbyServer(int port)
    {
        ep = new IPEndPoint(IPAddress.Any, port);
        SetupSocket();
    }

    public SchLobbyServer(string address)
    {
        ep = new IPEndPoint(Utils.GetAddress(address), 0);
        SetupSocket();
    }

    public SchLobbyServer(string address, int port)
    {
        ep = new IPEndPoint(Utils.GetAddress(address), port);
        SetupSocket();
    }

    public void SetupSocket()
    {
        UnityEngine.Debug.LogFormat("lobby server: setup server with ip {0}:{1}.", ep.Address, ep.Port);

        socket = new UdpClient(ep);

        #if PLATFORM_STANDALONE_WIN
        const int SIO_UDP_CONNRESET = -1744830452;
        byte[] inValue = new byte[] {0};
        byte[] outValue = new byte[] {0};
        socket.Client.IOControl(SIO_UDP_CONNRESET, inValue, outValue);
        #endif

        socket.BeginReceive(new AsyncCallback(Recv), socket);
    }

    public void Close()
    {
        socket.Close();
    }

    void Recv(IAsyncResult result)
    {
        IPEndPoint clientIP = new IPEndPoint(IPAddress.Any, 0);
        byte[] data = socket.EndReceive(result, ref clientIP);
        socket.BeginReceive(new AsyncCallback(Recv), socket);

        UnityEngine.Debug.Log("Lobby server Recv from " + clientIP.ToString());

        ProcessData(data, clientIP);
    }

    #if false
        void ProcessData(byte[] data, IPEndPoint clientIP) {
            NetworkReader reader = NetworkReaderPool.Get(data);
            OpCode opCode = (OpCode)reader.ReadByte();
            switch (opCode) {
                case OpCode.NoOp: {
                    UnityEngine.Debug.LogFormat("server: received NoOp from {0}", clientIP.Address);
                    break;
                }
                case OpCode.JoinLobby: {
                    var lobbyID = reader.ReadInt();
                    if (lobbyID >= lobbies.Count || lobbyID < 0) {

                    } else
                    {
                        var lobby = lobbies[lobbyID];

                        NetworkWriter writer = NetworkWriterPool.Get();
                        writer.WriteByte((byte)SchLobbyClient.OpCode.Success);
                        writer.WriteByte((byte)SchLobbyClient.SuccessCode.SJoinLobby);
                        writer.WriteIPEndPoint(lobby.hostIP);

                        Send(writer, clientIP);

                        NetworkWriter writerHost = NetworkWriterPool.Get();
                        writerHost.WriteByte((byte)SchLobbyClient.OpCode.NATPunch);
                        writerHost.WriteIPEndPoint(clientIP);

                        Send(writerHost, lobby.hostIP);
                    }
                    break;
                }
                case OpCode.ClientDisconnect: {
                    var clientHash = clientIP.GetHashCode();
                    if (!clientHashToServerHash.ContainsKey(clientHash))
                    {
                        NetworkWriter writer = NetworkWriterPool.Get();
                        writer.WriteByte((byte)SchLobbyClient.OpCode.Error);
                        writer.WriteByte((byte)SchLobbyClient.ErrorCode.EClientNotFound);
                        writer.WriteInt(clientHash);

                        Send(writer, clientIP);
                    } else {
                        clients.RemoveAt(clientHashToServerHash[clientHash]);
                        clientHashToServerHash.Remove(clientHash);

                        NetworkWriter writer = NetworkWriterPool.Get();
                        writer.WriteByte((byte)SchLobbyClient.OpCode.Success);

                        Send(writer, clientIP);
                    }
                    break;
                }
                case OpCode.ClientConnect: {
                    var clientHash = clientIP.GetHashCode();
                    if (clientHashToServerHash.ContainsKey(clientHash)) {
                        UnityEngine.Debug.LogError("server: failed to connect client because we already know it.");

                        NetworkWriter writer = NetworkWriterPool.Get();
                        writer.WriteByte((byte)SchLobbyClient.OpCode.Error);
                        writer.WriteByte((byte)SchLobbyClient.ErrorCode.EClientAlreadyExists);
                        writer.WriteInt(clientHash);

                        var arr = writer.ToArraySegment();
                        serverConnection.Send(arr.Array, arr.Count, clientIP);
                    } else {
                        UnityEngine.Debug.LogFormat("server: add client {0}", clientHash);

                        var writer = NetworkWriterPool.Get();
                        writer.WriteByte((byte)SchLobbyClient.OpCode.Success);
                        writer.WriteByte((byte)SchLobbyClient.SuccessCode.SConnected);

                        var arr = writer.ToArraySegment();
                        serverConnection.Send(arr.Array, arr.Count, clientIP);
                    }
                    break;
                }
                case OpCode.GetLobbyList: {
                    UnityEngine.Debug.Log("server: get lobby list.");

                    var writer = NetworkWriterPool.Get();
                    writer.WriteByte((byte)SchLobbyClient.OpCode.LobbyList);

                    writer.WriteUInt((uint)lobbies.Count);
                    foreach (var l in lobbies) {
                        writer.WriteString(l.name);
                        writer.WriteIPEndPoint(l.hostIP);
                    }

                    var arr = writer.ToArraySegment();
                    serverConnection.Send(arr.Array, arr.Count, clientIP);
                    break;
                }
                case OpCode.RemoveLobby: {
                    UnityEngine.Debug.Log("server: remove lobby.");
                    var clientHash = clientIP.GetHashCode();
                    if (!clientHashToLobbyHash.ContainsKey(clientHash)) {
                        UnityEngine.Debug.LogError("server: failed to remove lobby as we can't find one.");

                        NetworkWriter writer = NetworkWriterPool.Get();
                        writer.WriteByte((byte)SchLobbyClient.OpCode.Error);
                        writer.WriteByte((byte)SchLobbyClient.ErrorCode.ELobbyDontExists);
                        writer.WriteInt(clientHash);

                        var arr = writer.ToArraySegment();
                        serverConnection.Send(arr.Array, arr.Count, clientIP);

                        return;
                    }

                    lobbies.RemoveAt(clientHashToLobbyHash[clientHash]);
                    break;
                }
                case OpCode.CreateLobby: {
                    UnityEngine.Debug.Log("server: create lobby.");
                    var clientHash = clientIP.GetHashCode();
                    if (clientHashToLobbyHash.ContainsKey(clientHash)) {

                        UnityEngine.Debug.LogWarning("server: create lobby failed.");
                        NetworkWriter writer = NetworkWriterPool.Get();
                        writer.WriteByte((byte)SchLobbyClient.OpCode.Error);
                        writer.WriteByte((byte)SchLobbyClient.ErrorCode.ELobbyExists);
                        writer.WriteInt(clientHash);

                        var arr = writer.ToArraySegment();
                        serverConnection.Send(arr.Array, arr.Count, clientIP);
                    } else {
                        string lobbyName = reader.ReadString();
                        int lobbyPort = reader.ReadInt();

                        Lobby l = new Lobby();

                        l.name = lobbyName;
                        l.hostIP = clientIP;
                        l.hostIP.Port = lobbyPort;

                        // TODO: carefull threadings access
                        lobbies.Add(l);

                        var lobbyID = lobbies.Count - 1;
                        clientHashToLobbyHash.Add(clientHash, lobbyID);

                        UnityEngine.Debug.LogFormat("server: create lobby {0} with name {1} and host {2}:{3} succeeded.", lobbyID, l.name, l.hostIP.Address, l.hostIP.Port);

                        NetworkWriter writer = NetworkWriterPool.Get();
                        writer.WriteByte((byte)SchLobbyClient.OpCode.Success);
                        writer.WriteByte((byte)SchLobbyClient.SuccessCode.SLobbyCreated);
                        writer.WriteInt(lobbyID);

                        var arr = writer.ToArraySegment();
                        serverConnection.Send(arr.Array, arr.Count, clientIP);
                    }
                    break;
                }
            }
        }
#endif
};

public class SchLobbyClient
{
    public static readonly int defaultPort = 8888;
    public static readonly string defaultHost = "toffaraspberry.ddns.net";

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

    bool isAuthentified = false;
    int id = -1;

    int roomID = -1;

    //SchRoomServer roomServer = null;
    //SchRoomClient roomClient = null;

    public NetworkRoomManager roomManager;

    public Action<RoomListData> OnLobbyListRefreshed;
    public Action<RoomJoinedData> OnRoomJoined;
    public Action<RoomCreatedData> OnRoomCreated;
    public Action<int> OnConnected;
    public Action<IPEndPoint> OnStartNATPunch;

    // void CreateRoomServer()
    // {
    //     if (roomServer == null)
    //     {
    //         roomServer = new SchRoomServer();
    //     }
    // }

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
                    Send(writer, clientIP);
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

    UdpClient clientConnection = null;

    public IPEndPoint localEndPoint = null;
    public IPEndPoint remoteEndPoint = null;

    int port = -1;

    public int GetPort() => (clientConnection.Client.LocalEndPoint as IPEndPoint).Port;
    public string GetAddress() => (clientConnection.Client.LocalEndPoint as IPEndPoint).Address.ToString();

    public SchLobbyClient()
    {
        port = 0;
        SetupClient();
    }

    public SchLobbyClient(string address, int port) {
        this.port = port;
    }

    public SchLobbyClient(int port)
    {
        this.port = port;
        SetupClient();
    }

    void SetupClient(string address) {
        localEndPoint = new IPEndPoint(IPAddress.Loopback, port);
        clientConnection = new UdpClient(localEndPoint);

        #if PLATFORM_STANDALONE_WIN
        const int SIO_UDP_CONNRESET = -1744830452;
        byte[] inValue = new byte[] {0};
        byte[] outValue = new byte[] {0};
        clientConnection.Client.IOControl(SIO_UDP_CONNRESET, inValue, outValue);
        #endif


        clientConnection.BeginReceive(new AsyncCallback(Recv), clientConnection);
    }

    void SetupClient()
    {
        localEndPoint = new IPEndPoint(IPAddress.Any, port);
        clientConnection = new UdpClient(localEndPoint);

        #if PLATFORM_STANDALONE_WIN
        const int SIO_UDP_CONNRESET = -1744830452;
        byte[] inValue = new byte[] {0};
        byte[] outValue = new byte[] {0};
        clientConnection.Client.IOControl(SIO_UDP_CONNRESET, inValue, outValue);
        #endif

        clientConnection.BeginReceive(new AsyncCallback(Recv), clientConnection);
    }

    public void Send(SchLobbyServer.OpCode opCode)
    {
        clientConnection.Send(new byte[] { (byte)opCode }, 1, remoteEndPoint);
    }

    public void Send(NetworkWriter writer, IPEndPoint client) {
        var arr = writer.ToArraySegment();
        clientConnection.Send(arr.Array, arr.Count, client);
    }

    public void Send(NetworkWriter writer)
    {
        Send(writer.ToArraySegment());
    }

    public void Send(ArraySegment<byte> data)
    {
        clientConnection.Send(data.Array, data.Count, remoteEndPoint);
    }

    public bool IsSetup()
    {
        return port != -1 && clientConnection != null;
    }

    public bool IsConnected()
    {
        return remoteEndPoint != null;
    }

    public void Connect(IPEndPoint server)
    {
        remoteEndPoint = server;
        //clientConnection.Connect(remoteEndPoint);
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


        // if (roomClient != null)
        // {
        //     roomClient.Close();
        // }

        // if (roomServer != null)
        // {
        //     roomServer.Close();
        // }

        Disconnect();
        clientConnection.Close();
    }

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

#if false
        void ProcessData(byte[] data, IPEndPoint serverIP) {
            NetworkReader reader = NetworkReaderPool.Get(data);
            OpCode opCode = (OpCode)reader.ReadByte();
            switch (opCode) {
                case OpCode.Error: {
                    UnityEngine.Debug.Log("Error");
                    break;
                }
                case OpCode.Success: {
                    SuccessCode successCode = (SuccessCode)reader.ReadByte();
                    switch (successCode) {
                        case SuccessCode.SLobbyCreated: {
                            var lobbyID = reader.ReadInt();
                            UnityEngine.Debug.LogFormat("client: lobby created success with ID {0}.", lobbyID);
                            break;
                        }
                        case SuccessCode.SConnected: {
                            UnityEngine.Debug.Log("client: successfully connected to server.");
                            break;
                        }
                        case SuccessCode.SJoinLobby: {
                            var roomIp = reader.ReadIPEndPoint();

                            UnityEngine.Debug.Log("client: start join lobby.");
                            UnityEngine.Debug.Log("client: start NATPunching.");

                            Utils.StartNATPunch(clientConnection, roomIp);

                            break;
                        }
                    }
                    break;
                }
                case OpCode.LobbyList: {
                    var count = reader.ReadUInt();
                    List<Lobby> lobbies = new List<Lobby>((int)count);
                    for (int i = 0; i < count; ++i) {
                        Lobby l = new Lobby();
                        l.name = reader.ReadString();
                        l.hostIP = reader.ReadIPEndPoint();
                        lobbies.Add(l);

                        UnityEngine.Debug.LogFormat("Lobby {0} with name {1} and host {2}:{3}", i, l.name, l.hostIP.Address, l.hostIP.Port);
                    }
                    break;
                }
                case OpCode.StartNATPunch: {
                    UnityEngine.Debug.Log("client: start nat punch.");
                    var clientIP = reader.ReadIPEndPoint();
                    Utils.StartNATPunch(clientConnection, clientIP);
                    break;
                }
                case OpCode.NATPunch: {
                    UnityEngine.Debug.Log("client: received nat punch, communication is working.");
                    break;
                }
            }
        }
#endif
};

public class SchServerLobby : MonoBehaviour
{

    public NetworkRoomManager roomManager;
    public bool isLocal = false;
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
        localServer = new SchLobbyServer(serverAddress.ToString(), SchLobbyServer.defaultPort);
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
        Application.logMessageReceivedThreaded += OnLogReceived;
        UnityEngine.Debug.developerConsoleVisible = true;

        Init();
        #else
        localServer = new SchLobbyServer(SchLobbyServer.defaultPort);
        #endif
    }

    [Conditional("SCH_NOT_SERVER")]
    void Update()
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
            serverAddress = Utils.GetAddress(SchLobbyClient.defaultHost);
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
}



#if false
public class SchServerLobby : MonoBehaviour
{
    public bool localHost = false;


    SchLobbyServer lobbyServer;
    SchLobbyClient lobbyClient;

    public int serverPort = 7777;


    struct ReceivedLogs
    {
        public string log;
        public float timeDisplayed;
    };

    List<ReceivedLogs> receivedLogs = new List<ReceivedLogs>();

    void DisplayNextString(int i, string log)
    {
        GUI.Label(new Rect(25, 25 * i, 1000, 100), log);
    }

    void OnGUI()
    {
        int i = 0;
        foreach (var l in receivedLogs)
        {
            DisplayNextString(i, l.log);
            ++i;
        }
    }

    void OnLogReceived(string logString, string stackTrace, LogType type)
    {
        receivedLogs.Add(new ReceivedLogs { log = logString, timeDisplayed = 0 });
    }

    void Start()
    {
#if SERVER
        lobbyServer = new SchLobbyServer(SchLobbyServer.defaultPort);
#else
        Application.logMessageReceivedThreaded += OnLogReceived;
        Debug.developerConsoleVisible = true;


        UnityEngine.Debug.Log("Create lobby client.");
        lobbyClient = new SchLobbyClient();
#endif
    }

    void Update()
    {

#if SERVER
#else
        for (int i =0; i < receivedLogs.Count; ++i) {
            var l = receivedLogs[i];
            l.timeDisplayed += Time.deltaTime;
            receivedLogs[i] = l;
        }

        for (int i =0; i < receivedLogs.Count; ++i) {
            var l = receivedLogs[i];
            if (l.timeDisplayed > 3) {
                receivedLogs.RemoveAt(i);
            }
            ++i;
        }
        // Command: start lobby server.
        if (Input.GetKeyDown(KeyCode.P)) {
            UnityEngine.Debug.Log("[Input] Create lobby server.");
            lobbyServer = new SchLobbyServer();
        }
        // Command: start lobby client.
        if (Input.GetKeyDown(KeyCode.Y)) {
            UnityEngine.Debug.Log("[Input] Connect to server.");
            if (lobbyServer != null) {
                UnityEngine.Debug.LogFormat("Connection to server : {0}:{1}", lobbyServer.GetIP().Address, lobbyServer.GetIP().Port);
                lobbyClient.Connect(lobbyServer.GetIP());
            } else {
                var serverAddress = Utils.GetAddress("toffaraspberry.ddns.net");
                UnityEngine.Debug.LogFormat("Connection to server : {0}:{1}", serverAddress, SchLobbyServer.defaultPort);
                lobbyClient.Connect(new IPEndPoint(serverAddress, SchLobbyServer.defaultPort));
            }
        }
        // Command: create lobby.
        if (Input.GetKeyDown(KeyCode.A)) {
            UnityEngine.Debug.Log("[Input] Create lobby testName.");
            lobbyClient.CreateLobby("testName");
        }
        // Command: get lobby list.
        if (Input.GetKeyDown(KeyCode.O)) {
            UnityEngine.Debug.Log("[Input] Get lobby list and show.");
            lobbyClient.GetLobbies();
        }
        // Command: join first lobby.
        if (Input.GetKeyDown(KeyCode.E)) {
            UnityEngine.Debug.Log("[Input] Joint lobby 0.");
            lobbyClient.JoinLobby(0);
        }
#endif
    }

    void OnDestroy()
    {
#if SERVER
        lobbyServer.Close();
#else
        if (lobbyServer != null) lobbyServer.Close();
        if (lobbyClient != null) lobbyClient.Close();
#endif
    }
}

#endif
