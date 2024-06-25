using UnityEngine;
using UnityEngine.InputSystem;
public class BasisLocalInputActions : MonoBehaviour
{
    public InputActionReference MoveAction;
    public InputActionReference LookAction;
    public InputActionReference JumpAction;
    public InputActionReference CrouchAction;
    public InputActionReference RunButton;
    public InputActionReference Escape;

    public InputActionReference DesktopSwitch;
    public InputActionReference XRSwitch;

    public InputActionReference LeftMousePressed;
    public InputActionReference RightMousePressed;

    [SerializeField] public bool Crouching;
    [SerializeField] public Vector2 LookDirection;
    public BasisAvatarEyeInput CharacterEyeInput;
    public static BasisLocalInputActions Instance;
    public BasisLocalPlayer basisLocalPlayer;
    public PlayerInput Input;
    public void OnEnable()
    {
        DesktopSwitch.action.Enable();
        XRSwitch.action.Enable();

        MoveAction.action.Enable();
        LookAction.action.Enable();
        JumpAction.action.Enable();
        AddCallback();
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        BasisLocalCameraDriver.InstanceExists += SetupCamera;
    }
    public void SetupCamera()
    {
        Input.camera = BasisLocalCameraDriver.Instance.Camera;
    }
    public void OnDisable()
    {
        DesktopSwitch.action.Disable();
        XRSwitch.action.Disable();

        MoveAction.action.Disable();
        LookAction.action.Disable();
        JumpAction.action.Disable();
        RemoveCallback();
    }
    public void Initialize(BasisLocalPlayer localPlayer)
    {
        basisLocalPlayer = localPlayer;
    }
    public void AddCallback()
    {

        MoveAction.action.performed += ctx => MoveActionStarted(ctx.ReadValue<Vector2>());
        MoveAction.action.canceled += ctx => MoveActionCancelled();

        LookAction.action.performed += ctx => LookActionStarted(ctx.ReadValue<Vector2>());
        LookAction.action.canceled += ctx => LookActionCancelled();

        JumpAction.action.performed += ctx => JumpActionPerformed();
        JumpAction.action.canceled += ctx => JumpActionCancelled();

        CrouchAction.action.performed += ctx => CrouchStarted(ctx);
        CrouchAction.action.canceled += ctx => CrouchCancelled(ctx);

        RunButton.action.performed += ctx => RunStarted();
        RunButton.action.canceled += ctx => RunCancelled();

        Escape.action.performed += ctx => EscapePerformed();
        Escape.action.canceled += ctx => EscapeCancelled();

        DesktopSwitch.action.performed += ctx => SwitchDesktop();
        XRSwitch.action.performed += ctx => SwitchOpenXR();

        LeftMousePressed.action.performed += ctx => LeftMouse(ctx.ReadValue<float>());
        RightMousePressed.action.performed += ctx => RightMouse(ctx.ReadValue<float>());

        LeftMousePressed.action.canceled += ctx => LeftMouse(ctx.ReadValue<float>());
        RightMousePressed.action.canceled += ctx => RightMouse(ctx.ReadValue<float>());
    }
    public void RemoveCallback()
    {

        MoveAction.action.performed -= ctx => MoveActionStarted(ctx.ReadValue<Vector2>());
        MoveAction.action.canceled -= ctx => MoveActionCancelled();

        LookAction.action.performed -= ctx => LookActionStarted(ctx.ReadValue<Vector2>());
        LookAction.action.canceled -= ctx => LookActionCancelled();

        JumpAction.action.performed -= ctx => JumpActionPerformed();
        JumpAction.action.canceled -= ctx => JumpActionCancelled();

        CrouchAction.action.started -= ctx => CrouchStarted(ctx);
        CrouchAction.action.canceled -= ctx => CrouchCancelled(ctx);

        RunButton.action.performed -= ctx => RunStarted();
        RunButton.action.canceled -= ctx => RunCancelled();

        Escape.action.performed -= ctx => EscapePerformed();
        Escape.action.canceled -= ctx => EscapeCancelled();


        DesktopSwitch.action.performed -= ctx => SwitchDesktop();
        XRSwitch.action.performed -= ctx => SwitchOpenXR();

        LeftMousePressed.action.performed -= ctx => LeftMouse(ctx.ReadValue<float>());
        RightMousePressed.action.performed -= ctx => RightMouse(ctx.ReadValue<float>());

        LeftMousePressed.action.canceled -= ctx => LeftMouse(ctx.ReadValue<float>());
        RightMousePressed.action.canceled -= ctx => RightMouse(ctx.ReadValue<float>());
    }
    public void LeftMouse(float state)
    {
        if (CharacterEyeInput != null)
        {
            CharacterEyeInput.State.Trigger = state;
        }
    }
    public void RightMouse(float state)
    {
        if (CharacterEyeInput != null)
        {
        }
    }
    public void SwitchDesktop()
    {
        BasisDeviceManagement.ForceSetDesktop();
    }
    public void SwitchOpenXR()
    {
        BasisDeviceManagement.ForceLoadXR();

    }
    public void LookActionStarted(Vector2 LookVector)
    {
        if (CharacterEyeInput != null)
        {
            LookDirection = LookVector;
            CharacterEyeInput.HandleMouseRotation(LookDirection);
        }
    }
    public async void EscapePerformed()
    {
        if (BasisPlayerMenu.Instance == null)
        {
            if (BasisPlayerMenu.IsLoading == false)
            {
                await BasisPlayerMenu.OpenPlayerMenu();
            }
        }
        else
        {
            BasisPlayerMenu.Instance.CloseThisMenu();
        }
        //if (BasisHamburgerMenu.Instance == null)
        //{
        //    if (BasisHamburgerMenu.IsLoading == false)
        //    {
        //        await BasisHamburgerMenu.OpenHamburgerMenu();
        //    }
        //}
        //else
        //{
        //    BasisHamburgerMenu.Instance.CloseThisMenu();
        //}
    }
    public void EscapeCancelled()
    {

    }
    public void JumpActionPerformed()
    {
        basisLocalPlayer.Move.HandleJump();
    }
    public void MoveActionCancelled()
    {
        basisLocalPlayer.Move.MovementVector = new Vector2();
    }
    public void MoveActionStarted(Vector2 MovementVector)
    {
        basisLocalPlayer.Move.MovementVector = MovementVector;
    }
    public void RotateActionCancelled()
    {
        basisLocalPlayer.Move.Rotation = new Vector2();
    }
    public void RotateActionStarted(Vector2 Rotation)
    {
        // Calculate the rotation amount
        basisLocalPlayer.Move.Rotation = Rotation;
    }
    public void LookActionCancelled()
    {
        if (CharacterEyeInput != null)
        {
            LookDirection = new Vector2();
            CharacterEyeInput.HandleMouseRotation(LookDirection);
        }
    }
    public void JumpActionCancelled()
    {

    }
    public void CrouchStarted(InputAction.CallbackContext context)
    {
        CrouchToggle(context);
    }
    public void CrouchCancelled(InputAction.CallbackContext context)
    {
        CrouchToggle(context);
    }
    public void CrouchToggle(InputAction.CallbackContext context)
    {

        if (context.phase == InputActionPhase.Performed)
        {
            Crouching = !Crouching;
            if (CharacterEyeInput != null)
            {
                CharacterEyeInput.HandleMouseRotation(LookDirection);
            }
        }
    }
    public void RunStarted()
    {
        basisLocalPlayer.Move.RunningToggle();
    }
    public void RunCancelled()
    {
        basisLocalPlayer.Move.RunningToggle();
    }
}