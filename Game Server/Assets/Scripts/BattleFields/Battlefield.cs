using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battlefield : MonoBehaviour
{
    private Player[] m_players;

    void Awake()
    {
        m_players = new Player[2] { null, null };
    }

    public void AssignBattlePlayer(Player player)
    {
        for (int i = 0; i < m_players.Length; ++i)
        {
            if (m_players[i] == null)
            {
                m_players[i] = player;
                m_players[i].MoveToPosInSecWithRot(transform.position, BattleBoardRotationByIndex(i), 1f);
                return;
            }
        }
    }

    private Vector3 BattleBoardRotationByIndex(int index)
    {
        return index == 0 ? new Vector3(0f, 180f, 0f) : Vector3.zero;
    }

    public void ResetBattlePlayer()
    {
        for (int i = 0; i < m_players.Length; ++i)
        {
            if (m_players[i] != null)
            {
                m_players[i].ResetPositionAndRotation(2f);
                m_players[i] = null;
            }
        }
    }

    public void BattleClearance()
    {
        if (m_players[0].health <= 0)
            return;
        
        StartCoroutine(WinnerCheerUpAndClearance(m_players));
    }

    private IEnumerator WinnerCheerUpAndClearance(Player[] battlePlayers)
    {
        int[] activeUnitsSize = new int[2] { -1, -1 };
        
        int winner = -1;
        int activeSize = 0;
        for (int i = 0; i < battlePlayers.Length; ++i)
        {
            if (battlePlayers[i].health > 0)
            {
                activeUnitsSize[i] = battlePlayers[i].board.ActiveBattleUnitsSize();

                if (activeUnitsSize[i] > activeSize)
                {
                    activeSize = activeUnitsSize[i];
                    winner = i;
                }
                else if (activeUnitsSize[i] == activeSize && winner != -1)
                {
                    winner = -1;
                }
            }
        }

        if (winner != -1)
        {
            battlePlayers[winner].BattleWin();
        }

        yield return new WaitForSeconds(3f);

        int gameRound = GameManager.s_instance.gameRound;
        int damagePerUnit = gameRound / 7 < 2 ? 2 : gameRound / 7;

        for (int i = 0; i < battlePlayers.Length; ++i)
        {
            if (battlePlayers[i].health <= 0)
                continue;
                
            if (activeUnitsSize[1 - i] > 0)
            {
                battlePlayers[i].TakeDamage(activeUnitsSize[1 - i] * damagePerUnit);
            }
            else if (winner == -1 && activeUnitsSize[1 - i] == 0)
            {
                battlePlayers[i].TakeDamage(damagePerUnit);
            }
        }
    }
}
