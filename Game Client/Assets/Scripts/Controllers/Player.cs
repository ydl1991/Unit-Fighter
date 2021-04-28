using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public enum PlayerFlag
{
    kRed = 0,
    kYellow,
    kPurple,
    kOrange,
    kGreen,
    kBlue,
    kBrown,
    kBlack
}

public class Player : MonoBehaviour
{
    private static ReadOnlyCollection<Color32> s_kPlayerColor = new ReadOnlyCollection<Color32> (new Color32[] 
    {
        new Color32(255, 102, 102, 255),
        new Color32(247, 255,   0, 255),
        new Color32(147,   0, 205, 255),
        new Color32(255, 135,  18, 255),
        new Color32(104, 222,  91, 255),
        new Color32(  0,  65, 255, 255),
        new Color32(104,  70,  40, 255),
        new Color32(  0,   0,   0, 255),
    });

    private static ReadOnlyCollection<string> s_kPlayerDefaultName = new ReadOnlyCollection<string> (new string[]
    {
        "Red", "Yellow", "Purple", "Orange", "Green", "Blue", "Brown", "Black"
    });

    private static ReadOnlyCollection<int> s_kMaxExpByLevel = new ReadOnlyCollection<int> (new int[]
    {
        2, 4, 6, 10, 20, 36, 56, 80
    });

    private static ReadOnlyCollection<string> s_kCrystalIndexer = new ReadOnlyCollection<string> (new string[]
    {
        "Heal", "Shield", "Silence", "Sacred"
    });

    public int health { get; private set; }
    public int level { get; private set; }
    public int maxExp { get; private set; }
    public int exp { get; private set; }
    public int cash { get; private set; }
    public int clientId { get; private set; }
    public List<Card?> cards { get; private set; }
    public int[] numCrystals { get; private set; }
    public HalfBoard board { get; private set; }
    public PlayerFlag m_flagProp;
    public GameObject m_flag;
    public GameObject m_unitStorage;
    private Vector3 m_playerPos;
    private string m_defaultName;

    void Awake()
    {
        health = 100;
        level = 1;
        maxExp = s_kMaxExpByLevel[level - 1];
        exp = 0;
        board = GetComponentInChildren<HalfBoard>();
        cards = new List<Card?>() { null, null, null, null, null };
        numCrystals = new int[] { 0, 0, 0, 0 };
        m_defaultName = "Player " + s_kPlayerDefaultName[(int)m_flagProp];
        name = m_defaultName;
        m_playerPos = transform.position;

        InitFlagColor();
    }

    public void Init(int id, string playerName)
    {
        clientId = id;
        name = playerName;
        StatusBarManager.updateAllStatus();
    }
    
    public void TakeDamage(int damage)
    {
        if (health <= 0 || damage <= 0)
            return;
            
        health -= damage;
        DamagePopup.Create(m_flag.transform.position, new Damage(DamageType.kStrike, damage));
        StatusBarManager.updateAllStatus();

        if (health < 0)
        {
            PlayerDie();
        }
    }

    public void GainExp(int extraExp)
    {
        if (level >= 9)
            return;

        exp += extraExp;
        if (exp >= maxExp)
            LevelUp();
    }

    public void SyncExp(int newExp)
    {
        exp = newExp;
    }

    private void LevelUp()
    {
        ++level;
        exp -= maxExp;

        if (level < 9)
            maxExp = s_kMaxExpByLevel[level - 1];
        else
            exp = maxExp;
    }

    public void SyncLevel(int newLevel)
    {
        level = newLevel;
        
        if (level < 9)
            maxExp = s_kMaxExpByLevel[level - 1];
        else
            exp = maxExp;
    }

    public void GainCash(int gain)
    {
        cash += gain;
        board.UpdateCoins(cash);
    }

    public void SyncCash(int cash)
    {
        this.cash = cash;
        board.UpdateCoins(this.cash);
    }

    public bool SpendCash(int cost)
    {
        if (cost > cash)
            return false;
        
        cash -= cost;
        board.UpdateCoins(cash);
        return true;
    }

    public void SyncCards(List<Card> newCards)
    {
        for (int i = 0; i < newCards.Count; ++i)
        {
            cards[i] = newCards[i];
        }
    }

    public bool BuyExp()
    {
        if (SpendCash(4))
        {
            GainExp(4);
            return true;
        }

        return false;
    }

    public void BoughtCard(int index, Card card)
    {
        if (card.type == CardType.kUnit)
        {
            if (!board.CheckForSpawn())
                return; 

            Unit newUnit = SpawnUnit(card);
            if (CheckForUnitMatch(newUnit, out Unit[] matchUnits))
                UnitLevelUp(matchUnits);
        }
        else
        {
            SpawnCrystal(card);
        }

        cards[index] = null;
        return;
    }

    public Unit SpawnUnit(Card card)
    {
        UnitHolder unitHolder = board.NextAvailableUnitHolder();
        Unit newUnit = Instantiate(ResourceManager.s_instance.GetUnitPrefab(card.profAttr), unitHolder.transform.position, Quaternion.identity);
        newUnit.Init(CreateUnitInitializer(card));
        newUnit.transform.SetParent(m_unitStorage.transform);
        newUnit.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));

        board.PlaceUnitToBoard(unitHolder, newUnit);

        return newUnit;
    }

    public void SellUnit(Unit unit)
    {
        GainCash(unit.sellPrice);
        unit.DeleteUnit();
    }

    private UnitInitializer CreateUnitInitializer(Card card)
    {
        return new UnitInitializer(this, card.elemAttr, card.profAttr, card.deckId + 1);
    }

    public void SpawnCrystal(Card card)
    {
        int index = s_kCrystalIndexer.IndexOf(card.elemAttr);
        ++numCrystals[index];
    }

    public bool UseCrystal(int index, Unit affectedUnit)
    {
        if (affectedUnit == null || affectedUnit.OwnerFlag() != m_flagProp || numCrystals[index] <= 0)
            return false;

        // effect

        --numCrystals[index];
        return true;
    }

    public bool CheckForUnitMatch(Unit target, out Unit[] matchUnits)
    {
        Unit[] allUnits = GetComponentsInChildren<Unit>();
        matchUnits = Array.FindAll(allUnits, unit => unit.MatchingUnit(target));
        
        if (matchUnits.Length < 3)
            return false;
        
        return true;
    }

    public void UnitLevelUp(Unit[] units)
    {
        if (!units[0].LevelUp())
            return;
        
        units[1].DeleteUnit();
        units[2].DeleteUnit();

        if (CheckForUnitMatch(units[0], out Unit[] matchUnits))
            UnitLevelUp(matchUnits);
    }

    public Color32 PlayerColor()
    {
        return s_kPlayerColor[(int)m_flagProp];
    }

    public void SetPlayerName(string name)
    {
        this.name = name;
    }
    
    public string PlayerName()
    {
        return name;
    }

    public void MoveToPosInSecWithRot(Vector3 targetPos, Vector3 targetRot, float seconds)
    {
        StartCoroutine(GameUtil.MoveToPosInSec(transform, targetPos, seconds));
        StartCoroutine(GameUtil.RotateToTargetInSec(transform, targetRot, seconds));
    }

    public void ShowTransBoard(bool show)
    {
        board.ShowTransBoard(show);
    }

    public void ResetPositionAndRotation(float seconds)
    {
        MoveToPosInSecWithRot(m_playerPos, Vector3.zero, seconds);
    }

    public List<Unit> ActiveBattleUnits()
    {
        return board.ActiveBattleUnits();
    }

    public List<Unit> BattleUnits()
    {
        return board.BattleUnits();
    }

    public void ResetBattleUnits()
    {
        foreach (Unit unit in BattleUnits())
        {
            unit.gameObject.SetActive(true);
            unit.ResetUnit();
        }
    }

    public void InitFlagColor()
    {
        m_flag.GetComponentInChildren<SkinnedMeshRenderer>().materials[1].color = s_kPlayerColor[(int)m_flagProp];
    }

    public void PlayerDie()
    {
        health = 0;
        m_flag.SetActive(false);
        cash = 0;
        board.UpdateCoins(cash);

        board.DeleteAllUnits();
        cards.Clear();
        Array.Clear(numCrystals, 0, numCrystals.Length);
    }

    public void BattleWin()
    {
        foreach(Unit unit in ActiveBattleUnits())
        {
            unit.PlayWin();
        }
        GainCash(1);
    }
}
