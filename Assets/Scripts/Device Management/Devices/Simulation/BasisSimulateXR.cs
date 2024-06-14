using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

public class BasisSimulateXR
{
    public List<BasisInputXRSimulate> Inputs = new List<BasisInputXRSimulate>();
    public void StartSimulation()
    {

    }
    public void StopXR()
    {

    }
    public void CreatePhysicalTrackedDevice(string UniqueID, string UnUniqueID)
    {
        GameObject gameObject = new GameObject(UniqueID);
        gameObject.transform.parent = BasisLocalPlayer.Instance.LocalBoneDriver.transform;
        BasisInputXRSimulate BasisInput = gameObject.AddComponent<BasisInputXRSimulate>();
        Initalize(BasisInput, UniqueID, UnUniqueID);
        Inputs.Add(BasisInput);
        BasisDeviceManagement.Instance.AllInputDevices.Add(BasisInput);
    }
    public void Initalize(BasisInput BasisInput, string UniqueID, string UnUniqueID)
    {
        BasisInput.ActivateTracking(UniqueID, UnUniqueID);
    }
    public void DestroyXRInput(string ID)
    {
        foreach (var device in Inputs)
        {
            if (device.UniqueID == ID)
            {
                Inputs.Remove(device);
                UnityEngine.Object.Destroy(device.gameObject);
                break;
            }
        }
        List<BasisInput> Duplicate = new List<BasisInput>();
        Duplicate.AddRange(BasisDeviceManagement.Instance.AllInputDevices);
        foreach (var device in Duplicate)
        {
            if (device.UniqueID == ID)
            {
                BasisDeviceManagement.Instance.AllInputDevices.Remove(device);
            }
        }
    }
#if UNITY_EDITOR
    [MenuItem("Basis/Create Puck Tracker")]
    public static void CreatePuckTracker()
    {
        BasisDeviceManagement.Instance.BasisSimulateXR.CreatePhysicalTrackedDevice("{htc}vr_tracker_vive_3_0", "{htc}vr_tracker_vive_3_0");

        BasisDeviceManagement.ShowTrackers();
    }
    [MenuItem("Basis/Create 3Point Tracking")]
    public static void CreatePuck3Tracker()
    {
        BasisDeviceManagement.Instance.BasisSimulateXR.CreatePhysicalTrackedDevice("{htc}vr_tracker_vive_3_0", "{htc}vr_tracker_vive_3_0");
        BasisDeviceManagement.Instance.BasisSimulateXR.CreatePhysicalTrackedDevice("{htc}vr_tracker_vive_3_0", "{htc}vr_tracker_vive_3_0");
        BasisDeviceManagement.Instance.BasisSimulateXR.CreatePhysicalTrackedDevice("{htc}vr_tracker_vive_3_0", "{htc}vr_tracker_vive_3_0");

        var hips = BasisLocalPlayer.Instance.AvatarDriver.References.Hips;
        var leftFoot = BasisLocalPlayer.Instance.AvatarDriver.References.leftFoot;
        var rightFoot = BasisLocalPlayer.Instance.AvatarDriver.References.rightFoot;

        BasisInputXRSimulate BasisHips = BasisDeviceManagement.Instance.BasisSimulateXR.Inputs[0];
        BasisInputXRSimulate BasisLeftFoot = BasisDeviceManagement.Instance.BasisSimulateXR.Inputs[1];
        BasisInputXRSimulate BasisRightFoot = BasisDeviceManagement.Instance.BasisSimulateXR.Inputs[2];

        Vector3 HipsPosition = ModifyVector(hips.position);
        Vector3 leftFootPosition = ModifyVector(leftFoot.position);
        Vector3 rightFootPosition = ModifyVector(rightFoot.position);

        BasisHips.transform.SetPositionAndRotation(HipsPosition, hips.rotation);//* Random.rotation
        BasisLeftFoot.transform.SetPositionAndRotation(leftFootPosition, leftFoot.rotation);//* Random.rotation
        BasisRightFoot.transform.SetPositionAndRotation(rightFootPosition, rightFoot.rotation);//* Random.rotation

        BasisDeviceManagement.ShowTrackers();
    }
    public void CreateFakeOffset(out Quaternion Rotation, out Vector3 Position, BasisBoneControl control)
    {
        Rotation = Random.rotation;
        Position = ModifyVector(control.LocalRawPosition);
    }
    public static float randomRange = 0.1f;
    static Vector3 ModifyVector(Vector3 original)
    {
        float randomX = Random.Range(-randomRange, randomRange);
        float randomY = Random.Range(-randomRange, randomRange);
        float randomZ = Random.Range(-randomRange, randomRange);
        return new Vector3(original.x + randomX, original.y + randomY, original.z + randomZ);
    }
#endif
}