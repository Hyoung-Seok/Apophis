using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
{
    public void SetAimMode(EAimMode mode) => _aimMode = mode;
    
    [SerializeField] private WeaponData data;
    [SerializeField] private Transform firePoint;
    
    private EAimMode _aimMode;
    private Magazine _currentMag;
    private float _lastTime = float.NegativeInfinity;
    
    private float _curHalfAngle;
    private float _baseHalfAngle;
    private float _maxHalfAngle;
    
    protected virtual void Start()
    {
        _baseHalfAngle = data.BaseSpreadAngel / 2f;
        _curHalfAngle = _baseHalfAngle;
        _maxHalfAngle = data.MaxSpreadAngle / 2f;
    }

    private void Update()
    {
        if (_curHalfAngle <= _baseHalfAngle) return;
        
        _curHalfAngle = Mathf.MoveTowards(_curHalfAngle, _baseHalfAngle,
            Time.deltaTime * data.SpreadRecRate);
    }
    
    public virtual bool TryFire()
    {
        if (_currentMag.IsEmpty)
        {
            return false;
        }
        
        if (Time.time - _lastTime < data.FireDelay) return false;
        
        // TODO : ObjPooling을 이용해 총알 생성할것
        var bullet = Instantiate(_currentMag.LoadedAmmo.BulletPrefab, firePoint.position, 
            Quaternion.LookRotation(firePoint.forward));
        
        var dir = CalculateRecoil();
        bullet.Init(_currentMag.LoadedAmmo, dir, gameObject);

        _currentMag.CurrentAmmo--;

        _lastTime = Time.time;
        return true;
    }
    
    public Magazine ChangeMagazine(Magazine magazine)
    {
        var prevMag = _currentMag;
        _currentMag = magazine;
        
        return prevMag;
    }

    protected virtual Vector3 CalculateRecoil()
    {
        if (_curHalfAngle < _maxHalfAngle)
        {
            _curHalfAngle += (_maxHalfAngle - _curHalfAngle) * data.SpreadIncRate;
        }
        var randAngle = Random.Range(-_curHalfAngle, _curHalfAngle);
        var dir = Quaternion.AngleAxis(randAngle, Vector3.up) * firePoint.forward;

        return dir;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
        var left = Quaternion.AngleAxis(-_curHalfAngle, Vector3.up) * firePoint.forward;
        var right = Quaternion.AngleAxis(_curHalfAngle, Vector3.up) * firePoint.forward;
        
        var pos = firePoint.position;
        Gizmos.DrawLine(pos, pos + left * 10f);
        Gizmos.DrawLine(pos, pos + right * 10f);
    }
}