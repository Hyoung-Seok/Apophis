using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimController : MonoBehaviour
{
    [SerializeField] private PlayerStateController controller;
    [SerializeField] private float rotationSpeed;
    
    private InputAction _lookAction;
    private Camera _camera;

    private void Start()
    {
        _lookAction = controller.GameInput.Player.Look;
        _camera = Camera.main;
    }

    private void Update()
    {
        var screen = _lookAction.ReadValue<Vector2>();
        var ray = _camera.ScreenPointToRay(screen);
        
        var plane = new Plane(Vector3.up, controller.transform.position);
        if (!plane.Raycast(ray, out var dist)) return;
        
        var aimPoint = ray.GetPoint(dist);
        controller.CameraTarget.SetMouseWorldPos(aimPoint);
        var dir = aimPoint - controller.transform.position;
        dir.y = 0;
        controller.EquippedWeapon.SetAimDir(dir);

        if (dir.sqrMagnitude < 0.01f) return;
        
        var targetRot = Quaternion.LookRotation(dir);
        controller.transform.rotation = Quaternion.RotateTowards(
            controller.transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime);
    }
}
