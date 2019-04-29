using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SlowTrap : Item
{
    public AudioSource triggerSound;
    public Sprite triggeredSprite; 
    public float slowFactor;
    public float duration;
    public override void Apply(GameObject owner)
    {
        if (_triggered)
            return;

        Player player = owner.GetComponent<Player>();

        if (player.IsInvulnerable)
            return;

        if (player.Health > 0)
        {
            ApplyEffect(owner);
        }
        _triggered = true;
    }

    public override void ApplyEffect(GameObject owner)
    {
        Player player = owner.GetComponent<Player>();
        player.SetSlow(slowFactor, duration);
        GetComponent<SpriteRenderer>().sprite = triggeredSprite;
        triggerSound.Play();
    }
}
