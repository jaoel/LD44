using UnityEngine;
using System.Collections;

public class ShopItem : MonoBehaviour {

    public ItemDescription description;

    public Item InstantiateItem(Transform parent) {
        Vector3 pos = transform.position;
        pos.z = -1f;
        Item item = Instantiate(description.itemPrefab, pos, Quaternion.identity, parent);
        item.isShopItem = true;
        return item;
    }
}
