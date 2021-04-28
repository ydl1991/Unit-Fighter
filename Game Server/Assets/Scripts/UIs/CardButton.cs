using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardButton : MonoBehaviour
{
    public Image m_cardColor;
    public Image m_cardImage;
    public Image m_elemAttrIcon;
    public Image m_profAttrIcon;
    public Text m_elemAttrText;
    public Text m_profAttrText;
    public Text m_cardName;
    public Text m_cardCost;

    public void SetCardInfo(Card? card)
    {
        if (card.HasValue)
        {
            m_cardColor.gameObject.SetActive(true);
            m_cardColor.color = card.Value.color;
            m_elemAttrIcon.sprite = ResourceManager.s_instance.GetAttributeSprite(card.Value.elemAttr);
            m_profAttrIcon.sprite = ResourceManager.s_instance.GetAttributeSprite(card.Value.profAttr);
            m_elemAttrText.text = card.Value.elemAttr;
            m_profAttrText.text = card.Value.profAttr;
            m_cardName.text = m_elemAttrText.text + " " + m_profAttrText.text;
            m_cardCost.text = (card.Value.deckId + 1).ToString();

            if (card.Value.type == CardType.kUnit)
                m_cardImage.sprite = ResourceManager.s_instance.GetCardSprite(card.Value.profAttr);
            else
                m_cardImage.sprite = ResourceManager.s_instance.GetCardSprite(m_cardName.text);
        }
        else
        {
            m_cardColor.gameObject.SetActive(false);
        }
    }
    
}
