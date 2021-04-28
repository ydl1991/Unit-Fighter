using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : State
{
    public float stateLength { get; protected set; }
    protected GameManager game { get; set; }

    protected GameState(GameManager game)
    {
        this.game = game;
    }

    public override void UpdateState()
    {
        stateLength -= Time.deltaTime;
        if (stateLength < 0f) { stateLength = 0f; }
        game.m_headerBar.SetTimeText(stateLength);
    }

    public override bool StateDone()
    {
        return stateLength <= 0f;
    }
}

public class PrepareState : GameState
{
    public PrepareState(GameManager game) : base(game)
    {
        stateName = "PrepareState";
        stateLength = 30f;
    }

    public override void UpdateState()
    {
        base.UpdateState();

    }

    public override void EnterState()
    {
        game.IncrementGameRound();
        game.ShowPlayerTransBoards(true);
        game.ResetAllPlayerUnits();
        game.RoundStart();
        Camera.main.GetComponent<GameCam>().ResetCam();
    }

    public override void ExitState()
    {
        MessageBarManager.showMessage("Battle Assign Stage");
    }
}

public class BattleAssignState : GameState
{
    private Vector3 m_camPos;
    private Vector3 m_camRot;
    private GameCam m_cam;

    public BattleAssignState(GameManager game) : base(game)
    {
        stateName = "BattleAssignState";
        stateLength = 15f;
        m_camPos = new Vector3(150f, 250f, -100f);
        m_camRot = new Vector3(90f, 0f, 0f);
        m_cam = Camera.main.GetComponent<GameCam>();
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override void EnterState()
    {
        StatusBarManager.switchOnAndOff(false);
        AttributeBarManager.switchOnAndOff(false);
        game.ShowPlayerTransBoards(false);

        m_cam.MoveCameraToPositionWithRotation(m_camPos, m_camRot, 1f);
        game.BattleAssign();
    }

    public override void ExitState()
    {
        MessageBarManager.showMessage("Battle Stage");
        StatusBarManager.switchOnAndOff(true);
        AttributeBarManager.switchOnAndOff(true);
    }
}

public class BattleState : GameState
{
    public BattleState(GameManager game) : base(game)
    {
        stateName = "BattleState";
        stateLength = 30f;
    }

    public override void UpdateState()
    {
        base.UpdateState();

    }

    public override void EnterState()
    {
        game.AllUnitsEnterBattle();
    }

    public override void ExitState()
    {
        game.AllUnitsExitBattle();
        MessageBarManager.showMessage("Clearance Stage");
    }
}

public class ClearanceState : GameState
{
    public ClearanceState(GameManager game) : base(game)
    {
        stateName = "ClearanceState";
        stateLength = 5f;
    }

    public override void UpdateState()
    {
        base.UpdateState();

    }

    public override void EnterState()
    {
        game.ClearanceForAll();
    }

    public override void ExitState()
    {
        MessageBarManager.showMessage("Reset Stage");
    }
}

public class BattleResetState : GameState
{
    private Vector3 m_camPos;
    private Vector3 m_camRot;
    private GameCam m_cam;

    public BattleResetState(GameManager game) : base(game)
    {
        stateName = "BattleResetState";
        stateLength = 13f;
        m_camPos = new Vector3(150f, 250f, -100f);
        m_camRot = new Vector3(90f, 0f, 0f);
        m_cam = Camera.main.GetComponent<GameCam>();
    }

    public override void UpdateState()
    {
        base.UpdateState();

    }

    public override void EnterState()
    {
        StatusBarManager.switchOnAndOff(false);
        AttributeBarManager.switchOnAndOff(false);
        m_cam.MoveCameraToPositionWithRotation(m_camPos, m_camRot, 1f);
        game.BattleReset();
    }

    public override void ExitState()
    {
        MessageBarManager.showMessage("Prepare Stage");
        StatusBarManager.switchOnAndOff(true);
        AttributeBarManager.switchOnAndOff(true);
    }
}
