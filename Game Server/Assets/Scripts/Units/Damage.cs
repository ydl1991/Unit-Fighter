using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    kNormal,
    kStrike
}

public class Damage
{
    public DamageType type { get; private set; }
    public int amount { get; private set; }

    public Damage(DamageType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }

    public void SetDamageType(DamageType type)
    {
        this.type = type;
    }

    public void SetDamageAmount(int amount)
    {
        this.amount = amount;
    }
}
