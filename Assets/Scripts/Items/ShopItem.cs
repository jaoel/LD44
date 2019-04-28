﻿using UnityEngine;
using System.Collections;

public class ShopItem : MonoBehaviour {

    public ItemDescription description;

    public void InstantiateItem() {
        Vector3 pos = transform.position;
        pos.z = -1f;
        Item item = Instantiate(description.itemPrefab, pos, Quaternion.identity);
        item.isShopItem = true;
    }
}
