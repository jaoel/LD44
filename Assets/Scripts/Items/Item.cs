using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public ItemDescription Description;
    public bool isShopItem = false;
    protected bool _triggered = false;

    public virtual void Apply(GameObject owner) {
        Player player = owner.GetComponent<Player>();
        if (isShopItem) {
            player.MaxHealth -= Description.HealthCost;
        }
        if (player.Health > 0) {
            ApplyEffect(owner);
        }

        if (player.Health <= 0)
        {
            SoundManager.Instance.PlayPlayerDeath(UnityEngine.Random.Range(0.0f, 1.0f) < 0.1f);
        }
        else
        {
            SoundManager.Instance.PlayPainSound();
        }

        Destroy(gameObject);
    }

    public abstract void ApplyEffect(GameObject owner);

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject)
        {
            Apply(collision.gameObject);
        }
    }
}
