using System;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public BoxCollider2D trigger;
    public new BoxCollider2D collider;
    public Rigidbody2D rigidBody;
    public Animator animator;
    public AudioSource opening;
    public AudioSource accessDenied;

    public bool locked;
    public bool closed;
    public bool closeOnExit;

    public RectInt Bounds { get; set; }
    public bool IsGoalDoor { get; set; }
    public List<Door> Siblings = new List<Door>();


    private void Start()
    {
        if (!closed)
        {
            ToggleClosed(false);
        }
    }

    public void Open(bool unlock)
    {
        collider.enabled = false;
        if (MapManager.Instance.CurrentMap != null)
        {
            MapManager.Instance.CurrentMap.UpdateCollisionMap(Bounds, 0);
        }

        if (unlock)
        {
            locked = false;
        }
        closed = false;
    }

    public void Close(bool lockDoor)
    {
        collider.enabled = true;
        if (MapManager.Instance.CurrentMap != null)
        {
            MapManager.Instance.CurrentMap.UpdateCollisionMap(Bounds, 1);
        }

        if (lockDoor)
        {
            locked = true;
        }

        closed = true;
    }

    public void ToggleClosed(bool closed)
    {
        if (!closed)
        {
            animator.SetTrigger("OpenDoor");
        }
        else
        {
            animator.SetTrigger("CloseDoor");
        }

        opening.volume = SettingsManager.Instance.SFXVolume;
        opening.Play();

        this.closed = closed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject)
        {
            if (!locked && closed)
            {
                ToggleClosed(false);

                Siblings.ForEach(x =>
                {
                    x.ToggleClosed(false);
                });
            }
            else if (locked && closed)
            {
                Player player = collision.gameObject.GetComponent<Player>();
                if (player != null)
                {
                    if(player.UseKey(this))
                    {
                        ToggleClosed(false);
                        Siblings.ForEach(x =>
                        {
                            x.ToggleClosed(false);
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!closed && closeOnExit)
        {
            ToggleClosed(true);
        }
    }
}
