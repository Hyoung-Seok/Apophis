using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour, IGameSystem
{
    private Dictionary<BulletData, ObjectPool<Bullet>> _bulletPools;

    public void Init()
    {
        _bulletPools = new Dictionary<BulletData, ObjectPool<Bullet>>();
    }
    
    public Bullet Get(BulletData data)
    {
        var pool = AddBullet(data);
        return pool.Get();
    }
    
    private ObjectPool<Bullet> AddBullet(BulletData data)
    {
        if (_bulletPools.TryGetValue(data, out var result))
        {
            return result; 
        }

        ObjectPool<Bullet> pool = null;
        pool =  new ObjectPool<Bullet>(
            createFunc: () =>
            {
                var bullet = Instantiate(data.BulletPrefab, transform);
                bullet.SetPool(pool);
                return bullet;
            },
            OnGetBullet,
            OnReleaseBullet,
            OnDestroyBullet
        );
        
        _bulletPools.Add(data, pool);
        return pool;
    }
    
    private void OnGetBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(true);
    }

    private void OnReleaseBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void OnDestroyBullet(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }
}


