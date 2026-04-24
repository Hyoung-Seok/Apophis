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
}
