using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameClientSend : MonoBehaviour
{
    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to server to notify that we received the welcome message.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void WelcomeReceived()
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToServerPackets.kWelcomeReceived))
        {
            packet.Write(GameClient.s_instance.clientId);
            packet.Write(GameClient.s_instance.name);

            SendTcpDataToServer(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to server to request change of seat.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientSlotChangeRequest(int toSlotId)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToServerPackets.kClientSlotChangeRequest))
        {
            packet.Write(GameClient.s_instance.clientId);
            packet.Write(toSlotId);

            SendTcpDataToServer(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to server that the client is ready for game.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientReadyButtonClick()
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToServerPackets.kClientReadyButtonClick))
        {
            packet.Write(GameClient.s_instance.clientId);
            
            SendTcpDataToServer(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to server that the client's game loading finished.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientGameLoaded()
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToServerPackets.kClientGameLoaded))
        {
            packet.Write(GameClient.s_instance.clientId);
            packet.Write(GameClient.s_instance.clientSpot);
            
            Debug.Log("Send Client finish loading!");
            SendTcpDataToServer(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to server that the client request to refresh cards.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientCardsRefreshRequest()
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToServerPackets.kCardsRefreshRequest))
        {
            packet.Write(GameClient.s_instance.clientId);
            packet.Write(GameClient.s_instance.clientSpot);
            
            SendTcpDataToServer(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to server that the client request to buy exp.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientBuyExpRequest()
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToServerPackets.kBuyExpRequest))
        {
            packet.Write(GameClient.s_instance.clientId);
            packet.Write(GameClient.s_instance.clientSpot);
            
            SendTcpDataToServer(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to server that the client request to buy unit card.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientBuyCardRequest(int cardSlot)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToServerPackets.kBuyCardRequest))
        {
            packet.Write(GameClient.s_instance.clientId);
            packet.Write(GameClient.s_instance.clientSpot);
            packet.Write(cardSlot);
            
            SendTcpDataToServer(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to server that the client sold a unit.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientSellUnit(UnitHolder fromHolder)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToServerPackets.kSellUnit))
        {
            packet.Write(GameClient.s_instance.clientId);
            packet.Write(GameClient.s_instance.clientSpot);
            packet.Write((int)fromHolder.type);
            packet.Write(fromHolder.index);
            
            SendTcpDataToServer(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to server that the client moved a unit.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientMoveUnit(UnitHolder fromHolder, UnitHolder toHolder, int numBattleUnits)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToServerPackets.kMoveUnit))
        {
            packet.Write(GameClient.s_instance.clientId);
            packet.Write(GameClient.s_instance.clientSpot);
            packet.Write((int)fromHolder.type);
            packet.Write(fromHolder.index);
            packet.Write((int)toHolder.type);
            packet.Write(toHolder.index);
            packet.Write(numBattleUnits);

            SendTcpDataToServer(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Sync chat message to all clients.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientSendChat(string msg)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kChatMessage))
        {
            packet.Write(msg);
            
            Debug.Log($"Send message to all: {msg}.");

            SendTcpDataToAllClients(packet);
        }
    }

    private static void SendTcpDataToServer(NetworkPacket packet)
    {
        packet.WriteLength();
        GameClient.s_instance.serverTcp.SendData(packet);
    }

    private static void SendUdpDataToServer(NetworkPacket packet)
    {
        packet.WriteLength();
        GameClient.s_instance.serverUdp.SendData(packet);
    }

    private static void SendTcpDataToClient(int clientIndex, NetworkPacket packet)
    {
        packet.WriteLength();
        GameClient.s_instance.m_clientTcps[clientIndex].SendData(packet);
    }

    private static void SendTcpDataToAllClients(NetworkPacket packet)
    {
        packet.WriteLength();
        for (int i = 0; i < GameClient.s_instance?.m_clientTcps?.Count; ++i)
        {
            GameClient.s_instance.m_clientTcps[i]?.SendData(packet);
        }
    }

    public static void UDPTestReceived()
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToServerPackets.kUdpTestReceive))
        {
            packet.Write("Received a UDP packet.");
            SendUdpDataToServer(packet);
        }
    }
}
