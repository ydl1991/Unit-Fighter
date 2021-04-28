using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour, IController
{
    public ControllerType type { get; set; }
    public Player controllingPlayer { get; set; }
    public HalfBoard controllingBoard { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetController(Player player)
    {
        type = ControllerType.kAI;

        if (player != null)
        {
            transform.SetParent(player.transform);
            controllingPlayer = player;
            controllingBoard = controllingPlayer.GetComponentInChildren<HalfBoard>();
        }
    }

    public void BuyXP()
    {
        controllingPlayer.BuyExp();
    }

    public void Refresh()
    {
        controllingPlayer.RefreshCards();
    }

    public void BuyCard(int index)
    {
        controllingPlayer.BuyCard(index, out Card? boughtCard);
    }

    public void UseCrystal(int crystalIndex, Unit affectedUnit)
    {
        controllingPlayer.UseCrystal(crystalIndex, affectedUnit);
    }

    public void SellUnit(Unit unitToSell)
    {
        controllingPlayer.SellUnit(unitToSell);
    }

    public Player ControllingPlayer()
    {
        return controllingPlayer;
    }
}
