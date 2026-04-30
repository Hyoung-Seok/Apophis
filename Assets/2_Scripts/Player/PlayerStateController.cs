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

    private bool _isFiring = false;

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

    protected override void Update()
    {
        base.Update();
        
        if (_isFiring == true)
        {
            equippedWeapon.TryFire();
        }
    }

    private void OnEnable()
    {
        GameInput.Player.Aim.started += OnAimStart;
        GameInput.Player.Aim.canceled += OnAimStop;

        GameInput.Player.Fire.started += OnFireStart;
        GameInput.Player.Fire.canceled += OnFireEnd;
    }

    private void OnDisable()
    {
        GameInput.Player.Aim.started -= OnAimStart;
        GameInput.Player.Aim.canceled -= OnAimStop;
        
        GameInput.Player.Fire.started -= OnFireStart;
        GameInput.Player.Fire.canceled -= OnFireEnd;
    }

    private void OnAimStart(InputAction.CallbackContext _)
    {
        ChangeSubState(GetState<PlayerAimState>());
    }

    private void OnAimStop(InputAction.CallbackContext _)
    {
        ClearSubState();
    }

    private void OnFireStart(InputAction.CallbackContext _) => _isFiring = true;
    private void OnFireEnd(InputAction.CallbackContext _) => _isFiring = false;
}
