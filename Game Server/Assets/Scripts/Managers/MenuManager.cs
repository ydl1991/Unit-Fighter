using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager s_instance;

    public RawImage[] m_clientSlots;

    // Start is called before the first frame update
    void Start()
    {
        s_instance = this;
    }

    public void OccupySlot(int index, string username)
    {
        m_clientSlots[index].color = Color.yellow;
        m_clientSlots[index].GetComponentInChildren<Text>().text = username;
    }

    public void ReleaseSlot(int index)
    {
        m_clientSlots[index].color = Color.white;
        m_clientSlots[index].GetComponentInChildren<Text>().text = "Empty";
    }

    public void SlotReadyColor(int index, bool ready)
    {
        m_clientSlots[index].color = ready ? Color.green : Color.yellow;
    }
}
