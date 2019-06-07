using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ItemManager : MonoBehaviour, IEnumerable<Item>, IEnumerator<Item>
{
    [SerializeField]
    private Item[] items = new Item[0];
    private int _position = -1;

    private static ItemManager _instance = null;
    public static ItemManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindObjectOfType<ItemManager>();

            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs an ItemManager");
            }
            return _instance;
        }
    }

    public Item FindItemOfType<T>() where T : Item
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] is T)
            {
                return items[i];
            }
        }
        return null;
    }

    Item IEnumerator<Item>.Current => items[_position];

    object IEnumerator.Current => items[_position];

    IEnumerator<Item> IEnumerable<Item>.GetEnumerator()
    {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this;
    }

    bool IEnumerator.MoveNext()
    {
        _position++;
        return _position < items.Length;
    }

    void IEnumerator.Reset()
    {
        _position = -1;
    }

    void IDisposable.Dispose()
    {
        _position = -1;
    }
}
