using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

public class GameClient : MonoBehaviour
{
    public static GameClient s_instance;
    public static int s_kDataBufferSize = 4096;

    /// Callback functions get called when a network message client received from server or other clients. 
    /// Using ToClientPackets enum as index
    private delegate void PacketHandler(NetworkPacket packet);
    private static ReadOnlyCollection<PacketHandler> s_kPacketHandlers = new ReadOnlyCollection<PacketHandler>(new PacketHandler[]
    {
        GameClientHandle.WelcomeFromServer,
        GameClientHandle.ClientDisconnected,
        GameClientHandle.TcpPunchThrough,
        GameClientHandle.AssignClientSlot,
        GameClientHandle.ClientSetReady,
        GameClientHandle.LoadGame,
        GameClientHandle.StartGame,
        GameClientHandle.SyncTimer,
        GameClientHandle.AssignBattleGroup,
        GameClientHandle.SyncCards,
        GameClientHandle.PlayerStatusSync,
        GameClientHandle.GameStateChange,
        GameClientHandle.CardPurchaseAction,
        GameClientHandle.SellUnitAction,
        GameClientHandle.MoveUnitAction,
        GameClientHandle.AssignBattleWinner,
        GameClientHandle.TakeBattleDamage,
        GameClientHandle.ClientLose,
        GameClientHandle.UDPTest,
        GameClientHandle.ClientReceiveChatMessage,
    });

    public TCP serverTcp { get; set; }
    public UDP serverUdp { get; set; }
    public int clientId { get; set; }
    public int clientSpot { get; set; }
    public int m_clientListenerPort { get; private set; }
    public bool serverConnected { get; private set; }

    public string m_serverIp;
    public int m_serverPort;
    

    public List<TCP> m_clientTcps;
    //public static UDP[] s_clientUdps;
    private TcpListener m_tcpListener;
    //private static UdpClient s_udpListener;
    public HashSet<int> m_portSet;

    void Awake()
    {
        s_instance = this;
        serverConnected = false;
        m_portSet = new HashSet<int>();
        m_clientTcps = new List<TCP>();
        //s_clientUdps = new UDP[NetworkConstants.k_maxPlayer] { null, null, null, null, null, null, null, null };

        DontDestroyOnLoad(gameObject);
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }
    
    public void ConnectToServer()
    {
        serverTcp = new TCP();
        serverUdp = new UDP();

        InitClientData();
        serverTcp.Connect(m_serverIp, m_serverPort);
    }

    public void ConnectToClient(int clientId, string clientIp, int clientPort)
    {
        if (m_portSet.Contains(clientPort))
        {
            Debug.Log($"Listener port {clientPort} is already connected!");
            return;
        }

        m_portSet.Add(clientPort);
        
        m_clientTcps.Add(new TCP());
        //s_clientUdps[clientId] = new UDP();

        m_clientTcps[m_clientTcps.Count - 1].Connect(clientIp, clientPort);
    }

    public void ServerConnected(int clientListenerPort)
    {
        serverConnected = true;
        m_clientListenerPort = clientListenerPort;
        Debug.Log($"local port {m_clientListenerPort}");
        m_tcpListener = new TcpListener(IPAddress.Any, m_clientListenerPort);
        m_tcpListener?.Start();
        m_tcpListener?.BeginAcceptTcpClient(TCPConnectCallback, this);
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
        GameClient gameClient = (GameClient)result.AsyncState;

        TcpClient client = gameClient.m_tcpListener.EndAcceptTcpClient(result);

        // We want to keep receiving connection from other clients
        gameClient.m_tcpListener.BeginAcceptTcpClient(TCPConnectCallback, gameClient);
        
        IPEndPoint ep = (IPEndPoint)client.Client.RemoteEndPoint;
        Debug.Log($"Incoming connection from {ep.ToString()} ...");

        if (gameClient.m_portSet.Contains(ep.Port))
        {
            Debug.Log($"End point {ep} is already in record!");
            return;
        }

        gameClient.m_portSet.Add(ep.Port);
        gameClient.m_clientTcps.Add(new TCP(client));
    }

    public void Disconnect()
    {
        if (serverConnected)
        {
            serverConnected = false;
            serverTcp.socket?.Close();
            serverUdp.socket?.Close();

            Debug.Log("Disconnected from server.");
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Callback function added to Handler list, and gets called when a network message 
    //      being received.
    ///
    /// -----------------------------------------------------------------------------------------
    private void InitClientData()
    {
        Debug.Log("Initialized packets.");
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     TCP class is a socket wrapper class for game client to communicate ordered data
    ///     with network server.
    /// 
    /// -----------------------------------------------------------------------------------------
    public class TCP
    {
        public TcpClient socket { get; private set; }

        private NetworkStream m_stream;
        private NetworkPacket m_receivedData;
        private byte[] m_receiveBuffer;

        public TCP()
        {
            socket = null;
            m_stream = null;
            m_receivedData = null;
        }

        public TCP(TcpClient client)
        {
            socket = client;

            socket.ReceiveBufferSize = s_kDataBufferSize;
            socket.SendBufferSize = s_kDataBufferSize;

            m_stream = socket.GetStream();
            m_receivedData = new NetworkPacket();
            m_receiveBuffer = new byte[s_kDataBufferSize];
            m_stream.BeginRead(m_receiveBuffer, 0, s_kDataBufferSize, ReceiveCallback, null);
        }

        public void Connect(string ip, int port)
        {
            socket = new TcpClient { ReceiveBufferSize = s_kDataBufferSize, SendBufferSize = s_kDataBufferSize };
            m_receiveBuffer = new byte[s_kDataBufferSize];
            socket.BeginConnect(ip, port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);

            if (!socket.Connected)
            {
                return;
            }

            m_stream = socket.GetStream();
            m_receivedData = new NetworkPacket();
            m_stream.BeginRead(m_receiveBuffer, 0, s_kDataBufferSize, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = m_stream.EndRead(result);
                if (byteLength <= 0)
                {
                    s_instance.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(m_receiveBuffer, data, byteLength);

                m_receivedData.Reset(HandleData(data));
                // We want to continue receiving data
                m_stream.BeginRead(m_receiveBuffer, 0, s_kDataBufferSize, ReceiveCallback, null);
            }
            catch(Exception ex)
            {
                Debug.Log($"Error receiving TCP data: {ex}");
                Disconnect();
            }        
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;
            m_receivedData.SetBytes(data);
            if (m_receivedData.UnreadLength() >= 4)
            {
                packetLength = m_receivedData.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }

            while (packetLength > 0 && packetLength <= m_receivedData.UnreadLength())
            {
                byte[] packetBytes = m_receivedData.ReadBytes(packetLength);
                NetworkThreadManager.ExecuteOnMainThread(() => 
                {
                    using (NetworkPacket packet = new NetworkPacket(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        s_kPacketHandlers[packetId](packet);
                    }
                });

                packetLength = 0;
                if (m_receivedData.UnreadLength() >= 4)
                {
                    packetLength = m_receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        public void SendData(NetworkPacket packet)
        {
            try
            {
                if (socket != null)
                {
                    m_stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error sending data to server via TCP: {ex}");
            }
        }

        public void Disconnect()
        {
            s_instance.Disconnect();

            m_stream = null;
            m_receivedData = null;
            m_receiveBuffer = null;
            socket = null;
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     UDP class is a socket wrapper class for game client to communicate data that's less
    ///     care about order and more care about speed with network server.
    /// 
    /// -----------------------------------------------------------------------------------------   
    public class UDP
    {
        public UdpClient socket { get; private set; }

        public IPEndPoint m_endPoint;

        public void Connect(int localPort, string endPointIp, int endPointPort)
        {
            m_endPoint = new IPEndPoint(IPAddress.Parse(endPointIp), endPointPort);
            socket = new UdpClient(localPort);
            socket.Connect(m_endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (NetworkPacket packet = new NetworkPacket())
            {
                SendData(packet);
            }
        }

        public void SendData(NetworkPacket packet)
        {
            try
            {
                packet.InsertInt(s_instance.clientId);
                if (socket != null)
                {
                    socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Error sending data to server via UDP: {ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = socket.EndReceive(result, ref m_endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4)
                {
                    s_instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch (Exception ex)
            {
                Debug.Log($"Error receiving UDP data: {ex}");
                Disconnect();
            }

        }

        private void HandleData(byte[] data)
        {
            using (NetworkPacket packet = new NetworkPacket(data))
            {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength);
            }

            NetworkThreadManager.ExecuteOnMainThread(() => 
            {
                using (NetworkPacket packet = new NetworkPacket(data))
                {
                    int packetId = packet.ReadInt();
                    s_kPacketHandlers[packetId](packet);
                }
            });
        }

        public void Disconnect()
        {
            s_instance.Disconnect();

            m_endPoint = null;
            socket = null;
        }
    }
}
