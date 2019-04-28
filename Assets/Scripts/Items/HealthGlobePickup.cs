using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HealthGlobePickup : Item
{
    public int health;

    public override void ApplyEffect(GameObject owner)
    {
        owner.GetComponent<Player>().Health += health;
    }
} 
