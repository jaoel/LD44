using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class ShopRoom : MonoBehaviour
{
    public Transform spawnPoint;
    public ShopItem[] shopItems;

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
        List<Item> shuffledShopItems = ItemManager.Instance.ToList().Shuffle();

        shuffledShopItems = shuffledShopItems.Where(itemPrefab =>
        {
            if (itemPrefab is WeaponPickup weaponPickup)
            {
                return weaponPickup.weaponPrefab.GetComponent<Weapon>().uiImage != player.CurrentWeapon.uiImage;
            }
            else
            {
                return true;
            }
        }).ToList();

        for (int i = 0; i < shopItems.Length; i++)
        {

            if (Random.Range(0.0f, 1.0f) < healthGlobeDroprate && !healthGlobeAdded)
            {
                shopItems[i].itemPrefab = ItemManager.Instance.FindItemOfType<HealthGlobePickup>();
                shopItems[i].InstantiateItem(itemsParent.transform);
            }
            else
            {
                shopItems[i].itemPrefab = shuffledShopItems[i % shuffledShopItems.Count];
                shopItems[i].InstantiateItem(itemsParent.transform);

            }

            if (shopItems[i].itemPrefab is HealthGlobePickup)
            {
                shuffledShopItems.Remove(ItemManager.Instance.FindItemOfType<HealthGlobePickup>());
                healthGlobeAdded = true;
            }

            GameObject.Find("Item" + i).GetComponent<TextMeshProUGUI>().text = shopItems[i].itemPrefab.healthCost.ToString();
        }
    }

    public void ClearItems()
    {
        foreach (Transform child in itemsParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            ClearItems();
            Main.Instance.GenerateShop();
        }
    }

    public void ResetItem1()
    {
        ShopItem healthShopItem = shopItems[0];
        if (healthShopItem != null && healthShopItem.PickedUp)
        {
            healthShopItem.InstantiateItem(itemsParent.transform);
        }
    }
}
