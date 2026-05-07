using UnityEngine;
using UnityEngine.Pool;

public abstract class Bullet : MonoBehaviour
{
    protected BulletData Data;
    private Vector3 _dir;
    private GameObject _owner;

    private const float LIFE_TIME = 2f;
    private float _curTime = 0f;
    private ObjectPool<Bullet> _pool;

    protected abstract void OnHit(IDamageable hit);

    public void Init(BulletData data, Vector3 pos, Vector3 dir, GameObject owner)
    {
        Data = data;
        _dir = dir;
        _owner = owner;
        _curTime = 0;

        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    public void SetPool(ObjectPool<Bullet> pool)
    {
        _pool = pool;
    }

    private void Update()
    {
        if (_curTime >= LIFE_TIME)
        {
            _pool.Release(this);
            return;
        }
        
        transform.position += _dir * (Data.MuzzleVelocity *  Time.deltaTime);
        _curTime += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var hit) == true)
        {
            OnHit(hit);
        }
        
        _pool.Release(this);
    }
}
