using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public Item pistolPickup;
    public List<WeaponPickup> weaponShopItems = new List<WeaponPickup>();
    public List<ShopItem> weaponSlots;
    public GameObject itemContainer;

    private void Awake()
    {
        SpawnShopItems();
    }

    private void Start()
    {
        
    }

    private void SpawnShopItems()
    {
        if (Main.Instance.sessionData.shopTiers.Count > 0)
        {
            for(int i = 0; i < 3; i++)
            {
                WeaponPickup weaponPickup = weaponShopItems.SingleOrDefault(x => x.categoryIndex == i && x.tierIndex == Main.Instance.sessionData.shopTiers[i]);
                if (weaponPickup != null)
                {
                    weaponSlots[i].itemPrefab = weaponPickup;
                    weaponSlots[i].GetComponentInChildren<ShopTrigger>().item = weaponSlots[i].InstantiateItem(itemContainer.transform);
                }
            }
        }
        else
        {
            weaponSlots[1].itemPrefab = pistolPickup;
            weaponSlots[1].GetComponentInChildren<ShopTrigger>().item = weaponSlots[1].InstantiateItem(itemContainer.transform);
        }
    }
}
