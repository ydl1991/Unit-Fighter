using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

public class NetworkServer
{
    public static int port { get; private set; }
    public static List<NetworkClient> s_clients;    // Index is the client id
    private static TcpListener s_tcpListener;
    private static UdpClient s_udpListener;

    /// Callback functions get called when a network message server received from client. 
    /// Using ToServerPackets enum as index
    public delegate void PacketHandler(int fromClient, NetworkPacket packet);
    public static ReadOnlyCollection<PacketHandler> s_kPacketHandlers = new ReadOnlyCollection<PacketHandler>(new PacketHandler[]
    {
        NetworkServerHandle.WelcomeReceived,
        NetworkServerHandle.ChangeSlotRequestReceived,
        NetworkServerHandle.ReadyButtonClickReceived,
        NetworkServerHandle.ClientGameFinishedLoading,
        NetworkServerHandle.ClientCardsRefreshRequestReceived,
        NetworkServerHandle.ClientBuyExpRequestReceived,
        NetworkServerHandle.ClientBuyCardRequestReceived,
        NetworkServerHandle.ClientSellUnitActionReceived,
        NetworkServerHandle.ClientMoveUnitActionReceived,
        NetworkServerHandle.UDPTestReceived
    });

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Create and init server, gets it ready to receive client connection.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void Init(int port)
    {
        NetworkServer.port = port;

        Debug.Log("Server initializing...");
        InitServerData();

        s_tcpListener = new TcpListener(IPAddress.Any, NetworkServer.port);
        s_tcpListener.Start();
        s_tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        s_udpListener = new UdpClient(NetworkServer.port);
        s_udpListener.BeginReceive(UDPReceiveCallback, null);
        
        Debug.Log($"Server started on {NetworkServer.port}.");
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Callback function added to BeginAcceptTcpClient, and gets called when a tcp client 
    ///     is connected.
    ///
    /// -----------------------------------------------------------------------------------------
    private static void TCPConnectCallback(IAsyncResult result)
    {
        TcpClient client = s_tcpListener.EndAcceptTcpClient(result);

        // We want to keep receiving connection from other clients
        s_tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        Debug.Log($"Incoming connection from {client.Client.RemoteEndPoint}...");

        if (SceneTransitionServer.s_instance.CurrentSceneIndex() != 0)
        {
            Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: Game started!");
            return;
        }

        for (int i = 0; i < NetworkConstants.k_maxPlayer; ++i)
        {
            if (s_clients[i].tcp.socket == null)
            {
                s_clients[i].tcp.Connect(client);
                return;
            }
        }

        Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Callback function added to BeginReceive, and gets called when a udp client 
    ///     is connected.
    ///
    /// -----------------------------------------------------------------------------------------
    private static void UDPReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = s_udpListener.EndReceive(result, ref clientEndPoint);

            s_udpListener.BeginReceive(UDPReceiveCallback, null);

            if (data.Length < 4)
            {
                return;
            }

            using (NetworkPacket packet = new NetworkPacket(data))
            {
                int clientId = packet.ReadInt();
                if (clientId < 0 || clientId >= NetworkConstants.k_maxPlayer)
                    return;

                if (s_clients[clientId].udp.m_endPoint == null)
                {
                    s_clients[clientId].udp.Connect(clientEndPoint);
                    return;
                }

                if (s_clients[clientId].udp.m_endPoint.ToString() == clientEndPoint.ToString())
                {
                    s_clients[clientId].udp.HandleData(packet);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error receiving UDP data: {ex}");
        }
    }

    public static void SendUdpData(IPEndPoint clientEndPoint, NetworkPacket packet)
    {
        try
        {
            if (clientEndPoint != null)
            {
                s_udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error sending data to {clientEndPoint} via UDP: {ex}");
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Callback functions get called when a network message being received.
    ///
    /// -----------------------------------------------------------------------------------------
    private static void InitServerData()
    {
        s_clients = new List<NetworkClient>(NetworkConstants.k_maxPlayer);
        for (int i = 0; i < NetworkConstants.k_maxPlayer; ++i)
        {
            s_clients.Add(new NetworkClient(i));
        }

        Debug.Log("Initialize packets.");
    }

    public static void Stop()
    {
        s_tcpListener.Stop();
        s_udpListener.Close();
    }
}
