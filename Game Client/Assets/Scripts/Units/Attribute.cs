using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttributeLevel
{
    kNone = 0,
    kBronze,
    kSilver,
    kGold
}

public class Attribute
{
    public string attrName { get; private set; }
    public int currentFormation { get; private set; }
    public AttributeLevel level { get; private set; }

    private UnityEngine.Events.UnityAction[] m_battleEffectCallback;

    public Attribute(string name)
    {
        attrName = name;
        currentFormation = 0;
        level = AttributeLevel.kNone;
    }
    
    public void SetEffectCallback(int index, UnityEngine.Events.UnityAction callback)
    {
        m_battleEffectCallback[index] = callback;
    }

    public void Increment()
    {
        ++currentFormation;
        UpdateAttributeLevel();
    }

    public void Decrement()
    {
        --currentFormation;
        UpdateAttributeLevel();
    }

    private void UpdateAttributeLevel()
    {
        level = AttributeManager.s_instance.CheckAttributeLevel(attrName, currentFormation);
    }
}
