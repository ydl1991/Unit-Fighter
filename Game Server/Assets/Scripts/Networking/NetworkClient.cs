using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkClient
{
    public static int s_kDataBufferSize = 4096;
    public static int s_listenerPort = 26951;

    public int id { get; private set; }
    public TCP tcp { get; private set; }
    public UDP udp { get; private set; }
    public Player player { get; private set; }
    public int playerSlot { get; private set; }
    public string username { get; private set; }
    public int listenerPort { get; private set; }

    public NetworkClient(int clientId)
    {
        id = clientId;
        tcp = new TCP(id);
        udp = new UDP(id);
        listenerPort = s_listenerPort++;
    }

    private void Disconnect()
    {
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} has disconnected.");

        ReleasePossessedPlayer();
        username = null;
        ServerNetworkManager.s_instance.ReleaseClientSlot(id);

        tcp.Disconnect();
        udp.Disconnect();

        NetworkServerSend.ClientDisconnected(id, playerSlot);
    }

    public void PossessPlayer(Player player)
    {
        this.player = player;
        player.Init(id, username);
    }

    public void ReleasePossessedPlayer()
    {
        if (player != null)
        {
            player.SetPlayerName(player.defaultName);
            player = null;
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Function get called when a client first connect to the server, and we send
    ///     the client to be aware of by all other clients.
    ///
    /// -----------------------------------------------------------------------------------------
    public void SendIntoGame(string playerName)
    {
        username = playerName;
        playerSlot = ServerNetworkManager.s_instance.NextAvailableClientSlot();

        ServerNetworkManager.s_instance.TakeSlot(playerSlot, id, username, out int oldSlot);

        foreach (NetworkClient client in NetworkServer.s_clients)
        {
            if (client.username != null && client.id != id)
            {
                NetworkServerSend.AssignClientSlot(id, client.id, client.username, client.playerSlot, -1);
                IPEndPoint ep = (IPEndPoint)client.tcp.socket.Client.RemoteEndPoint;
                NetworkServerSend.TcpPunchThrough(id, client.id, ep.Address.ToString(), client.listenerPort);
            }
        }

        foreach (NetworkClient client in NetworkServer.s_clients)
        {
            if (client.username != null)
            {
                NetworkServerSend.AssignClientSlot(client.id, id, username, playerSlot, oldSlot);
            }
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Function gets called in game menu when a client trying to change it's spot in game.
    ///
    /// -----------------------------------------------------------------------------------------
    public void TryChangeSlot(int toSlot)
    {
        if (ServerNetworkManager.s_instance.TakeSlot(toSlot, id, username, out int oldSlot))
        {
            playerSlot = toSlot;
            foreach (NetworkClient client in NetworkServer.s_clients)
            {
                if (client.username != null)
                {
                    NetworkServerSend.AssignClientSlot(client.id, id, username, playerSlot, oldSlot);
                }
            }
        }
        else
        {
            Debug.Log($"{tcp.socket.Client.RemoteEndPoint} failed relocate slot. Request denied.");
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Function gets called in game menu when a client clicked ready button to get ready for game.
    ///
    /// -----------------------------------------------------------------------------------------
    public void SetReadyStatus()
    {
        bool currentStatus = ServerNetworkManager.s_instance.ClientReadyButtonClick(id);

        foreach (NetworkClient client in NetworkServer.s_clients)
        {
            if (client.username != null)
            {
                NetworkServerSend.SetReadyStatus(client.id, id, playerSlot, currentStatus);
            }
        }
    }

    public class TCP
    {
        private readonly int m_id;
        private NetworkStream m_stream;
        private NetworkPacket m_receivedData;
        private byte[] m_receiveBuffer;

        public TcpClient socket { get; private set; }

        public TCP(int id)
        {
            m_id = id;
        }

        /// -----------------------------------------------------------------------------------------
        ///
        /// Summary
        ///     Set up TcpClient connection socket, data transition buffer size, and get ready to 
        ///     read data.
        ///
        /// -----------------------------------------------------------------------------------------
        public void Connect(TcpClient socket)
        {
            this.socket = socket;
            this.socket.ReceiveBufferSize = s_kDataBufferSize;
            this.socket.SendBufferSize = s_kDataBufferSize;

            m_stream = this.socket.GetStream();
            m_receivedData = new NetworkPacket();
            m_receiveBuffer = new byte[s_kDataBufferSize];
            m_stream.BeginRead(m_receiveBuffer, 0, s_kDataBufferSize, ReceiveCallback, null);
            
            NetworkServerSend.Welcome(m_id, "Welcome to the server!");
        }

        /// -----------------------------------------------------------------------------------------
        ///
        /// Summary
        ///     Send data through Tcp to correspond game client.
        ///
        /// -----------------------------------------------------------------------------------------
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
                Debug.Log($"Error sending data to player {m_id} via TCP: {ex}");
            }
        }

        /// -----------------------------------------------------------------------------------------
        ///
        /// Summary
        ///     Callback function added to BeginRead, and gets called when the socket receives data.
        /// 
        /// -----------------------------------------------------------------------------------------
        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = m_stream.EndRead(result);
                if (byteLength <= 0)
                {
                    NetworkServer.s_clients[m_id].Disconnect();
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
                NetworkServer.s_clients[m_id].Disconnect();
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
                        NetworkServer.s_kPacketHandlers[packetId](m_id, packet);
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

        public void Disconnect()
        {
            Debug.Log("TCP Disconnected!");
            socket.Close();
            m_stream = null;
            m_receivedData = null;
            m_receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint m_endPoint;
        private int m_id;

        public UDP(int id)
        {
            m_id = id;
        }

        /// -----------------------------------------------------------------------------------------
        ///
        /// Summary
        ///     Set up UdpClient connection socket, data transition buffer size, and get ready to 
        ///     read data.
        ///
        /// -----------------------------------------------------------------------------------------
        public void Connect(IPEndPoint endPoint)
        {
            m_endPoint = endPoint;
            NetworkServerSend.UDPTest(m_id);
        }

        /// -----------------------------------------------------------------------------------------
        ///
        /// Summary
        ///     Send data through Udp to correspond game client.
        ///
        /// -----------------------------------------------------------------------------------------
        public void SendData(NetworkPacket packet)
        {
            NetworkServer.SendUdpData(m_endPoint, packet);
        }

        public void HandleData(NetworkPacket packet)
        {
            int packetLength = packet.ReadInt();
            byte[] packetBytes = packet.ReadBytes(packetLength);

            NetworkThreadManager.ExecuteOnMainThread(() => 
            {
                using (NetworkPacket packet = new NetworkPacket(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    NetworkServer.s_kPacketHandlers[packetId](m_id, packet);
                }
            });
        }

        public void Disconnect()
        {
            Debug.Log("UDP Disconnected!");
            m_endPoint = null;
        }
    }
}
