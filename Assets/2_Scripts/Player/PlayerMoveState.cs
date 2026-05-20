using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveState : BaseState<PlayerStateController>
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float aimSpeed;
    [SerializeField] private float runRotationSpeed;
    
    private InputAction _moveAction;
    private readonly int _speedKey = Animator.StringToHash("Speed");

    public override void Init(BaseStateController controller)
    {
        base.Init(controller);
        
        _moveAction = Controller.GameInput.Player.Move;
    }

    public override void OnStateEnter()
    {
     
    }

    public override void OnUpdate()
    {
        var input = _moveAction.ReadValue<Vector2>();
        var curSpeed = NormalizeSpeed();
        var dir = new Vector3(input.x, 0, input.y).normalized * moveSpeed;

        // if (Controller.SubState == null && dir.sqrMagnitude > 0.01f)
        // {
        //     var targetRot = Quaternion.LookRotation(dir);
        //     Controller.transform.rotation = Quaternion.RotateTowards(
        //         Controller.transform.rotation,
        //         targetRot,
        //         runRotationSpeed * Time.deltaTime);
        // }
        
        Controller.Cc.Move(dir * Time.deltaTime);
        Controller.Animator.SetFloat(_speedKey, curSpeed, 0.15f, Time.deltaTime);
    }

    public override void OnStateExit()
    {
        
    }

    private float NormalizeSpeed()
    {
        var rawSpeed = Controller.Cc.velocity.magnitude;
        var normalize = rawSpeed / runSpeed;

        return normalize;
    }
}
