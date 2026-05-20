using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateController : BaseStateController
{
    public GameInput GameInput {get; private set; }
    public CharacterController Cc => cc;
    public Weapon EquippedWeapon => equippedWeapon;
    public PlayerCameraTarget CameraTarget => cameraTarget;
    public Animator Animator => animator;
    
    [Header("Components")]
    [SerializeField] private CharacterController cc;
    [SerializeField] private Weapon equippedWeapon;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private PlayerCameraTarget cameraTarget;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerAimController aimController;

    private bool _isFiring = false;
    private bool _isAiming = false;
    
    protected override void Awake()
    {
        GameInput = new GameInput();
        GameInput.Enable();
        
        aimController.Init(this);
        
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
        _isAiming = true;
        
        cameraTarget.SetAimState(_isAiming);
        equippedWeapon.SetAimMode(EAimMode.Aimed);
    }

    private void OnAimStop(InputAction.CallbackContext _)
    {
        _isAiming = false;
        
        cameraTarget.SetAimState(_isAiming);
        equippedWeapon.SetAimMode(EAimMode.Hip);
    }

    private void OnFireStart(InputAction.CallbackContext _)
    {
        _isFiring = true;
        EquippedWeapon.OnFirePress();
    }

    private void OnFireEnd(InputAction.CallbackContext _)
    {
        _isFiring = false;
        EquippedWeapon.OnFireRelease();
    }
    
    private void OnSwitchFireMode(InputAction.CallbackContext _) => equippedWeapon.SwitchFireMode();
}
