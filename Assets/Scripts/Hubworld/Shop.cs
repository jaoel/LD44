using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public List<ShopItem> shopItems;
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
        List<Item> shuffledShopItems = ItemManager.Instance.ToList().Shuffle();

        for (int i = 0; i < shopItems.Count; i++)
        {
            shopItems[i].itemPrefab = shuffledShopItems[i % shuffledShopItems.Count];
            shopItems[i].InstantiateItem(itemContainer.transform);
        }
    }
}
