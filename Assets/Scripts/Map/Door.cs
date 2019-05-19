using System;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public CircleCollider2D trigger;
    public new BoxCollider2D collider;
    public Rigidbody2D rigidBody;
    public Animator animator;
    public AudioSource opening;
    public AudioSource accessDenied;

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

    public void Unlock()
    {
        Destroy(trigger);
        Destroy(collider);
        Destroy(rigidBody);
        _locked = false;
    }

    public void ToggleLock(bool locked)
    {
        if (!locked)
        {
            animator.SetTrigger("OpenDoor");
        }
        else
        {
            animator.SetTrigger("CloseDoor");
        }

        opening.volume = SettingsManager.Instance.SFXVolume;
        opening.Play();

        _locked = locked;
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
                    ToggleLock(false);

                    Siblings.ForEach(x =>
                    {
                        x.ToggleLock(false);
                    });
                }
                else
                {
                    accessDenied.volume = SettingsManager.Instance.SFXVolume;
                    accessDenied.Play();
                }
            }
        }
    }
}
