using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    public static ChatManager s_instance;

    public GameObject m_displayPanel;
    public InputField m_messageInputField;
    public GameObject m_messageHolder;
    public GameObject m_messagePrefab;

    public float m_displayPanelMaxActiveTime;
    private float m_displayPanelActiveTime;

    void Awake()
    {
        s_instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!m_displayPanel.activeSelf)
            {
                m_displayPanelActiveTime = float.PositiveInfinity;
                m_displayPanel.SetActive(true);
            }
            else
            {
                m_displayPanelActiveTime = m_displayPanelMaxActiveTime;
            }
            
            if (!m_messageInputField.gameObject.activeSelf)
            {
                m_messageInputField.gameObject.SetActive(true);
                m_messageInputField.ActivateInputField();
            }
            else 
            {
                if (m_messageInputField.text != "")
                {
                    GameObject newMessage = Instantiate(m_messagePrefab, m_messageHolder.transform);
                    string msg = FormatMessage();
                    newMessage.GetComponent<Text>().text = msg;
                    m_messageInputField.text = "";
                    GameClientSend.ClientSendChat(msg);
                }
                m_messageInputField.gameObject.SetActive(false);
            }

            m_displayPanel.GetComponent<Image>().enabled = m_messageInputField.gameObject.activeSelf;
        }

        if (m_displayPanel.activeSelf)
        {
            m_displayPanelActiveTime -= Time.deltaTime;
            if (m_displayPanelActiveTime < 0f)
            {
                m_displayPanel.SetActive(false);
            }
        }
    }

    public string FormatMessage()
    {
        return $"<b><color=cyan>{GameClient.s_instance.name}:</color></b> {m_messageInputField.text}";
    }

    public void AddMessage(string message)
    {
        m_displayPanel?.SetActive(true);
        m_displayPanelActiveTime = m_displayPanelMaxActiveTime;
        GameObject newMessage = Instantiate(m_messagePrefab, m_messageHolder.transform);
        newMessage.GetComponent<Text>().text = message;
    }
}
