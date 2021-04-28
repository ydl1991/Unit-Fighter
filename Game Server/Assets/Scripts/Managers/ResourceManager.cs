using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager s_instance;

    private Dictionary<string, Sprite> m_cardSprites;
    private Dictionary<string, Sprite> m_attrSprites;
    private Dictionary<string, Unit> m_unitPrefabs;
    private DamagePopup m_popupUIPrefab;
    private AttributeUI m_attrUIPrefab;
    public GameObject m_popupUIHolder;

    void Awake()
    {
        s_instance = this;
        LoadSprites();
        LoadPrefabs();
    }

    private void LoadSprites()
    {
        LoadCardSprites();
        LoadAttributeSprites();
    }

    private void LoadCardSprites()
    {
        m_cardSprites = new Dictionary<string, Sprite>() 
        {
            { "Fighter", Resources.Load<Sprite>("Units/Fighter") },
            { "Assassin", Resources.Load<Sprite>("Units/Assassin") },
            { "Guardian", Resources.Load<Sprite>("Units/Guardian") },
            { "Gunner", Resources.Load<Sprite>("Units/Gunner") }, 
            { "Heal Crystal", Resources.Load<Sprite>("Crystals/Heal Crystal Card") }, 
            { "Shield Crystal", Resources.Load<Sprite>("Crystals/Shield Crystal Card") }, 
            { "Silence Crystal", Resources.Load<Sprite>("Crystals/Silence Crystal Card") }, 
            { "Sacred Crystal", Resources.Load<Sprite>("Crystals/Sacred Crystal Card") }
        };
    }

    private void LoadAttributeSprites()
    {
        m_attrSprites = new Dictionary<string, Sprite>()
        {
            { "Fire", Resources.Load<Sprite>("Elemental Attributes/Fire") },
            { "Metal", Resources.Load<Sprite>("Elemental Attributes/Metal") },
            { "River", Resources.Load<Sprite>("Elemental Attributes/River") },
            { "Earth", Resources.Load<Sprite>("Elemental Attributes/Earth") },
            { "Ice", Resources.Load<Sprite>("Elemental Attributes/Ice") },
            { "Wind", Resources.Load<Sprite>("Elemental Attributes/Wind") },
            { "Forest", Resources.Load<Sprite>("Elemental Attributes/Forest") },
            { "Thunder", Resources.Load<Sprite>("Elemental Attributes/Thunder") },
            { "Heal", Resources.Load<Sprite>("Elemental Attributes/Heal") },
            { "Shield", Resources.Load<Sprite>("Elemental Attributes/Shield") },
            { "Silence", Resources.Load<Sprite>("Elemental Attributes/Silence") },
            { "Sacred", Resources.Load<Sprite>("Elemental Attributes/Sacred") },
            { "Fighter", Resources.Load<Sprite>("Professional Attributes/Fighter") },
            { "Assassin", Resources.Load<Sprite>("Professional Attributes/Assassin") },
            { "Guardian", Resources.Load<Sprite>("Professional Attributes/Guardian") },
            { "Gunner", Resources.Load<Sprite>("Professional Attributes/Gunner") }, 
            { "Crystal", Resources.Load<Sprite>("Professional Attributes/Crystal") }
        };
    }

    public Sprite GetAttributeSprite(string key)
    {
        return m_attrSprites[key];
    }

    public Sprite GetCardSprite(string key)
    {
        return m_cardSprites[key];
    }

    private void LoadPrefabs()
    {
        LoadUnitPrefabs();
        LoadUIPrefabs();
    }

    private void LoadUnitPrefabs()
    {
        m_unitPrefabs = new Dictionary<string, Unit>() 
        {
            { "Fighter", Resources.Load<Unit>("Game/Units/Fighter") },
            { "Assassin", Resources.Load<Unit>("Game/Units/Assassin") },
            { "Guardian", Resources.Load<Unit>("Game/Units/Guardian") },
            { "Gunner", Resources.Load<Unit>("Game/Units/Gunner") }            
        };
    }

    private void LoadUIPrefabs()
    {
        m_popupUIPrefab = Resources.Load<DamagePopup>("UI/Damage Popup");
        m_attrUIPrefab = Resources.Load<AttributeUI>("UI/Attribute UI");
    }

    public Unit GetUnitPrefab(string key)
    {
        return m_unitPrefabs[key];
    }

    public DamagePopup GetDamagePopup()
    {
        return m_popupUIPrefab;
    }

    public AttributeUI GetAttributeUI()
    {
        return m_attrUIPrefab;
    }
}
