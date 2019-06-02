using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SignalFlareBulletBehaviour : BulletBehaviour
{
    public GameObject signalFlarePrefab;

    public override void UpdateBullet(BulletInstance bullet)
    {
        base.UpdateBullet(bullet);
    }

    public override void BeforeDestroyed(GameObject hitTarget, Vector2 velocity = new Vector2())
    {
        if (hitTarget != null)
        {
            GameObject go = Instantiate(signalFlarePrefab, transform.position, Quaternion.Euler(45.0f, 0.0f, 0.0f), hitTarget.transform);
        }
        else
        {
            SignalFlare sf = Instantiate(signalFlarePrefab, transform.position, Quaternion.identity).GetComponent<SignalFlare>();
            sf.timer = 0.0f;
        }
    }
}
