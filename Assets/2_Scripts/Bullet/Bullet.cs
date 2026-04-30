using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    protected BulletData Data;
    private Vector3 _dir;
    private GameObject _owner;

    private const float LIFE_TIME = 2f;
    private float _curTime = 0f;

    public abstract void OnHit(IDamageable hit);

    public void Init(BulletData data, Vector3 dir, GameObject owner)
    {
        Data = data;
        _dir = dir;
        _owner = owner;
    }

    private void Update()
    {
        if (_curTime >= LIFE_TIME)
        {
            // TODO : 나중에 풀로 돌아가도록 교체
            Destroy(gameObject);
            return;
        }
        
        transform.position += _dir * Data.MuzzleVelocity *  Time.deltaTime;
        _curTime += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var hit) == true)
        {
            OnHit(hit);
        }
        
        // TODO : 풀로 돌아가도록 교체
        Destroy(gameObject);
    }
}
