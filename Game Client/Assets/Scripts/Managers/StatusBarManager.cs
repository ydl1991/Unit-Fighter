using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusBarManager : MonoBehaviour
{
    public delegate void SwitchOnAndOff(bool on);
    public static SwitchOnAndOff switchOnAndOff;
    public delegate void UpdateAllStatus();
    public static UpdateAllStatus updateAllStatus;

    public PlayerStatus[] m_playerStatuses;

    void Awake()
    {
        switchOnAndOff = ShowSelf;
        updateAllStatus = UpdateAllStatusInfo;
    }

    public void InitAllStatusInfo(Player[] players)
    {
        for (int i = 0; i < players.Length; ++i)
        {
            m_playerStatuses[i].Init(players[i]);
        }
    }

    public void UpdateAllStatusInfo()
    {
        Array.ForEach(m_playerStatuses, status => status.UpdateInfo());
        ReorderPlayerStatus();
    }

    private void ReorderPlayerStatus()
    {
        Array.Sort(
            m_playerStatuses, 
            delegate(PlayerStatus p1, PlayerStatus p2) { return p2.PlayerHP().CompareTo(p1.PlayerHP()); }
        );

        for (int i = 0; i < m_playerStatuses.Length; ++i)
        {
            m_playerStatuses[i].transform.SetSiblingIndex(i);
        }
    }

    private void ShowSelf(bool on)
    {
        gameObject.SetActive(on);
    }
}
