using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeaderBarManager : MonoBehaviour
{
    public Text m_roundText;
    public Text m_timeText;

    public void SetRoundText(int round)
    {
        m_roundText.text = "ROUND " + round.ToString();
    }

    public void SetTimeText(float time)
    {
        int flooredTime = Mathf.FloorToInt(time);
        m_timeText.text = flooredTime.ToString();
        NetworkServerSend.SyncTimer(flooredTime);
    }
}
