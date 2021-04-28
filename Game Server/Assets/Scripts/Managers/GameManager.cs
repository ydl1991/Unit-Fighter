using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager s_instance;

    public CardDeck deck { get; private set; }
    public XOrShiftRNG rng { get; private set; }
    public GameCam gameCam { get; private set; }
    public int gameRound { get; private set; }

    public AIController m_AIControllerPrefab;
    public Player[] m_players;
    public Battlefield[] m_battleFields;
    private List<AIController> m_AIControllers;
    public StatusBarManager m_statusBar;
    public HeaderBarManager m_headerBar;
    private StateMachine m_stateMachine;

    void Awake()
    {
        s_instance = this;
        gameRound = 0;

        rng = new XOrShiftRNG();
        deck = new CardDeck();
        m_AIControllers = new List<AIController>();
    }

    void Start()
    {
        InitStatusBar();
    }

    void Update()
    {
        UpdateStateMachine();
    }

    public void CreateAIController(int index)
    {
        AIController ct = Instantiate(m_AIControllerPrefab, Vector3.zero, Quaternion.identity);
        ct.SetController(m_players[index]);
        m_AIControllers.Add(ct);
        LoadingScreen.s_instance.SetPlayerLoaded(index);
    }

    public Player GetPlayer(int id)
    { 
        if (id < 0 || id >= m_players.Length)
        {
            Debug.Log($"Error: The given ID {id} is out of range.");
            return null;
        }

        return m_players[id];
    }

    public void InitGameCam()
    {
        gameCam = Camera.main.GetComponent<GameCam>();
        gameCam.Init(m_players[0]);
    }

    private void InitStatusBar()
    {
        m_statusBar.InitAllStatusInfo(m_players);
    }

    public void InitStateMachine()
    {
        m_stateMachine = new StateMachine();
        m_stateMachine.SetCurrentState(new PrepareState(this));
    }

    private void UpdateStateMachine()
    {
        if (m_stateMachine == null)
            return;

        m_stateMachine.UpdateStateMachine();

        if (m_stateMachine.currentState.StateDone())
        {
            NetworkServerSend.NotifyGameStateChange();
            m_stateMachine.SetCurrentState(NextGameState(m_stateMachine.currentState.stateName));
        }
    }

    private State NextGameState(string stateName)
    {
        switch (stateName)
        {
            case "PrepareState":
                return new BattleAssignState(this);

            case "BattleAssignState":
                return new BattleState(this);

            case "BattleState":
                return new ClearanceState(this);

            case "ClearanceState":
                return new BattleResetState(this);

            case "BattleResetState":
                return new PrepareState(this);            

            default :
                return null;
        }
    }

    public void IncrementGameRound()
    {
        ++gameRound;
        m_headerBar.SetRoundText(gameRound);
    }

    public string State()
    {
        return m_stateMachine.currentState.stateName;
    }

    public void ShowPlayerTransBoards(bool show)
    {
        foreach (Player player in m_players)
        {
            player.ShowTransBoard(show);
        }
    }

    public void BattleAssign()
    {
        List<int> activePlayers = new List<int>();
        List<int> inactivePlayers = new List<int>();
        for (int i = 0; i < m_players.Length; ++i)
        {
            if (m_players[i].health > 0)
                activePlayers.Add(i);
            else
                inactivePlayers.Add(i);
        }

        GameUtil.Shuffle(activePlayers, rng);
        activePlayers.AddRange(inactivePlayers);

        NetworkServerSend.AssignBattleGroup(activePlayers);
        StartCoroutine(BattleAssigningProcess(activePlayers));
    }

    private IEnumerator BattleAssigningProcess(List<int> list)
    {
        yield return new WaitForSeconds(2f);

        int battleFieldIndex = 0;

        for (int i = 0; i < list.Count; ++i)
        {
            m_battleFields[battleFieldIndex].AssignBattlePlayer(m_players[list[i]]);

            yield return new WaitForSeconds(1.2f);

            if (i % 2 == 1)
                ++battleFieldIndex;
        }

        yield return new WaitForSeconds(1.5f);

        gameCam.ResetCam();
    }

    public void BattleReset()
    {
        StartCoroutine(BattleResetProcess());
    }

    private IEnumerator BattleResetProcess()
    {
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < m_battleFields.Length; ++i)
        {
            m_battleFields[i].ResetBattlePlayer();

            yield return new WaitForSeconds(2.4f);
        }
    }

    public void ResetAllPlayerUnits()
    {
        foreach (Player player in m_players)
        {
            if (player != null && player.health > 0)
            {
                player.ResetBattleUnits();
            }
        }
    }

    public void AllUnitsEnterBattle()
    {
        foreach (Player player in m_players)
        {
            if (player.health > 0)
            {
                List<Unit> battleUnits = player.BattleUnits();
                battleUnits.ForEach(unit => unit.EnterBattle());
            }
        }
    }

    public void AllUnitsExitBattle()
    {
        foreach (Player player in m_players)
        {
            if (player.health > 0)
            {
                List<Unit> battleUnits = player.BattleUnits();
                battleUnits.ForEach(unit => unit.ExitBattle());
            }
        }
    }

    public void ClearanceForAll()
    {
        foreach (Battlefield battleField in m_battleFields)
        {
            battleField.BattleClearance();
        }
    }

    public void RoundStart()
    {
        int cashReward = gameRound < 2 ? 2 : gameRound > 10 ? 10 : gameRound;
        for (int i = 0; i < m_players.Length; ++i)
        {
            if (m_players[i].health > 0)
            {
                m_players[i].GainCash(cashReward);
                m_players[i].GainExp(2);
                m_players[i].RefreshCardsForFree();
            }
        }

        foreach (NetworkClient client in NetworkServer.s_clients)
        {
            if (client.username != null)
            {
                NetworkServerSend.SyncCards(client.id, GameManager.s_instance.m_players[client.playerSlot].cards);
                NetworkServerSend.PlayerStatusSyncToAll(
                    client.playerSlot, 
                    GameManager.s_instance.m_players[client.playerSlot].level, 
                    GameManager.s_instance.m_players[client.playerSlot].exp,
                    GameManager.s_instance.m_players[client.playerSlot].cash
                );
            }
        }
    }
}
