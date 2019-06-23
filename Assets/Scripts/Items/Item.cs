using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public AudioSource pickupSound;

    public List<string> shopkeeperDescription = new List<string>(); 
    public int healthCost;

    public bool isShopItem = false;
    public bool destroyOnPickup = true;
    protected bool _triggered = false;

    public virtual void Apply(GameObject owner)
    {
        Player player = owner.GetComponent<Player>();

        if (player != null)
        {
            if (isShopItem && !player.GodMode)
            {
                player.MaxHealth -= healthCost;
                Main.Instance.sessionData.UpdatePlayerData(player);
            }

            if (player.Health > 0)
            {
                ApplyEffect(owner);
            }

            if (player.Health <= 0)
            {
                SoundManager.Instance.PlayPlayerDeath(UnityEngine.Random.Range(0.0f, 1.0f) < 0.1f);
            }

            if (pickupSound == null)
            {
                SoundManager.Instance.PlayItemPickup();
            }
            else
            {
                pickupSound.volume = SettingsManager.Instance.SFXVolume;
                pickupSound.Play();
            }

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }

    public abstract void ApplyEffect(GameObject owner);

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject)
        {
            Apply(collision.gameObject);
        }
    }

    public string GetShopkeeperDescription()
    {
        if (shopkeeperDescription.Count == 0)
        {
            return healthCost.ToString() + " blood";
        }
        else
        {
            string result = shopkeeperDescription[UnityEngine.Random.Range(0, shopkeeperDescription.Count)];
            return result.Replace("%COST%", healthCost.ToString());
        }
    }
}
