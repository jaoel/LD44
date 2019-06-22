using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ArmoryTrigger : InteractiveObject
{
    private Item _itemPrefab;
    private Item _itemInstance;

    public void SetPrefab(Item prefab)
    {
        if (_itemInstance != null)
        {
            Item old = _itemInstance;
            Destroy(old.gameObject);
            _itemInstance = null;
        }

        _itemPrefab = prefab;

        InstantiateItem();
    }

    private void InstantiateItem()
    {
        _itemInstance = Instantiate(_itemPrefab, transform);
        _itemInstance.healthCost = 0;
        _itemInstance.transform.localPosition = Vector3.zero;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (_itemInstance == null && _itemPrefab != null)
        {
            InstantiateItem();
        }

        base.OnTriggerExit2D(collision);
    }
}
