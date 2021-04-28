using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UnitHolderType
{
    kIdle = 0,
    kBattle
}

public class UnitHolder : MonoBehaviour
{
    public Unit holdingUnit { get; private set; }
    public UnitHolderType type { get; private set; }
    public int index { get; private set; }

    private MeshRenderer m_renderer;
    private Material m_materialFree;
    private Material m_materialOccupied;
    private bool m_show;

    void Awake()
    {
        holdingUnit = null;
        m_show = false;
        m_renderer = GetComponent<MeshRenderer>();
        m_materialFree = Resources.Load<Material>("Unit Placement (free)");
        m_materialOccupied = Resources.Load<Material>("Unit Placement (occupied)");
    }

    void Update()
    {
        RenderUnitHolder();
    }

    public void Hide()
    {
        m_show = false;
    }

    public void Show()
    {
        m_show = true;
    }

    public void InitHolder(UnitHolderType type, int index)
    {
        this.type = type;
        this.index = index;
    }

    public void SetHoldingUnit(Unit unit)
    {
        holdingUnit = unit;
    }

    public void SetOccupiedMaterial()
    {
        m_renderer.material = m_materialOccupied;
    }

    public void SetFreeMaterial()
    {
        m_renderer.material = m_materialFree;
    }

    private void RenderUnitHolder()
    {
        if (m_show)
            m_renderer.enabled = RenderCheck();
        else
            m_renderer.enabled = false;
    }

    private bool RenderCheck()
    {
        if (type == UnitHolderType.kIdle || GameManager.s_instance.State() == "PrepareState")
            return true;

        return false;
    }
}
