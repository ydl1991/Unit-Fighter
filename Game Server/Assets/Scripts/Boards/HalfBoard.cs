using System;
using System.Collections.Generic;
using UnityEngine;

public class HalfBoard : MonoBehaviour
{
    public UnitHolder[] m_unitHoldersBattle;
    public UnitHolder[] m_unitHoldersIdle;
    public GameObject[] m_coins;
    public GameObject m_transBoard;

    void Awake()
    {
        InitUnitHolders();
    }

    void Start()
    {
        SetRenderUnitHolders(false);
    }

    private void InitUnitHolders()
    {
        for (int i = 0; i < m_unitHoldersBattle.Length; ++i)
        {
            m_unitHoldersBattle[i].InitHolder(UnitHolderType.kBattle, i);
        }

        for (int i = 0; i < m_unitHoldersIdle.Length; ++i)
        {
            m_unitHoldersIdle[i].InitHolder(UnitHolderType.kIdle, i);
        }
    }

    public UnitHolder LocateUnitHolder(UnitHolderType type, int holderIndex)
    {
        return type == UnitHolderType.kBattle ? m_unitHoldersBattle[holderIndex] : m_unitHoldersIdle[holderIndex];
    }

    public bool CheckForSpawn()
    {
        return NextAvailableUnitHolder() != null;
    }

    public UnitHolder NextAvailableUnitHolder()
    {
        for (int i = 0; i < m_unitHoldersIdle.Length; ++i)
        {
            if (m_unitHoldersIdle[i].holdingUnit == null)
                return m_unitHoldersIdle[i];
        }

        return null;
    }

    public void PlaceUnitToBoard(UnitHolder unitHolder, Unit unit)
    {
        unitHolder.SetHoldingUnit(unit);
        unit.SetUnitHolder(unitHolder);
        unit.transform.position = unitHolder.transform.position;
    }

    public void SetRenderUnitHolders(bool show)
    {
        if (show)
        {
            Array.ForEach(m_unitHoldersBattle, unitHolder => unitHolder.Show());
            Array.ForEach(m_unitHoldersIdle, unitHolder => unitHolder.Show());
        }
        else
        {
            Array.ForEach(m_unitHoldersBattle, unitHolder => unitHolder.Hide());
            Array.ForEach(m_unitHoldersIdle, unitHolder => unitHolder.Hide());
        }
    }

    public List<Unit> BattleUnits()
    {
        List<Unit> result = new List<Unit>();

        foreach (UnitHolder holder in m_unitHoldersBattle)
        {
            if (holder.holdingUnit != null)
                result.Add(holder.holdingUnit);
        }
        
        return result;
    }

    public int BattleUnitsSize()
    {
        int result = 0;

        foreach (UnitHolder holder in m_unitHoldersBattle)
        {
            if (holder.holdingUnit != null)
                ++result;
        }
        
        return result;
    }

    public List<Unit> ActiveBattleUnits()
    {
        List<Unit> result = new List<Unit>();

        foreach (UnitHolder holder in m_unitHoldersBattle)
        {
            if (holder.holdingUnit != null && holder.holdingUnit.gameObject.activeSelf)
                result.Add(holder.holdingUnit);
        }
        
        return result;
    }

    public int ActiveBattleUnitsSize()
    {
        int result = 0;

        foreach (UnitHolder holder in m_unitHoldersBattle)
        {
            if (holder.holdingUnit != null && holder.holdingUnit.gameObject.activeSelf)
                ++result;
        }
        
        return result;
    }

    public void UpdateCoins(int cash)
    {
        int numToShow = cash / 10;
        for (int i = 0; i < m_coins.Length; ++i)
        {
            if (i < numToShow)
                m_coins[i].SetActive(true);
            else
                m_coins[i].SetActive(false);
        }
    }

    public void ShowTransBoard(bool show)
    {
        m_transBoard.SetActive(show);
    }

    public void DeleteAllUnits()
    {
        foreach (UnitHolder holder in m_unitHoldersBattle)
        {
            if (holder.holdingUnit != null)
            {
                holder.holdingUnit.DeleteUnit();
            }
        }

        foreach (UnitHolder holder in m_unitHoldersIdle)
        {
            if (holder.holdingUnit != null)
            {
                holder.holdingUnit.DeleteUnit();
            }
        }
    }
}
