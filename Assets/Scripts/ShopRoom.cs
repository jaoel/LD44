using UnityEngine;
using System.Collections.Generic;

public class ShopRoom : MonoBehaviour {
    public Transform spawnPoint;
    public ShopItem[] shopItems;
    public ItemContainer ItemContainer;

    public GameObject itemsParent;
    public float healthGlobeDroprate;

    public void MovePlayerToSpawn(Player player) {
        player.transform.position = spawnPoint.position + new Vector3(0.5f, 0.5f, 0.0f);
        CameraManager.Instance.SetCameraPosition(player.transform.position);
    }

    public void GenerateRandomItems() {
        bool healthGlobeAdded = false;
        List<ItemDescription> shuffledShopItems = GetShuffledShopItems();
        for (int i = 0; i < shopItems.Length; i++) {

            if (Random.Range(0.0f, 1.0f) < healthGlobeDroprate && !healthGlobeAdded)
            {
                shopItems[i].description = ItemContainer.HealthGlobe;
                shopItems[i].InstantiateItem(itemsParent.transform);
            }
            else
            {
                shopItems[i].description = shuffledShopItems[i % shuffledShopItems.Count];
                shopItems[i].InstantiateItem(itemsParent.transform);
            }

            if (shopItems[i].description == ItemContainer.HealthGlobe)
            {
                shuffledShopItems.Remove(ItemContainer.HealthGlobe);
                healthGlobeAdded = true;
            }
        }
    }

    public void ClearItems() {
        foreach(Transform child in itemsParent.transform) {
            Destroy(child.gameObject);
        }
    }

    private List<ItemDescription> GetShuffledShopItems() {
        List<ItemDescription> shuffled = ItemContainer.GetAllItems();

        int n = shuffled.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            ItemDescription value = shuffled[k];
            shuffled[k] = shuffled[n];
            shuffled[n] = value;
        }

        return shuffled;
    }
}
