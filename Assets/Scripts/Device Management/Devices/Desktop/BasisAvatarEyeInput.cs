using UnityEngine;
public class BasisAvatarEyeInput : MonoBehaviour
{
    public Camera Camera;
    public BasisLocalAvatarDriver Calibration;
    public BasisLocalInputActions characterInputActions;
    public BasisBoneControl Head;
    public BasisBoneControl Eye;
    public BasisBoneControl Hips;
    public BasisBoneControl UpperChest;
    public BasisBoneControl Chest;
    public BasisBoneControl Spine;
    public static BasisAvatarEyeInput Instance;
    public BasisLocalBoneDriver Driver;
    public float DelayedHips = 6;
    public float DelayedUpperChest = 6;
    public float DelayedChest = 6;
    public float DelayedSpine = 6;
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
    public void PlayerInitialized()
    {
        if (BasisHelpers.CheckInstance(Instance))
        {
            Instance = this;
        }
        Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        if (Driver.FindBone(out Eye, BasisBoneTrackedRole.CenterEye))
        {
            Eye.HasTrackerPositionDriver = BasisBoneControl.BasisHasTracked.HasInterpretedTracker;
            Eye.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.HasInterpretedTracker;
        }
        if (Driver.FindBone(out Hips, BasisBoneTrackedRole.Hips))
        {
            if (Hips.HasTrackerRotationDriver != BasisBoneControl.BasisHasTracked.HasVRTracker)
            {
                Hips.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.hasVirtualTracker;
            }
        }

        if (Driver.FindBone(out UpperChest, BasisBoneTrackedRole.UpperChest))
        {
            if (UpperChest.HasTrackerRotationDriver != BasisBoneControl.BasisHasTracked.HasVRTracker)
            {
                UpperChest.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.hasVirtualTracker;
            }
        }
        if (Driver.FindBone(out Chest, BasisBoneTrackedRole.Chest))
        {
            if (Chest.HasTrackerRotationDriver != BasisBoneControl.BasisHasTracked.HasVRTracker)
            {
                Chest.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.hasVirtualTracker;
            }
        }

        if (Driver.FindBone(out Spine, BasisBoneTrackedRole.Spine))
        {
            if (Spine.HasTrackerRotationDriver != BasisBoneControl.BasisHasTracked.HasVRTracker)
            {
                Spine.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.hasVirtualTracker;
            }
        }
        characterInputActions = BasisLocalInputActions.Instance;
        characterInputActions.CharacterEyeInput = this;
        Calibration = BasisLocalPlayer.Instance.LocalAvatarDriver;
        Camera = BasisLocalCameraDriver.Instance.Camera;
    }
    public void OnDisable()
    {
        BasisLocalPlayer.OnLocalAvatarChanged -= PlayerInitialized;
        if (Eye != null)
        {
            Eye.HasTrackerPositionDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;
            Eye.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;
        }
        if (Driver.FindBone(out Head, BasisBoneTrackedRole.Head))
        {
        }
        if (Hips != null)
        {
            Hips.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;
        }
        if (UpperChest != null)
        {
            UpperChest.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;
        }
        if (Chest != null)
        {
            Chest.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;
        }
        if (Spine != null)
        {
            Spine.HasTrackerRotationDriver = BasisBoneControl.BasisHasTracked.HasNoTracker;
        }
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
        float DeltaTime = Time.deltaTime;
        rotationX += lookVector.x * rotationSpeed;
        rotationY -= lookVector.y * rotationSpeed;

        // Apply modulo operation to keep rotation within 0 to 360 range
         rotationX %= 360f;
         rotationY %= 360f;

        // Clamp rotationY to stay within the specified range
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
        Eye.LocalRawRotation = Quaternion.Euler(rotationY, rotationX, 0);

      Quaternion coreRotation = Quaternion.Euler(0, rotationX, 0);

        if (AngleCheck(coreRotation, Hips.LocalRawRotation, RangeOfMotionBeforeTurn))
        {
            // Slerp rotation for hips and upper body
            if (Hips.HasTrackerRotationDriver == BasisBoneControl.BasisHasTracked.hasVirtualTracker)
            {
                Hips.LocalRawRotation = SlerpYRotation(Hips.LocalRawRotation, coreRotation, DelayedHips * DeltaTime);
            }
            if (UpperChest.HasTrackerRotationDriver == BasisBoneControl.BasisHasTracked.hasVirtualTracker)
            {
                UpperChest.LocalRawRotation = SlerpYRotation(UpperChest.LocalRawRotation, coreRotation, DelayedUpperChest * DeltaTime);
            }
            if (Chest.HasTrackerRotationDriver == BasisBoneControl.BasisHasTracked.hasVirtualTracker)
            {
                Chest.LocalRawRotation = SlerpYRotation(Chest.LocalRawRotation, coreRotation, DelayedChest * DeltaTime);
            }
            if (Spine.HasTrackerRotationDriver == BasisBoneControl.BasisHasTracked.hasVirtualTracker)
            {
                Spine.LocalRawRotation = SlerpYRotation(Spine.LocalRawRotation, coreRotation, DelayedSpine * DeltaTime);
            }
        }
        Vector3 adjustedHeadPosition = new Vector3(0, Eye.RestingLocalSpace.BeginningPosition.y,0);
        if (characterInputActions.Crouching)
        {
            adjustedHeadPosition.y -= Eye.RestingLocalSpace.BeginningPosition.y * crouchPercentage;
        }

        CalculateAdjustment();
        adjustedHeadPosition.y -= adjustment;
        Eye.LocalRawPosition = adjustedHeadPosition;
    }
    private Quaternion SlerpYRotation(Quaternion from, Quaternion to, float t)
    {
        Vector3 fromEuler = from.eulerAngles;
        Vector3 toEuler = to.eulerAngles;
        Vector3 resultEuler = new Vector3(fromEuler.x, Mathf.LerpAngle(fromEuler.y, toEuler.y, t), fromEuler.z);
        return Quaternion.Euler(resultEuler);
    }
    public bool AngleCheck(Quaternion AngleA, Quaternion AngleB, float MaximumTolerance = 0.005f)
    {
        float Angle = Quaternion.Angle(AngleA, AngleB);
        bool AngleLargeEnough = Angle > MaximumTolerance;
        return AngleLargeEnough;
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