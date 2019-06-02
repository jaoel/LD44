using UnityEngine;

public class SlingshotRockBehaviour : BulletBehaviour
{
    public Transform visualTransform;
    private float _rotation = 0f;

    private void Start()
    {
        _rotation = Random.Range(0f, 360f);
    }

    public override void UpdateBullet(BulletInstance bullet)
    {
        bullet.transform.position += (Vector3)bullet.velocity * Time.deltaTime * bullet.charge;

        _rotation += 300f * Time.deltaTime;
        visualTransform.rotation = Quaternion.Euler(0f, 0f, _rotation);
    }
}
