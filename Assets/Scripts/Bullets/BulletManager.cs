using UnityEngine;
using System.Collections.Generic;

public class BulletManager : MonoBehaviour
{
    private int _preAllocateCount = 50;

    private GameObject _bulletPoolWrapper;

    // Dictionary indexed by prefab
    private Dictionary<Bullet, List<BulletInstance>> _bulletList = new Dictionary<Bullet, List<BulletInstance>>();

    private static BulletManager _instance = null;
    public static BulletManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            _instance = FindObjectOfType<BulletManager>();
            if (_instance == null || _instance.Equals(null))
            {
                Debug.LogError("The scene needs a BulletManager");
            }
            return _instance;
        }
    }

    void Start()
    {
        _bulletPoolWrapper = new GameObject("Bullet Pool");
        _bulletPoolWrapper.transform.parent = transform;
    }

    void FixedUpdate()
    {
        foreach (var keyValue in _bulletList)
        {
            List<BulletInstance> bullets = keyValue.Value;
            foreach (BulletInstance bullet in bullets)
            {
                if (!bullet.instance.gameObject.activeInHierarchy)
                {
                    bullet.active = false;
                }

                if (bullet.active)
                {
                    bullet.lifetime -= Time.deltaTime;
                    if (bullet.lifetime <= 0f)
                    {
                        bullet.instance.BeforeDestroyed();
                        bullet.instance.gameObject.SetActive(false);
                        bullet.active = false;
                    }
                    else
                    {
                        bullet.instance.UpdateBullet(bullet);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        Destroy(_bulletPoolWrapper);
        _bulletList.Clear();
        _instance = null;
    }

    public void SpawnBullet(BulletDescription description, Vector2 position, Vector2 velocity, GameObject owner)
    {
        PreAllocateBullets(description);
        BulletInstance availableBullet = FindAvailableBulletInstance(description);

        Bullet bullet = availableBullet.instance;
        bullet.gameObject.SetActive(true);
        bullet.transform.position = new Vector3(position.x, position.y, 0.0f);
        bullet.SetSize(description.size);
        bullet.SetTint(description.tint);
        bullet.SetOwner(owner);
        bullet.SetVelocity(velocity);
        bullet.Description = description;

        availableBullet.velocity = velocity;
        availableBullet.lifetime = description.lifetime;
        availableBullet.active = true;
    }

    private BulletInstance FindAvailableBulletInstance(BulletDescription description)
    {
        List<BulletInstance> bulletInstances = _bulletList[description.bulletPrefab];
        for (int i = 0; i < bulletInstances.Count; i++)
        {
            BulletInstance bulletInstance = bulletInstances[i];
            if (!bulletInstance.active)
            {
                return bulletInstance;
            }
        }

        BulletInstance newInstance = InstantiateBullet(description, true);
        bulletInstances.Add(newInstance);

        return newInstance;
    }

    public void Clear()
    {
        Destroy(_bulletPoolWrapper);
        _bulletList.Clear();
        Start();
    }

    private void PreAllocateBullets(BulletDescription description)
    {
        if (_bulletList.ContainsKey(description.bulletPrefab))
        {
            return;
        }

        List<BulletInstance> bullets = new List<BulletInstance>();
        for (int i = 0; i < _preAllocateCount; i++)
        {
            bullets.Add(InstantiateBullet(description, false));
        }
        _bulletList[description.bulletPrefab] = bullets;
    }

    private BulletInstance InstantiateBullet(BulletDescription description, bool active)
    {
        Bullet instantiatedBullet = Instantiate<Bullet>(description.bulletPrefab, _bulletPoolWrapper.transform);
        instantiatedBullet.gameObject.SetActive(active);
        BulletInstance instance = new BulletInstance
        {
            active = active,
            description = description,
            instance = instantiatedBullet,
            lifetime = 0f,
            velocity = Vector2.zero,
        };
        return instance;
    }
}
