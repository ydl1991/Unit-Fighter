
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition s_instance;
    
    public AudioSource m_menuBGM; 
    public Animator m_animator;
    public float m_transitionTime = 1f;

    void Awake()
    {
        s_instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    public int CurrentSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    public void FadeToNextLevel()
    {
        int levelIndex = SceneManager.GetActiveScene().buildIndex + 1;
        StartCoroutine(LoadLevel(levelIndex >= SceneManager.sceneCountInBuildSettings ? 0 : levelIndex));
    }

    public void FadeToMenu()
    {
        StartCoroutine(LoadLevel(0));
    }

    public void DieAndFadeToMenu()
    {
        StartCoroutine(DieAndLoadMenu());
    }

    public void WinAndFadeToMenu()
    {
        StartCoroutine(WinAndLoadMenu());
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        m_animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(m_transitionTime);
        if (m_menuBGM != null)
            m_menuBGM.Stop();

        // Wait until the asynchronous scene fully loads    
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelIndex);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        GameManager.s_instance.InitPlayer(GameClient.s_instance.clientId, GameClient.s_instance.name, GameClient.s_instance.clientSpot);
    }

    IEnumerator DieAndLoadMenu()
    {
        m_animator.SetTrigger("DieAndFadeOut");
        yield return new WaitForSeconds(m_transitionTime * 4);
        SceneManager.LoadScene(0);
    }

    IEnumerator WinAndLoadMenu()
    {
        m_animator.SetTrigger("WinAndFadeOut");
        yield return new WaitForSeconds(m_transitionTime * 4);
        SceneManager.LoadScene(0);
    }

    public void FadeOut()
    {
        m_animator.SetTrigger("FadeOut");
    }

    public void FadeIn()
    {
        m_animator.SetTrigger("FadeIn");
    }

    public void MuteAll()
    {
        AudioListener.volume = 0;
    }

    public void ResumeSound()
    {
        AudioListener.volume = 1;
    }
}
