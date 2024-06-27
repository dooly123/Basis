using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static BaseBoneDriver;
public abstract class BasisInput : MonoBehaviour
{
    public string SubSystem;
    public BasisLocalBoneDriver Driver;
    private BasisBoneTrackedRole trackedRole;
    public bool hasRoleAssigned = false;
    public BasisBoneControl Control = new BasisBoneControl();
    public string UniqueID;
    public Vector3 LocalRawPosition;
    public Quaternion LocalRawRotation;
    public Vector3 pivotOffset;
    public Quaternion rotationOffset;
    public bool HasUIInputSupport = false;
    public string UnUniqueDeviceID;
    public BasisVisualTracker BasisVisualTracker;
    public BasisPointRaycaster BasisPointRaycaster;//used to raycast against things like UI
    public AddressableGenericResource LoadedDeviceRequest;
    public event SimulationHandler AfterControlApply;
    public GameObject BasisPointRaycasterRef;
    public BasisDeviceMatchableNames BasisDeviceMatchableNames;
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
        if (BasisDeviceManagement.Instance.BasisDeviceNameMatcher.GetAssociatedDeviceMatchableNames(unUniqueDeviceID, out BasisDeviceMatchableNames))
        {
            rotationOffset = Quaternion.Euler(BasisDeviceMatchableNames.RotationOffset);
            HasUIInputSupport = BasisDeviceMatchableNames.HasRayCastSupport;
            if (HasUIInputSupport)
            {
                CreateRayCaster(this);
            }
        }
        else
        {
            Debug.Log("Missing ID " + unUniqueDeviceID);
            rotationOffset = Quaternion.identity;
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
                    Control.InitialOffset.Use = false;
                    Debug.Log("skipping calibration offset for " + TrackedRole);
                }
                else
                {
                    //this is the tracker
                    //target is the following along
                    Vector3 RelativePosition = Control.BoneTransform.position - transform.position;
                    Control.InitialOffset.Position = Quaternion.Inverse(transform.localRotation) * RelativePosition;
                    Control.InitialOffset.Rotation = Quaternion.Inverse(transform.localRotation) * Control.BoneTransform.localRotation;
                    Control.InitialOffset.Use = true;
                }
                // Do nothing if bone is found successfully
                SetRealTrackers(BasisHasTracked.HasTracker, BasisHasRigLayer.HasRigLayer);
            }
        }
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
        if (HasUIInputSupport)
        {
            BasisPointRaycaster.RayCastUI();
        }
        LastState = State;
        AfterControlApply?.Invoke();
    }
    public async Task ShowTrackedVisual()
    {
        if (BasisVisualTracker == null || LoadedDeviceRequest == null)
        {
            if (BasisDeviceManagement.Instance.BasisDeviceNameMatcher.GetAssociatedDeviceID(UnUniqueDeviceID, out string LoadRequest, out bool ShowVisual))
            {
                if (ShowVisual)
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
            else
            {
                Debug.Log("cant find Model for " + UnUniqueDeviceID);
            }
        }
    }
    public void HideTrackedVisual()
    {
        if (BasisVisualTracker != null)
        {
            GameObject.Destroy(BasisVisualTracker.gameObject);
        }
        if (LoadedDeviceRequest != null)
        {
            AddressableLoadFactory.ReleaseResource(LoadedDeviceRequest);
        }
    }
    public async void CreateRayCaster(BasisInput BaseInput)
    {
        Debug.Log("Adding RayCaster");
        BasisPointRaycasterRef = new GameObject(nameof(BasisPointRaycaster));
        BasisPointRaycasterRef.transform.parent = this.transform;
        BasisPointRaycasterRef.transform.SetLocalPositionAndRotation(BasisDeviceMatchableNames.PivotRaycastOffset, Quaternion.Euler(BasisDeviceMatchableNames.RotationRaycastOffset));
        BasisPointRaycaster = BasisHelpers.GetOrAddComponent<BasisPointRaycaster>(BasisPointRaycasterRef);
      await  BasisPointRaycaster.Initialize(BaseInput);
    }
}