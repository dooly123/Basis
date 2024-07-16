using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static BaseBoneDriver;
using static BasisAvatarIKStageCalibration;
public abstract class BasisInput : MonoBehaviour
{
    public bool HasAssignedEvents = false;
    public string SubSystemIdentifier;
    [SerializeField] private BasisBoneTrackedRole trackedRole;
    [SerializeField] public bool hasRoleAssigned;
    public BasisBoneControl Control = new BasisBoneControl();
    public bool HasControl = false;
    public string UniqueDeviceIdentifier;
    public string ClassName;
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
    public string CommonDeviceIdentifier;
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
    public bool AssociatedFound;
    public GeneralLocation GeneralLocation;
    public bool TryGetRole(out BasisBoneTrackedRole BasisBoneTrackedRole)
    {
        if (hasRoleAssigned)
        {
            BasisBoneTrackedRole = trackedRole;
            return true;
        }
        BasisBoneTrackedRole = BasisBoneTrackedRole.CenterEye;
        return false;
    }
    public void AssignRoleAndTracker(BasisBoneTrackedRole Role)
    {
        hasRoleAssigned = true;
        foreach (BasisInput Input in BasisDeviceManagement.Instance.AllInputDevices)
        {
            if (Input.TryGetRole(out var found) && Input != this)
            {
                if (found == Role)
                {
                    Debug.LogError("Already Found tracker for  " + Role);
                    return;
                }
            }
        }
        trackedRole = Role;
        if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out Control, trackedRole))
        {
            HasControl = true;
        }
        if (HasControl)
        {
            if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(trackedRole))//we dont want to offset these ones
            {
                Control.InverseOffsetFromBone.position = Quaternion.Inverse(transform.rotation) * (Control.CurrentWorldData.position - transform.position);
                Control.InverseOffsetFromBone.rotation = Quaternion.Inverse(transform.rotation) * Control.CurrentWorldData.rotation;
                Control.InverseOffsetFromBone.Use = true;
            }
            SetRealTrackers(BasisHasTracked.HasTracker, BasisHasRigLayer.HasRigLayer);
        }
        else
        {
            Debug.LogError("Attempted to find " + Role + " but it did not exist");
        }
    }
    public void UnAssignRoleAndTracker()
    {
        if (BasisDeviceMatchableNames == null || BasisDeviceMatchableNames.HasTrackedRole == false)
        {
            //unassign last
            if (hasRoleAssigned)
            {
                SetRealTrackers(BasisHasTracked.HasNoTracker, BasisHasRigLayer.HasNoRigLayer);
            }
            hasRoleAssigned = false;
            trackedRole = BasisBoneTrackedRole.CenterEye;
            Control = null;
            HasControl = false;
        }
    }
    public void OnDisable()
    {
        StopTracking();
    }
    public void OnDestroy()
    {
        StopTracking();
    }
    /// <summary>
    /// initalize the tracking of this input
    /// </summary>
    /// <param name="uniqueID"></param>
    /// <param name="unUniqueDeviceID"></param>
    /// <param name="subSystems"></param>
    /// <param name="ForceAssignTrackedRole"></param>
    /// <param name="basisBoneTrackedRole"></param>
    public void InitalizeTracking(string uniqueID, string unUniqueDeviceID, string subSystems, bool ForceAssignTrackedRole, BasisBoneTrackedRole basisBoneTrackedRole)
    {
        //unassign the old tracker
        UnAssignTracker();
        Debug.Log("Finding ID " + unUniqueDeviceID);
        AvatarRotationOffset = Quaternion.identity;
        //configure device identifier
        SubSystemIdentifier = subSystems;
        CommonDeviceIdentifier = unUniqueDeviceID;
        UniqueDeviceIdentifier = uniqueID;
        // lets check to see if there is a override from a devices matcher
        AssociatedFound = BasisDeviceManagement.Instance.BasisDeviceNameMatcher.GetAssociatedDeviceMatchableNames(CommonDeviceIdentifier, out BasisDeviceMatchableNames);
        if (AssociatedFound)
        {
            if (BasisDeviceMatchableNames.HasTrackedRole)
            {
                Debug.Log("Overriding Tracker " + BasisDeviceMatchableNames.DeviceID);
                AssignRoleAndTracker(BasisDeviceMatchableNames.TrackedRole);
            }

            if (hasRoleAssigned)
            {
                if (HasControl)
                {
                    AvatarRotationOffset = Quaternion.Euler(BasisDeviceMatchableNames.AvatarRotationOffset);
                    AvatarPositionOffset = BasisDeviceMatchableNames.AvatarPositionOffset;

                    HasUIInputSupport = BasisDeviceMatchableNames.HasRayCastSupport;
                    if (HasUIInputSupport)
                    {
                        CreateRayCaster(this);
                    }
                }
                else
                {
                    Debug.LogError("Missing Tracked Role " + trackedRole);
                }
            }
        }
        else
        {
            //assign a new tracker if requested
            if (ForceAssignTrackedRole)
            {
                AssignRoleAndTracker(basisBoneTrackedRole);
            }
        }
        //events
        if (HasAssignedEvents == false)
        {
            HasAssignedEvents = true;
            BasisLocalPlayer.Instance.LocalBoneDriver.OnSimulate += PollData;
            BasisLocalPlayer.Instance.OnAvatarSwitched += UnAssignFullBodyTrackers;
        }
        else
        {
            Debug.Log("has device events assigned already " + UniqueDeviceIdentifier);
        }
    }
    public void UnAssignFullBodyTrackers()
    {
        if (hasRoleAssigned && HasControl)
        {
            if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(trackedRole))
            {
                UnAssignTracker();
            }
        }
    }
    public void UnAssignFBTracker()
    {
        if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(trackedRole))
        {
            UnAssignTracker();
        }
    }
    /// <summary>
    /// this api makes it so after a calibration the inital offset is reset.
    /// will only do its logic if has role assigned
    /// </summary>
    public void UnAssignTracker()
    {
        if (hasRoleAssigned)
        {
            if (HasControl)
            {
                Debug.Log("UnAssigning Tracker " + Control.Name);
                Control.InverseOffsetFromBone.position = Vector3.zero;
                Control.InverseOffsetFromBone.rotation = Quaternion.identity;
                Control.InverseOffsetFromBone.Use = false;
            }
            UnAssignRoleAndTracker();
        }
    }
    public void ApplyTrackerCalibration(BasisBoneTrackedRole Role)
    {
        UnAssignTracker();
        Debug.Log("ApplyTrackerCalibration " + Role + " to tracker " + UniqueDeviceIdentifier);
        AssignRoleAndTracker(Role);
    }
    public void StopTracking()
    {
        if (BasisLocalPlayer.Instance.LocalBoneDriver == null)
        {
            Debug.LogError("Missing Driver!");
            return;
        }
        BasisLocalPlayer.Instance.LocalBoneDriver.OnSimulate -= PollData;
        UnAssignRoleAndTracker();
    }
    public void SetRealTrackers(BasisHasTracked hasTracked, BasisHasRigLayer HasLayer)
    {
        if (Control != null && Control.HasBone)
        {
            Control.HasTracked = hasTracked;
            Control.HasRigLayer = HasLayer;
            if (Control.HasRigLayer == BasisHasRigLayer.HasNoRigLayer)
            {
                hasRoleAssigned = false;
                if (TryGetRole(out BasisBoneTrackedRole Role))
                {
                    BasisLocalPlayer.Instance.AvatarDriver.ApplyHint(Role, 0);
                }
            }
            else
            {
                hasRoleAssigned = true;
                if (TryGetRole(out BasisBoneTrackedRole Role))
                {
                    BasisLocalPlayer.Instance.AvatarDriver.ApplyHint(Role, 1);
                }
            }
            Debug.Log("Set Tracker State for tracker " + UniqueDeviceIdentifier + " with bone " + Control.Name + " as " + Control.HasTracked.ToString() + " | " + Control.HasRigLayer.ToString());
        }
        else
        {
            Debug.LogError("Missing Controller Or Bone");
        }
    }
    public abstract void PollData();
    public void UpdatePlayerControl()
    {
        switch (trackedRole)
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
                break;
            case BasisBoneTrackedRole.RightHand:
               BasisLocalPlayer.Instance.Move.Rotation = InputState.Primary2DAxis;
                if (InputState.PrimaryButtonGetState)
                {
                    BasisLocalPlayer.Instance.Move.HandleJump();
                }
                break;
            case BasisBoneTrackedRole.CenterEye:
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
            if (BasisDeviceManagement.Instance.BasisDeviceNameMatcher.GetAssociatedDeviceMatchableNames(CommonDeviceIdentifier, out BasisDeviceMatchableNames Match))
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
                            gameObject.name = CommonDeviceIdentifier;
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
                Debug.Log("cant find Model for " + CommonDeviceIdentifier);
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