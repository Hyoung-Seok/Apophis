using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;

    private PlayerStateController _controller;
    private InputAction _lookAction;
    private Camera _camera;

    public void Init(PlayerStateController controller)
    {
        _controller = controller;
    }
        
    private void Start()
    {
        _lookAction = _controller.GameInput.Player.Look;
        _camera = Camera.main;
    }

    private void Update()
    {
        var screen = _lookAction.ReadValue<Vector2>();
        var ray = _camera.ScreenPointToRay(screen);
        
        var plane = new Plane(Vector3.up, _controller.transform.position);
        if (!plane.Raycast(ray, out var dist)) return;
        
        var aimPoint = ray.GetPoint(dist);
        _controller.CameraTarget.SetMouseWorldPos(aimPoint);
        var dir = aimPoint - _controller.transform.position;
        dir.y = 0;
        _controller.EquippedWeapon.SetAimDir(dir);

        if (dir.sqrMagnitude < 0.01f) return;
        
        var targetRot = Quaternion.LookRotation(dir);
        _controller.transform.rotation = Quaternion.RotateTowards(
            _controller.transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime);
    }
}
