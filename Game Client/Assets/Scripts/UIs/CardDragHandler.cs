using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public int m_cardIndex;
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
        m_playerControl.BuyCard(m_cardIndex);
    }
}
