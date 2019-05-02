using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SlowTrap : Trap
{
    public override void ApplyEffect(Player player, GameObject playerGO)
    {
        player.SetSlow(description.slowFactor, description.slowDuration);
        base.ApplyEffect(player, playerGO);
    }
}