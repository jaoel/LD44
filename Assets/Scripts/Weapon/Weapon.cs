using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    //how long before weapon can shoot again
    [SerializeField]
    private float _cooldown;

    //how long you need to hold fire button before shooting
    //only starts counting after cooldown has passed
    [SerializeField]
    private float _chargeTime;

    //determines if we should reset charge meter when a shot has been fired or 
    //if it should be more like a windup
    [SerializeField]
    private bool _resetChargeOnShot;

    //how many bullets to be fired before you have to reload
    [SerializeField]
    private int _magazineSize;

    //how long it takes to reload
    [SerializeField]
    private float _reloadTime;

    //for burst fire guns if > 1
    [SerializeField]
    private int _bulletsPerShot;

    //how long between bullets in burst
    [SerializeField]
    private float _burstInterval;

    //how much the one firing should be knocked back when firing
    [SerializeField]
    private float _knockback;

    //defines the arc in which the gun shoots
    [SerializeField]
    private float _firingArc;

    //defines if bullet pattern should be random or regular within arc
    [SerializeField]
    private bool _randomizePattern;

    //all bullet related information
    //bullet speed should be moved here, don't understand why it's not yet
    [SerializeField]
    private BulletDescription _bulletDescription;

    //Sound that should be played when shooting
    [SerializeField]
    private AudioSource _shotSound;

    //Sound that should be played when shooting
    [SerializeField]
    private AudioSource _reloadSound;

    //Who is shooting
    private IWeaponOwner _owner;

    private float _currentCooldown;
    private float _currentChargeTime;
    private float _bulletsLeft;
    private float _halfAngle;
    private float _degPerBullet;
    private bool _reloading;
    private bool _charging;
    private Coroutine _firingSequence;

    protected virtual void Awake()
    {
        _currentCooldown = _cooldown;
        _currentChargeTime = 0.0f;
        _bulletsLeft = _magazineSize;
        _charging = false;
        _reloading = false;
        _halfAngle = _firingArc / 2.0f;
        _degPerBullet = _firingArc / _bulletsPerShot;
    }

    public void SetOwner(IWeaponOwner owner)
    {
        _owner = owner;
    }

    protected virtual IEnumerator FireBullets()
    {
        _shotSound.volume = SettingsManager.Instance.SFXVolume;
        _shotSound.Play();

        float halfAngle = _firingArc / 2.0f;

        for(int i = 0; i < _bulletsPerShot; i++)
        {
            Quaternion rotation = Quaternion.identity;
            if (_randomizePattern)
            {
                rotation = Quaternion.Euler(0.0f, 0.0f, Random.Range(-_halfAngle, _halfAngle));
            }
            else
            {
                rotation = Quaternion.Euler(0.0f, 0.0f, -_halfAngle + (_degPerBullet * i));
            }

            BulletManager.Instance.SpawnBullet(_bulletDescription, _owner.GetBulletOrigin(),
               rotation * _owner.GetAimVector() * _bulletDescription.speed, _owner.GetGameObject());

            if (_burstInterval > 0)
            {
                _owner.Knockback(-_owner.GetAimVector(), _knockback);
                yield return new WaitForSeconds(_burstInterval);
            }
        }

        if (_burstInterval == 0)
        {
            _owner.Knockback(-_owner.GetAimVector(), _knockback);
        }

        if (_bulletsLeft <= 0)
        {
            TriggerReload();
        }

        yield return null;
    }

    protected virtual IEnumerator Reload()
    {
        if(_firingSequence != null)
        {
            StopCoroutine(_firingSequence);
        }

        _reloadSound.volume = SettingsManager.Instance.SFXVolume;
        _reloadSound.Play();

        _reloading = true;

        yield return new WaitForSeconds(_reloadTime);

        _bulletsLeft = _magazineSize;
        _reloading = false;

        yield return null;
    }

    public virtual void Shoot()
    {
        if (_bulletsLeft <= 0)
        {
            return;
        }

        if (_currentCooldown >= _cooldown)
        {
            _charging = true;
            _currentChargeTime += Time.deltaTime;

            if (_currentChargeTime >= _chargeTime)
            {
                if (_firingSequence != null)
                {
                    StopCoroutine(_firingSequence);
                }

                _bulletsLeft--;
                _firingSequence = StartCoroutine(FireBullets());

                if (_resetChargeOnShot)
                {
                    _currentChargeTime = 0.0f;
                }
                _currentCooldown = 0.0f;
            }
        }
    }

    public void TriggerReload()
    {
        if (_reloading)
        {
            return;
        }

        StartCoroutine(Reload());
    }

    public virtual void StoppedShooting()
    {
        _charging = false;
    }

    protected virtual void Update()
    {
        _currentCooldown += Time.deltaTime;

        if (!_charging && _currentChargeTime > 0.0f)
        {
            _currentChargeTime -= Time.deltaTime;
            _currentChargeTime = Mathf.Max(0.0f, _currentChargeTime);
        }
    }
}
