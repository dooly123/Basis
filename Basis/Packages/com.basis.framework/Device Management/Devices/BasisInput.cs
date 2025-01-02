using Basis.Scripts.Addressable_Driver;
using Basis.Scripts.Addressable_Driver.Factory;
using Basis.Scripts.Avatar;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.TransformBinders.BoneControl;
using Basis.Scripts.UI;
using Basis.Scripts.UI.UI_Panels;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Basis.Scripts.Drivers.BaseBoneDriver;

namespace Basis.Scripts.Device_Management.Devices
{
    public abstract class BasisInput : MonoBehaviour
    {
        public bool HasEvents = false;
        public string SubSystemIdentifier;
        [SerializeField] private BasisBoneTrackedRole trackedRole;
        [SerializeField] public bool hasRoleAssigned;
        public BasisBoneControl Control = new BasisBoneControl();
        public bool HasControl = false;
        public string UniqueDeviceIdentifier;
        public string ClassName;
        [Header("Raw data from tracker unmodified")]
        public float3 LocalRawPosition;
        public quaternion LocalRawRotation;
        [Header("Final Data normally just modified by EyeHeight/AvatarEyeHeight)")]
        public float3 FinalPosition;
        public quaternion FinalRotation;
        [Header("Avatar Offset Applied Per Frame")]
        public float3 AvatarPositionOffset = Vector3.zero;
        public float3 AvatarRotationOffset = Vector3.zero;

        public bool HasUIInputSupport = false;
        public string CommonDeviceIdentifier;
        public BasisVisualTracker BasisVisualTracker;
        public BasisPointRaycaster BasisPointRaycaster;//used to raycast against things like UI
        public AddressableGenericResource LoadedDeviceRequest;
        public event SimulationHandler AfterControlApply;
        public GameObject BasisPointRaycasterRef;
        public BasisDeviceMatchSettings BasisDeviceMatchableNames;
        [SerializeField]
        public BasisInputState InputState = new BasisInputState();
        [SerializeField]
        public BasisInputState LastState = new BasisInputState();
        public BasisGeneralLocation GeneralLocation;
        public static BasisBoneTrackedRole[] CanHaveMultipleRoles = new BasisBoneTrackedRole[] { BasisBoneTrackedRole.LeftHand, BasisBoneTrackedRole.RightHand };
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
            int InputsCount = BasisDeviceManagement.Instance.AllInputDevices.Count;
            for (int Index = 0; Index < InputsCount; Index++)
            {
                BasisInput Input = BasisDeviceManagement.Instance.AllInputDevices[Index];
                if (Input.TryGetRole(out BasisBoneTrackedRole found) && Input != this)
                {
                    if (found == Role)
                    {
                        if (CanHaveMultipleRoles.Contains(found) == false)
                        {
                            BasisDebug.LogError("Already Found tracker for  " + Role, BasisDebug.LogTag.Input);
                            return;
                        }
                        else
                        {
                            BasisDebug.Log("Has Multiple Roles assigned for " + found + " most likely ok.", BasisDebug.LogTag.Input);
                        }
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
                    InitalRotation = Quaternion.Inverse(transform.rotation);
                    InitalBoneRotation = Control.OutgoingWorldData.rotation;
                    Control.InverseOffsetFromBone.position = Quaternion.Inverse(transform.rotation) * ((Vector3)Control.OutgoingWorldData.position - transform.position);
                    Control.InverseOffsetFromBone.rotation = InitalRotation * InitalBoneRotation;
                    Control.InverseOffsetFromBone.Use = true;
                    if (BasisBoneTrackedRoleCommonCheck.CheckIfHintRole(trackedRole))//we dont want to offset these ones
                    {
                        Control.IsHintRoleIgnoreRotation = true;
                    }
                    else
                    {
                        Control.IsHintRoleIgnoreRotation = false;
                    }
                }
                SetRealTrackers(BasisHasTracked.HasTracker, BasisHasRigLayer.HasRigLayer);
            }
            else
            {
                BasisDebug.LogError("Attempted to find " + Role + " but it did not exist");
            }
        }
        public Quaternion InitalRotation;
        public Quaternion InitalBoneRotation;
        public void UnAssignRoleAndTracker()
        {
            if(Control != null)
            {
                Control.IncomingData.position = Vector3.zero;
                Control.IncomingData.rotation = Quaternion.identity;
            }
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
            BasisDebug.Log("Finding ID " + unUniqueDeviceID, BasisDebug.LogTag.Input);
            AvatarRotationOffset = Quaternion.identity.eulerAngles;
            //configure device identifier
            SubSystemIdentifier = subSystems;
            CommonDeviceIdentifier = unUniqueDeviceID;
            UniqueDeviceIdentifier = uniqueID;
            // lets check to see if there is a override from a devices matcher
            BasisDeviceMatchableNames = BasisDeviceManagement.Instance.BasisDeviceNameMatcher.GetAssociatedDeviceMatchableNames(CommonDeviceIdentifier, basisBoneTrackedRole, ForceAssignTrackedRole);
            if (BasisDeviceMatchableNames.HasTrackedRole)
            {
                BasisDebug.Log("Overriding Tracker " + BasisDeviceMatchableNames.DeviceID, BasisDebug.LogTag.Input);
                AssignRoleAndTracker(BasisDeviceMatchableNames.TrackedRole);
            }

            if (hasRoleAssigned)
            {
                AvatarRotationOffset = BasisDeviceMatchableNames.AvatarRotationOffset;
                AvatarPositionOffset = BasisDeviceMatchableNames.AvatarPositionOffset;
                HasUIInputSupport = BasisDeviceMatchableNames.HasRayCastSupport;
                if (HasUIInputSupport)
                {
                    CreateRayCaster(this);
                }
            }
            /*            if (ForceAssignTrackedRole)
                {
                    AssignRoleAndTracker(basisBoneTrackedRole);
                }
             */
            //events
            if (HasEvents == false)
            {
                BasisLocalPlayer.Instance.LocalBoneDriver.OnSimulate += PollData;
                BasisLocalPlayer.Instance.OnAvatarSwitched += UnAssignFullBodyTrackers;
                BasisLocalPlayer.Instance.Move.ReadyToRead += ApplyFinalMovement;
                HasEvents = true;
            }
            else
            {
                BasisDebug.Log("has device events assigned already " + UniqueDeviceIdentifier, BasisDebug.LogTag.Input);
            }
        }
        public void ApplyFinalMovement()
        {
            transform.SetLocalPositionAndRotation(FinalPosition, FinalRotation);
        }
        public void UnAssignFullBodyTrackers()
        {
            if (hasRoleAssigned && HasControl)
            {
                if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(trackedRole))
                {
                    Control.IsHintRoleIgnoreRotation = false;
                    UnAssignTracker();
                }
            }
        }
        public void UnAssignFBTracker()
        {
            if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(trackedRole))
            {
                Control.IsHintRoleIgnoreRotation = false;
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
                    BasisDebug.Log("UnAssigning Tracker " + Control.Name, BasisDebug.LogTag.Input);
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
            BasisDebug.Log("ApplyTrackerCalibration " + Role + " to tracker " + UniqueDeviceIdentifier, BasisDebug.LogTag.Input);
            AssignRoleAndTracker(Role);
        }
        public void StopTracking()
        {
            if (BasisLocalPlayer.Instance.LocalBoneDriver == null)
            {
                BasisDebug.LogError("Missing Driver!");
                return;
            }
            UnAssignRoleAndTracker();
            if (HasEvents)
            {
                BasisLocalPlayer.Instance.LocalBoneDriver.OnSimulate -= PollData;
                BasisLocalPlayer.Instance.OnAvatarSwitched -= UnAssignFullBodyTrackers;
                BasisLocalPlayer.Instance.Move.ReadyToRead -= ApplyFinalMovement;
                HasEvents = false;
            }
            else
            {
                BasisDebug.Log("has device events assigned already " + UniqueDeviceIdentifier, BasisDebug.LogTag.Input);
            }
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
                BasisDebug.Log("Set Tracker State for tracker " + UniqueDeviceIdentifier + " with bone " + Control.Name + " as " + Control.HasTracked.ToString() + " | " + Control.HasRigLayer.ToString(), BasisDebug.LogTag.Input);
            }
            else
            {
                BasisDebug.LogError("Missing Controller Or Bone", BasisDebug.LogTag.Input);
            }
        }
        public void PollData()
        {
            LastUpdatePlayerControl();
            DoPollData();
        }
        public abstract void DoPollData();
        public void UpdatePlayerControl()
        {
            switch (trackedRole)
            {
                case BasisBoneTrackedRole.LeftHand:
                    float largestValue = Mathf.Abs(InputState.Primary2DAxis.x) > Mathf.Abs(InputState.Primary2DAxis.y)
                        ? InputState.Primary2DAxis.x
                        : InputState.Primary2DAxis.y;
                    //0 to 1 largestValue

                    BasisLocalPlayer.Instance.Move.SpeedMultiplyer = largestValue;
                    BasisLocalPlayer.Instance.Move.MovementVector = InputState.Primary2DAxis;
                    //only open ui after we have stopped pressing down on the secondary button
                    if (InputState.SecondaryButtonGetState == false && LastState.SecondaryButtonGetState)
                    {
                        if (BasisHamburgerMenu.Instance == null)
                        {
                            BasisHamburgerMenu.OpenHamburgerMenuNow();
                        }
                        else
                        {
                            BasisHamburgerMenu.Instance.CloseThisMenu();
                        }
                    }
                    if (InputState.PrimaryButtonGetState == false && LastState.PrimaryButtonGetState)
                    {
                        if (BasisInputModuleHandler.Instance.HasHoverONInput == false)
                        {
                            BasisLocalPlayer.Instance.MicrophoneRecorder.ToggleIsPaused();
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
                    if (InputState.PrimaryButtonGetState == false && LastState.PrimaryButtonGetState)
                    {
                        if (BasisInputModuleHandler.Instance.HasHoverONInput == false)
                        {
                            BasisLocalPlayer.Instance.MicrophoneRecorder.ToggleIsPaused();
                        }
                    }
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
            AfterControlApply?.Invoke();
        }
        public void LastUpdatePlayerControl()
        {
            InputState.CopyTo(LastState);
        }
        public void ShowTrackedVisual()
        {
            if (BasisVisualTracker == null && LoadedDeviceRequest == null)
            {
                BasisDeviceMatchSettings Match = BasisDeviceManagement.Instance.BasisDeviceNameMatcher.GetAssociatedDeviceMatchableNames(CommonDeviceIdentifier);
                if (Match.CanDisplayPhysicalTracker)
                {
                    var op = Addressables.LoadAssetAsync<GameObject>(Match.DeviceID);
                    GameObject go = op.WaitForCompletion();
                    GameObject gameObject = Object.Instantiate(go);
                    gameObject.name = CommonDeviceIdentifier;
                    gameObject.transform.parent = this.transform;
                    if (gameObject.TryGetComponent(out BasisVisualTracker))
                    {
                        BasisVisualTracker.Initialization(this);
                    }
                }
                else
                {
                    if (UseFallbackModel())
                    {
                        UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> op = Addressables.LoadAssetAsync<GameObject>(FallbackDeviceID);
                        GameObject go = op.WaitForCompletion();
                        GameObject gameObject = Object.Instantiate(go);
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
        public bool UseFallbackModel()
        {
            if (hasRoleAssigned == false)
            {
                return true;
            }
            else
            {
                if (TryGetRole(out BasisBoneTrackedRole Role))
                {
                    if (Role == BasisBoneTrackedRole.Head || Role == BasisBoneTrackedRole.CenterEye || Role == BasisBoneTrackedRole.Neck)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public static string FallbackDeviceID = "FallbackSphere";
        public void HideTrackedVisual()
        {
            BasisDebug.Log("HideTrackedVisual", BasisDebug.LogTag.Input);
            if (BasisVisualTracker != null)
            {
                BasisDebug.Log("Found and removing  HideTrackedVisual", BasisDebug.LogTag.Input);
                GameObject.Destroy(BasisVisualTracker.gameObject);
            }
            if (LoadedDeviceRequest != null)
            {
                BasisDebug.Log("Released Memory", BasisDebug.LogTag.Input);
                AddressableLoadFactory.ReleaseResource(LoadedDeviceRequest);
            }
        }
        public async void CreateRayCaster(BasisInput BaseInput)
        {
            BasisDebug.Log("Adding RayCaster");
            BasisPointRaycasterRef = new GameObject(nameof(BasisPointRaycaster));
            BasisPointRaycasterRef.transform.parent = this.transform;
            BasisPointRaycasterRef.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            BasisPointRaycaster = BasisHelpers.GetOrAddComponent<BasisPointRaycaster>(BasisPointRaycasterRef);
            await BasisPointRaycaster.Initialize(BaseInput);
        }
    }
}
