using System;
using UnityEngine;

public class PlayerCameraTarget : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float maxOffset = 5f;
    [SerializeField, Range(0f, 1f)] private float aimWeight = 0.5f;
    [SerializeField, Range(0f, 1f)] private float offsetSmoothTime = 0.4f;

    private Vector3 _currentOffset;
    private Vector3 _offsetVelocity;
    private bool _isAiming = false;
    private Vector3 _mouseWorldPos;
    
    public void SetAimState(bool aiming) => _isAiming = aiming;
    public void SetMouseWorldPos(Vector3 mouseWorldPos) => _mouseWorldPos = mouseWorldPos;

    private void LateUpdate()
    {
        var basePos = target.position;
        var offset = Vector3.zero;

        if (_isAiming)
        {
            var dir = _mouseWorldPos - target.position;
            dir.y = 0f;
            offset = Vector3.ClampMagnitude(dir * aimWeight, maxOffset);
        }
        
        _currentOffset = Vector3.SmoothDamp(_currentOffset, offset, ref _offsetVelocity, offsetSmoothTime);
        
        transform.position = basePos + _currentOffset;
    }
}
