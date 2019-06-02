using System.Collections;
using UnityEngine;

public class SignalFlare : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float timer;
    public float damageRadius;
    public int damage;

    private void Start()
    {
        StartCoroutine(CreateExplosion());
    }

    private void Update()
    {

    }

    private IEnumerator CreateExplosion()
    {
        yield return new WaitForSeconds(timer);

        SoundManager.Instance.PlayExplosionSound();
        CameraManager.Instance.ShakeCamera(0.6f, 0.25f, 1.25f);
        Main.Instance.DamageAllEnemiesInCircle(transform.position, damageRadius, damage, true);
        Instantiate(explosionPrefab, transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
        Destroy(gameObject);

        yield return null;
    }
}
