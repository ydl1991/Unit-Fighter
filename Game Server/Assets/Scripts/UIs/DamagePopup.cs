using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public static void Create(Vector3 worldPos, Damage damage)
    {
        DamagePopup damagePopup = Instantiate(ResourceManager.s_instance.GetDamagePopup());
        damagePopup.SetDamageText(damage);
        damagePopup.transform.SetParent(ResourceManager.s_instance.m_popupUIHolder.transform);

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        screenPos.y += 50f;
        damagePopup.transform.position = screenPos;
    }

    private static ReadOnlyCollection<Color32> s_kDamageColor = new ReadOnlyCollection<Color32> (new Color32[] 
    {
        new Color32(255, 208, 119, 255),
        new Color32(245, 128, 106, 255)
    });

    private static ReadOnlyCollection<int> s_kDamageFont = new ReadOnlyCollection<int> (new int[] 
    {
        15, 20
    });

    private TextMeshProUGUI m_textMesh;
    private Color32[] m_damageColor;

    // Start is called before the first frame update
    void Awake()
    {
        m_textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    public void SetDamageText(Damage damage)
    {
        m_textMesh.SetText(damage.amount.ToString());
        m_textMesh.color = s_kDamageColor[(int)damage.type];
        m_textMesh.fontSize = s_kDamageFont[(int)damage.type];
    }

    public void SelfDestruction()
    {
        Destroy(gameObject);
    }
}
