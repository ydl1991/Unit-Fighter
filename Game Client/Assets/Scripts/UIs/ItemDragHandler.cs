using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public int m_itemIndex;
    private PlayerController m_playerControl;

    void Start()
    {
        m_playerControl = GameManager.s_instance.GetPlayerController();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.localPosition = Vector3.zero;
        m_playerControl.UseCrystal(m_itemIndex, FindHitUnit());
    }

    private Unit FindHitUnit()
    {
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 1000f, (1 << 8)))
        {
            Unit unit = hit.collider.GetComponent<Unit>();
            if (unit != null)
                return unit;
        }

        return null;
    }
}
