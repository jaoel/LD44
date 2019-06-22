using UnityEngine;
using System.Collections;

public class ShopItem : MonoBehaviour
{
    public Item itemPrefab;
    private GameObject itemObject = null;

    public bool PickedUp => itemObject == null || itemObject.Equals(null);

    private bool _addedToPlayer;

    private void Awake()
    {
        _addedToPlayer = false;
    }

    public Item InstantiateItem(Transform parent)
    {
        if (itemPrefab != null)
        {
            Vector3 pos = transform.position;
            Item item = Instantiate(itemPrefab, pos, Quaternion.identity, parent);
            item.isShopItem = true;
            itemObject = item.gameObject;
            return item;
        }
        Debug.LogError("Tried to instantiate a shop item that does not exist.");
        return null;
    }

    private void Update()
    {
        if (itemPrefab != null && PickedUp && !_addedToPlayer)
        {
            _addedToPlayer = true;
            Main.Instance.sessionData.AddItem(itemPrefab);
        }
    }
}
