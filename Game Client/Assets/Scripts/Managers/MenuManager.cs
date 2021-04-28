using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager s_instance;

    public GameObject m_connectMenu;
    public InputField m_usernameField;
    public InputField m_serverIpField;
    public GameObject m_entranceMenu;
    public GameObject m_loadingPanel;
    public GameObject m_chatSystem;

    public Image[] m_clientSlots;
    public Sprite[] m_statusSprites;

    private bool m_connecting;
    private float m_connectWaitTime;
    
    void Awake()
    {
        s_instance = this;
        m_connecting = false;
        m_connectWaitTime = NetworkConstants.k_connectWaitTime;
    }

    void Update()
    {
        if (m_connecting)
        {
            m_connectWaitTime -= Time.deltaTime;
            if (m_connectWaitTime < 0f)
            {
                Debug.Log("Connection failed! Try again!");
                m_connecting = false;
                GameClient.s_instance.Disconnect();
                m_loadingPanel.SetActive(false);
                m_connectMenu.SetActive(true);
            }
        }
    }

    public void ConnectToServer()
    {
        if (m_usernameField.text != "")
            GameClient.s_instance.name = m_usernameField.text;
        
        if (m_serverIpField.text != "")
            GameClient.s_instance.m_serverIp = m_serverIpField.text;

        m_connectMenu.SetActive(false);
        m_loadingPanel.SetActive(true);

        GameClient.s_instance.ConnectToServer();
        m_connecting = true;
        m_connectWaitTime = NetworkConstants.k_connectWaitTime;
    }

    public void EnterGameRoom()
    {
        m_connecting = false;
        m_loadingPanel.SetActive(false);
        m_entranceMenu.SetActive(true);
        m_chatSystem.SetActive(true);
    }

    public void ClickOnSlot(int index)
    {
        GameClientSend.ClientSlotChangeRequest(index);
    }

    public void ClickReadyButton()
    {
        GameClientSend.ClientReadyButtonClick();
    }

    public void OccupySlot(int index, string username)
    {
        m_clientSlots[index].sprite = m_statusSprites[1];
        m_clientSlots[index].GetComponentInChildren<Text>().text = username;
    }

    public void ReleaseSlot(int index)
    {
        if (index < 0 || index >= m_clientSlots.Length)
            return;

        m_clientSlots[index].sprite = m_statusSprites[0];
        m_clientSlots[index].GetComponentInChildren<Text>().text = "Empty";
    }

    public void SlotReadyColor(int index, bool ready)
    {
        m_clientSlots[index].sprite = ready ? m_statusSprites[2] : m_statusSprites[1];
    }
}
