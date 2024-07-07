using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static BaseBoneDriver;
public abstract class BasisInput : MonoBehaviour
{
    public bool HasAssignedEvents = false;
    public string SubSystem;
    public BasisLocalBoneDriver Driver;
    private BasisBoneTrackedRole trackedRole;
    public bool hasRoleAssigned = false;
    public BasisBoneControl Control = new BasisBoneControl();
    public string UniqueID;
    [Header("Raw data from tracker unmodified")]
    public Vector3 LocalRawPosition;
    public Quaternion LocalRawRotation;
    [Header("Final Data normally just modified by EyeHeight/AvatarEyeHeight)")]
    public Vector3 FinalPosition;
    public Quaternion FinalRotation;
    [Header("Avatar Offset Applied Per Frame")]
    public Vector3 AvatarPositionOffset = Vector3.zero;
    public Quaternion AvatarRotationOffset = Quaternion.identity;

    public bool HasUIInputSupport = false;
    public string UnUniqueDeviceID;
    public BasisVisualTracker BasisVisualTracker;
    public BasisPointRaycaster BasisPointRaycaster;//used to raycast against things like UI
    public AddressableGenericResource LoadedDeviceRequest;
    public event SimulationHandler AfterControlApply;
    public GameObject BasisPointRaycasterRef;
    public BasisDeviceMatchableNames BasisDeviceMatchableNames;
    [SerializeField]
    public BasisInputState InputState = new BasisInputState();
    [SerializeField]
    public BasisInputState LastState = new BasisInputState();
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
    public void ActivateTracking(string UniqueID, string unUniqueDeviceID, string subSystems,bool AssignTrackedRole, BasisBoneTrackedRole basisBoneTrackedRole)
    {
        this.TrackedRole = BasisBoneTrackedRole.CenterEye;
        hasRoleAssigned = false;
        Debug.Log("Finding ID " + unUniqueDeviceID);

        AvatarRotationOffset = Quaternion.identity;
        SubSystem = subSystems;
        this.UnUniqueDeviceID = unUniqueDeviceID;
        this.UniqueID = UniqueID;
        if (AssignTrackedRole)
        {
            this.TrackedRole = basisBoneTrackedRole;
        }
        Driver = BasisLocalPlayer.Instance.LocalBoneDriver;
        if (Driver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        OnAvatarSwitched();
        ApplyDeviceAssociatedData();
        if (HasAssignedEvents == false)
        {
            Driver.OnSimulate += PollData;
            HasAssignedEvents = true;
            BasisLocalPlayer.Instance.OnAvatarSwitched += OnAvatarSwitched;
        }
        else
        {
            Debug.Log("has device events assigned already " + UniqueID);
        }
    }
    public void ApplyDeviceAssociatedData()
    {
        //unassign last
        SetRealTrackers(BasisHasTracked.HasNoTracker, BasisHasRigLayer.HasNoRigLayer);
        bool AssociatedFound = BasisDeviceManagement.Instance.BasisDeviceNameMatcher.GetAssociatedDeviceMatchableNames(this.UnUniqueDeviceID, out BasisDeviceMatchableNames);
        if (AssociatedFound)
        {
            if (BasisDeviceMatchableNames.HasTrackedRole)
            {
                Debug.Log("Overriding Tracker " + BasisDeviceMatchableNames.DeviceID);
                TrackedRole = BasisDeviceMatchableNames.TrackedRole;
            }
        }
        if (hasRoleAssigned)
        {
            if (Driver.FindBone(out Control, TrackedRole))
            {
                Control.HasRigLayer = BasisHasRigLayer.HasRigLayer;
                if (AssociatedFound)
                {
                    AvatarRotationOffset = Quaternion.Euler(BasisDeviceMatchableNames.AvatarRotationOffset);
                    AvatarPositionOffset = BasisDeviceMatchableNames.AvatarPositionOffset;

                    HasUIInputSupport = BasisDeviceMatchableNames.HasRayCastSupport;
                    if (HasUIInputSupport)
                    {
                        CreateRayCaster(this);
                    }
                }
                // Do nothing if bone is found successfully
                SetRealTrackers(BasisHasTracked.HasTracker, BasisHasRigLayer.HasRigLayer);
            }
            else
            {
                Debug.LogError("Missing Tracked Role " + TrackedRole);
            }
        }
    }

    /// <summary>
    /// this api makes it so after a calibration the inital offset is reset.
    /// will only do its logic if has role assigned
    /// </summary>
    public void OnAvatarSwitched()
    {
        if (hasRoleAssigned)
        {
            Control.InitialOffset.position = Vector3.zero;
            Control.InitialOffset.rotation = Quaternion.identity;
            Control.InitialOffset.Use = false;
            if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(TrackedRole))
            {
                SetRealTrackers(BasisHasTracked.HasNoTracker, BasisHasRigLayer.HasNoRigLayer);//basically lets unassign the trackers from there jobs
            }
        }
    }
    public void ApplyTrackerCalibration()
    {
        //unassign last
        SetRealTrackers(BasisHasTracked.HasNoTracker, BasisHasRigLayer.HasNoRigLayer);
        if (hasRoleAssigned)
        {
            if (Driver.FindBone(out Control, TrackedRole))
            {
                if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(TrackedRole))//we dont want to offset these ones
                {
                    Control.InitialOffset.position = Quaternion.Inverse(transform.rotation) * (Control.FinalisedWorldData.position - transform.position);
                    Control.InitialOffset.rotation = Quaternion.Inverse(transform.rotation) * Control.FinalisedWorldData.rotation;
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
    public void SetRealTrackers(BasisHasTracked hasTracked, BasisHasRigLayer HasLayer)
    {
        if (Control.HasBone)
        {
            Control.HasTrackerPositionDriver = hasTracked;
            Control.HasTrackerRotationDriver = hasTracked;
            Control.HasRigLayer = HasLayer;
            if (Control.HasRigLayer == BasisHasRigLayer.HasNoRigLayer)
            {
                hasRoleAssigned = false;
            }
            else
            {
                hasRoleAssigned = true;
            }
        }
    }
    public abstract void PollData();
    public void UpdatePlayerControl()
    {
        switch (TrackedRole)
        {
            case BasisBoneTrackedRole.LeftHand:
                BasisLocalPlayer.Instance.Move.MovementVector = InputState.Primary2DAxis;
                //only open ui after we have stopped pressing down on the secondary button
                if (InputState.SecondaryButtonGetState == false && LastState.SecondaryButtonGetState)
                {
                    if (BasisHamburgerMenu.Instance == null)
                    {
                        BasisHamburgerMenu.OpenHamburgerMenuNow();
                        BasisDeviceManagement.ShowTrackers();
                    }
                    else
                    {
                        BasisHamburgerMenu.Instance.CloseThisMenu();
                        BasisDeviceManagement.HideTrackers();
                    }
                }
                Control.ApplyMovement();
                break;
            case BasisBoneTrackedRole.RightHand:
                BasisLocalPlayer.Instance.Move.Rotation = InputState.Primary2DAxis;
                if (InputState.PrimaryButtonGetState)
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
        InputState.CopyTo(LastState);
        AfterControlApply?.Invoke();
    }
    public async Task ShowTrackedVisual()
    {
        if (BasisVisualTracker == null && LoadedDeviceRequest == null)
        {
            if (BasisDeviceManagement.Instance.BasisDeviceNameMatcher.GetAssociatedDeviceMatchableNames(UnUniqueDeviceID, out BasisDeviceMatchableNames Match))
            {
                if (Match.CanDisplayPhysicalTracker)
                {
                    (List<GameObject>, AddressableGenericResource) data = await AddressableResourceProcess.LoadAsGameObjectsAsync(Match.DeviceID, new UnityEngine.ResourceManagement.ResourceProviders.InstantiationParameters());
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
        Debug.Log("HideTrackedVisual");
        if (BasisVisualTracker != null)
        {
            Debug.Log("Found and removing  HideTrackedVisual");
            GameObject.Destroy(BasisVisualTracker.gameObject);
        }
        if (LoadedDeviceRequest != null)
        {
            Debug.Log("Released Memory");
            AddressableLoadFactory.ReleaseResource(LoadedDeviceRequest);
        }
    }
    public async void CreateRayCaster(BasisInput BaseInput)
    {
        Debug.Log("Adding RayCaster");
        BasisPointRaycasterRef = new GameObject(nameof(BasisPointRaycaster));
        BasisPointRaycasterRef.transform.parent = this.transform;
        BasisPointRaycasterRef.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        BasisPointRaycaster = BasisHelpers.GetOrAddComponent<BasisPointRaycaster>(BasisPointRaycasterRef);
        await BasisPointRaycaster.Initialize(BaseInput);
    }
}