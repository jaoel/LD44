using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SignalFlare : MonoBehaviour
{
    [SerializeField]
    private GameObject _burningDebuff;

    [SerializeField]
    private float _timer;

    [SerializeField]
    private float _explosionRadius;

    [SerializeField]
    private int _explosionDamage;

    [SerializeField]
    private GameObject _explosionPrefab;

    [SerializeField]
    private AudioSource _flameAudioSource;
    private void Awake()
    {
        StartCoroutine(Run()); 
    }

    private IEnumerator Run()
    {
        yield return new WaitForSeconds(_timer);

        List<Enemy> enemies = MapManager.Instance.CurrentMap.GetEnemiesInCircle(transform.position.ToVector2(), _explosionRadius);
        enemies.ForEach(x =>
        {
            x.AddStatusEffect(Instantiate(_burningDebuff.gameObject, x.transform).GetComponent<BurningDebuff>());
        });

        if ((transform.position - Main.Instance.player.transform.position).magnitude <= _explosionRadius)
        {
            Main.Instance.player.AddStatusEffect(Instantiate(_burningDebuff.gameObject, Main.Instance.player.transform)
                .GetComponent<BurningDebuff>());
        }

        MapManager.Instance.DamageAllEnemiesInCircle(transform.position.ToVector2(), _explosionRadius, _explosionDamage, true);
        CameraManager.Instance.ShakeCamera(0.6f, 0.25f, 1.25f);

        _flameAudioSource.volume = SettingsManager.Instance.SFXVolume;
        _flameAudioSource.Play();

        Instantiate(_explosionPrefab, transform.position.ToVector2(), Quaternion.Euler(0.0f, 0.0f, UnityEngine.Random.Range(0.0f, 360.0f)));
        gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
        Destroy(gameObject.GetComponentInChildren<ParticleSystem>());
        Destroy(gameObject, _flameAudioSource.clip.length);

        yield return null;
    }
}
