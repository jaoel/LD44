using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    protected AudioSource _shotSound = null;
    protected bool _destroyedSound = false;
    public WeaponDescription description;

    public abstract void Shoot(Vector2 aimVector, Vector2 position, GameObject owner);
    public virtual void StoppedShooting()
    {
        if (_shotSound != null)
        {
            Destroy(_shotSound.gameObject, 0.2f);
        }
    }
}
