using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen s_instance;

    public GameObject m_game;
    
    void Awake()
    {
        s_instance = this;
    }

    // Update is called once per frame
    public void ReadyToGame()
    {
        GameManager.s_instance.InitGameCam();
        GameManager.s_instance.InitStateMachine();
        SceneTransition.s_instance.ResumeSound();
        SceneTransition.s_instance.FadeIn();
        enabled = false;
        gameObject.SetActive(false);
    }
}
