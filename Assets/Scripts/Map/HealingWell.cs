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

    private int _adjustedHeal;
    private int _adjustedDamage;

    private float _timer;
    private float _multiplierTimer = 0.0f;

    private void OnEnable()
    {
        _timer = 0.0f;
        _multiplierTimer = 0.0f;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        _adjustedHeal = heal;
        _adjustedDamage = maxHealthDamage;
        base.OnTriggerEnter2D(collision);
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            _timer += Time.deltaTime;
            _multiplierTimer += Time.deltaTime;

            if (_multiplierTimer > 5.0f)
            {
                _adjustedHeal *= 2;
                _adjustedDamage *= 2;
                _multiplierTimer = 0.0f;
                SoundManager.Instance.PlayPainSound();
            }

            if (_timer >= tickRate && player.Health < player.MaxHealth)
            {
                player.MaxHealth -= _adjustedDamage;
                player.Health += _adjustedHeal;

                _timer = 0.0f;
            }
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        _timer = 0.0f;
        _multiplierTimer = 0.0f;
        base.OnTriggerExit2D(collision);
    }
}
