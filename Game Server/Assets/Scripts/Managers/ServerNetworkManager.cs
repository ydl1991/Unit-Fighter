using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerNetworkManager : MonoBehaviour
{
    public static ServerNetworkManager s_instance;
    
    private int[] m_clientOccupationList;
    private bool[] m_clientReadyList;

    void Awake()
    {
        s_instance = this;

        m_clientOccupationList = Enumerable.Repeat(-1, NetworkConstants.k_maxPlayer).ToArray();
        m_clientReadyList = new bool[NetworkConstants.k_maxPlayer];
    }

    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = NetworkConstants.k_ticksPerSec;

#if UNITY_EDITOR
        Debug.Log("Build the project to start the server!");
#else
        NetworkServer.Init(26950);
#endif

    }

    void OnApplicationQuit()
    {
        NetworkServer.Stop();
    }

    public int NextAvailableClientSlot()
    {
        return Array.FindIndex(m_clientOccupationList, slot => slot == -1);
    }

    public bool SlotTaken(int index)
    {
        return m_clientOccupationList[index] != -1;
    }

    public bool TakeSlot(int index, int clientId, string username, out int oldSlot)
    {
        if (m_clientOccupationList[index] != -1)
        {
            Debug.Log("Error: trying to occupy an already-taken slot.");
            oldSlot = -1;
            return false;
        }

        oldSlot = ReleaseClientSlot(clientId);
        m_clientOccupationList[index] = clientId;
        MenuManager.s_instance.OccupySlot(index, username);

        return true;
    }

    public int ReleaseClientSlot(int clientId)
    {
        int index = Array.FindIndex(m_clientOccupationList, id => id == clientId);
        if (index != -1)
        {
            m_clientOccupationList[index] = -1;
            m_clientReadyList[index] = false;
            MenuManager.s_instance.ReleaseSlot(index);
        }

        return index;
    }

    public bool AllClientsReady()
    {
        for (int i = 0; i < m_clientOccupationList.Length; ++i)
        {
            if (m_clientOccupationList[i] != -1 && !m_clientReadyList[i])
            {
                return false;
            }
        }

        return true;
    }

    public int[] GetClientSlots()
    {
        return m_clientOccupationList.ToArray();
    }

    public bool ClientReadyButtonClick(int clientId)
    {
        int index = Array.FindIndex(m_clientOccupationList, id => id == clientId);
        if (index == -1)
        {
            Debug.Log("Error: failed to locate client.");
            return false;
        }
        
        m_clientReadyList[index] = !m_clientReadyList[index];
        MenuManager.s_instance.SlotReadyColor(index, m_clientReadyList[index]);

        return m_clientReadyList[index];
    }
}
