using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield
{
    public int shieldAmount { get; protected set; }

    public Shield(int amount)
    {
        shieldAmount = amount;
    }

    public void AbsortDamage(int damage)
    {
        shieldAmount = damage >= shieldAmount ? 0 : shieldAmount - damage;
    }
}
