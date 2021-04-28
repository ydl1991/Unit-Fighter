using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControllerType
{
    kPlayer = 0,
    kAI
}

public interface IController
{
    ControllerType type { get; set; }
    Player controllingPlayer { get; set; }
    HalfBoard controllingBoard { get; set; }

    void SetController(Player player);
    void BuyXP();
    void Refresh();
    void BuyCard(int index);
    void UseCrystal(int crystalIndex, Unit affectedUnit);
    void SellUnit(Unit unitToSell);
    Player ControllingPlayer();
}