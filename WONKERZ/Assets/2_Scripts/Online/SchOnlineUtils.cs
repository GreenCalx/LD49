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
}
