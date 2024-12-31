using Basis.Scripts.Addressable_Driver.Resource;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using Basis.Scripts.UI.UI_Panels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Basis.Scripts.Device_Management.Devices.Desktop
{
    [DefaultExecutionOrder(15003)]
    public class BasisLocalInputActions : MonoBehaviour
    {
        public InputActionReference MoveAction;
        public InputActionReference LookAction;
        public InputActionReference JumpAction;
        public InputActionReference CrouchAction;
        public InputActionReference RunButton;
        public InputActionReference Escape;
        public InputActionReference PrimaryButtonGetState;

        public InputActionReference DesktopSwitch;
        public InputActionReference XRSwitch;

        public InputActionReference LeftMousePressed;
        public InputActionReference RightMousePressed;

        [SerializeField] public static bool Crouching;
        [SerializeField] public static Vector2 LookDirection;
        public BasisAvatarEyeInput CharacterEyeInput;
        public static BasisLocalInputActions Instance;
        public BasisLocalPlayer basisLocalPlayer;
        public PlayerInput Input;
        public static string InputActions = "InputActions";
        public bool HasEvents = false;
        public bool IgnoreCrouchToggle = false;
        public static Action AfterAvatarChanges;
        [SerializeField]
        public BasisInputState InputState = new BasisInputState();
        public void OnEnable()
        {
            InputSystem.settings.SetInternalFeatureFlag("USE_OPTIMIZED_CONTROLS", true);
            InputSystem.settings.SetInternalFeatureFlag("USE_READ_VALUE_CACHING", true);
            DesktopSwitch.action.Enable();
            XRSwitch.action.Enable();

            MoveAction.action.Enable();
            LookAction.action.Enable();
            JumpAction.action.Enable();
            if (BasisHelpers.CheckInstance(Instance))
            {
                Instance = this;
            }
            if (HasEvents == false)
            {
                BasisLocalCameraDriver.InstanceExists += SetupCamera;
                AddCallback();
                HasEvents = true;
            }
            Application.onBeforeRender += OnBeforeRender;
        }

        private void OnBeforeRender()
        {
            BasisLocalPlayer.Instance.LocalBoneDriver.SimulateAndApply(Time.timeAsDouble,Time.deltaTime);
            AfterAvatarChanges?.Invoke();
        }

        public void Update()
        {
            InputSystem.Update();
        }
        public static async Task CreateInputAction(BasisLocalPlayer Local)
        {
            ChecksRequired Required = new ChecksRequired
            {
                UseContentRemoval = false,
                DisableAnimatorEvents = false
            };
            var data = await AddressableResourceProcess.LoadAsGameObjectsAsync(InputActions, new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters(), Required);
            List<GameObject> Gameobjects = data.Item1;
            if (Gameobjects.Count != 0)
            {
                foreach (GameObject gameObject in Gameobjects)
                {
                    if (gameObject.TryGetComponent(out BasisLocalInputActions CharacterInputActions))
                    {
                        CharacterInputActions.Initialize(Local);
                    }
                }
            }
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
            if (HasEvents)
            {
                BasisLocalCameraDriver.InstanceExists -= SetupCamera;
                RemoveCallback();
                HasEvents = false;
            }
            Application.onBeforeRender -= OnBeforeRender;
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

            PrimaryButtonGetState.action.performed += ctx => PrimaryGet();
            PrimaryButtonGetState.action.canceled += ctx => CancelPrimaryGet();

            DesktopSwitch.action.performed += ctx => SwitchDesktop();
            XRSwitch.action.performed += ctx => SwitchOpenXR();

            LeftMousePressed.action.performed += ctx => LeftMouse(ctx.ReadValue<float>());
            RightMousePressed.action.performed += ctx => RightMouse(ctx.ReadValue<float>());

            LeftMousePressed.action.canceled += ctx => LeftMouse(ctx.ReadValue<float>());
            RightMousePressed.action.canceled += ctx => RightMouse(ctx.ReadValue<float>());
        }
        public void PrimaryGet()
        {
            InputState.PrimaryButtonGetState = true;
        }
        public void CancelPrimaryGet()
        {
            InputState.PrimaryButtonGetState = false;
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

            PrimaryButtonGetState.action.performed -= ctx => PrimaryGet();
            PrimaryButtonGetState.action.canceled -= ctx => CancelPrimaryGet();

            DesktopSwitch.action.performed -= ctx => SwitchDesktop();
            XRSwitch.action.performed -= ctx => SwitchOpenXR();

            LeftMousePressed.action.performed -= ctx => LeftMouse(ctx.ReadValue<float>());
            RightMousePressed.action.performed -= ctx => RightMouse(ctx.ReadValue<float>());

            LeftMousePressed.action.canceled -= ctx => LeftMouse(ctx.ReadValue<float>());
            RightMousePressed.action.canceled -= ctx => RightMouse(ctx.ReadValue<float>());
        }
        public void LeftMouse(float state)
        {
            InputState.Trigger = state;
        }
        public void RightMouse(float state)
        {
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
        public void EscapePerformed()
        {
            if (BasisHamburgerMenu.Instance == null)
            {
                BasisHamburgerMenu.OpenHamburgerMenuNow();
            }
            else
            {
                BasisHamburgerMenu.Instance.CloseThisMenu();
            }
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
                if (!IgnoreCrouchToggle) Crouching = !Crouching;
                if (CharacterEyeInput != null)
                {
                    CharacterEyeInput.HandleMouseRotation(LookDirection);
                }
                if (Crouching)
                {
                    basisLocalPlayer.Move.SpeedMultiplyer = 0;
                }
                else
                {
                    basisLocalPlayer.Move.SpeedMultiplyer = 0.5f;
                }
            }
        }
        public void RunStarted()
        {
            if (Crouching)
            {
            }
            else
            {
                basisLocalPlayer.Move.SpeedMultiplyer = 1;
            }
        }
        public void RunCancelled()
        {
            if (Crouching)
            {
                basisLocalPlayer.Move.SpeedMultiplyer = 0;
            }
            else
            {
                basisLocalPlayer.Move.SpeedMultiplyer = 0.5f;
            }
        }
    }
}