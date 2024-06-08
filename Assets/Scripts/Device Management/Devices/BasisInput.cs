using UnityEngine;

public abstract class BasisInput : MonoBehaviour
{
    public BasisLocalBoneDriver Driver;
    private BasisBoneTrackedRole trackedrole;
    public bool hasRoleAssigned = false;
    public BasisBoneControl Control;
    public string ID;
    public Vector3 LocalRawPosition;
    public Quaternion LocalRawRotation;

    public Vector2 primary2DAxis;
    public Vector2 secondary2DAxis;
    public bool gripButton;
    public bool menuButton;
    public bool primaryButton;
    public bool secondaryButton;
    public float Trigger;
    public bool secondary2DAxisClick;
    public bool primary2DAxisClick;

    public BasisBoneTrackedRole TrackedRole
    {
        get => trackedrole;
        set
        {
            trackedrole = value;
            hasRoleAssigned = true;
        }
    }

    public void OnDisable()
    {
        DisableTracking();
    }
    public void OnDestroy()
    {
        DisableTracking();
    }

    public virtual void Initialize(string iD)
    {
        Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        ID = iD;
        ActivateTracking();
    }

    public void ActivateTracking()
    {
        if (Driver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        if (hasRoleAssigned)
        {
            if (Driver.FindBone(out Control, TrackedRole))
            {
                Control.HasRigLayer = BasisBoneControl.BasisHasRigLayer.HasRigLayer;
                // Do nothing if bone is found successfully
            }
        }
        Driver.OnSimulate += PollData;
        SetRealTrackers(BasisBoneControl.BasisHasTracked.HasTracker, BasisBoneControl.BasisHasRigLayer.HasRigLayer);
    }

    public void DisableTracking()
    {
        if (Driver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        Driver.OnSimulate -= PollData;
        SetRealTrackers(BasisBoneControl.BasisHasTracked.HasNoTracker, BasisBoneControl.BasisHasRigLayer.HasNoRigLayer);
    }

    public abstract void PollData();

    public void SetRealTrackers(BasisBoneControl.BasisHasTracked hasTracked, BasisBoneControl.BasisHasRigLayer HasLayer)
    {
        if (Driver.FindBone(out Control, TrackedRole))
        {
            Control.HasTrackerPositionDriver = hasTracked;
            Control.HasTrackerRotationDriver = hasTracked;
            Control.HasRigLayer = HasLayer;
        }
    }
    public void UpdatePlayerControl()
    {
        switch (TrackedRole)
        {
            case BasisBoneTrackedRole.LeftHand:
                BasisLocalPlayer.Instance.Move.MovementVector = primary2DAxis;
                if (secondaryButton)
                {
                    if (BasisHamburgerMenu.Instance == null && !BasisUIBase.IsLoading)
                    {
                        BasisHamburgerMenu.OpenMenu();
                    }
                    else
                    {
                        BasisHamburgerMenu.Instance.CloseThisMenu();
                    }
                }
                Control.ApplyMovement();
                break;
            case BasisBoneTrackedRole.RightHand:
                BasisLocalPlayer.Instance.Move.Rotation = primary2DAxis;
                if (primaryButton)
                {
                    BasisLocalPlayer.Instance.Move.HandleJump();
                }
                Control.ApplyMovement();
                break;
            case BasisBoneTrackedRole.CenterEye:
                Control.ApplyMovement();
                break;
            case BasisBoneTrackedRole.Head:
                break;
            case BasisBoneTrackedRole.Neck:
                break;
            case BasisBoneTrackedRole.Chest:
                break;
            case BasisBoneTrackedRole.Hips:
                break;
            case BasisBoneTrackedRole.Spine:
                break;
            case BasisBoneTrackedRole.LeftUpperLeg:
                break;
            case BasisBoneTrackedRole.RightUpperLeg:
                break;
            case BasisBoneTrackedRole.LeftLowerLeg:
                break;
            case BasisBoneTrackedRole.RightLowerLeg:
                break;
            case BasisBoneTrackedRole.LeftFoot:
                break;
            case BasisBoneTrackedRole.RightFoot:
                break;
            case BasisBoneTrackedRole.UpperChest:
                break;
            case BasisBoneTrackedRole.LeftShoulder:
                break;
            case BasisBoneTrackedRole.RightShoulder:
                break;
            case BasisBoneTrackedRole.LeftUpperArm:
                break;
            case BasisBoneTrackedRole.RightUpperArm:
                break;
            case BasisBoneTrackedRole.LeftLowerArm:
                break;
            case BasisBoneTrackedRole.RightLowerArm:
                break;
            case BasisBoneTrackedRole.LeftToes:
                break;
            case BasisBoneTrackedRole.RightToes:
                break;
            case BasisBoneTrackedRole.Mouth:
                break;
        }
    }
}