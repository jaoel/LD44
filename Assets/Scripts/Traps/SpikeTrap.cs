using UnityEngine;

public class SpikeTrap : Item
{
    public int damage;

    public override void Apply(GameObject owner)
    {
        if (_triggered)
            return;

        Player player = owner.GetComponent<Player>();
        if (player.Health > 0)
        {
            ApplyEffect(owner);
        }
        _triggered = true;
    }

    public override void ApplyEffect(GameObject owner)
    {
        Player player = owner.GetComponent<Player>();
        player.ReceiveDamage(damage, Vector2.zero);
    }
} 
