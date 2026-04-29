using System;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public void SetAimMode(EAimMode mode) => _aimMode = mode;
    
    [SerializeField] private WeaponData data;
    [SerializeField] private Transform firePoint;
    
    private EAimMode _aimMode;
    private Magazine _currentMag;
    private float _lastTime = float.NegativeInfinity;
    
    public virtual bool TryFire()
    {
        if(_currentMag.IsEmpty) return false;
        if (Time.time - _lastTime < data.FireDelay) return false;
        
        // TODO : 나중에 반동에 의한 사격각을 조절하는 코드 추가. 지금은 임시 코드
        // TODO : ObjPooling을 이용해 총알 생성할것
        // TODO : 총알 감소 로직 추가할것.
        var bullet = Instantiate(_currentMag.LoadedAmmo.BulletPrefab, firePoint.position, 
            Quaternion.LookRotation(firePoint.forward));
        bullet.Init(_currentMag.LoadedAmmo, firePoint.forward, gameObject);

        _lastTime = Time.time;
        return true;
    }
    
    public Magazine ChangeMagazine(Magazine magazine)
    {
        var prevMag = _currentMag;
        _currentMag = magazine;
        
        return prevMag;
    }
}