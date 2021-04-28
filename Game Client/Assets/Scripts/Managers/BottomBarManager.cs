using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomBarManager : MonoBehaviour
{
    public delegate void SwitchOnAndOff(bool on);
    public static SwitchOnAndOff switchOnAndOff;

    public Text m_levelText;
    public Text m_expText;
    public Text m_cashText;
    public Text[] m_dropRateTexts;
    public Text[] m_crystalNumberTexts;
    public Button m_buyXPButton;
    public Button m_refreshButton;
    public CardButton[] m_unitCards;
    public RectTransform m_sellUnitBoardTrans;

    void Awake()
    {
        switchOnAndOff = ShowSelf;
    }

    public void UpdateBottomBar(Player playerInfo)
    {
        SetLevelText(playerInfo.level);
        SetDropRates(playerInfo.level);
        SetExpText(playerInfo.exp, playerInfo.maxExp);
        SetCashText(playerInfo.cash);
        SetAllCardInfo(playerInfo.cards);
        SetCrystalItemNumber(playerInfo.numCrystals);
    }

    public void SetLevelText(int level)
    {
        m_levelText.text = "Lvl. " + level.ToString();
    }

    public void SetExpText(int currentExp, int maxExp)
    {
        m_expText.text = "Exp. " + currentExp.ToString() + " / " + maxExp.ToString();
    }

    public void SetCashText(int cash)
    {
        m_cashText.text = cash.ToString();
    }

    public void SetDropRates(int playerLevel)
    {
        List<float> dropRates = CardDeck.DropRates(playerLevel);

        for (int i = 0; i < dropRates.Count; ++i)
        {
            m_dropRateTexts[i].text = " • " + Mathf.FloorToInt(dropRates[i] * 100f).ToString() + "%";
        }
    }

    public void SetAllCardInfo(List<Card?> cards)
    {
        for (int i = 0; i < m_unitCards.Length; ++i)
        {
            m_unitCards[i].SetCardInfo(cards[i]);
        }
    }

    public void SetCrystalItemNumber(int[] crystalNumber)
    {
        for (int i = 0; i < m_crystalNumberTexts.Length; ++i)
        {
            m_crystalNumberTexts[i].text = crystalNumber[i].ToString();
        }
    }

    public void SetBuyXPButtonCallback(UnityEngine.Events.UnityAction callback)
    {
        m_buyXPButton.onClick.AddListener(callback);
    }

    public void SetRefreshButtonCallback(UnityEngine.Events.UnityAction callback)
    {
        m_refreshButton.onClick.AddListener(callback);
    }

    public void SetBuyCardButtonCallback(int index, UnityEngine.Events.UnityAction callback)
    {
        m_unitCards[index].GetComponent<Button>().onClick.AddListener(callback);
    }

    public bool MouseOverSellUnitBoard()
    {
        Vector2 localMousePosition = m_sellUnitBoardTrans.InverseTransformPoint(Input.mousePosition);
        if (m_sellUnitBoardTrans.rect.Contains(localMousePosition))
            return true;

        return false;
    }

    public void ShowSellUnitBoard(bool show)
    {
        m_sellUnitBoardTrans.gameObject.SetActive(show);
    }

    private void ShowSelf(bool on)
    {
        gameObject.SetActive(on);
    }
}
