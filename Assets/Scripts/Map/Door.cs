﻿using System;
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

    public RectInt Bounds { get; set; }
    public bool IsGoalDoor { get; set; }
    public List<Door> Siblings { get; set; }
    private bool _locked;
    private bool _closed;

    private void Awake()
    {
        Siblings = new List<Door>();
        _locked = true;
        _closed = true;
    }

    public void Open(bool unlock)
    {
        collider.enabled = false;
        MapManager.Instance.CurrentMap.UpdateCollisionMap(Bounds, 0);

        if (unlock)
        {
            _locked = false;
        }
        _closed = false;
    }

    public void Close(bool lockDoor)
    {
        collider.enabled = true;
        MapManager.Instance.CurrentMap.UpdateCollisionMap(Bounds, 1);

        if (lockDoor)
        {
            _locked = true;
        }

        _closed = true;
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

        _closed = closed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject)
        {
            if (!_locked && _closed)
            {
                ToggleClosed(false);

                Siblings.ForEach(x =>
                {
                    x.ToggleClosed(false);
                });
            }
            else if (_locked && _closed)
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
}
