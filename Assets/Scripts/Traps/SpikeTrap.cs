using UnityEngine;

public class SpikeTrap : Trap
{
    public override void ApplyEffect(Player player, GameObject playerGO)
    {
        player.SetSlow(description.slowFactor, description.slowDuration);
        player.ReceiveDamage(description.damage, -playerGO.transform.forward);

        base.ApplyEffect(player, playerGO);
    }
} 
