using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class ItemContainer : ScriptableObject
{
    public ItemDescription Shotgun;
    public ItemDescription Pistol;
    public ItemDescription AK47;
    public ItemDescription Slingshot;
    public ItemDescription Grenade;
    public ItemDescription HealthGlobe;
    public ItemDescription MaxHealth;
    public ItemDescription WeakHealthGlobe;
    public ItemDescription FiringRateUpgrade;

    [System.NonSerialized]
    int numItems = 7; // INCREMENT THIS WHEN ADDING ITEMS

    public ItemDescription GetEnemyDrop()
    {
        List<ItemDescription> drops = new List<ItemDescription>() { MaxHealth, WeakHealthGlobe, WeakHealthGlobe };

        return drops[Random.Range(0, drops.Count)];
    }

    public ItemDescription GetRandomItem()
    {
        return GetItemByIndex(Random.Range(0, numItems));
    }

    public List<ItemDescription> GetRareItems()
    {
        List<ItemDescription> items = new List<ItemDescription>();
        items.Add(Shotgun);
        items.Add(AK47);
        items.Add(Grenade);

        return items;
    }

    public List<ItemDescription> GetAllItems()
    {
        List<ItemDescription> items = new List<ItemDescription>();
        for (int i = 0; i < numItems; i++)
        {
            items.Add(GetItemByIndex(i));
        }
        return items;
    }

    private ItemDescription GetItemByIndex(int index)
    {
        switch (index)
        {
            case 0:
                {
                    return Shotgun;
                }
            case 1:
                {
                    return Pistol;
                }
            case 2:
                {
                    return AK47;
                }
            case 3:
                {
                    return Slingshot;
                }
            case 4:
                {
                    return Grenade;
                }
            case 5:
                {
                    return HealthGlobe;
                }
            case 6:
                {
                    return FiringRateUpgrade;
                }
            default:
                {
                    return Pistol;
                }
        }
    }
}
