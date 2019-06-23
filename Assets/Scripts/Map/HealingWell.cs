using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HealingWell : InteractiveObject
{
    public int heal;
    public int maxHealthDamage;
    public float tickRate;

    private float _timer;

    private void OnEnable()
    {
        _timer = 0.0f;
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            _timer += Time.deltaTime;

            if (_timer >= tickRate && player.Health < player.MaxHealth)
            {
                player.MaxHealth -= maxHealthDamage;
                player.Health += heal;

                _timer = 0.0f;
            }
        }
    }
}
