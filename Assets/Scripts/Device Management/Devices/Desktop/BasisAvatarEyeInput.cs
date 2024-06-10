using UnityEngine;
public class BasisAvatarEyeInput : MonoBehaviour
{
    public Camera Camera;
    public BasisLocalAvatarDriver AvatarDriver;
    public BasisLocalInputActions characterInputActions;
    public BasisBoneControl Eye;
    public static BasisAvatarEyeInput Instance;
    public BasisLocalBoneDriver Driver;
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
    public void OnEnable()
    {
        BasisLocalPlayer.OnLocalAvatarChanged += PlayerInitialized;
    }
    public void OnDestroy()
    {
        Debug.Log("deleting " + nameof(BasisAvatarEyeInput));
        Instance = null;
    }
    public void Start()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        PlayerInitialized();
    }
    public void PlayerInitialized()
    {
        Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        if (Driver.FindBone(out Eye, BasisBoneTrackedRole.CenterEye))
        {
            Eye.HasTrackerPositionDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;
            Eye.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;
        }
        characterInputActions = BasisLocalInputActions.Instance;
        if (characterInputActions != null)
        {
            characterInputActions.CharacterEyeInput = this;
        }
        AvatarDriver = BasisLocalPlayer.Instance.AvatarDriver;
        Camera = BasisLocalCameraDriver.Instance.Camera;
    }
    public void OnDisable()
    {
        BasisLocalPlayer.OnLocalAvatarChanged -= PlayerInitialized;
    }
    public bool isCursorLocked = true;
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
        Eye.LocalRawRotation = Quaternion.Euler(rotationY, rotationX, 0);
        Vector3 adjustedHeadPosition = new Vector3(0, Eye.RestingLocalSpace.BeginningPosition.y, 0);
        if (characterInputActions.Crouching)
        {
            adjustedHeadPosition.y -= Eye.RestingLocalSpace.BeginningPosition.y * crouchPercentage;
        }

        CalculateAdjustment();
        adjustedHeadPosition.y -= adjustment;
        Eye.LocalRawPosition = adjustedHeadPosition;
    }
    public void CalculateAdjustment()
    {
        if (rotationY > 0)
        {
            // Positive rotation
            adjustment = Mathf.Abs(rotationY) * (headDownwardForce / Eye.RestingLocalSpace.BeginningPosition.y);
        }
        else
        {
            // Negative rotation
            adjustment = Mathf.Abs(rotationY) * (headUpwardForce / Eye.RestingLocalSpace.BeginningPosition.y);
        }
    }
}