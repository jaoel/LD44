using System;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public CircleCollider2D trigger;
    public new BoxCollider2D collider;
    public Rigidbody2D rigidBody;

    public RectInt Bounds { get; set; }
    public List<Key> Keys { get; set; }
    public List<Door> Siblings { get; set; }
    private bool _locked;

    private void Awake()
    {
        Keys = new List<Key>();
        Siblings = new List<Door>();
        _locked = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_locked)
        {
            return;
        }

        if (collision.gameObject)
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                if (player.UseKey(this))
                {
                    Destroy(trigger);
                    Destroy(collider);
                    Destroy(rigidBody);
                    _locked = false;
                }
            }
        }
    }
}
