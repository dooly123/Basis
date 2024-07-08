using System;
using System.Collections.Generic;
using UnityEngine;
public static partial class BasisAvatarIKStageCalibration
{
    public static float MaxDistanceBeforeMax = 0.2f;
    public static void FullBodyCalibration()
    {
        BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTPose();

        List<BasisBoneTrackedRole> rolesToDiscover = GetAllRoles();
        List<BasisInput> Trackers = GetAllInputsExcludingEyeAndHands(ref rolesToDiscover);
        List<CalibrationConnector> availableBoneControl = GetAvailableBoneControls(rolesToDiscover);

        List<CalibrationConnector> Left = new List<CalibrationConnector>();
        List<CalibrationConnector> Right = new List<CalibrationConnector>();
        List<CalibrationConnector> Middle = new List<CalibrationConnector>();

        foreach (CalibrationConnector Connector in availableBoneControl)
        {
            if (Connector.GeneralLocation == GeneralLocation.Left)
            {
                Left.Add(Connector);
            }
            else
            {
                if (Connector.GeneralLocation == GeneralLocation.Right)
                {
                    Right.Add(Connector);
                }
                else
                {
                    if (Connector.GeneralLocation == GeneralLocation.Middle)
                    {
                        Middle.Add(Connector);
                    }
                }
            }
        }

        RunFor(Trackers.Count, availableBoneControl, out List<CalibrationConnector> MiddleOutput);
        FindOptimalMatches(Trackers, MiddleOutput);

        BasisLocalPlayer.Instance.AvatarDriver.CalibrateRoles();
        BasisLocalPlayer.Instance.AvatarDriver.ResetAvatarAnimator();
    }
    public static void RunFor(int AvaliableTrackersCount, List<CalibrationConnector> Connectors, out List<CalibrationConnector> LatestConnectors)
    {
        LatestConnectors = new List<CalibrationConnector>();

        for (int Index = 0; Index < Connectors.Count; Index++)
        {
            CalibrationConnector Connector = Connectors[Index];
            // Case for 3 or fewer trackers
            if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(Connector.BasisBoneTrackedRole) == false)
            {
                LatestConnectors.Add(Connector);
            }
        }
    }
    public static bool IsRight(Transform TransformRightCheck, Transform avatarMiddle)
    {
        Vector3 directionToOther = TransformRightCheck.position - avatarMiddle.position;
        // Vector3 avatarForward = avatarMiddle.forward;
        Vector3 avatarRight = avatarMiddle.right;
        // Check if the other transform is to the left or right
        float dotProduct = Vector3.Dot(avatarRight, directionToOther);

        if (dotProduct > 0)
        {
          //  Debug.Log(TransformRightCheck.name + " is on the right.");
            return true;
        }
        else if (dotProduct < 0)
        {
           // Debug.Log(TransformRightCheck.name + " is on the left.");
            return false;
        }
        else
        {
          //  Debug.Log(TransformRightCheck.name + " is directly in front or behind.");
        }
        return false;
    }
    private static List<BasisBoneTrackedRole> GetAllRoles()
    {
        List<BasisBoneTrackedRole> rolesToDiscover = new List<BasisBoneTrackedRole>();
        foreach (BasisBoneTrackedRole role in Enum.GetValues(typeof(BasisBoneTrackedRole)))
        {
            rolesToDiscover.Add(role);
        }
        return rolesToDiscover;
    }
    private static List<BasisInput> GetAllInputsExcludingEyeAndHands(ref List<BasisBoneTrackedRole> rolesToDiscover)
    {
        List<BasisInput> trackInput = new List<BasisInput>();
        foreach (BasisInput baseInput in BasisDeviceManagement.Instance.AllInputDevices)
        {
            if (baseInput.hasRoleAssigned == false)
            {
                Debug.Log("Add Tracker with name " + baseInput.name);
                trackInput.Add(baseInput);
            }
            else
            {
                if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(baseInput.TrackedRole))
                {
                    Debug.Log("Add Tracker that had last role " + baseInput.TrackedRole + " with name " + baseInput.name);
                    trackInput.Add(baseInput);
                }
                else
                {
                    Debug.Log("Removing role " + baseInput.TrackedRole);
                    rolesToDiscover.Remove(baseInput.TrackedRole);
                }
            }
        }
        Debug.Log("Completed input tracking");
        return trackInput;
    }
    private static List<CalibrationConnector> GetAvailableBoneControls(List<BasisBoneTrackedRole> rolesToDiscover)
    {
        List<CalibrationConnector> availableBoneControl = new List<CalibrationConnector>();
        foreach (BasisBoneTrackedRole role in rolesToDiscover)
        {
            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl control, role))
            {
                CalibrationConnector calibrationConnector = new CalibrationConnector
                {
                    BasisBoneTrackedRole = role,
                    BasisBoneControl = control
                };
                if (BasisAvatarDriver.IsApartOfSpineVertical(calibrationConnector.BasisBoneTrackedRole))
                {
                    calibrationConnector.GeneralLocation = GeneralLocation.Middle;
                }
                else
                {
                    if (IsRight(calibrationConnector.BasisBoneControl.BoneModelTransform, BasisLocalCameraDriver.Instance.Camera.transform))
                    {
                        calibrationConnector.GeneralLocation = GeneralLocation.Right;
                    }
                    else
                    {
                        calibrationConnector.GeneralLocation = GeneralLocation.Left;
                    }
                }
                availableBoneControl.Add(calibrationConnector);
            }
            else
            {
                Debug.LogError("Missing bone control for role " + role);
            }
        }
        Debug.Log("Completed bone control setup");
        return availableBoneControl;
    }
    private static void FindOptimalMatches(List<BasisInput> inputDevices, List<CalibrationConnector> Connectors)
    {
        List<BasisBoneTransformMapping> boneTransformMappings = new List<BasisBoneTransformMapping>();
        float Scaler = MaxDistanceBeforeMax / BasisLocalPlayer.Instance.RatioPlayerToAvatarScale;
        foreach (BasisInput bone in inputDevices)
        {
            BasisBoneTransformMapping mapping = new BasisBoneTransformMapping(bone, Connectors.ToArray(), Scaler);
            boneTransformMappings.Add(mapping);
        }
        foreach (BasisBoneTransformMapping mapping in boneTransformMappings)
        {
            for (int topDistanceIndex = 0; topDistanceIndex < mapping.Distances.Length; topDistanceIndex++)
            {
                if (WasThereASmallerIndex(boneTransformMappings, mapping, topDistanceIndex, mapping.Distances[topDistanceIndex]) == false)
                {
                    mapping.Closest = Connectors[topDistanceIndex];
                    ApplyToTarget(mapping.Bone, mapping.Closest.BasisBoneTrackedRole);
                }
            }
        }
    }
    public static void ApplyToTarget(BasisInput Input, BasisBoneTrackedRole Role)
    {
        Debug.Log($"Tracker role assigning for {Input.name}: {Input.TrackedRole}");
        // Assign the tracked role and apply it
        Input.ApplyTrackerCalibration(Role);
        Debug.Log($"Tracker role assigned for {Input.name}: {Input.TrackedRole}");
    }
    public static bool WasThereASmallerIndex(List<BasisBoneTransformMapping> boneTransformMappings, BasisBoneTransformMapping mapping, int topDistanceIndex, float topdistance)
    {
        foreach (BasisBoneTransformMapping secondPass in boneTransformMappings)
        {
            if (secondPass != mapping)
            {
                if (secondPass.Distances[topDistanceIndex] < topdistance)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public static bool CheckForNextPriority6Point(BasisBoneTrackedRole Role)
    {
        return (Role == BasisBoneTrackedRole.Chest || Role == BasisBoneTrackedRole.LeftUpperLeg || Role == BasisBoneTrackedRole.RightUpperLeg);
    }
    public static bool CheckForNextPriority9Point(BasisBoneTrackedRole Role)
    {
        return (Role == BasisBoneTrackedRole.UpperChest || Role == BasisBoneTrackedRole.LeftUpperArm || Role == BasisBoneTrackedRole.RightUpperArm);
    }
    public static bool CheckForNextPriority11Point(BasisBoneTrackedRole Role)
    {
        return (Role == BasisBoneTrackedRole.LeftToes || Role == BasisBoneTrackedRole.RightToes || Role == BasisBoneTrackedRole.Neck);
    }
    public static bool CheckForNextPriority13Point(BasisBoneTrackedRole Role)
    {
        return true;
    }
    public static bool ReplaceMeOnceyouhaveTime(BasisBoneTrackedRole Role)
    {
        if (Role == BasisBoneTrackedRole.Head || Role == BasisBoneTrackedRole.Neck || Role == BasisBoneTrackedRole.CenterEye || Role == BasisBoneTrackedRole.Mouth)
        {
            return false;
        }
        return true;
    }
    public static bool DisableAsIhaveNotImplemented(BasisBoneTrackedRole Role)
    {
        if (Role == BasisBoneTrackedRole.Chest || Role == BasisBoneTrackedRole.UpperChest || Role == BasisBoneTrackedRole.Spine)
        {
            return false;
        }
        return true;
    }
}