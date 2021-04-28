using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IController
{
    public ControllerType type { get; set; }
    public Player controllingPlayer { get; set; }
    public HalfBoard controllingBoard { get; set; }

    private Unit m_draggingUnit;
    private float m_dragUnitY;
    private const float k_dragUnitYOffset = 0.5f;
    private UnitHolder m_mouseOverUnitHolder;

    private BottomBarManager m_bottomBarManager;
    private AttributeBarManager m_attrBarManager;

    // Update is called once per frame
    void Update()
    {
        DragAndDropUnit();
    }

    public void SetController(Player player)
    {
        type = ControllerType.kPlayer;

        if (player != null)
        {
            transform.SetParent(player.transform);
            controllingPlayer = player;
            controllingBoard = controllingPlayer.GetComponentInChildren<HalfBoard>();
        }
    
        InitController();
        UpdateInfoPanel();        
    }

    private void InitController()
    {
        if (type == ControllerType.kPlayer && controllingPlayer != null)
        {
            LoadUIManager();
            SetUpOperationalButtons();
        }
    }

    private void LoadUIManager()
    {
        m_bottomBarManager = GameObject.FindWithTag("Bottom Bar").GetComponent<BottomBarManager>();
        m_attrBarManager = GameObject.FindWithTag("Attribute Bar").GetComponent<AttributeBarManager>();
    }

    private void SetUpOperationalButtons()
    {
        m_bottomBarManager.SetBuyXPButtonCallback(delegate{ BuyXP(); });
        m_bottomBarManager.SetRefreshButtonCallback(delegate{ Refresh(); });
        m_bottomBarManager.SetBuyCardButtonCallback(0, delegate{ BuyCard(0); });
        m_bottomBarManager.SetBuyCardButtonCallback(1, delegate{ BuyCard(1); });
        m_bottomBarManager.SetBuyCardButtonCallback(2, delegate{ BuyCard(2); });
        m_bottomBarManager.SetBuyCardButtonCallback(3, delegate{ BuyCard(3); });
        m_bottomBarManager.SetBuyCardButtonCallback(4, delegate{ BuyCard(4); });
    }

    private void DragAndDropUnit()
    {
        if (Input.GetMouseButtonDown(0) && m_draggingUnit == null && MouseOnOwningAvailableUnit(out var unitToDrag))
        {
            DragUpUnit(unitToDrag);
        }
        else if (Input.GetMouseButtonUp(0) && m_draggingUnit != null)
        {
            DropUnit();
        }
        else if (m_draggingUnit != null)
        {
            MoveUnitWithMouse();
            CheckMouseOverSellUnitUI();
        }
    }

    private bool MouseOnOwningAvailableUnit(out Unit mouseOnUnit)
    {
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            Unit hitUnit = hit.collider.GetComponent<Unit>();
            if (DragCheck(hitUnit))
            {
                mouseOnUnit = hitUnit;
                return true;
            }
        }

        mouseOnUnit = null;
        return false;
    }

    private bool DragCheck(Unit hitUnit)
    {
        if (hitUnit != null && hitUnit.OwnerFlag() == controllingPlayer.m_flagProp &&
            (GameManager.s_instance.State() == "PrepareState" || hitUnit.UnitStatus() == UnitHolderType.kIdle))
            return true;

        return false;
    }

    private UnitHolder FindUnitHolderAtMousePosition()
    {
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        UnitHolder holder = null;

        if (Physics.Raycast(ray, out hit, 1000f, (1 << 7)))
            holder = hit.collider.GetComponent<UnitHolder>();

        return ValidUnitHolderCheck(holder) ? holder : null;
    }

    private bool ValidUnitHolderCheck(UnitHolder holder)
    {
        if (holder == null || (holder.type == UnitHolderType.kBattle && GameManager.s_instance.State() != "PrepareState"))
            return false;
        
        return true;
    }
    
    private void DragUpUnit(Unit unitToDrag)
    {
        m_draggingUnit = unitToDrag;
        m_dragUnitY = m_draggingUnit.transform.position.y + k_dragUnitYOffset;

        controllingBoard.SetRenderUnitHolders(true);
    }

    private void DropUnit()
    {
        if (m_bottomBarManager.MouseOverSellUnitBoard())
        {
            SellUnit(m_draggingUnit);
            m_bottomBarManager.ShowSellUnitBoard(false);
            UpdateInfoPanel();
        }
        else
        {
            MoveDraggingUnit();
        }

        UpdateAttributeInfo();
        controllingBoard.SetRenderUnitHolders(false);
        m_draggingUnit = null;
    }

    private void MoveUnitWithMouse()
    {
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 pos = m_draggingUnit.transform.position;

        if(Physics.Raycast(ray, out hit, 1000f, ((1 << 6) | (1 << 7))))
        {
            UnitHolder unitHolder = hit.collider.GetComponent<UnitHolder>();
            if (unitHolder != null && unitHolder != m_mouseOverUnitHolder)
            {
                m_mouseOverUnitHolder?.SetFreeMaterial();
                m_mouseOverUnitHolder = unitHolder;
                m_mouseOverUnitHolder?.SetOccupiedMaterial();
            }

            pos = hit.point;
            pos.y = m_dragUnitY;
        }

        m_draggingUnit.transform.position = pos;
    }

    private void CheckMouseOverSellUnitUI()
    {
        m_bottomBarManager.ShowSellUnitBoard(m_bottomBarManager.MouseOverSellUnitBoard() ? true : false);
    }

    public void UpdateInfoPanel()
    {
        if (type == ControllerType.kPlayer && controllingPlayer != null)
        {
            m_bottomBarManager.UpdateBottomBar(controllingPlayer);
        }
    }

    public void UpdateAttributeInfo()
    {
        if (type == ControllerType.kPlayer && controllingPlayer != null)
        {
            m_attrBarManager.UpdateAttributeBar(controllingPlayer.m_flagProp);
        }
    }

    public void BuyXP()
    {
        GameClientSend.ClientBuyExpRequest();
    }

    public void Refresh()
    {
        GameClientSend.ClientCardsRefreshRequest();
    }

    public void BuyCard(int index)
    {
        GameClientSend.ClientBuyCardRequest(index);
    }

    public void UseCrystal(int crystalIndex, Unit affectedUnit)
    {
        if (controllingPlayer.UseCrystal(crystalIndex, affectedUnit))
        {
            UpdateInfoPanel();
        }
    }

    public void SellUnit(Unit unitToSell)
    {
        GameClientSend.ClientSellUnit(unitToSell.unitHolder);
        controllingPlayer.SellUnit(unitToSell);
    }

    public void MoveDraggingUnit()
    {
        UnitHolder newHolder = FindUnitHolderAtMousePosition();
        int numBattleUnits = controllingBoard.BattleUnitsSize();
        
        GameClientSend.ClientMoveUnit(m_draggingUnit.unitHolder, newHolder, numBattleUnits);
        m_draggingUnit.ChangePosition(newHolder, numBattleUnits);
    }
    
    public Player ControllingPlayer()
    {
        return controllingPlayer;
    }
}
