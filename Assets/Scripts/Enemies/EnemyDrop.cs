using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum DropType
{
    MaxHealth,
    Heal,
    Score
}

public class EnemyDrop : MonoBehaviour
{
    private int _maxHealth;
    private int _heal;
    private int _score;


    [SerializeField]
    private Navigation _navigation;

    [SerializeField]
    private Rigidbody2D _rigidbody;

    [SerializeField]
    private float _acceleration;

    private bool _movingToPlayer;

    private void Awake()
    {
        _navigation.Initialize(_rigidbody, 10.0f, _acceleration);
        _navigation.MoveTo(transform.position.ToVector2() + Utility.RandomPointOnCircleEdge(UnityEngine.Random.Range(2.0f, 5.0f)), true);
        _movingToPlayer = false;
    }

    public void SetType(DropType type, int score = 0)
    {
        switch (type)
        {
            case DropType.MaxHealth:
                GetComponentInChildren<SpriteRenderer>().color = Utility.RGBAColor(100, 149, 237, 1);
                _maxHealth = 1;
                break;
            case DropType.Heal:
                GetComponentInChildren<SpriteRenderer>().color = Color.red;
                _heal = 1;
                break;
            case DropType.Score:
                _score = score;
                break;
            default:
                break;
        }
    }

    private void FixedUpdate()
    {
        if (_navigation.AtDestination() && !_movingToPlayer)
        {
            _movingToPlayer = true;
        }

        if (_movingToPlayer)
        {
            _navigation.MoveTo(Main.Instance.player.gameObject, true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == Layers.Player)
        {
            Main.Instance.player.MaxHealth += _maxHealth;
            Main.Instance.player.Health += _heal;
            Destroy(gameObject);
        }
    }
}
