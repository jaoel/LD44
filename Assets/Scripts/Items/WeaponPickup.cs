using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponPickup : Item
{
    public int categoryIndex;
    public int tierIndex;
    public GameObject weaponPrefab;

    public override void ApplyEffect(GameObject owner)
    {
        Player player = owner.GetComponent<Player>();

        if (player != null && Keybindings.Use)
        {
            if (isShopItem)
            {
                if (categoryIndex == -1)
                {
                    Main.Instance.sessionData.shopTiers = new List<int>();
                    for (int i = 0; i < 3; i++)
                    {
                        Main.Instance.sessionData.shopTiers.Add(0);
                    }
                }
                else
                {
                    Main.Instance.sessionData.shopTiers[categoryIndex] = tierIndex + 1;
                }
            }

            owner.GetComponent<Player>().AddWeapon(weaponPrefab);
        }
    }
}
