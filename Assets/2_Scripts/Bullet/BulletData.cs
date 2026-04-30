using UnityEngine;

[CreateAssetMenu(fileName = "BulletData", menuName = "Weapon/BulletData")]
public class BulletData : ScriptableObject
{
    public float BaseDamage => baseDamage;
    public float CriticalChance => criticalChance;
    public float CriticalRatio => criticalRatio;
    public float MuzzleVelocity => muzzleVelocity;
    public float RecoilMultiplier => recoilMultiplier;
    public Bullet BulletPrefab => bulletPrefab;

    [SerializeField] private float baseDamage;
    [SerializeField] private float criticalChance;
    [SerializeField] private float criticalRatio;
    [SerializeField] private float muzzleVelocity;
    [SerializeField] private float recoilMultiplier;
    [SerializeField] private Bullet bulletPrefab;
}
