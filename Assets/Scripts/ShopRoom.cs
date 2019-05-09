using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class ShopRoom : MonoBehaviour
{
    public Transform spawnPoint;
    public ShopItem[] shopItems;
    public ItemContainer ItemContainer;

    public GameObject shopUI;
    public GameObject itemsParent;
    public float healthGlobeDroprate;

    public void MovePlayerToSpawn(Player player)
    {
        player.transform.position = spawnPoint.position + new Vector3(0.5f, 0.5f, 0.0f);
        CameraManager.Instance.SetCameraPosition(player.transform.position);
    }

    public void GenerateRandomItems(int currentLevel, Player player)
    {
        bool healthGlobeAdded = false;
        List<ItemDescription> shuffledShopItems = GetShuffledShopItems();
        List<ItemDescription> rareItems = GetRareItems();

        shuffledShopItems = shuffledShopItems.Where(itemDesc =>
        {
            if (itemDesc.itemPrefab is WeaponPickup weaponPickup)
            {
                return weaponPickup.Weapon.description != player.CurrentWeapon.description;
            }
            else
            {
                return true;
            }
        }).ToList();

        if (currentLevel < 5)
        {
            rareItems.ForEach(x => shuffledShopItems.Remove(x));
        }

        for (int i = 0; i < shopItems.Length; i++)
        {

            if (Random.Range(0.0f, 1.0f) < healthGlobeDroprate && !healthGlobeAdded)
            {
                shopItems[i].description = ItemContainer.HealthGlobe;
                shopItems[i].InstantiateItem(itemsParent.transform);
            }
            else
            {
                float chance = (float)((20 + System.Math.Pow(currentLevel, 2)) / 100.0f);
                if (Random.Range(0.0f, 1.0f) < chance)
                {
                    shopItems[i].description = rareItems[i % rareItems.Count];
                    shopItems[i].InstantiateItem(itemsParent.transform);
                }
                else
                {
                    shopItems[i].description = shuffledShopItems[i % shuffledShopItems.Count];
                    shopItems[i].InstantiateItem(itemsParent.transform);
                }

            }

            if (shopItems[i].description == ItemContainer.HealthGlobe)
            {
                shuffledShopItems.Remove(ItemContainer.HealthGlobe);
                healthGlobeAdded = true;
            }

            GameObject.Find("Item" + i).GetComponent<TextMeshProUGUI>().text = shopItems[i].description.HealthCost.ToString();
        }
    }

    public void ClearItems()
    {
        foreach (Transform child in itemsParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private List<ItemDescription> GetRareItems()
    {
        List<ItemDescription> shuffled = ItemContainer.GetRareItems();

        int n = shuffled.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            ItemDescription value = shuffled[k];
            shuffled[k] = shuffled[n];
            shuffled[n] = value;
        }

        return shuffled;
    }

    private List<ItemDescription> GetShuffledShopItems()
    {
        List<ItemDescription> shuffled = ItemContainer.GetAllItems();

        int n = shuffled.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            ItemDescription value = shuffled[k];
            shuffled[k] = shuffled[n];
            shuffled[n] = value;
        }

        return shuffled;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            ClearItems();
            Main.Instance.GenerateShop();
        }
    }
}
