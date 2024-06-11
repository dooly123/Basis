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

    public GameObject TrackedRepresentation;

    public BasisBoneTrackedRole TrackedRole
    {
        get => trackedrole;
        set
        {
            trackedrole = value;
            hasRoleAssigned = true;
        }
    }
    public void OnEnable()
    {
        ShowTrackedVisual();
    }
    public void OnDisable()
    {
        DisableTracking();
    }
    public void OnDestroy()
    {
        DisableTracking();
    }
    public void ActivateTracking(string iD)
    {
        ID = iD;
        Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        if (Driver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        if (hasRoleAssigned)
        {
            if (Driver.FindBone(out Control, TrackedRole))
            {
                Control.HasRigLayer = BasisHasRigLayer.HasRigLayer;
                // Do nothing if bone is found successfully
            }
        }

        Driver.OnSimulate += PollData;
        SetRealTrackers(BasisHasTracked.HasTracker, BasisHasRigLayer.HasRigLayer);
    }

    public void DisableTracking()
    {
        if (Driver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        Driver.OnSimulate -= PollData;
        SetRealTrackers(BasisHasTracked.HasNoTracker, BasisHasRigLayer.HasNoRigLayer);
    }

    public abstract void PollData();

    public void SetRealTrackers(BasisHasTracked hasTracked, BasisHasRigLayer HasLayer)
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
    public void ShowTrackedVisual()
    {
        if (TrackedRepresentation == null)
        {
            TrackedRepresentation = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            TrackedRepresentation.transform.parent = this.transform;
            TrackedRepresentation.transform.SetLocalPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);
            TrackedRepresentation.transform.localScale = new Vector3(0.075f, 0.075f, 0.075f);
            if (TrackedRepresentation.TryGetComponent(out MeshRenderer Renderer))
            {
            }
            if (TrackedRepresentation.TryGetComponent(out Collider Collider))
            {
                Destroy(Collider);
            }
        }
    }
    public void HideTrackedVisual()
    {
        if (TrackedRepresentation == null)
        {
            GameObject.Destroy(TrackedRepresentation);
        }
    }
}