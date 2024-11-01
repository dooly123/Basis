using Basis.Scripts.Avatar;
using Basis.Scripts.BasisSdk.Players;
using Basis.Scripts.Common;
using Basis.Scripts.Device_Management;
using Basis.Scripts.Device_Management.Devices;
using Basis.Scripts.Device_Management.Devices.Desktop;
using Basis.Scripts.Device_Management.Devices.Simulation;
using Basis.Scripts.Networking;
using Basis.Scripts.Networking.Transmitters;
using Basis.Scripts.Profiler;
using Basis.Scripts.TransformBinders.BoneControl;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Profiling;
using Unity.Profiling.Editor;
using UnityEditor;
using UnityEngine;
using static Basis.Scripts.Device_Management.BasisDeviceManagement;
using static SerializableDarkRift;

public static class BasisMenuItemsEditor
{
    [MenuItem("Basis/Avatar/ReloadAvatar")]
    public static async Task ReloadAvatar()
    {
        if (BasisDataStore.LoadAvatar(BasisLocalPlayer.LoadFileNameAndExtension, BasisLocalPlayer.DefaultAvatar, BasisPlayer.LoadModeLocal, out BasisDataStore.BasisSavedAvatar LastSavedAvatar))
        {
            await BasisLocalPlayer.Instance.LoadInitalAvatar(LastSavedAvatar);
        }
    }
    [MenuItem("Basis/Trackers/Hide Trackers")]
    public static void HideTrackersEditor()
    {
        HideTrackers();
    }

    [MenuItem("Basis/Trackers/Show Trackers")]
    public static void ShowTrackersEditor()
    {
        ShowTrackers();
    }
    [MenuItem("Basis/Trackers/Destroy All Tracker")]
    public static void DestroyXRInput()
    {
        List<BasisInput> allDevicesToRemove = new List<BasisInput>(BasisDeviceManagement.Instance.AllInputDevices);

        // Remove devices from AllInputDevices list after iteration
        foreach (var device in allDevicesToRemove)
        {
            BasisDeviceManagement.Instance.RemoveDevicesFrom("BasisSimulateXR", device.UniqueDeviceIdentifier);
        }
    }
    [MenuItem("Basis/Trackers/Destroy And Restore XR Input")]
    public static void DestroyAndRebuildXRInput()
    {
        DestroyXRInput();
        List<StoredPreviousDevice> allDevicesToRemove = new List<StoredPreviousDevice>(BasisDeviceManagement.Instance.PreviouslyConnectedDevices);
        foreach (var device in allDevicesToRemove)
        {
           FindSimulate().CreatePhysicalTrackedDevice(device.UniqueID, "{htc}vr_tracker_vive_3_0");
        }
    }
    public static BasisSimulateXR FindSimulate()
    {
        if (BasisDeviceManagement.Instance.TryFindBasisBaseTypeManagement("SimulateXR", out List<BasisBaseTypeManagement> Matched))
        {
            foreach (var m in Matched)
            {
                BasisSimulateXR XR = (BasisSimulateXR)m;
                return XR;
            }
        }
        return null;
    }
    [MenuItem("Basis/Trackers/Create Puck Tracker")]
    public static void CreatePuckTracker()
    {
        BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTPose();

        FindSimulate().CreatePhysicalTrackedDevice("{htc}vr_tracker_vive_3_0" + Random.Range(-9999999999999, 999999999999), "{htc}vr_tracker_vive_3_0");
        BasisDeviceManagement.ShowTrackers();
        BasisLocalPlayer.Instance.AvatarDriver.ResetAvatarAnimator();
    }
    [MenuItem("Basis/Trackers/Create Vive Right Controller")]
    public static void CreateViveRightTracker()
    {
        BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl RightHand, BasisBoneTrackedRole.RightHand);
        BasisInputXRSimulate RightTracker = FindSimulate().CreatePhysicalTrackedDevice("{indexcontroller}valve_controller_knu_3_0_right" + Random.Range(-9999999999999, 999999999999), "{indexcontroller}valve_controller_knu_3_0_right", BasisBoneTrackedRole.RightHand, true);
        RightTracker.FollowMovement.position = RightHand.BoneModelTransform.position;
        RightTracker.FollowMovement.rotation = Quaternion.identity;
        BasisDeviceManagement.ShowTrackers();
    }
    [MenuItem("Basis/Trackers/Create Vive Left Controller")]
    public static void CreateViveLeftTracker()
    {
        BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl LeftHand, BasisBoneTrackedRole.LeftHand);
        BasisInputXRSimulate LeftTracker =  FindSimulate().CreatePhysicalTrackedDevice("{indexcontroller}valve_controller_knu_3_0_left" + Random.Range(-9999999999999, 999999999999), "{indexcontroller}valve_controller_knu_3_0_left", BasisBoneTrackedRole.LeftHand, true);
        LeftTracker.FollowMovement.position = LeftHand.BoneModelTransform.position;
        LeftTracker.FollowMovement.rotation = Quaternion.identity;
        BasisDeviceManagement.ShowTrackers();
    }
    [MenuItem("Basis/Trackers/Create Left And Right Hands")]
    public static void CreateLRTracker()
    {
        CreateViveLeftTracker();
        CreateViveRightTracker();
    }
    [MenuItem("Basis/Trackers/Create 3Point Tracking")]
    public static void CreatePuck3Tracker()
    {
        BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTPose();
       BasisSimulateXR XR = FindSimulate();
        BasisInputXRSimulate BasisHips =  XR.CreatePhysicalTrackedDevice("{htc}vr_tracker_vive_3_0 BasisHips | " + Random.Range(-9999999999999, 999999999999), "{htc}vr_tracker_vive_3_0");
        BasisInputXRSimulate BasisLeftFoot = XR.CreatePhysicalTrackedDevice("{htc}vr_tracker_vive_3_0 BasisLeftFoot | " + Random.Range(-9999999999999, 999999999999), "{htc}vr_tracker_vive_3_0");
        BasisInputXRSimulate BasisRightFoot =  XR.CreatePhysicalTrackedDevice("{htc}vr_tracker_vive_3_0 BasisRightFoot | " + Random.Range(-9999999999999, 999999999999), "{htc}vr_tracker_vive_3_0");

        var hips = BasisLocalPlayer.Instance.AvatarDriver.References.Hips;
        var leftFoot = BasisLocalPlayer.Instance.AvatarDriver.References.leftFoot;
        var rightFoot = BasisLocalPlayer.Instance.AvatarDriver.References.rightFoot;

        Vector3 HipsPosition = ModifyVector(hips.position);
        Vector3 leftFootPosition = ModifyVector(leftFoot.position);
        Vector3 rightFootPosition = ModifyVector(rightFoot.position);

        BasisHips.FollowMovement.position = HipsPosition;
        BasisLeftFoot.FollowMovement.position = leftFootPosition;
        BasisRightFoot.FollowMovement.position = rightFootPosition;

        BasisHips.FollowMovement.rotation = Random.rotation;
        BasisLeftFoot.FollowMovement.rotation = Random.rotation;
        BasisRightFoot.FollowMovement.rotation = Random.rotation;
        BasisLocalPlayer.Instance.AvatarDriver.ResetAvatarAnimator();
        // Show the trackers
        BasisDeviceManagement.ShowTrackers();
    }
    [MenuItem("Basis/Trackers/Create MaxTracker Tracking")]
    public static void CreateFullMaxTracker()
    {
        //  BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTPose();
        // Create an array of the tracker names for simplicity
        string trackerName = "{htc}vr_tracker_vive_3_0";

        var avatarDriver = BasisLocalPlayer.Instance.AvatarDriver.References;

        // Array of all relevant body parts
        Transform[] bodyParts = new Transform[]
        {
            avatarDriver.Hips, avatarDriver.spine, avatarDriver.chest, avatarDriver.neck, avatarDriver.head,
            avatarDriver.leftShoulder, avatarDriver.leftUpperArm,
            avatarDriver.leftLowerArm, avatarDriver.RightShoulder, avatarDriver.RightUpperArm,
            avatarDriver.RightLowerArm, avatarDriver.LeftUpperLeg, avatarDriver.LeftLowerLeg,
            avatarDriver.leftFoot, avatarDriver.leftToes, avatarDriver.RightUpperLeg, avatarDriver.RightLowerLeg,
            avatarDriver.rightFoot, avatarDriver.rightToes // avatarDriver.rightHand, avatarDriver.leftHand,  avatarDriver.LeftEye, avatarDriver.RightEye,
        };

        // Create an array of the BasisInputXRSimulate instances
        BasisInputXRSimulate[] trackers = new BasisInputXRSimulate[bodyParts.Length];
        BasisSimulateXR XR = FindSimulate();
        for (int Index = 0; Index < bodyParts.Length; Index++)
        {
            XR.CreatePhysicalTrackedDevice(trackerName + " part " + bodyParts[Index].name, trackerName);
            trackers[Index] = XR.Inputs[Index];
        }

        // Set positions and rotations for each tracker
        for (int Index = 0; Index < bodyParts.Length; Index++)
        {
            Transform bodyPart = bodyParts[Index];
            Vector3 bodyPartPosition = ModifyVector(bodyPart.position);
            trackers[Index].FollowMovement.position = bodyPartPosition;
            trackers[Index].FollowMovement.rotation = Random.rotation;
        }
        //   BasisLocalPlayer.Instance.AvatarDriver.ResetAvatarAnimator();

        // BasisAvatarIKStageCalibration.Calibrate();//disable for delayed testing
        // Show the trackers
        BasisDeviceManagement.ShowTrackers();
    }
    [MenuItem("Basis/Trackers/Create Mostly MaxTracker Tracking")]
    public static void CreateSemiMaxTracker()
    {
        //  BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTPose();
        // Create an array of the tracker names for simplicity
        string trackerName = "{htc}vr_tracker_vive_3_0";

        var avatarDriver = BasisLocalPlayer.Instance.AvatarDriver.References;

        // Array of all relevant body parts
        Transform[] bodyParts = new Transform[]
        {
            avatarDriver.Hips,
            avatarDriver.leftShoulder, avatarDriver.leftUpperArm,
            avatarDriver.leftLowerArm, avatarDriver.RightShoulder, avatarDriver.RightUpperArm,
            avatarDriver.RightLowerArm, avatarDriver.LeftUpperLeg, avatarDriver.LeftLowerLeg,
            avatarDriver.leftFoot, avatarDriver.leftToes, avatarDriver.RightUpperLeg, avatarDriver.RightLowerLeg,
            avatarDriver.rightFoot, avatarDriver.rightToes //  avatarDriver.chest,  avatarDriver.neck, avatarDriver.head, avatarDriver.spine, avatarDriver.rightHand, avatarDriver.leftHand,  avatarDriver.LeftEye, avatarDriver.RightEye,
        };

        // Create an array of the BasisInputXRSimulate instances
        BasisInputXRSimulate[] trackers = new BasisInputXRSimulate[bodyParts.Length];
        BasisSimulateXR XR = FindSimulate();
        for (int Index = 0; Index < bodyParts.Length; Index++)
        {
            if (bodyParts[Index] != null)
            {
                XR.CreatePhysicalTrackedDevice(trackerName + " part " + bodyParts[Index].name, trackerName);
                trackers[Index] = XR.Inputs[Index];
            }
        }

        // Set positions and rotations for each tracker
        for (int Index = 0; Index < bodyParts.Length; Index++)
        {
            Transform bodyPart = bodyParts[Index];
            if (bodyPart != null)
            {
                Vector3 bodyPartPosition = ModifyVector(bodyPart.position);
                trackers[Index].FollowMovement.SetPositionAndRotation(bodyPartPosition, Random.rotation);
            }
        }
        //   BasisLocalPlayer.Instance.AvatarDriver.ResetAvatarAnimator();

        // BasisAvatarIKStageCalibration.Calibrate();//disable for delayed testing
        // Show the trackers
        BasisDeviceManagement.ShowTrackers();
    }
    [MenuItem("Basis/Avatar/TPose Animator")]
    public static void PutAvatarIntoTpose()
    {
        BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTPose();
    }
    [MenuItem("Basis/Avatar/Normal Animator")]
    public static void ResetAvatarAnimator()
    {
        BasisLocalPlayer.Instance.AvatarDriver.ResetAvatarAnimator();
    }
    public static float randomRange = 0.1f;
    static Vector3 ModifyVector(Vector3 original)
    {
        float randomX = Random.Range(-randomRange, randomRange);
        float randomY = Random.Range(-randomRange, randomRange);
        float randomZ = Random.Range(-randomRange, randomRange);
        return new Vector3(original.x + randomX, original.y + randomY, original.z + randomZ);
    }
    [MenuItem("Basis/Calibration/CalibrateFB")]
    public static void CalibrateEditor()
    {
        BasisAvatarIKStageCalibration.FullBodyCalibration();
    }
    [MenuItem("Basis/Calibration/ProvideRandomData and create 3 point")]
    public static void ProvideRandomData()
    {
        Vector3 RotationVector = Random.rotation.eulerAngles;
        Vector3 OnlyY = new Vector3(0, RotationVector.y, 0);
        BasisLocalPlayer.Instance.transform.eulerAngles = OnlyY;

        BasisAvatarEyeInput basisAvatarEyeInput = GameObject.FindFirstObjectByType<BasisAvatarEyeInput>();
        if (basisAvatarEyeInput != null)
        {
            basisAvatarEyeInput.InjectedX = Random.Range(-3, 3);
            basisAvatarEyeInput.InjectedZ = Random.Range(-3, 3);
            basisAvatarEyeInput.rotationX = Random.Range(-360, 360);
        }
        BasisLocalPlayer.Instance.StartCoroutine(WaitAndCreatePuck3Tracker());
    }
    private static IEnumerator WaitAndCreatePuck3Tracker()
    {
        // Wait for the end of the frame
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return null;
        // Call the final API
        CreatePuck3Tracker();
    }
    [MenuItem("Basis/Player/Spawn Fake Remote")]
    public static void SpawnFakeRemote()
    {
        ServerReadyMessage serverSideSyncPlayerMessage = new ServerReadyMessage
        {
            playerIdMessage = new PlayerIdMessage
            {
                playerID = (ushort)(BasisNetworkManagement.Players.Count + 1)
            },
            localReadyMessage = new ReadyMessage()
        };
        serverSideSyncPlayerMessage.localReadyMessage.clientAvatarChangeMessage = new ClientAvatarChangeMessage();
        serverSideSyncPlayerMessage.localReadyMessage.localAvatarSyncMessage = new LocalAvatarSyncMessage();
        BasisNetworkTransmitter Transmitter = GameObject.FindFirstObjectByType<BasisNetworkTransmitter>();
        if (Transmitter != null)
        {
            Debug.Log("Apply SpawnFakeRemote");
            serverSideSyncPlayerMessage.localReadyMessage.localAvatarSyncMessage = Transmitter.LASM;
        }
        CreateTestRemotePlayer(serverSideSyncPlayerMessage);
    }
    public async static void CreateTestRemotePlayer(ServerReadyMessage ServerReadyMessage)
    {
        BasisNetworkManagement NetworkConnector = BasisNetworkManagement.Instance;
        if (NetworkConnector != null)
        {
            await BasisNetworkHandleRemote.CreateRemotePlayer(ServerReadyMessage, NetworkConnector.transform);
        }
    }
    [System.Serializable]
    [ProfilerModuleMetadata("Byte Array Count")]
    public class BasisNetworkingProfilerModule : ProfilerModule
    {
        static readonly ProfilerCounterDescriptor[] k_Counters = new ProfilerCounterDescriptor[]
        {
        new ProfilerCounterDescriptor(BasisNetworkProfiler.AvatarUpdatePacketText, "Avatar Packet Size"),
        new ProfilerCounterDescriptor(BasisNetworkProfiler.AudioUpdatePacketText, "Audio Packet Size"),
        };
        // Ensure that both ProfilerCategory.Scripts and ProfilerCategory.Memory categories are enabled when our module is active.
        static readonly string[] k_AutoEnabledCategoryNames = new string[]
        {
        ProfilerCategory.Scripts.Name,
         ProfilerCategory.Network.Name,
        };
        public BasisNetworkingProfilerModule() : base(k_Counters, autoEnabledCategoryNames: k_AutoEnabledCategoryNames) { }
    }
}