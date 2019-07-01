using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SacrificeStatue : InteractiveObject
{
    public int tier;
    private bool _active;

    private void Awake()
    {
        _active = true;
    }

    public override void OnActivate()
    {
        if (!_active)
        {
            return;
        }

        _active = false;

        if (tier > MapManager.Instance.selectedDungeonData.bossHealthCosts.Count)
        {
            Debug.Log("Missing tier " + tier + " for boss cost");
            return;
        }

        Main.Instance.player.MaxHealth -= MapManager.Instance.selectedDungeonData.bossHealthCosts[tier];
        Main.Instance.PayBossTribute(tier);
        Main.Instance.sessionData.UpdatePlayerData(Main.Instance.player);

        base.OnActivate();
    }
}
