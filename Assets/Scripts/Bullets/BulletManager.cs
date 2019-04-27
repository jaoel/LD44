using UnityEngine;
using System.Collections.Generic;

public class BulletManager : MonoBehaviour {
    public int bulletZIndex = -3;
    private int preAllocateCount = 50;

    private GameObject bulletPoolWrapper;

    // Dictionary indexed by prefab
    private Dictionary<Bullet, List<BulletInstance>> bulletList = new Dictionary<Bullet, List<BulletInstance>>();

    private static BulletManager instance = null;
    public static BulletManager Instance {
        get {
            if(instance != null) {
                return instance;
            }
            instance = FindObjectOfType<BulletManager>();
            if(instance == null || instance.Equals(null)) {
                Debug.LogError("The scene needs a BulletManager");
            }
            return instance;
        }
    }

    void Start() {
        bulletPoolWrapper = new GameObject("Bullet Pool");
        bulletPoolWrapper.transform.parent = transform;
    }

    void FixedUpdate() {
        foreach(var keyValue in bulletList) {
            List<BulletInstance> bullets = keyValue.Value;
            foreach(BulletInstance bullet in bullets) {
                if (bullet.active) {
                    bullet.lifetime -= Time.deltaTime;
                    if (bullet.lifetime <= 0f) {
                        bullet.instance.gameObject.SetActive(false);
                        bullet.active = false;
                    } else {
                        bullet.instance.transform.position += (Vector3)bullet.velocity * Time.deltaTime;
                    }
                }
            }
        }
    }

    private void OnDestroy() {
        Destroy(bulletPoolWrapper);
        bulletList.Clear();
        instance = null;
    }

    public void SpawnBullet(BulletDescription description, Vector2 position, Vector2 velocity) {
        PreAllocateBullets(description);
        BulletInstance availableBullet = FindAvailableBulletInstance(description);

        Bullet bullet = availableBullet.instance;
        bullet.gameObject.SetActive(true);
        bullet.transform.position = new Vector3(position.x, position.y, bulletZIndex);
        bullet.SetSize(description.size);
        bullet.SetTint(description.tint);
        bullet.description = description;

        availableBullet.velocity = velocity;
        availableBullet.lifetime = description.lifetime;
        availableBullet.active = true;
    }

    private BulletInstance FindAvailableBulletInstance(BulletDescription description) {
        List<BulletInstance> bulletInstances = bulletList[description.bulletPrefab];
        for (int i = 0; i < bulletInstances.Count; i++) {
            BulletInstance bulletInstance = bulletInstances[i];
            if (!bulletInstance.active) {
                return bulletInstance;
            }
        }
        
        BulletInstance newInstance = InstantiateBullet(description, true);
        bulletInstances.Add(newInstance);

        return newInstance;
    }

    public void Clear() {
        Destroy(bulletPoolWrapper);
        bulletList.Clear();
        Start();
    }

    private void PreAllocateBullets(BulletDescription description) {
        if (bulletList.ContainsKey(description.bulletPrefab)) {
            return;
        }

        List<BulletInstance> bullets = new List<BulletInstance>();
        for(int i = 0; i < preAllocateCount; i++) {
            bullets.Add(InstantiateBullet(description, false));
        }
        bulletList[description.bulletPrefab] = bullets;
    }

    private BulletInstance InstantiateBullet(BulletDescription description, bool active) {
        Bullet instantiatedBullet = Instantiate<Bullet>(description.bulletPrefab, bulletPoolWrapper.transform);
        instantiatedBullet.gameObject.SetActive(active);
        BulletInstance instance = new BulletInstance {
            active = active,
            description = description,
            instance = instantiatedBullet,
            lifetime = 0f,
            velocity = Vector2.zero,
        };
        return instance;
    }
}
