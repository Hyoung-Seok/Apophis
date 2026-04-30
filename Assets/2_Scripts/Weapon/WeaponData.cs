using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    public ECaliber Caliber => caliber;
    public float FireDelay => fireDelay;
    public float BaseSpreadAngel => baseSpreadAngle;
    public float MaxSpreadAngle => maxSpreadAngle;
    public float SpreadIncRate => spreadIncRate;
    public float SpreadRecRate => spreadRecRate;

    [SerializeField] private ECaliber caliber;
    [SerializeField] private float fireDelay;
    [SerializeField, Range(0f, 90f)] private float baseSpreadAngle;
    [SerializeField, Range(0f, 90f)] private float maxSpreadAngle;
    [SerializeField, Range(0f, 1f)] private float spreadIncRate;
    [SerializeField, Range(0.1f, 5f)] private float spreadRecRate;
}
