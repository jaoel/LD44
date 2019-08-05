using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HealingWell : InteractiveObject
{
    [SerializeField]
    private bool _dungeonWell;

    [SerializeField]
    private int _healPerTick;

    [SerializeField]
    private int _heal;

    [SerializeField]
    private int _maxHealthDamage;

    [SerializeField]
    private float _tickRate;

    private int _adjustedHeal;
    private int _adjustedDamage;

    private float _timer;
    private float _multiplierTimer = 0.0f;
    private bool _disabled;

    private void OnEnable()
    {
        _timer = 0.0f;
        _multiplierTimer = 0.0f;
        _disabled = false;
    }

    public override void OnActivate()
    {
        if (_dungeonWell && !_disabled)
        {
            if (Main.Instance.player.Health == Main.Instance.player.MaxHealth)
            {
                return;
            }

            int missingHealth = Main.Instance.player.MaxHealth - Main.Instance.player.Health;
            Main.Instance.player.Health += _heal;
            _heal = Mathf.Max(0, _heal - missingHealth);

            if (_heal == 0)
            {
                _disabled = true;
            }
        }

        base.OnActivate();
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_dungeonWell)
        {
            _adjustedHeal = _healPerTick;
            _adjustedDamage = _maxHealthDamage;
            _disabled = true;
        }
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        if (!_dungeonWell && Keybindings.Use)
        {
            _disabled = !_disabled;
        }

        if (!_dungeonWell && !_disabled)
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

                if (_timer >= _tickRate && player.Health < player.MaxHealth)
                {
                    player.MaxHealth -= _adjustedDamage;
                    player.Health += _adjustedHeal;

                    _timer = 0.0f;
                }
            }
        }

        base.OnTriggerStay2D(collision);
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if(!_dungeonWell)
        {
            _timer = 0.0f;
            _multiplierTimer = 0.0f;
            _disabled = true;
        }

        base.OnTriggerExit2D(collision);
    }
}
