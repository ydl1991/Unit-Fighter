using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameClientHandle : MonoBehaviour
{
    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when client receives a welcome message from server and notify
    ///     server that we received it.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void WelcomeFromServer(NetworkPacket packet)
    {
        string msg = packet.ReadString();
        int id = packet.ReadInt();
        int clientListenerPort = packet.ReadInt();

        Debug.Log($"Message from server: {msg}");
        GameClient.s_instance.clientId = id;
        GameClient.s_instance.ServerConnected(clientListenerPort);
        MenuManager.s_instance.EnterGameRoom();
        GameClientSend.WelcomeReceived();

        GameClient.s_instance.serverUdp.Connect(
            ((IPEndPoint)GameClient.s_instance.serverTcp.socket.Client.LocalEndPoint).Port,
            GameClient.s_instance.m_serverIp,
            GameClient.s_instance.m_serverPort
        );
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when client receives a welcome message from server and notify
    ///     server that we received it.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientDisconnected(NetworkPacket packet)
    {
        int id = packet.ReadInt();
        int slot = packet.ReadInt();

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            MenuManager.s_instance.ReleaseSlot(slot);
        }
        else
        {

        }

        Debug.Log($"Client {id} has disconnected!");
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Tcp punch through to connect to other clients
    ///
    /// -----------------------------------------------------------------------------------------
    public static void RecordNewClient(NetworkPacket packet)
    {
        int newClientId = packet.ReadInt();
        string newClientEP = packet.ReadString();

        //GameClient.s_instance.m_ipToClientId[newClientEP] = newClientId;

        Debug.Log($"New client End Point address {newClientEP} is in record!");
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Tcp punch through to connect to other clients
    ///
    /// -----------------------------------------------------------------------------------------
    public static void TcpPunchThrough(NetworkPacket packet)
    {
        int punchId = packet.ReadInt();
        string punchIp = packet.ReadString();
        int punchPort = packet.ReadInt();

        if (punchId != GameClient.s_instance.clientId)
        {
            GameClient.s_instance.ConnectToClient(punchId, punchIp, punchPort);
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when server notifies client to change seat.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void AssignClientSlot(NetworkPacket packet)
    {
        int clientId = packet.ReadInt();
        string username = packet.ReadString();
        int slotNum = packet.ReadInt();
        int oldslotNum = packet.ReadInt();

        MenuManager.s_instance.ReleaseSlot(oldslotNum);
        MenuManager.s_instance.OccupySlot(slotNum, username);

        if (clientId == GameClient.s_instance.clientId)
        {
            GameClient.s_instance.clientSpot = slotNum;
        }

        Debug.Log($"Client {clientId} has been asssigned to slot {slotNum}");
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when server notifies client to change local ready status.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientSetReady(NetworkPacket packet)
    {
        int clientId = packet.ReadInt();
        int slot = packet.ReadInt();
        bool ready = packet.ReadBool();

        MenuManager.s_instance.SlotReadyColor(slot, ready);

        Debug.Log($"Client {clientId} has set ready status to {ready.ToString()}");
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when server notifies client to load game.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void LoadGame(NetworkPacket packet)
    {
        if (SceneTransition.s_instance.CurrentSceneIndex() != 1)
        {
            Debug.Log($"Client is ready to load game.");
            SceneTransition.s_instance.FadeToNextLevel();
        }
        else
        {
            GameClientSend.ClientGameLoaded();
        }
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Handle function called when server notifies client to start game.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void StartGame(NetworkPacket packet)
    {
        Debug.Log($"Client is ready to start game.");
        LoadingScreen.s_instance.ReadyToGame();
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Set timer received from server.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void SyncTimer(NetworkPacket packet)
    {
        int time = packet.ReadInt();
        GameManager.s_instance.m_headerBar.SetTimeText(time);
    } 

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Battle group assigned from server.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void AssignBattleGroup(NetworkPacket packet)
    {
        Debug.Log($"Client received battle assign data.");
        int length = packet.ReadInt();
        List<int> activePlayers = new List<int>(length);
        for (int i = 0; i < length; ++i)
        {
            activePlayers.Add(packet.ReadInt());
        }

        GameManager.s_instance.BattleAssign(activePlayers);
    } 

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Cards sync command from server.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void SyncCards(NetworkPacket packet)
    {
        int len = packet.ReadInt();
        List<Card> newCards = new List<Card>(len);
        for (int i = 0; i < len; ++i)
        {
            newCards.Add(packet.ReadCard());
        }

        GameManager.s_instance.m_players[GameClient.s_instance.clientSpot].SyncCards(newCards);
        GameManager.s_instance.GetPlayerController().UpdateInfoPanel();
        Debug.Log($"Client {GameClient.s_instance.clientId} successfully synced cards.");
    } 

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Player status sync from server.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void PlayerStatusSync(NetworkPacket packet)
    {
        int spot = packet.ReadInt();
        int level = packet.ReadInt();
        int newExp = packet.ReadInt();
        int cashLeft = packet.ReadInt();

        GameManager.s_instance.m_players[spot].SyncLevel(level);
        GameManager.s_instance.m_players[spot].SyncExp(newExp);
        GameManager.s_instance.m_players[spot].SyncCash(cashLeft);
        if (spot == GameClient.s_instance.clientSpot)
        {
            GameManager.s_instance.GetPlayerController().UpdateInfoPanel();
        }
        Debug.Log($"Player {spot} data successfully synced.");
    } 

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Receive Game State change from server.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void GameStateChange(NetworkPacket packet)
    {
        GameManager.s_instance.ChangeToNextState();
        Debug.Log($"Client {GameClient.s_instance.clientId} change state request received.");
    } 

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Player unit card action sync from server.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void CardPurchaseAction(NetworkPacket packet)
    {
        int playerSpot = packet.ReadInt();
        int cardSlot = packet.ReadInt();
        Card card = packet.ReadCard();

        GameManager.s_instance.m_players[playerSpot].BoughtCard(cardSlot, card);
        if (playerSpot == GameClient.s_instance.clientSpot)
        {
            PlayerController controller = GameManager.s_instance.GetPlayerController();
            controller.UpdateAttributeInfo();
            controller.UpdateInfoPanel();
        }
        Debug.Log($"Client {GameClient.s_instance.clientId} successfully bought EXP.");
    } 

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Player sell unit action sync from server.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void SellUnitAction(NetworkPacket packet)
    {
        int playerSpot = packet.ReadInt();
        int holderType = packet.ReadInt();
        int holderIndex = packet.ReadInt();

        UnitHolder unitHolder = GameManager.s_instance.m_players[playerSpot].board.LocateUnitHolder((UnitHolderType)holderType, holderIndex);
        GameManager.s_instance.m_players[playerSpot].SellUnit(unitHolder.holdingUnit);

        Debug.Log($"Player {playerSpot} successfully sold a unit.");
    } 

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Player move unit action sync from server.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void MoveUnitAction(NetworkPacket packet)
    {
        int playerSpot = packet.ReadInt();
        int fromType = packet.ReadInt();
        int fromIndex = packet.ReadInt();
        int toType = packet.ReadInt();
        int toIndex = packet.ReadInt();
        int numBattleUnits = packet.ReadInt();

        UnitHolder curHolder = GameManager.s_instance.m_players[playerSpot].board.LocateUnitHolder((UnitHolderType)fromType, fromIndex);
        UnitHolder newHolder = GameManager.s_instance.m_players[playerSpot].board.LocateUnitHolder((UnitHolderType)toType, toIndex);
        curHolder.holdingUnit.ChangePosition(newHolder, numBattleUnits);
        AttributeBarManager.updateAttributeBar((PlayerFlag)playerSpot);

        Debug.Log($"Player {playerSpot} successfully moved a unit.");
    } 

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Player is a battle winner.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void AssignBattleWinner(NetworkPacket packet)
    {
        int playerSpot = packet.ReadInt();

        GameManager.s_instance.m_players[playerSpot].BattleWin();
        Debug.Log($"Player {playerSpot} is a battle winner.");
    } 

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Player is taking battle damage.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void TakeBattleDamage(NetworkPacket packet)
    {
        int playerSpot = packet.ReadInt();
        int damage = packet.ReadInt();

        GameManager.s_instance.m_players[playerSpot].TakeDamage(damage);
        Debug.Log($"Player {playerSpot} took {damage} battle damage.");
    } 

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Client has lost the game.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientLose(NetworkPacket packet)
    {
        BottomBarManager.switchOnAndOff(false);

        // exit game selection
    } 

    public static void UDPTest(NetworkPacket packet)
    {
        string msg = packet.ReadString();

        Debug.Log($"Received packet via UDP. Message: {msg}");
        GameClientSend.UDPTestReceived();
    }

    /// -----------------------------------------------------------------------------------------
    ///
    /// Summary
    ///     Client receives message.
    ///
    /// -----------------------------------------------------------------------------------------
    public static void ClientReceiveChatMessage(NetworkPacket packet)
    {
        string msg = packet.ReadString();
        
        ChatManager.s_instance?.AddMessage(msg);
    } 

}
