using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBarManager : MonoBehaviour
{
    public delegate void ShowMessage(string msg);
    public static ShowMessage showMessage;

    public RawImage m_messageBackground;
    public Text m_message;
    private Animator m_anim;

    void Awake()
    {
        showMessage = SetMessage;
        m_anim = GetComponent<Animator>();
    }

    public void SetMessage(string message)
    {
        m_message.text = message;
        m_anim.SetTrigger("ShowMessage");
    }
}
