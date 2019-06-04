using System;
using UnityEngine;

public class SlingshotBullet : Bullet
{
    [SerializeField]
    private Transform _visualTransform;
    private float _rotation = 0f;

    protected override void Start()
    {
        _rotation = UnityEngine.Random.Range(0.0f, 360.0f);
        base.Start();
    }

    public override void UpdateBullet()
    {
        _rotation += 300.0f * Time.deltaTime;
        _visualTransform.rotation = Quaternion.Euler(0.0f, 0.0f, _rotation);

        base.UpdateBullet();
    }
}
