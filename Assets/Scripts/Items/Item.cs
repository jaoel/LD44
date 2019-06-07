using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public int healthCost;

    public bool isShopItem = false;
    protected bool _triggered = false;

    public virtual void Apply(GameObject owner)
    {
        Player player = owner.GetComponent<Player>();

        if (player != null)
        {
            if (isShopItem && !player.GodMode)
            {
                player.MaxHealth -= healthCost;
            }
            if (player.Health > 0)
            {
                ApplyEffect(owner);
            }

            if (player.Health <= 0)
            {
                SoundManager.Instance.PlayPlayerDeath(UnityEngine.Random.Range(0.0f, 1.0f) < 0.1f);
            }

            SoundManager.Instance.PlayItemPickup();
            Destroy(gameObject);
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
}
