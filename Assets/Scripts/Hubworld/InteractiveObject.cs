using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class InteractiveObject : MonoBehaviour
{
    public UnityEvent onActivate;
    public UnityEvent onEnter;
    public UnityEvent onStay;
    public UnityEvent onExit;

    protected virtual void Start()
    {
        
    }

    public virtual void OnActivate()
    {
        onActivate?.Invoke();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Player>() != null)
        {
            onEnter?.Invoke();
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject != null && collision.gameObject.GetComponent<Player>() != null && Keybindings.Use)
        {
            onStay?.Invoke();
            OnActivate();
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Player>() != null)
        {
            onExit?.Invoke();
        }
    }
}
