using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Drivers;
using Basis.Scripts.TransformBinders.BoneControl;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Basis.Scripts.Device_Management.Devices.Desktop
{
    public class BasisAvatarEyeInput : BasisInput
    {
        public Camera Camera;
        public BasisLocalAvatarDriver AvatarDriver;
        public BasisLocalInputActions characterInputActions;
        public static BasisAvatarEyeInput Instance;
        public float crouchPercentage = 0.5f;
        public float rotationSpeed = 0.1f;
        public float rotationY;
        public float rotationX;
        public float minimumY = -80f;
        public float maximumY = 80f;
        [HideInInspector]
        public float FallBackHeight = 1.73f;
        public bool BlockCrouching;
        public float InjectedX = 0;
        public float InjectedZ = 0;
        public bool HasEyeEvents = false;
        public bool PauseLook = false;
        public void Initalize(string ID = "Desktop Eye", string subSystems = "BasisDesktopManagement")
        {
            BasisDebug.Log("Initalizing Avatar Eye", BasisDebug.LogTag.Input);
            if (BasisLocalPlayer.Instance.AvatarDriver != null)
            {
                BasisDebug.Log("Using Configured Height " + BasisLocalPlayer.Instance.PlayerEyeHeight, BasisDebug.LogTag.Input);
                LocalRawPosition = new Vector3(InjectedX, BasisLocalPlayer.Instance.PlayerEyeHeight, InjectedZ);
                LocalRawRotation = Quaternion.identity;
            }
            else
            {
                BasisDebug.Log("Using Fallback Height " + FallBackHeight, BasisDebug.LogTag.Input);
                LocalRawPosition = new Vector3(InjectedX, FallBackHeight, InjectedZ);
                LocalRawRotation = Quaternion.identity;
            }
            FinalPosition = LocalRawPosition;
            FinalRotation = LocalRawRotation;
            InitalizeTracking(ID, ID, subSystems, true, BasisBoneTrackedRole.CenterEye);
            if (BasisHelpers.CheckInstance(Instance))
            {
                Instance = this;
            }
            PlayerInitialized();
            BasisCursorManagement.OverrideableLock(nameof(BasisAvatarEyeInput));
            if (HasEyeEvents == false)
            {
                BasisLocalPlayer.Instance.OnLocalAvatarChanged += PlayerInitialized;
                BasisLocalPlayer.Instance.OnPlayersHeightChanged += BasisLocalPlayer_OnPlayersHeightChanged;
                BasisLocalPlayer_OnPlayersHeightChanged();
                BasisCursorManagement.OnCursorStateChange += OnCursorStateChange;
                BasisPointRaycaster.UseWorldPosition = false;
                BasisVirtualSpine.Initialize();
                HasEyeEvents = true;
            }
        }
        private void OnCursorStateChange(CursorLockMode cursor, bool newCursorVisible)
        {
            BasisDebug.Log("cursor changed to : " + cursor.ToString() + " | Cursor Visible : " + newCursorVisible, BasisDebug.LogTag.Input);
            if (cursor == CursorLockMode.Locked)
            {
                PauseLook = false;
            }
            else
            {
                PauseLook = true;
            }
        }
        public new void OnDestroy()
        {
            if (HasEyeEvents)
            {
                BasisLocalPlayer.Instance.OnLocalAvatarChanged -= PlayerInitialized;
                BasisLocalPlayer.Instance.OnPlayersHeightChanged -= BasisLocalPlayer_OnPlayersHeightChanged;
                BasisCursorManagement.OnCursorStateChange -= OnCursorStateChange;
                HasEyeEvents = false;

                BasisVirtualSpine.DeInitialize();
            }
            base.OnDestroy();
        }
        private void BasisLocalPlayer_OnPlayersHeightChanged()
        {
            //   Vector3 Pos = new Vector3(0, BasisLocalPlayer.Instance.AvatarDriver.ActiveAvatarEyeHeight(), 0);
            //  BasisLocalPlayer.Instance.AvatarDriver.GetWorldSpaceRotAndPos(() => Pos, out quaternion rot, out float3 position);
            //  BasisLocalPlayer.Instance.PlayerEyeHeight = -position.y;
          // float avatarHeight = BasisLocalPlayer.Instance.AvatarDriver?.ActiveAvatarEyeHeight() ?? 0;
            BasisLocalPlayer.Instance.PlayerEyeHeight = BasisLocalPlayer.Instance.AvatarEyeHeight;
        }
        public void PlayerInitialized()
        {
            characterInputActions = BasisLocalInputActions.Instance;
            if (characterInputActions != null)
            {
                characterInputActions.CharacterEyeInput = this;
            }
            AvatarDriver = BasisLocalPlayer.Instance.AvatarDriver;
            Camera = BasisLocalCameraDriver.Instance.Camera;
            BasisDeviceManagement Device = BasisDeviceManagement.Instance;
            int count = Device.BasisLockToInputs.Count;
            for (int Index = 0; Index < count; Index++)
            {
                Device.BasisLockToInputs[Index].FindRole();
            }
        }
        public new void OnDisable()
        {
            BasisLocalPlayer.Instance.OnLocalAvatarChanged -= PlayerInitialized;
            base.OnDisable();
        }
        public void HandleMouseRotation(Vector2 lookVector)
        {
            BasisPointRaycaster.ScreenPoint = Mouse.current.position.value;
            if (!isActiveAndEnabled || PauseLook)
            {
                return;
            }
            rotationX += lookVector.x * rotationSpeed;
            rotationY -= lookVector.y * rotationSpeed;
        }
        public float InjectedZRot = 0;
        public override void DoPollData()
        {
            if (hasRoleAssigned)
            {
                characterInputActions.InputState.CopyTo(InputState);
                // InputState.CopyTo(characterInputActions.InputState);
                // Apply modulo operation to keep rotation within 0 to 360 range
                rotationX %= 360f;
                rotationY %= 360f;
                // Clamp rotationY to stay within the specified range
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
                LocalRawRotation = Quaternion.Euler(rotationY, rotationX, InjectedZRot);
                Vector3 adjustedHeadPosition = new Vector3(InjectedX, BasisLocalPlayer.Instance.PlayerEyeHeight, InjectedZ);
                if (BasisLocalInputActions.Crouching)
                {
                    adjustedHeadPosition.y -= Control.TposeLocal.position.y * crouchPercentage;
                }
                LocalRawPosition = adjustedHeadPosition;
                Control.IncomingData.position = LocalRawPosition;
                Control.IncomingData.rotation = LocalRawRotation;
                FinalPosition = LocalRawPosition;
                FinalRotation = LocalRawRotation;
                UpdatePlayerControl();
            }
        }
        public BasisVirtualSpineDriver BasisVirtualSpine = new BasisVirtualSpineDriver();
    }
}
