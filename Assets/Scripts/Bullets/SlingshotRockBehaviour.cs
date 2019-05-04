using UnityEngine;

public class SlingshotRockBehaviour : BulletBehaviour {
    public Transform visualTransform;
    private float rot = 0f;

    private void Start() {
        rot = Random.Range(0f, 360f);
    }

    public override void UpdateBullet(BulletInstance bullet) {
        base.UpdateBullet(bullet);
        rot += 300f * Time.deltaTime;
        visualTransform.rotation = Quaternion.Euler(0f, 0f, rot);
    }
}
