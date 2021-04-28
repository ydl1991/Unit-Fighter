using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkServerSend
{
    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to client to welcome it for joining with an assigned client id.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void Welcome(int toClientId, string msg)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kWelcomeFromServer))
        {
            packet.Write(msg);
            packet.Write(toClientId);
            packet.Write(NetworkServer.s_clients[toClientId].listenerPort);
            
            Debug.Log($"Sending welcome message to client {toClientId}.");

            SendTcpData(toClientId, packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to all clients that a client is disconnected.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientDisconnected(int clientId, int playerSlot)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kClientDisconnected))
        {
            packet.Write(clientId);
            packet.Write(playerSlot);

            SendTcpDataToAll(packet);
        }        
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Punch through between clients.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void TcpPunchThrough(int toClient, int punchClientId, string clientIp, int clientPort)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kTcpPunchThrough))
        {
            packet.Write(punchClientId);
            packet.Write(clientIp);
            packet.Write(clientPort);

            SendTcpData(toClient, packet);
        }        
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to client to assign a seat for it.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void AssignClientSlot(int toClient, int fromClient, string username, int slotNum, int oldSlot)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kAssignClientSlot))
        {
            packet.Write(fromClient);
            packet.Write(username);
            packet.Write(slotNum);
            packet.Write(oldSlot);

            SendTcpData(toClient, packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to client to change it's local status to 'ready for game'.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void SetReadyStatus(int toClient, int fromClient, int playerSlot, bool ready)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kClientSetReady))
        {
            packet.Write(fromClient);
            packet.Write(playerSlot);
            packet.Write(ready);

            SendTcpData(toClient, packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to client to load game.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void LoadGame()
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kClientLoadGame))
        {
            SendTcpDataToAll(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Send to client to start game.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void StartGame()
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kClientStartGame))
        {
            SendTcpDataToAll(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Sync timer to client using UDP.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void SyncTimer(int time)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kSyncTimer))
        {
            packet.Write(time);
            SendUdpDataToAll(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Sync battle assignment to client.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void AssignBattleGroup(List<int> activePlayers)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kAssignBattleGroup))
        {
            packet.Write(activePlayers.Count);
            foreach (int index in activePlayers)
            {
                packet.Write(index);
            }

            SendTcpDataToAll(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Respond to client cards refresh request.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void SyncCards(int toClient, List<Card?> cards)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kCardsSync))
        {
            packet.Write(cards.Count);
            foreach (Card? card in cards)
            {
                packet.Write(card.Value);
            }

            SendTcpData(toClient, packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Sync client data to all.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void PlayerStatusSyncToAll(int playerSpot, int level, int newExp, int cashLeft)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kPlayerDataSync))
        {
            packet.Write(playerSpot);
            packet.Write(level);
            packet.Write(newExp);
            packet.Write(cashLeft);

            SendTcpDataToAll(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Notify clients to change game states.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void NotifyGameStateChange()
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kGameStateChange))
        {
            SendTcpDataToAll(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Respond to client buy card request.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void CardPurchaseAction(int playerSpot, int cardSlot, Card card)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kCardPurchaseAction))
        {
            packet.Write(playerSpot);
            packet.Write(cardSlot);
            packet.Write(card);

            SendTcpDataToAll(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Sync client sell unit action to all except the client.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void SellUnitAction(int fromClient, int playerSpot, int holderType, int holderIndex)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kSellUnitAction))
        {
            packet.Write(playerSpot);
            packet.Write(holderType);
            packet.Write(holderIndex);

            SendTcpDataToAll(fromClient, packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Sync client move unit action to all except the client.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void MoveUnitAction(int fromClient, int playerSpot, int fromType, int fromIndex, int toType, int toIndex, int numBattleUnits)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kMoveUnitAction))
        {
            packet.Write(playerSpot);
            packet.Write(fromType);
            packet.Write(fromIndex);
            packet.Write(toType);
            packet.Write(toIndex);
            packet.Write(numBattleUnits);

            SendTcpDataToAll(fromClient, packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Declare battle winner.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void DeclareBattleWinner(int playerSpot)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kDeclareBattleWinner))
        {
            packet.Write(playerSpot);

            SendTcpDataToAll(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Declare battle loser.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void TakeBattleDamage(int playerSpot, int damage)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kTakeBattleDamage))
        {
            packet.Write(playerSpot);
            packet.Write(damage);

            SendTcpDataToAll(packet);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Declare client lose.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientLose(int toClient)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kClientLose))
        {
            SendTcpData(toClient, packet);
        }
    }

    private static void SendTcpData(int toClientId, NetworkPacket packet)
    {
        packet.WriteLength();
        NetworkServer.s_clients[toClientId].tcp.SendData(packet);
    }

    private static void SendTcpDataToAll(NetworkPacket packet)
    {
        packet.WriteLength();
        for (int i = 0; i < NetworkConstants.k_maxPlayer; ++i)
        {
            NetworkServer.s_clients[i].tcp.SendData(packet);
        }
    }

    private static void SendTcpDataToAll(int excludeClient, NetworkPacket packet)
    {
        packet.WriteLength();
        for (int i = 0; i < NetworkConstants.k_maxPlayer; ++i)
        {
            if (i != excludeClient)
                NetworkServer.s_clients[i].tcp.SendData(packet);
        }
    }

    private static void SendUdpData(int toClientId, NetworkPacket packet)
    {
        packet.WriteLength();
        NetworkServer.s_clients[toClientId].udp.SendData(packet);
    }

    private static void SendUdpDataToAll(NetworkPacket packet)
    {
        packet.WriteLength();
        for (int i = 0; i < NetworkConstants.k_maxPlayer; ++i)
        {
            NetworkServer.s_clients[i].udp.SendData(packet);
        }
    }

    private static void SendUdpDataToAll(int excludeClient, NetworkPacket packet)
    {
        packet.WriteLength();
        for (int i = 0; i < NetworkConstants.k_maxPlayer; ++i)
        {
            if (i != excludeClient)
                NetworkServer.s_clients[i].udp.SendData(packet);
        }
    }

    public static void UDPTest(int toClientId)
    {
        using (NetworkPacket packet = new NetworkPacket((int)ToClientPackets.kUdpTest))
        {
            packet.Write("A udp test!");
            SendUdpData(toClientId, packet);
        }
    }
}
