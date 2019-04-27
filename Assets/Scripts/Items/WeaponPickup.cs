using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponPickup : Item
{
    public Weapon Weapon;

    public override void ApplyEffect(GameObject owner)
    {
        Player player = owner.GetComponent<Player>();
        player.CurrentHealth -= Description.HealthCost;

        owner.GetComponent<Player>().CurrentWeapon = Weapon;
    }
}
