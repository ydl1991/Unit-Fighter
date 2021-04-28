using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    public Text m_playerHPText;
    public Text m_playerNameText;
    public Image m_playerIcon;
    public Button m_statusButton;
    public Slider m_healthSlider;
    private Player m_owner;

    void Start()
    {
        m_statusButton.onClick.AddListener(delegate{ Camera.main.GetComponent<GameCam>().SetCameraToPlayer(m_owner); });
    }

    public void Init(Player player)
    {
        m_owner = player;
        m_playerIcon.color = player.PlayerColor();
        m_playerHPText.text = player.health.ToString();
        m_healthSlider.value = player.health;
        m_playerNameText.text = player.name;
    }

    public void UpdateInfo()
    {
        m_playerNameText.text = m_owner.name;
        m_playerHPText.text = m_owner.health.ToString();
        m_healthSlider.value = m_owner.health;
        AdjustIconColor();
    }

    public int PlayerHP()
    {
        return m_owner.health;
    }

    private void AdjustIconColor()
    {
        if (m_owner.health <= 0)
            m_playerIcon.color = new Color32(183, 183, 183, 255);
    }
}
