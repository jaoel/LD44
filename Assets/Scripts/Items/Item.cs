using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public ItemDescription Description;

    public abstract void ApplyEffect(GameObject owner);

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject)
        {
            ApplyEffect(collision.gameObject);
        }
    }
}
