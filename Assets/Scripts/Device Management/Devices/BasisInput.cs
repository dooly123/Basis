using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BasisInput : MonoBehaviour
{
    public BasisLocalBoneDriver Driver;
    private BasisBoneTrackedRole trackedRole;
    public bool hasRoleAssigned = false;
    public BasisBoneControl Control;
    public string UniqueID;
    public Vector3 LocalRawPosition;
    public Quaternion LocalRawRotation;
    public Vector3 pivotOffset;
    public Vector3 rotationOffset;

    public string UnUniqueDeviceID;
    public BasisVisualTracker BasisVisualTracker;
    public AddressableGenericResource LoadedDeviceRequest;
    [SerializeField]
    public BasisInputState State = new BasisInputState();
    [SerializeField]
    public BasisInputState LastState = new BasisInputState();
    [System.Serializable]
    public struct BasisInputState
    {
        public bool gripButton;
        public bool menuButton;
        public bool primaryButtonGetState;
        public bool secondaryButtonGetState;
        public float Trigger;
        public bool secondary2DAxisClick;
        public bool primary2DAxisClick;
        public Vector2 primary2DAxis;
        public Vector2 secondary2DAxis;
    }

    public BasisBoneTrackedRole TrackedRole
    {
        get => trackedRole;
        set
        {
            trackedRole = value;
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
    /// <summary>
    /// activate!
    /// </summary>
    /// <param name="UniqueID"></param>
    /// <param name="unUniqueDeviceID"></param>
    public void ActivateTracking(string UniqueID, string unUniqueDeviceID)
    {
        this.UnUniqueDeviceID = unUniqueDeviceID;
        this.UniqueID = UniqueID;
        Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        if (Driver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        ApplyRole();
        if (BasisDeviceManagement.Instance.BasisDeviceNameMatcher.GetAssociatedPivotOffset(unUniqueDeviceID, out pivotOffset))
        {
        }
        if (BasisDeviceManagement.Instance.BasisDeviceNameMatcher.GetAssociatedRotationOffset(unUniqueDeviceID, out rotationOffset))
        {
        }
        Driver.OnSimulate += PollData;
    }
    public void ApplyRole()
    {
        if (hasRoleAssigned)
        {
            if (Driver.FindBone(out Control, TrackedRole))
            {
                Control.HasRigLayer = BasisHasRigLayer.HasRigLayer;
                if (TrackedRole == BasisBoneTrackedRole.CenterEye || TrackedRole == BasisBoneTrackedRole.LeftHand || TrackedRole == BasisBoneTrackedRole.RightHand)
                {
                    Control.initialOffset.Use = false;
                    Debug.Log("skipping calibration offset for " + TrackedRole);
                }
                else
                {

                    // Calculate the initial offset in local space
                    Vector3 relativePosition = transform.position - Control.BoneTransform.position;
                    Control.initialOffset.OffsetPosition = Quaternion.Inverse(transform.rotation) * relativePosition;

                    Control.initialOffset.OffsetRotation = Quaternion.Inverse(transform.rotation) * Control.BoneTransform.rotation;

                    //   Control.initialOffset.OffsetPosition = transform.InverseTransformPoint(Control.BoneTransform.position);
                    // During calibration: setting the calibration offset
                    //  Control.initialOffset.OffsetRotation = Quaternion.Inverse(transform.rotation) * Control.BoneTransform.rotation;

                    // Applying the calibration offset to the local raw rotation
                    //  LocalRawRotation = Control.CalibrationOffset.OffsetRotation * LocalRawRotation;
                    Control.initialOffset.Use = true;
                    Debug.Log("calibration set for " + TrackedRole);
                }
                // Do nothing if bone is found successfully
            }
        }
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
                BasisLocalPlayer.Instance.Move.MovementVector = State.primary2DAxis;
                //only open ui after we have stopped pressing down on the secondary button
                if (State.secondaryButtonGetState == false && LastState.secondaryButtonGetState)
                {
                    if (BasisHamburgerMenu.Instance == null && !BasisUIBase.IsLoading)
                    {
                        BasisHamburgerMenu.OpenMenu();
                        BasisDeviceManagement.ShowTrackers();
                    }
                    else
                    {
                        BasisHamburgerMenu.Instance.CloseThisMenu();
                        BasisDeviceManagement.HideTrackers();
                    }
                }
                if (State.primaryButtonGetState == false && LastState.primaryButtonGetState)
                {
                    BasisAvatarIKStageCalibration.Calibrate();
                }
                Control.ApplyMovement();
                break;
            case BasisBoneTrackedRole.RightHand:
                BasisLocalPlayer.Instance.Move.Rotation = State.primary2DAxis;
                if (State.primaryButtonGetState)
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
        LastState = State;
    }
    public async Task ShowTrackedVisual()
    {
        if (BasisVisualTracker == null || LoadedDeviceRequest == null)
        {
            Debug.Log("UnUniqueDeviceID " + UnUniqueDeviceID);
            if (BasisDeviceManagement.Instance.BasisDeviceNameMatcher.GetAssociatedDeviceID(UnUniqueDeviceID, out string LoadRequest))
            {
                (List<GameObject>, AddressableGenericResource) data = await AddressableResourceProcess.LoadAsGameObjectsAsync(LoadRequest, new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters());
                List<GameObject> gameObjects = data.Item1;
                if (gameObjects == null)
                {
                    return;
                }
                if (gameObjects.Count != 0)
                {
                    foreach (GameObject gameObject in gameObjects)
                    {
                        gameObject.name = UnUniqueDeviceID;
                        gameObject.transform.parent = this.transform;
                        if (gameObject.TryGetComponent(out BasisVisualTracker))
                        {
                            BasisVisualTracker.Initialization(this);
                        }
                    }
                }
            }
        }
    }
    public void HideTrackedVisual()
    {
        if (BasisVisualTracker == null)
        {
            GameObject.Destroy(BasisVisualTracker);
        }
        if (LoadedDeviceRequest != null)
        {
            AddressableLoadFactory.ReleaseResource(LoadedDeviceRequest);
        }
    }
}