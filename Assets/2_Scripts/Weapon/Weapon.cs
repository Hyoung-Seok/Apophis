using UnityEngine;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
{
    public EFireMode CurFireMode => _curFireMode;
    public void SetAimDir(Vector3 dir) => _curAimDir = dir;
    
    [SerializeField] private WeaponData data;
    [SerializeField] private Transform firePoint;

    private EFireMode _curFireMode;
    private EAimMode _aimMode;
    private Magazine _currentMag;
    private BulletPool _bulletPool;
    private float _lastTime = float.NegativeInfinity;
    
    private float _baseHalfAngle;
    private float _curHalfAngle;
    private float _maxHalfAngle;
    private float _curIncRate;
    private float _curRecRate;
    
    private bool _isFiring = false;
    private int _burstCount = 0;
    private Vector3 _curAimDir;
    private const int BURST_SHOT_COUNT = 3;
    private const float AIM_ANGLE_THRESHOLD = 10f;
    
    protected virtual void Start()
    {
        _curFireMode = data.SupportedMode[0];
        SetAimMode(EAimMode.Hip);
        _bulletPool = GameManager.Instance.Get<BulletPool>();
    }

    private void Update()
    {
        UpdateFiring();
        UpdateSpreadRecovery();
    }
    
    public virtual void OnFirePress()
    {
        if (_isFiring) return;
        _isFiring = true;
    }

    public virtual void OnFireRelease()
    {
        if (_burstCount != 0) return;
        
        _isFiring = false;
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

    public void SwitchFireMode()
    {
        if (_isFiring) return;
        
        var curModeIndex = data.SupportedMode.IndexOf(_curFireMode);
        var nextIndex = (curModeIndex + 1) % data.SupportedMode.Count;
        _curFireMode = data.SupportedMode[nextIndex];
        
        Debug.Log($"Current Fire Mode: {_curFireMode}");
    }
    
    protected virtual bool TryFire()
    {
        if (_currentMag.IsEmpty)
        {
            return false;
        }
        
        if (Time.time - _lastTime < data.FireDelay) return false;
        
        var bullet = _bulletPool.Get(_currentMag.LoadedAmmo);
        var dir = CalculateRecoil();
        
        bullet.Init(_currentMag.LoadedAmmo, firePoint.position, dir, gameObject);

        _currentMag.CurrentAmmo--;

        _lastTime = Time.time;
        return true;
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

    private void UpdateFiring()
    {
        if (_isFiring == false) return;

        if (Vector3.Angle(firePoint.forward, _curAimDir) > AIM_ANGLE_THRESHOLD) return;
 
        switch (_curFireMode)
        {
            case EFireMode.Single:
                if(TryFire()) _isFiring = false;
                break;
            
            case EFireMode.Burst:
                if (TryFire() == false) return;

                _burstCount++;
                if (_burstCount < BURST_SHOT_COUNT) return;
                
                _burstCount = 0;
                _isFiring = false;
                break;

            case EFireMode.Auto:
                TryFire();
                break;
            
            default:
                return;
        }
    }

    private void UpdateSpreadRecovery()
    {
        if (_curHalfAngle <= _baseHalfAngle) return;
        
        _curHalfAngle = Mathf.MoveTowards(_curHalfAngle, _baseHalfAngle,
            Time.deltaTime * _curRecRate);
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