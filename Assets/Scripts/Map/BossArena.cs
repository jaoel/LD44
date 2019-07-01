using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BossArena : MonoBehaviour
{
    [SerializeField]
    protected GameObject _hubTeleporter;

    protected bool _exitSpawned;

    private float _timer;

    protected virtual void Awake()
    {
        _exitSpawned = false;
        _timer = 0.0f;

        if (Main.Instance.bossTiers.Contains(0))
        {
            SoundManager.Instance.PlayPlayerDeath(true);
        }

        if (Main.Instance.bossTiers.Contains(1))
        {
            SoundManager.Instance.PlayExplosionSound();
        }

        if (Main.Instance.bossTiers.Contains(2))
        {
            Main.Instance.player.ReceiveDamage(10, Vector2.zero, false, true);
        }
    }

    protected virtual void Update()
    {
        _timer += Time.deltaTime;
        if (CheckWinCondition())
        {
            SpawnExits();
        }
    }
    protected virtual bool CheckWinCondition()
    {
        if (_timer > 5.0f)
        {
            return true;
        }

        return false;
    }

    protected virtual void SpawnExits()
    {
        if (_exitSpawned)
        {
            return;
        }

        _exitSpawned = true;
        _hubTeleporter.SetActive(true);
    }
}
