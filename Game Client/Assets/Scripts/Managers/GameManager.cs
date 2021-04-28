using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager s_instance;
    public int gameRound { get; private set; }
    public GameCam gameCam { get; private set; }

    public PlayerController m_controllerPrefab;
    public Player[] m_players;
    public Battlefield[] m_battleFields;
    private PlayerController m_playerController;
    public StatusBarManager m_statusBar;
    public HeaderBarManager m_headerBar;
    private StateMachine m_stateMachine;
    

    void Awake()
    {
        s_instance = this;
        gameRound = 0;
    }

    void Start()
    {
        InitStatusBar();
    }

    public void InitPlayer(int clientId, string username, int slotNum)
    {
        if (clientId == GameClient.s_instance.clientId)
        {
            CreatePlayerController(slotNum);
        }

        m_players[slotNum].Init(clientId, username);
        GameClientSend.ClientGameLoaded();
    }

    public void CreatePlayerController(int playerIndex)
    {
        PlayerController ct = Instantiate(m_controllerPrefab, Vector3.zero, Quaternion.identity);
        ct.SetController(m_players[playerIndex]);
        m_playerController = ct;
    }

    public PlayerController GetPlayerController()
    {
        return m_playerController;
    }

    public void InitGameCam()
    {
        gameCam = Camera.main.GetComponent<GameCam>();
        gameCam.Init(m_playerController.ControllingPlayer());
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

    public void ChangeToNextState()
    {
        m_stateMachine.SetCurrentState(NextGameState(m_stateMachine.currentState.stateName));
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

    public void BattleAssign(List<int> activePlayers)
    {
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
}
