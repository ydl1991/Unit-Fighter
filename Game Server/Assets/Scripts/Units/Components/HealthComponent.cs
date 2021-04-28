using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public int health { get; private set; }

    private int m_maxHealth;
    private int m_fadingHealth;
    public RectTransform m_healthBar;
    public RectTransform m_fadingHealthBar;
    public RectTransform m_innerShieldBar;
    public RectTransform m_outterShieldBar;
    private float m_originalBarWidth;

    void Awake()
    {
        m_originalBarWidth = m_healthBar.sizeDelta.x;
    }

    // Update is called once per frame
    void Update()
    {
        RotateToCamera();
        UpdateFadingHealthBar();
    }

    private void RotateToCamera()
    {
        transform.LookAt(Camera.main.transform);
        Vector3 camRot = Camera.main.transform.eulerAngles;
        Vector3 rotation = transform.rotation.eulerAngles;
        float newX = MathUtil.WrapAngle(rotation.x);
        rotation.x = camRot.y == 0 ? -1 * newX : newX;
        rotation.y = 0;
        rotation.z = 0;
        
        transform.rotation = Quaternion.Euler(rotation);
    }

    public void SetMaxHealth(int maxHealth)
    {
        health = maxHealth;
        m_maxHealth = maxHealth;
        m_fadingHealth = health;
        UpdateHealthBar();
        UpdateFadingHealthBar();
    }

    public void TakeDamage(int damage, List<Shield> shields)
    {
        FilterDamageFromShield(damage, shields, out int outDamage, out int leftShield);
        health = Mathf.Clamp(health - outDamage, 0, m_maxHealth);

        UpdateShieldBar(leftShield);
        UpdateHealthBar();
    }

    private void FilterDamageFromShield(int damage, List<Shield> shields, out int outDamage, out int leftShield)
    {
        leftShield = 0;
        
        for (int i = shields.Count - 1; i >= 0; --i)
        {
            shields[i].AbsortDamage(damage);
            damage = damage > shields[i].shieldAmount ? damage - shields[i].shieldAmount : 0;
            leftShield += shields[i].shieldAmount;
        }

        outDamage = damage;
    }

    private void UpdateShieldBar(int shieldAmount)
    {
        float innerShieldPercent = 0;
        float outterShieldPercent = 0;

        if (health < m_maxHealth)
        {
            float healthDiff = m_maxHealth - health;
            innerShieldPercent = shieldAmount < healthDiff ? (float)(health + shieldAmount) / (float)m_maxHealth : 1f;
            outterShieldPercent = shieldAmount < healthDiff ? 0f : (float)(shieldAmount - healthDiff) / (float)m_maxHealth;
        }
        else
        {
            outterShieldPercent = (float)shieldAmount / (float)m_maxHealth;
        }

        m_innerShieldBar.sizeDelta = new Vector2(m_originalBarWidth * innerShieldPercent, m_outterShieldBar.sizeDelta.y);
        m_outterShieldBar.sizeDelta = new Vector2(m_originalBarWidth * outterShieldPercent, m_outterShieldBar.sizeDelta.y);
    }

    private void UpdateHealthBar()
    {
        float percent = (float)health / (float)m_maxHealth;
        m_healthBar.sizeDelta = new Vector2(m_originalBarWidth * percent, m_healthBar.sizeDelta.y);
    }

    private void UpdateFadingHealthBar()
    {
        if (health == m_fadingHealth)
            return;

        m_fadingHealth = health >= m_fadingHealth ? health : m_fadingHealth - Mathf.FloorToInt(Time.deltaTime * 500f);

        float percent = (float)m_fadingHealth / (float)m_maxHealth;
        m_fadingHealthBar.sizeDelta = new Vector2(m_originalBarWidth * percent, m_fadingHealthBar.sizeDelta.y);
    }   

    public void Reset()
    {
        health = m_maxHealth;
        m_fadingHealth = health;
        UpdateHealthBar();
        UpdateFadingHealthBar();
    }
}
