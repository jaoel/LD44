using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    protected virtual void Start()
    {
    }

    public virtual void OnActivate()
    {
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject != null && collision.gameObject.GetComponent<Player>() != null && Keybindings.Use)
        {

            OnActivate();
        }
    }
}
