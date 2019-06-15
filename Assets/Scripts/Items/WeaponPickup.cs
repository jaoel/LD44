using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponPickup : Item
{
    public GameObject weaponPrefab;

    public override void ApplyEffect(GameObject owner)
    {
        owner.GetComponent<Player>().AddWeapon(weaponPrefab);
    }
}
