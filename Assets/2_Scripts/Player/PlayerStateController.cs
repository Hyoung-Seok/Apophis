using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateController : BaseStateController
{
    public GameInput GameInput {get; private set; }
    public CharacterController Cc => cc;

    [Header("Components")] 
    [SerializeField] private CharacterController cc;

    protected override void Awake()
    {
        GameInput = new GameInput();
        GameInput.Enable();
        
        base.Awake();
    }

    private void OnEnable()
    {
        GameInput.Player.Aim.started += OnAimStart;
        GameInput.Player.Aim.canceled += OnAimStop;
        
    }

    private void OnDisable()
    {
        GameInput.Player.Aim.started -= OnAimStart;
        GameInput.Player.Aim.canceled -= OnAimStop;
    }

    private void OnAimStart(InputAction.CallbackContext _)
    {
        ChangeSubState(GetState<PlayerAimState>());
    }

    private void OnAimStop(InputAction.CallbackContext _)
    {
        ClearSubState();
    }
}
