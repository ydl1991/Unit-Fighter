using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public struct AttrLevelIndicator
{
    public int[] formationCount;
    public AttributeLevel[] matchingLevels;
}

public class AttributeManager : MonoBehaviour
{
    public static AttributeManager s_instance;

    private Attribute[][] m_playerAttributeCombos;

    void Awake()
    {
        s_instance = this;
        InitPlayerAttributes();
    }

    private void InitPlayerAttributes()
    {
        m_playerAttributeCombos = new Attribute[8][];

        for (int i = 0; i < m_playerAttributeCombos.Length; ++i)
        {
            m_playerAttributeCombos[i] = new Attribute[] 
            {
                new Attribute("Fighter"),
                new Attribute("Assassin"),
                new Attribute("Guardian"),
                new Attribute("Gunner"),
                new Attribute("Fire"),
                new Attribute("Metal"),
                new Attribute("River"),
                new Attribute("Earth"),
                new Attribute("Ice"),
                new Attribute("Wind"),
                new Attribute("Forest"),
                new Attribute("Thunder"),
                new Attribute("Sacred")
            };
        }
    }

    public Attribute GetAttribute(PlayerFlag flag, string attrName)
    {
        return Array.Find(m_playerAttributeCombos[(int)flag], attr => attr.attrName == attrName);
    }

    public Attribute[] GetValidAttributes(PlayerFlag flag)
    {
        Attribute[] attributes = Array.FindAll(m_playerAttributeCombos[(int)flag], attr => attr.currentFormation > 0);
        Array.Sort(attributes, delegate(Attribute attr1, Attribute attr2) {
            return attr2.currentFormation.CompareTo(attr1.currentFormation);
        });
        
        return attributes;
    }

    public AttributeLevel CheckAttributeLevel(string attrName, int currentFormation)
    {
        AttrLevelIndicator attrIndicator = s_kAttrLevelMap[attrName];

        for (int i = attrIndicator.matchingLevels.Length - 1; i >= 0; --i)
        {
            if (currentFormation >= attrIndicator.formationCount[i])
            {
                return attrIndicator.matchingLevels[i];
            }
        }

        return AttributeLevel.kNone;
    }

    public string GetAttributeLevelText(string attrName)
    {
        StringBuilder levelText = new StringBuilder();
        int[] levelFormation = s_kAttrLevelMap[attrName].formationCount;
        for (int i = 0; i < levelFormation.Length; ++i)
        {
            levelText.Append(levelFormation[i]);

            if (i < levelFormation.Length - 1)
                levelText.Append("  >  ");
        }

        return levelText.ToString();
    }

    private static ReadOnlyDictionary<string, AttrLevelIndicator> s_kAttrLevelMap = new ReadOnlyDictionary<string, AttrLevelIndicator> (new Dictionary<string, AttrLevelIndicator> 
    {
        {
            "Fighter",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 2, 4, 6 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kBronze, AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },

        {
            "Assassin",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 2, 4, 6 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kBronze, AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },

        {
            "Guardian",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 2, 4, 6 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kBronze, AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },

        {
            "Gunner",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 2, 4, 6 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kBronze, AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },

        {
            "Fire",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 3, 6, 9 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kBronze, AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },

        {
            "Metal",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 2, 4, 6 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kBronze, AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },

        {
            "River",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 2, 4, 6 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kBronze, AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },

        {
            "Earth",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 2, 4, 6 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kBronze, AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },

        {
            "Ice",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 3, 6, 9 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kBronze, AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },

        {
            "Wind",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 3, 6, 9 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kBronze, AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },

        {
            "Forest",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 3, 6, 9 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kBronze, AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },

        {
            "Thunder",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 3, 6 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },

        {
            "Sacred",
            new AttrLevelIndicator 
            {
                formationCount = new int[] { 1, 2 },
                matchingLevels = new AttributeLevel[] { AttributeLevel.kSilver, AttributeLevel.kGold }
            }
        },
    });
}
