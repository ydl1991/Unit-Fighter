using System;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen s_instance;

    public GameObject m_game;

    private bool[] m_playerLoadingList;

    void Awake()
    {
        s_instance = this;
        m_playerLoadingList = new bool[NetworkConstants.k_maxPlayer];
    }

    private bool AllLoaded()
    {
        int index = Array.FindIndex(m_playerLoadingList, loadStatus => loadStatus == false);
        return index == -1;
    }

    public void SetPlayerLoaded(int index)
    {
        if (index < 0 || index >= NetworkConstants.k_maxPlayer || m_playerLoadingList[index])
            return;

        Debug.Log($"Player {index} is loaded!");

        m_playerLoadingList[index] = true;

        if (AllLoaded())
        {
            NetworkServerSend.StartGame();
            GameManager.s_instance.InitGameCam();
            GameManager.s_instance.InitStateMachine();
            SceneTransitionServer.s_instance.FadeIn();
            gameObject.SetActive(false);
        }
    }
}
