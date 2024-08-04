using Basis;
using UnityEngine;
public class BasisAvatarEyeInput : BasisInput
{
    public Camera Camera;
    public BasisLocalAvatarDriver AvatarDriver;
    public BasisLocalInputActions characterInputActions;
    public static BasisAvatarEyeInput Instance;
    public float RangeOfMotionBeforeTurn = 13;
    public float headDownwardForce = 0.003f;
    public float headUpwardForce = 0.001f;
    public float adjustment;
    public float crouchPercentage = 0.5f;
    public float rotationSpeed = 0.1f;
    private float rotationY;
    public float rotationX;
    public float minimumY = -80f;
    public float maximumY = 80f;
    public float DelayedResponseForRotation = 0.6f;
    public float FallBackHeight = 1.73f;
    public bool BlockCrouching;

    public float InjectedX = 0;
    public float InjectedZ = 0;
    public bool HasEyeEvents = false;
    public void Initalize(string ID = "Desktop Eye", string subSystems = "BasisDesktopManagement")
    {
        Debug.Log("Initalizing Avatar Eye");
        if (BasisLocalPlayer.Instance.AvatarDriver != null)
        {
            LocalRawPosition = new Vector3(InjectedX, BasisLocalPlayer.Instance.PlayerEyeHeight, InjectedZ);
            LocalRawRotation = Quaternion.identity;
        }
        else
        {
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
        LockCursor();
        if (HasEyeEvents == false)
        {
            BasisLocalPlayer.Instance.OnLocalAvatarChanged += PlayerInitialized;
            BasisLocalPlayer.Instance.OnPlayersHeightChanged += BasisLocalPlayer_OnPlayersHeightChanged;
            HasEyeEvents = true;
        }
    }
    public new void OnDestroy()
    {
        if (HasEyeEvents )
        {
            BasisLocalPlayer.Instance.OnLocalAvatarChanged -= PlayerInitialized;
            BasisLocalPlayer.Instance.OnPlayersHeightChanged -= BasisLocalPlayer_OnPlayersHeightChanged;
            HasEyeEvents = false;
        }
        base.OnDestroy();
    }

    private void BasisLocalPlayer_OnPlayersHeightChanged()
    {
        BasisLocalPlayer.Instance.PlayerEyeHeight = BasisLocalPlayer.Instance.AvatarDriver.ActiveEyeHeight;
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
        foreach (BasisLockToInput Input in BasisDeviceManagement.Instance.BasisLockToInputs)
        {
            Input.FindRole();
        }
    }
    public new void OnDisable()
    {
        BasisLocalPlayer.Instance.OnLocalAvatarChanged -= PlayerInitialized;
        base.OnDisable();
        UnlockCursor();
    }
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Hide the cursor
    }
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Show the cursor
    }
    public void HandleMouseRotation(Vector2 lookVector)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }
        rotationX += lookVector.x * rotationSpeed;
        rotationY -= lookVector.y * rotationSpeed;

    }
    public override void PollData()
    {
        if (hasRoleAssigned)
        {
            // Apply modulo operation to keep rotation within 0 to 360 range
            rotationX %= 360f;
            rotationY %= 360f;
            // Clamp rotationY to stay within the specified range
            rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
            LocalRawRotation = Quaternion.Euler(rotationY, rotationX, 0);
            Vector3 adjustedHeadPosition = new Vector3(InjectedX, BasisLocalPlayer.Instance.PlayerEyeHeight, InjectedZ);
            if (BasisLocalInputActions.Crouching && BlockCrouching == false)
            {
                adjustedHeadPosition.y -= Control.TposeLocal.position.y * crouchPercentage;
            }

            CalculateAdjustment();
            adjustedHeadPosition.y -= adjustment;
            LocalRawPosition = adjustedHeadPosition;
            Control.IncomingData.position = LocalRawPosition;
            Control.IncomingData.rotation = LocalRawRotation;
        }
        FinalPosition = LocalRawPosition;
        FinalRotation = LocalRawRotation;
        UpdatePlayerControl();

        BasisInputEye.LeftPosition = this.transform.position;
        BasisInputEye.RightPosition = this.transform.position;
    }
    public void CalculateAdjustment()
    {
        if (rotationY > 0)
        {
            // Positive rotation
            adjustment = Mathf.Abs(rotationY) * ((headDownwardForce * BasisLocalPlayer.Instance.AvatarDriver.ActiveEyeHeight) / Control.TposeLocal.position.y);
        }
        else
        {
            // Negative rotation
            adjustment = Mathf.Abs(rotationY) * ((headUpwardForce * BasisLocalPlayer.Instance.AvatarDriver.ActiveEyeHeight) / Control.TposeLocal.position.y);
        }
    }
}