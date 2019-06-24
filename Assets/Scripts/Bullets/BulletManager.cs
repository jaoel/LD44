using UnityEngine;
using System.Collections.Generic;

public class BulletManager : MonoBehaviour
{
    private int _preAllocateCount = 50;
    private GameObject _bulletPoolWrapper;
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
                    if (!bullet.instance.UpdateLifetime())
                    {
                        bullet.instance.BeforeDestroyed(null);
                        bullet.instance.gameObject.SetActive(false);
                        bullet.active = false;
                    }
                    else
                    {
                        bullet.instance.UpdateBullet();
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

    public Bullet SpawnBullet(Bullet prefab, Vector2 position, Vector2 direction, float charge, GameObject owner)
    {
        PreAllocateBullets(prefab);
        BulletInstance availableBullet = FindAvailableBulletInstance(prefab);
        availableBullet.active = true;

        Bullet bullet = availableBullet.instance;
        bullet.gameObject.SetActive(true);
        bullet.transform.position = new Vector3(position.x, position.y, 0.0f);
        bullet.Initialize(charge, direction, owner);

        return bullet;
    }

    private BulletInstance FindAvailableBulletInstance(Bullet prefab)
    {
        List<BulletInstance> bulletInstances = _bulletList[prefab];
        for (int i = 0; i < bulletInstances.Count; i++)
        {
            BulletInstance bulletInstance = bulletInstances[i];
            if (!bulletInstance.active)
            {
                return bulletInstance;
            }
        }

        BulletInstance newInstance = InstantiateBullet(prefab, true);
        bulletInstances.Add(newInstance);

        return newInstance;
    }

    public void Clear()
    {
        Destroy(_bulletPoolWrapper);
        _bulletList.Clear();
        Start();
    }

    private void PreAllocateBullets(Bullet prefab)
    {
        if (_bulletList.ContainsKey(prefab))
        {
            return;
        }

        List<BulletInstance> bullets = new List<BulletInstance>();
        for (int i = 0; i < _preAllocateCount; i++)
        {
            bullets.Add(InstantiateBullet(prefab, false));
        }
        _bulletList[prefab.GetComponent<Bullet>()] = bullets;
    }

    private BulletInstance InstantiateBullet(Bullet prefab, bool active)
    {
        Bullet bullet = Instantiate<Bullet>(prefab, _bulletPoolWrapper.transform);
        bullet.gameObject.SetActive(active);
        BulletInstance instance = new BulletInstance
        {
            active = active,
            instance = bullet
        };
        return instance;
    }
}
