using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkServerHandle
{
    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when client notifies server that it received our welcome message sent.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void WelcomeReceived(int fromClient, NetworkPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        string username = packet.ReadString();

        Debug.Log(
            $"{NetworkServer.s_clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}."
        );

        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        }

        NetworkServer.s_clients[fromClient].SendIntoGame(username);
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when client notifies server that it wants to change a seat spot.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ChangeSlotRequestReceived(int fromClient, NetworkPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        int changeTo = packet.ReadInt();

        Debug.Log(
            $"{NetworkServer.s_clients[fromClient].tcp.socket.Client.RemoteEndPoint} requested to change to game slot {changeTo}."
        );

        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        }

        NetworkServer.s_clients[fromClient].TryChangeSlot(changeTo);
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when client notifies server that it's ready for game. The server
    ///     will approve it and check if all players are ready then start a game.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ReadyButtonClickReceived(int fromClient, NetworkPacket packet)
    {
        int clientIdCheck = packet.ReadInt();

        Debug.Log(
            $"{NetworkServer.s_clients[fromClient].tcp.socket.Client.RemoteEndPoint} clicked ready button."
        );

        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        }

        NetworkServer.s_clients[fromClient].SetReadyStatus();
        if (ServerNetworkManager.s_instance.AllClientsReady())
        {
            SceneTransitionServer.s_instance.FadeAllClientsToNextLevel(ServerNetworkManager.s_instance.GetClientSlots());
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when client notifies server that it's ready for game. The server
    ///     will approve it and check if all players are ready then start a game.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientGameFinishedLoading(int fromClient, NetworkPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        int clientSpot = packet.ReadInt();

        Debug.Log(
            $"{NetworkServer.s_clients[fromClient].tcp.socket.Client.RemoteEndPoint} finished loading game."
        );

        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        }

        NetworkServer.s_clients[fromClient].PossessPlayer(GameManager.s_instance.m_players[clientSpot]);
        LoadingScreen.s_instance.SetPlayerLoaded(clientSpot);
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when client requests to refresh cards.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientCardsRefreshRequestReceived(int fromClient, NetworkPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        int clientSpot = packet.ReadInt();

        Debug.Log(
            $"{NetworkServer.s_clients[fromClient].tcp.socket.Client.RemoteEndPoint} refreshed cards."
        );

        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        }

        if (GameManager.s_instance.m_players[clientSpot].RefreshCards())
        {
            NetworkServerSend.SyncCards(clientIdCheck, GameManager.s_instance.m_players[clientSpot].cards);
            NetworkServerSend.PlayerStatusSyncToAll(
                clientSpot, 
                GameManager.s_instance.m_players[clientSpot].level, 
                GameManager.s_instance.m_players[clientSpot].exp,
                GameManager.s_instance.m_players[clientSpot].cash
            );
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when client requests to buy exp.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientBuyExpRequestReceived(int fromClient, NetworkPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        int clientSpot = packet.ReadInt();

        Debug.Log(
            $"{NetworkServer.s_clients[fromClient].tcp.socket.Client.RemoteEndPoint} bought EXP."
        );

        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        }

        if (GameManager.s_instance.m_players[clientSpot].BuyExp())
        {
            NetworkServerSend.PlayerStatusSyncToAll(
                clientSpot, 
                GameManager.s_instance.m_players[clientSpot].level, 
                GameManager.s_instance.m_players[clientSpot].exp,
                GameManager.s_instance.m_players[clientSpot].cash
            );
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when client requests to buy card.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientBuyCardRequestReceived(int fromClient, NetworkPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        int clientSpot = packet.ReadInt();
        int cardSlot = packet.ReadInt();

        Debug.Log(
            $"{NetworkServer.s_clients[fromClient].tcp.socket.Client.RemoteEndPoint} bought a unit card."
        );

        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        }

        if (GameManager.s_instance.m_players[clientSpot].BuyCard(cardSlot, out Card? card))
        {
            if (!card.HasValue)
            {
                Debug.Log($"Error: Bought card does not exist! Card slot: {cardSlot}");
                return;
            }

            NetworkServerSend.CardPurchaseAction(clientSpot, cardSlot, card.Value);
            NetworkServerSend.PlayerStatusSyncToAll(
                clientSpot, 
                GameManager.s_instance.m_players[clientSpot].level, 
                GameManager.s_instance.m_players[clientSpot].exp,
                GameManager.s_instance.m_players[clientSpot].cash
            );
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when client sold a unit.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientSellUnitActionReceived(int fromClient, NetworkPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        int clientSpot = packet.ReadInt();
        int holderType = packet.ReadInt();
        int holderIndex = packet.ReadInt();

        Debug.Log(
            $"{NetworkServer.s_clients[fromClient].tcp.socket.Client.RemoteEndPoint} sold a unit."
        );

        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        }

        NetworkServerSend.SellUnitAction(fromClient, clientSpot, holderType, holderIndex);

        UnitHolder unitHolder = GameManager.s_instance.m_players[clientSpot].board.LocateUnitHolder((UnitHolderType)holderType, holderIndex);
        GameManager.s_instance.m_players[clientSpot].SellUnit(unitHolder.holdingUnit);
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when client move a unit.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientMoveUnitActionReceived(int fromClient, NetworkPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        int clientSpot = packet.ReadInt();
        int fromType = packet.ReadInt();
        int fromIndex = packet.ReadInt();
        int toType = packet.ReadInt();
        int toIndex = packet.ReadInt();
        int numBattleUnits = packet.ReadInt();

        Debug.Log(
            $"{NetworkServer.s_clients[fromClient].tcp.socket.Client.RemoteEndPoint} moved a unit."
        );

        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        }

        NetworkServerSend.MoveUnitAction(fromClient, clientSpot, fromType, fromIndex, toType, toIndex, numBattleUnits);

        UnitHolder curHolder = GameManager.s_instance.m_players[clientSpot].board.LocateUnitHolder((UnitHolderType)fromType, fromIndex);
        UnitHolder newHolder = GameManager.s_instance.m_players[clientSpot].board.LocateUnitHolder((UnitHolderType)toType, toIndex);
        curHolder.holdingUnit.ChangePosition(newHolder, numBattleUnits);
        AttributeBarManager.updateAttributeBar((PlayerFlag)clientSpot);
    }

    public static void UDPTestReceived(int fromClient, NetworkPacket packet)
    {
        string msg = packet.ReadString();
        Debug.Log($"Received packet via UDP. Message: {msg}");   
    }
}
