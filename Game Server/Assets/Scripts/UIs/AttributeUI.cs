using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class AttributeUI : MonoBehaviour
{
    private static ReadOnlyCollection<Color32> s_kIconLevelColor = new ReadOnlyCollection<Color32>(new Color32[] {
        new Color32(255, 255, 255, 60),
        new Color32(123, 85, 50, 255),
        new Color32(192, 192, 192, 255),
        new Color32(255, 219, 0, 255),
    });

    public Image m_attributeIconFrame;
    public Material m_silverIconFrameMat;
    public Image m_attributeIcon;
    public Text m_attributeName;
    public Text m_attributeLevels;
    public Text m_currentFormation;

    public void UpdateInfo(Attribute attr)
    {
        AdjustIconColorByLevel(attr.level);
        m_attributeIcon.sprite = ResourceManager.s_instance.GetAttributeSprite(attr.attrName);
        m_attributeName.text = attr.attrName;
        m_currentFormation.text = attr.currentFormation.ToString();
        m_attributeLevels.text = AttributeManager.s_instance.GetAttributeLevelText(attr.attrName);
    }

    private void AdjustIconColorByLevel(AttributeLevel level)
    {
        m_attributeIconFrame.material = level != AttributeLevel.kSilver ? null : m_silverIconFrameMat;
        m_attributeIconFrame.color = s_kIconLevelColor[(int)level];
        float iconAlpha = level != AttributeLevel.kNone ? 1f : 0.24f;
        m_attributeIcon.color =  new Color(1, 1, 1, iconAlpha);
    }
}
