using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveState : BaseState<PlayerStateController>
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float rotationSpeed;
    
    private InputAction _moveAction;

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
        var dir = new Vector3(input.x, 0, input.y).normalized * moveSpeed;

        if (dir.sqrMagnitude > 0.01f)
        {
            var targetRot = Quaternion.LookRotation(dir);
            Controller.transform.rotation = Quaternion.RotateTowards(
                Controller.transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime);
        }
        
        Controller.Cc.Move(dir * Time.deltaTime);
    }

    public override void OnStateExit()
    {
        
    }
}
