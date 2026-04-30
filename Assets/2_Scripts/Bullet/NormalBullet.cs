using UnityEngine;

public class NormalBullet : Bullet
{
    public override void OnHit(IDamageable hit)
    {
        // TODO : 데미지 계산식 세운 뒤 적용할것
        hit.TakeDamage(Data.BaseDamage);
    }
}
