using UnityEngine;
public class BasisAvatarEyeInput : BasisInput
{
    public Camera Camera;
    public BasisLocalAvatarDriver AvatarDriver;
    public BasisLocalInputActions characterInputActions;
    public static BasisAvatarEyeInput Instance;
    public float RangeOfMotionBeforeTurn = 13;
    public float headDownwardForce = 0.004f;
    public float headUpwardForce = 0.001f;
    public float adjustment;
    public float crouchPercentage = 0.5f;
    public float rotationSpeed = 0.1f;
    private float rotationY;
    public float rotationX;
    public float minimumY = -80f;
    public float maximumY = 80f;
    public float DelayedResponseForRotation = 0.6f;
    public bool isCursorLocked = true;
    public float FallBackHeight = 1.73f;
    public void Initalize(string ID = "Desktop Eye")
    {
        Debug.Log("Initalizing Avatar Eye");
        if (BasisLocalPlayer.Instance.AvatarDriver != null)
        {
            LocalRawPosition = new Vector3(0, BasisLocalPlayer.Instance.AvatarDriver.ActiveHeight, 0);
            LocalRawRotation = Quaternion.identity;
        }
        else
        {
            LocalRawPosition = new Vector3(0, FallBackHeight, 0);
            LocalRawRotation = Quaternion.identity;
        }
        TrackedRole = BasisBoneTrackedRole.CenterEye;
        ActivateTracking(ID, ID);
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        PlayerInitialized();
        BasisLocalPlayer.OnLocalAvatarChanged += PlayerInitialized;
    }
    public void PlayerInitialized()
    {
        Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
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
        BasisLocalPlayer.OnLocalAvatarChanged -= PlayerInitialized;
        base.OnDisable();
    }
    public void HandleEscape()
    {
        if (isCursorLocked)
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen
        Cursor.visible = false; // Hide the cursor
        isCursorLocked = true;
    }
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Show the cursor
        isCursorLocked = false;
    }
    public void HandleMouseRotation(Vector2 lookVector)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }
        if (isCursorLocked == false)
        {
            return;
        }
        rotationX += lookVector.x * rotationSpeed;
        rotationY -= lookVector.y * rotationSpeed;

        // Apply modulo operation to keep rotation within 0 to 360 range
        rotationX %= 360f;
        rotationY %= 360f;
        // Clamp rotationY to stay within the specified range
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
        LocalRawRotation = Quaternion.Euler(rotationY, rotationX, 0);
        Vector3 adjustedHeadPosition = new Vector3(0, Control.RestingLocalSpace.Position.y, 0);
        if (characterInputActions.Crouching)
        {
            adjustedHeadPosition.y -= Control.RestingLocalSpace.Position.y * crouchPercentage;
        }

        CalculateAdjustment();
        adjustedHeadPosition.y -= adjustment;
        LocalRawPosition = adjustedHeadPosition;
    }
    public void CalculateAdjustment()
    {
        if (rotationY > 0)
        {
            // Positive rotation
            adjustment = Mathf.Abs(rotationY) * (headDownwardForce / Control.RestingLocalSpace.Position.y);
        }
        else
        {
            // Negative rotation
            adjustment = Mathf.Abs(rotationY) * (headUpwardForce / Control.RestingLocalSpace.Position.y);
        }
    }
    public override void PollData()
    {
        if (hasRoleAssigned)
        {
            Control.TrackerData.Rotation = LocalRawRotation;
        }
        if (hasRoleAssigned)
        {
            Control.TrackerData.Position = LocalRawPosition;
        }
        UpdatePlayerControl();
    }
}