using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public enum UnitLevel
{
    kBronze,
    kSilver,
    kGold
}

public struct UnitInitializer
{
    public Player owner { get; private set; }
    public string elemAttr { get; private set; }
    public string profAttr { get; private set; }
    public int cost { get; private set; }

    public UnitInitializer(Player owner, string elemAttr, string profAttr, int cost)
    {
        this.owner = owner;
        this.elemAttr = elemAttr;
        this.profAttr = profAttr;
        this.cost = cost;
    }
}

public class Unit : MonoBehaviour
{
    private static ReadOnlyDictionary<string, Color> s_kElemColors = new ReadOnlyDictionary<string, Color> (new Dictionary<string, Color>
    {
        { "Metal", new Color32(255, 255, 255, 255) },
        { "River", new Color32(0, 35, 222, 255) },
        { "Earth", new Color32(75, 41, 28, 255) },
        { "Fire", new Color32(255, 87, 0, 255) },
        { "Ice", new Color32(83, 203, 241, 255) },
        { "Wind", new Color32(70, 236, 139, 255) },
        { "Forest", new Color32(69, 255, 0, 255) },
        { "Thunder", new Color32(129, 0, 255, 179) },
        { "Sacred", new Color32(253, 255, 86, 117) },
    });

    private static ReadOnlyDictionary<string, int[]> s_kInitialMaxHealthByProf = new ReadOnlyDictionary<string, int[]>(new Dictionary<string, int[]> 
    {
        { "Guardian", new int[] { 760, 1330, 3600 } },
        { "Fighter", new int[] { 650, 1180, 2850 } },
        { "Assassin", new int[] { 590, 960, 1720 } },
        { "Gunner", new int[] { 530, 890, 1580 } }
    });

    public Player owner { get; private set; }
    public UnitHolder unitHolder { get; private set; }
    public UnitLevel unitLevel { get; private set; }
    public int sellPrice { get; private set; }
    
    private HealthComponent m_unitHP;
    private Attribute m_elemAttr;
    private Attribute m_profAttr;

    private LevelStar[] m_levelStars;
    private int m_unitInitialCost;
    private Animator m_anim;
    private XOrShiftRNG m_rng;

    void Awake()
    {
        unitLevel = UnitLevel.kBronze;
        m_rng = new XOrShiftRNG();
        m_levelStars = GetComponentsInChildren<LevelStar>();
        Array.ForEach(m_levelStars, start => start.SetOwner(this));
        m_unitHP = GetComponentInChildren<HealthComponent>();
        m_anim = GetComponent<Animator>();
    }

    void Start()
    {
        m_unitHP.gameObject.SetActive(false);
        UpdateShowingStars();
    }

    void Update()
    {
        if (m_unitHP.health <= 0)
            return;
    }

    public void Init(UnitInitializer initializer)
    {
        owner = initializer.owner;
        m_elemAttr = AttributeManager.s_instance.GetAttribute(owner.m_flagProp, initializer.elemAttr);
        m_profAttr = AttributeManager.s_instance.GetAttribute(owner.m_flagProp, initializer.profAttr);
        m_unitHP.SetMaxHealth(s_kInitialMaxHealthByProf[initializer.profAttr][(int)unitLevel]);
        m_unitInitialCost = initializer.cost;
        UpdateSellPrice();
        InitElementalParticle(initializer.elemAttr);
    }

    private void InitElementalParticle(string element)
    {
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var particle in particles)
        {
            var main = particle.main;
            main.startColor = s_kElemColors[element];
        }
    }

    public void SetUnitHolder(UnitHolder holder)
    {
        unitHolder = holder;
    }

    public bool ChangePosition(UnitHolder newHolder, int numBattleUnits)
    {
        if (newHolder == null)
        {
            transform.position = unitHolder.transform.position;
            return false;
        }
        else
        {
            return newHolder.holdingUnit != null ? ChangeToOccupiedHolder(newHolder) : ChangeToEmptyHolder(newHolder, numBattleUnits);
        }
    }

    public bool ChangeToOccupiedHolder(UnitHolder newHolder)
    {
        newHolder.holdingUnit.AdjustAttributes(newHolder, unitHolder);
        AdjustAttributes(unitHolder, newHolder);

        unitHolder.SetHoldingUnit(newHolder.holdingUnit);
        newHolder.holdingUnit.unitHolder = unitHolder;
        newHolder.holdingUnit.transform.position = unitHolder.transform.position;
        
        newHolder.SetHoldingUnit(this);
        unitHolder = newHolder;
        transform.position = newHolder.transform.position;

        ShowHealthBarForBattleUnit();
        return true;
    }

    public bool ChangeToEmptyHolder(UnitHolder newHolder, int numBattleUnits)
    {
        // if (unitHolder.type == UnitHolderType.kIdle && unitHolder.type != newHolder.type && numBattleUnits >= owner.level)
        // {
        //     transform.position = unitHolder.transform.position;
        //     return false;
        // }
        
        AdjustAttributes(unitHolder, newHolder);
        unitHolder.SetHoldingUnit(null);
        newHolder.SetHoldingUnit(this);
        unitHolder = newHolder;
        transform.position = newHolder.transform.position;

        ShowHealthBarForBattleUnit();
        return true;
    }

    public void AdjustAttributes(UnitHolder oldHolder, UnitHolder newHolder)
    {
        if (oldHolder.type == newHolder.type)
        {
            return;
        }
        else if (oldHolder.type == UnitHolderType.kIdle)
        {
            m_elemAttr.Increment();
            m_profAttr.Increment();
        }
        else
        {
            m_elemAttr.Decrement();
            m_profAttr.Decrement();
        }
    }

    private void ShowHealthBarForBattleUnit()
    {
        if (unitHolder.type == UnitHolderType.kBattle)
            m_unitHP.gameObject.SetActive(true);
        else
            m_unitHP.gameObject.SetActive(false);
    }

    public PlayerFlag OwnerFlag()
    {
        return owner.m_flagProp;
    }

    public void UpdateShowingStars()
    {
        for (int i = 0; i < m_levelStars.Length; ++i)
        {
            if (i <= (int)unitLevel)
                m_levelStars[i].gameObject.SetActive(true);
            else
                m_levelStars[i].gameObject.SetActive(false);
        }
    }

    public bool LevelUp()
    {
        if (unitLevel >= UnitLevel.kGold)
            return false;
        
        unitLevel += 1;
        m_unitHP.SetMaxHealth(s_kInitialMaxHealthByProf[m_profAttr.attrName][(int)unitLevel]);
        
        UpdateSellPrice();
        UpdateShowingStars();
        return true;
    }

    public bool MatchingUnit(string elemAttr, string profAttr, UnitLevel level)
    {
        return m_elemAttr.attrName == elemAttr && m_profAttr.attrName == profAttr && unitLevel == level;
    }

    public bool MatchingUnit(Unit unit)
    {
        return m_elemAttr.attrName == unit.m_elemAttr.attrName &&
               m_profAttr.attrName == unit.m_profAttr.attrName &&
               unitLevel == unit.unitLevel;
    }

    public void DeleteUnit()
    {
        if (unitHolder.type == UnitHolderType.kBattle)
        {
            m_elemAttr.Decrement();
            m_profAttr.Decrement();
        }
        
        unitHolder.SetHoldingUnit(null);
        GameManager.s_instance.deck.RecycleUnit(this);
        Destroy(gameObject);
    }

    private void UpdateSellPrice()
    {
        if (m_unitInitialCost == 1)
            sellPrice = m_unitInitialCost * (int)Mathf.Pow(3, (int)unitLevel);
        else
            sellPrice = Mathf.CeilToInt(0.6f * ((float)m_unitInitialCost * Mathf.Pow(3, (int)unitLevel))); 
    }

    public UnitHolderType UnitStatus()
    {
        return unitHolder.type;
    }

    public void PlayAttack()
    {
        m_anim.SetBool("Running", false);
        m_anim.SetTrigger("Attack" + m_rng.RandomIntRange(1, 3).ToString());
    }

    public void PlayRun()
    {
        m_anim.SetBool("Running", true);
    }

    public void PlayIdle()
    {
        m_anim.SetBool("Running", false);
        m_anim.SetBool("Winning", false);
    }

    public void PlayDie()
    {
        m_anim.SetTrigger("Die");
    }

    public void PlayWin()
    {
        m_anim.SetBool("Winning", true);
    }

    public void ResetUnit()
    {
        m_unitHP.Reset();
        ResetPositionAndRotation();
        PlayIdle();
    }

    private void ResetPositionAndRotation()
    {
        transform.position = unitHolder.transform.position;
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }

    public void TakeDamage(Damage damage)
    {
        if (m_unitHP.health <= 0)
            return;

        m_unitHP.TakeDamage(damage.amount, new List<Shield>());
        DamagePopup.Create(transform.position, damage);

        if (m_unitHP.health <= 0)
        {
            PlayDie();
        }
    }

    public void DeactivateUnit()
    {
        gameObject.SetActive(false);
    }

    public void EnterBattle()
    {
        GetComponent<BattleAI>().enabled = true;
    }

    public void ExitBattle()
    {
        GetComponent<BattleAI>().enabled = false;
    }

    public int BelongToDeck()
    {
        return m_unitInitialCost - 1;
    }

    public Card ToCard()
    {
        return new Card(m_elemAttr.attrName, m_profAttr.attrName, m_unitInitialCost - 1, CardType.kUnit);
    }
}
