using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MaxHealthPickup : Item
{
    public int maxHealth;

    public override void ApplyEffect(GameObject owner)
    {
        owner.GetComponent<Player>().MaxHealth += maxHealth;
    }
}
