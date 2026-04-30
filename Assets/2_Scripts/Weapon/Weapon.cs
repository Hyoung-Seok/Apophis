using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
{
    [SerializeField] private WeaponData data;
    [SerializeField] private Transform firePoint;

    private EAimMode _aimMode;
    private Magazine _currentMag;
    private float _lastTime = float.NegativeInfinity;
    
    private float _baseHalfAngle;
    private float _curHalfAngle;
    private float _maxHalfAngle;
    private float _curIncRate;
    private float _curRecRate;
    
    protected virtual void Start()
    {
        SetAimMode(EAimMode.Hip);
    }

    private void Update()
    {
        if (_curHalfAngle <= _baseHalfAngle) return;
        
        _curHalfAngle = Mathf.MoveTowards(_curHalfAngle, _baseHalfAngle,
            Time.deltaTime * _curRecRate);
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

    public void SetAimMode(EAimMode mode)
    { 
        _aimMode = mode;
        
        _baseHalfAngle = data.BaseSpreadAngle / 2f;
        _curHalfAngle = _baseHalfAngle;
        
        switch (_aimMode)
        {
            case EAimMode.Hip:
                _maxHalfAngle = data.HipMaxSpreadAngle / 2f;

                _curIncRate = data.HipSpreadIncRate;
                _curRecRate = data.HipSpreadRecRate;
                break;
            
            case EAimMode.Aimed:
                _maxHalfAngle = data.MaxSpreadAngle / 2f;

                _curIncRate = data.SpreadIncRate;
                _curRecRate = data.SpreadRecRate;
                break;
            
            default:
                return;
        }
    } 

    protected virtual Vector3 CalculateRecoil()
    {
        if (_curHalfAngle < _maxHalfAngle)
        {
            _curHalfAngle += (_maxHalfAngle - _curHalfAngle) * _curIncRate;
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