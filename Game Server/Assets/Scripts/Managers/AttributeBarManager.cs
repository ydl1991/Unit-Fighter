using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeBarManager : MonoBehaviour
{
    public delegate void AttributeBarUpdate(PlayerFlag flag);
    public static AttributeBarUpdate updateAttributeBar;
    public delegate void SwitchOnAndOff(bool on);
    public static SwitchOnAndOff switchOnAndOff;

    public GameObject m_attrPanel;
    private RectTransform m_rect;
    private const float k_attrUIHeight = 60f; 

    void Awake()
    {
        m_rect = GetComponent<RectTransform>();
        updateAttributeBar = UpdateAttributeBar;
        switchOnAndOff = ShowSelf;
    }

    public void UpdateAttributeBar(PlayerFlag flag)
    {
        if (GameManager.s_instance.gameCam.currentFocus != flag)
            return;
            
        Attribute[] playerAttributes = AttributeManager.s_instance.GetValidAttributes(flag);
        AttributeUI[] currentAttrUIs = m_attrPanel.GetComponentsInChildren<AttributeUI>(true);
        int len = playerAttributes.Length >= currentAttrUIs.Length ? playerAttributes.Length : currentAttrUIs.Length;

        for (int i = 0; i < len; ++i)
        {
            if (i < currentAttrUIs.Length && i < playerAttributes.Length)
            {
                currentAttrUIs[i].gameObject.SetActive(true);
                currentAttrUIs[i].UpdateInfo(playerAttributes[i]);
            }
            else if (i >= currentAttrUIs.Length)
            {
                CreateNewAttributeUI(playerAttributes[i]);
            }
            else
            {
                currentAttrUIs[i].gameObject.SetActive(false);
            }
        }

        m_rect.sizeDelta = new Vector2(m_rect.sizeDelta.x, k_attrUIHeight * playerAttributes.Length);
    }

    private void CreateNewAttributeUI(Attribute attr)
    {
        AttributeUI newAttrUI = Instantiate(ResourceManager.s_instance.GetAttributeUI());
        newAttrUI.UpdateInfo(attr);
        newAttrUI.transform.SetParent(m_attrPanel.transform);
    }

    private void ShowSelf(bool on)
    {
        gameObject.SetActive(on);
    }
}
