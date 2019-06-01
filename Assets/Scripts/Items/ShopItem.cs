using UnityEngine;
using System.Collections;

public class ShopItem : MonoBehaviour
{
    public ItemDescription description;
    private GameObject itemObject = null;

    public bool PickedUp => itemObject == null || itemObject.Equals(null);

    public Item InstantiateItem(Transform parent)
    {
        Vector3 pos = transform.position;
        Item item = Instantiate(description.itemPrefab, pos, Quaternion.identity, parent);
        item.isShopItem = true;
        itemObject = item.gameObject;
        return item;
    }
}
