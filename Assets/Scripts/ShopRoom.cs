using UnityEngine;
using System.Collections.Generic;

public class ShopRoom : MonoBehaviour {
    public Transform spawnPoint;
    public ShopItem[] shopItems;
    public ItemContainer ItemContainer;

    public void MovePlayerToSpawn(Player player) {
        player.transform.position = spawnPoint.position + new Vector3(0.5f, 0.5f, 0.0f);
    }

    public void GenerateRandomItems() {
        List<ShopItem> shuffledShopItems = GetShuffledShopItems();
        for (int i = 0; i < shuffledShopItems.Count; i++) {
            shuffledShopItems[i].description = ItemContainer.GetRandomItem();
            shuffledShopItems[i].InstantiateItem();
        }
    }

    private List<ShopItem> GetShuffledShopItems() {
        List<ShopItem> shuffled = new List<ShopItem>(shopItems);

        int n = shuffled.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            ShopItem value = shuffled[k];
            shuffled[k] = shuffled[n];
            shuffled[n] = value;
        }

        return shuffled;
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
