using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFireState : BaseState<PlayerStateController>
{
    [SerializeField] private float rotateSpeed = 1080f;

    private InputAction _lookAction;
    private Camera _camera;
    
    public override void Init(BaseStateController controller)
    {
        base.Init(controller);

        _lookAction = Controller.GameInput.Player.Look;
        _camera = Camera.main;
    }
    
    public override void OnStateEnter()
    {

    }

    public override void OnUpdate()
    {
        var screen = _lookAction.ReadValue<Vector2>();
        var ray = _camera.ScreenPointToRay(screen);
        
        var plane = new Plane(Vector3.up, Controller.transform.position);
        if (!plane.Raycast(ray, out var dist)) return;
        
        var aimPoint = ray.GetPoint(dist);
        Controller.CameraTarget.SetMouseWorldPos(aimPoint);
        var dir = aimPoint - Controller.transform.position;
        dir.y = 0;
        Controller.EquippedWeapon.SetAimDir(dir);

        if (dir.sqrMagnitude < 0.01f) return;
        
        var targetRot = Quaternion.LookRotation(dir);
        Controller.transform.rotation = Quaternion.RotateTowards(
            Controller.transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime);
    }

    public override void OnStateExit()
    {

    }
}
