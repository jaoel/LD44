using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class ItemContainer : ScriptableObject {
    public ItemDescription Shotgun;
    public ItemDescription Pistol;
    public ItemDescription AK47;
    public ItemDescription Slingshot;
    public ItemDescription Grenade;

    int numItems = 5; // INCREMENT THIS WHEN ADDING ITEMS

    public ItemDescription GetRandomItem() {
        return GetItemByIndex(Random.Range(0, numItems));
    }

    public List<ItemDescription> GetAllItems() {
        List<ItemDescription> items = new List<ItemDescription>();
        for (int i = 0; i < numItems; i++) {
            items.Add(GetItemByIndex(i));
        }
        return items;
    }

    private ItemDescription GetItemByIndex(int index) {
        switch (index) {
            case 0:
                return Shotgun;
            case 1:
                return Pistol;
            case 2:
                return AK47;
            case 3:
                return Slingshot;
            case 4:
                return Grenade;
            default:
                return Pistol;
        }
    }
}
