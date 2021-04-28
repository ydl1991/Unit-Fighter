
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionServer : MonoBehaviour
{
    public static SceneTransitionServer s_instance;
    
    public Animator m_animator;
    public float m_transitionTime = 1f;

    void Awake()
    {
        s_instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void FadeAllClientsToNextLevel(int[] clientSlots)
    {
        int levelIndex = SceneManager.GetActiveScene().buildIndex + 1;
        StartCoroutine(LoadLevelAsync(levelIndex >= SceneManager.sceneCountInBuildSettings ? 0 : levelIndex, clientSlots));
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

    public int CurrentSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }

    IEnumerator LoadLevelAsync(int levelIndex, int[] clientSlots)
    {
        m_animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(m_transitionTime);

        // Wait until the asynchronous scene fully loads    
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelIndex);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        NetworkServerSend.LoadGame();

        // Init AI controllers and Tell client to load game
        for (int i = 0; i < clientSlots.Length; ++i)
        {
            if (clientSlots[i] == -1)
            {
                GameManager.s_instance.CreateAIController(i);
            }
        }  
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        m_animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(m_transitionTime);
        SceneManager.LoadScene(levelIndex);
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
}
