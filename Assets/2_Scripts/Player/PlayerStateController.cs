using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateController : BaseStateController
{
    public GameInput GameInput {get; private set; }
    public CharacterController Cc => cc;
    public Weapon EquippedWeapon => equippedWeapon;

    [SerializeField] private CharacterController cc;
    [SerializeField] private Weapon equippedWeapon;
    [SerializeField] private PlayerInventory inventory;

    protected override void Awake()
    {
        GameInput = new GameInput();
        GameInput.Enable();
        
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        equippedWeapon.ChangeMagazine(inventory.Magazines[0]);
    }

    private void OnEnable()
    {
        GameInput.Player.Aim.started += OnAimStart;
        GameInput.Player.Aim.canceled += OnAimStop;

        GameInput.Player.Fire.started += OnFireStart;
        GameInput.Player.Fire.canceled += OnFireEnd;

        GameInput.Player.ChangeFireMode.started += OnSwitchFireMode;
    }

    private void OnDisable()
    {
        GameInput.Player.Aim.started -= OnAimStart;
        GameInput.Player.Aim.canceled -= OnAimStop;
        
        GameInput.Player.Fire.started -= OnFireStart;
        GameInput.Player.Fire.canceled -= OnFireEnd;
        
        GameInput.Player.ChangeFireMode.started -= OnSwitchFireMode;
    }

    private void OnAimStart(InputAction.CallbackContext _)
    {
        ChangeSubState(GetState<PlayerAimState>());
    }

    private void OnAimStop(InputAction.CallbackContext _)
    {
        ClearSubState();
    }

    private void OnFireStart(InputAction.CallbackContext _) => equippedWeapon.OnFirePress();
    private void OnFireEnd(InputAction.CallbackContext _) => equippedWeapon.OnFireRelease();
    private void OnSwitchFireMode(InputAction.CallbackContext _) => equippedWeapon.SwitchFireMode();
}
