using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    [SerializeField] private BulletData data;

    public abstract void OnHit(IDamageable hit);

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var hit) == true)
        {
            OnHit(hit);
        }
    }
}
