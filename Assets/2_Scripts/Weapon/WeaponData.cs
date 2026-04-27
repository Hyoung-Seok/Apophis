using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    public ECaliber Caliber => caliber;
    public float FireDelay => fireDelay;

    [SerializeField] private ECaliber caliber;
    [SerializeField] private float fireDelay;
}
