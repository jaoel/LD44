﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Weapon : MonoBehaviour
{
    public Sprite uiImage;
    public int BulletsLeft => _bulletsLeft;

    //how long before weapon can shoot again
    [SerializeField]
    protected float _cooldown;

    //how long you need to hold fire button before shooting
    //only starts counting after cooldown has passed
    [SerializeField]
    protected float _chargeTime;

    //determines if we should reset charge meter when a shot has been fired or 
    //if it should be more like a windup
    [SerializeField]
    protected bool _resetChargeOnShot;

    //how many bullets to be fired before you have to reload
    [SerializeField]
    protected int _magazineSize;

    //how long it takes to reload
    [SerializeField]
    protected float _reloadTime;

    //for burst fire guns if > 1
    [SerializeField]
    protected int _bulletsPerShot;

    //how long between bullets in burst
    [SerializeField]
    protected float _burstInterval;

    //how much the one firing should be knocked back when firing
    [SerializeField]
    protected float _knockback;

    //defines the arc in which the gun shoots
    [SerializeField]
    protected float _firingArc;

    //defines if bullet pattern should be random or regular within arc
    [SerializeField]
    protected bool _randomizePattern;

    //Screen shake parameters
    [SerializeField]
    protected float _screenShakeDuration = 0.1f;

    [SerializeField]
    protected float _screenShakeAmount = 0.25f;

    [SerializeField]
    protected int _screenShakeVibrato = 20;

    //all bullet related information
    [SerializeField]
    protected Bullet _bulletPrefab;

    //Sound that should be played when shooting
    [SerializeField]
    protected AudioSource _shotSound;

    //Sound that should be played when shooting
    [SerializeField]
    protected AudioSource _reloadSound;

    [SerializeField]
    protected AudioSource _chargeSound;

    [SerializeField]
    protected AudioSource _maxChargeSound;

    //Who is shooting
    protected IWeaponOwner _owner;
    protected bool _isPlayerOwned;

    protected float _adjustedCooldown;
    protected float _currentCooldown;
    protected float _currentChargeTime;
    protected int _bulletsLeft;
    protected float _halfAngle;
    protected float _degPerBullet;
    protected bool _reloading;
    protected bool _charging;
    protected Coroutine _firingSequence;
    protected Coroutine _reloadSequence;

    protected Color _reloadGoalColor = Color.green;
    protected Color _chargeGoalColor = Color.cyan;
    protected Color _clearColor = Utility.RGBAColor(0, 0, 0, 1.0f);

    protected float _superChargeTimeWindow;
    protected Tween _chargeFlash;
    protected bool _superCharged;

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (this.GetType() != obj.GetType())
        {
            return false;
        }

        //Temporary comparison
        Weapon other = (Weapon)obj;
        if (other.uiImage == uiImage)
        {
            return true;
        }

        return false;
    }

    protected virtual void Awake()
    {
        _superChargeTimeWindow = 0.25f;
        _currentCooldown = _cooldown;
        _adjustedCooldown = _cooldown;
        _currentChargeTime = 0.0f;
        _bulletsLeft = _magazineSize;
        _charging = false;
        _reloading = false;
        _halfAngle = _firingArc / 2.0f;
        _degPerBullet = _firingArc / _bulletsPerShot;
        _chargeFlash = null;
        _superCharged = false;

        UpdatePlayerUI();
    }

    public void SetOwner(IWeaponOwner owner, bool isPlayerOwned = false)
    {
        _owner = owner;
        _isPlayerOwned = isPlayerOwned;

        UpdatePlayerUI();
    }

    protected virtual IEnumerator FireBullets()
    {
        _shotSound.volume = SettingsManager.Instance.SFXVolume;
        _shotSound.Play();

        if (_isPlayerOwned)
        {
            CameraManager.Instance.ShakeCamera(_screenShakeDuration, _screenShakeAmount, _screenShakeVibrato);
        }

        float halfAngle = _firingArc / 2.0f;
        bool isSuperCharged = _superCharged;

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

            _currentChargeTime = Mathf.Min(_currentChargeTime, _chargeTime);
            float charge = _chargeTime > 0.0f ? _currentChargeTime / _chargeTime : 1.0f;
            BulletManager.Instance.SpawnBullet(_bulletPrefab, _owner.GetBulletOrigin(), rotation * _owner.GetAimVector(), 
                charge, _owner.GetGameObject(), isSuperCharged);

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

        if (_bulletsLeft <= 0 && _magazineSize > 0)
        {
            TriggerReload();
        }

        yield return null;
    }

    protected virtual IEnumerator Reload()
    {
        if (_isPlayerOwned)
        {
            UIManager.Instance.playerUI.SetChargeMeterColor(_clearColor);
            UIManager.Instance.playerUI.chargeMeter.value = 0.0f;
        }
      
        _charging = false;
        _currentChargeTime = 0.0f;

        if (_firingSequence != null)
        {
            StopCoroutine(_firingSequence);
        }

        _reloadSound.volume = SettingsManager.Instance.SFXVolume;
        _reloadSound.Play();

        _reloading = true;

        float timePassed = 0.0f;

        while (timePassed < _reloadTime)
        {
            if (_isPlayerOwned)
            {
                UIManager.Instance.playerUI.SetChargeMeterColor(_clearColor, _reloadGoalColor, timePassed / _reloadTime);
                UIManager.Instance.playerUI.chargeMeter.value = Mathf.Lerp(0.0f, 1.0f, timePassed / _reloadTime);
            }

            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }


        _bulletsLeft = _magazineSize;
        _reloading = false;

        UpdatePlayerUI();

        if (_isPlayerOwned)
        {
            UIManager.Instance.playerUI.SetChargeMeterColor(_clearColor);
            UIManager.Instance.playerUI.chargeMeter.value = 0.0f;
        }
       
        yield return null;
    }

    private void DestroyChargeFlash()
    {
        if (_chargeFlash != null)
        {
            _chargeFlash.Kill();
            _chargeFlash = null;
        }

        _superCharged = false;
    }

    public virtual void Shoot()
    {
        if ((_bulletsLeft <= 0 || _reloading) && _magazineSize > 0)
        {
            return;
        }

        if (_currentCooldown >= _adjustedCooldown)
        {
            _charging = true;

            if (_chargeSound != null && !_chargeSound.isPlaying && _currentChargeTime < _chargeTime)
            {
                _chargeSound.time = _currentChargeTime;
                _chargeSound.volume = SettingsManager.Instance.SFXVolume;
                _chargeSound.Play();
            }

            if (_currentChargeTime >= _chargeTime)
            {
                if (_currentChargeTime < _chargeTime + _superChargeTimeWindow && _chargeFlash == null)
                {
                    _chargeFlash = UIManager.Instance.playerUI.FlashChargeMeter(Color.red, _chargeGoalColor, _superChargeTimeWindow, 5);
                    _superCharged = true;
                }

                if (_currentChargeTime >= _chargeTime + _superChargeTimeWindow)
                {
                    DestroyChargeFlash();
                }

                if (_maxChargeSound != null && !_maxChargeSound.isPlaying)
                {
                    _maxChargeSound.volume = SettingsManager.Instance.SFXVolume;
                    _maxChargeSound.Play();
                }
            }

            _currentChargeTime += Time.deltaTime;

            if (_isPlayerOwned && _chargeTime > 0.0f)
            {
                if (_chargeFlash == null)
                {
                    UIManager.Instance.playerUI.SetChargeMeterColor(_clearColor, _chargeGoalColor, _currentChargeTime / _chargeTime);
                }
                UIManager.Instance.playerUI.chargeMeter.value = _currentChargeTime / _chargeTime;
            }

            if ((_currentChargeTime >= _chargeTime && !_resetChargeOnShot) ||_chargeTime == 0.0f)
            {
                Fire();
            }
        }
    }

    public virtual void StopShooting()
    {
        if (_firingSequence != null)
        {
            StopCoroutine(_firingSequence);
        }

        if (_reloadSequence != null)
        {
            _reloading = false;
            StopCoroutine(_reloadSequence);
        }

        UIManager.Instance.playerUI.SetChargeMeterColor(_clearColor);
        UIManager.Instance.playerUI.chargeMeter.value = 0.0f;
    }

    private void Fire()
    {
        if (_chargeSound != null)
        {
            _chargeSound.Stop();
        }

        if (_maxChargeSound != null)
        {
            _maxChargeSound.Stop();
        }

        if (_firingSequence != null)
        {
            StopCoroutine(_firingSequence);
        }

        if (_magazineSize > 0)
        {
            _bulletsLeft--;
        }
        _firingSequence = StartCoroutine(FireBullets());

        UpdatePlayerUI();

        if (_chargeTime > 0)
        {
            if (_currentChargeTime < _chargeTime)
            {
                _adjustedCooldown = _cooldown + _chargeTime - _currentChargeTime;
            }
            else
            {
                _adjustedCooldown = _cooldown;
            }
        }

        if (_resetChargeOnShot)
        {
            _currentChargeTime = 0.0f;
            _charging = false;

            if (_isPlayerOwned)
            {
                UIManager.Instance.playerUI.SetChargeMeterColor(_clearColor);
                UIManager.Instance.playerUI.chargeMeter.value = 0.0f;
            }
        }
        _currentCooldown = 0.0f;
        DestroyChargeFlash();
    }

    public void UpdatePlayerUI()
    {
        if (_isPlayerOwned)
        {
            string magSize = _magazineSize > 0 ? _magazineSize.ToString() : "∞";
            string bulletsLeft = _magazineSize > 0 ? _bulletsLeft.ToString() : "1";
            UIManager.Instance.playerUI.weaponText.text = bulletsLeft + "/" + magSize;
        }
    }

    public void TriggerReload()
    {
        if (_reloading || _bulletsLeft == _magazineSize)
        {
            return;
        }

        _reloadSequence = StartCoroutine(Reload());
    }

    public virtual void StoppedShooting()
    {
        if (_currentChargeTime > 0 && _chargeTime > 0)
        {
            Fire();
        }

        _charging = false;
    }

    protected virtual void Update()
    {
        _currentCooldown += Time.deltaTime;

        if (!_charging && _currentChargeTime > 0.0f && _chargeTime > 0.0f)
        {
            DestroyChargeFlash();

            _chargeSound?.Stop();
            _maxChargeSound?.Stop();

            _currentChargeTime -= Time.deltaTime;
            _currentChargeTime = Mathf.Max(0.0f, _currentChargeTime);

            if (_isPlayerOwned)
            {
                UIManager.Instance.playerUI.SetChargeMeterColor(_chargeGoalColor, _clearColor, _currentChargeTime / _chargeTime);
                UIManager.Instance.playerUI.chargeMeter.value = _currentChargeTime / _chargeTime;
            }
        }
    }
}
