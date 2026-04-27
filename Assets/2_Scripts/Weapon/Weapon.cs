using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponData data;
    private Magazine _currentMag;

    public virtual bool TryFire()
    {
        return true;
    }

    public Magazine ChangeMagazine(Magazine magazine)
    {
        return null;
    }
}