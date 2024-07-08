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
        FindOptimalMatches(Trackers, availableBoneControl);

        BasisLocalPlayer.Instance.AvatarDriver.CalibrateRoles();
        BasisLocalPlayer.Instance.AvatarDriver.ResetAvatarAnimator();
    }
    #region DiscoverWhatsPossible
    private static List<BasisBoneTrackedRole> GetAllRoles()
    {
        List<BasisBoneTrackedRole> rolesToDiscover = new List<BasisBoneTrackedRole>();
        foreach (BasisBoneTrackedRole role in Enum.GetValues(typeof(BasisBoneTrackedRole)))
        {
            rolesToDiscover.Add(role);
        }
        // Define the desired order
        BasisBoneTrackedRole[] desiredOrder = new BasisBoneTrackedRole[]
        {
            BasisBoneTrackedRole.Hips,
            BasisBoneTrackedRole.LeftFoot,
            BasisBoneTrackedRole.RightFoot,
            BasisBoneTrackedRole.LeftUpperLeg,
            BasisBoneTrackedRole.RightUpperLeg,
            BasisBoneTrackedRole.LeftLowerLeg,
            BasisBoneTrackedRole.RightLowerLeg,
            BasisBoneTrackedRole.LeftShoulder,
            BasisBoneTrackedRole.RightShoulder,
            BasisBoneTrackedRole.LeftUpperArm,
            BasisBoneTrackedRole.RightUpperArm,
            BasisBoneTrackedRole.LeftLowerArm,
            BasisBoneTrackedRole.RightLowerArm,
            BasisBoneTrackedRole.LeftToes,
            BasisBoneTrackedRole.RightToes,
            BasisBoneTrackedRole.CenterEye,
            BasisBoneTrackedRole.Head,
            BasisBoneTrackedRole.Neck,
            BasisBoneTrackedRole.Chest,
            BasisBoneTrackedRole.LeftHand,
            BasisBoneTrackedRole.RightHand,
        };

        // Sort the list based on the desired order
        rolesToDiscover.Sort((x, y) => Array.IndexOf(desiredOrder, x).CompareTo(Array.IndexOf(desiredOrder, y)));

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
    #endregion
    private static void FindOptimalMatches(List<BasisInput> inputDevices, List<CalibrationConnector> Connectors)
    {
        List<BasisBoneTransformMapping> boneTransformMappings = new List<BasisBoneTransformMapping>();
        float Scaler = BasisAvatarIKStageCalibration.MaxDistanceBeforeMax * BasisLocalPlayer.Instance.RatioAvatarToAvatarEyeDefaultScale;
        foreach (BasisInput bone in inputDevices)
        {
            BasisBoneTransformMapping mapping = new BasisBoneTransformMapping(bone, Connectors, Scaler);
            boneTransformMappings.Add(mapping);
        }
        List<BasisBoneTrackedRole> FoundRoles = new List<BasisBoneTrackedRole>();
        foreach (BasisBoneTransformMapping connector in boneTransformMappings)
        {
            float SmallestForConnector = float.MaxValue;
            CalibrationConnector CalibrationConnector = new CalibrationConnector();
            bool HasFoundTarget = false;
            foreach (KeyValuePair<CalibrationConnector, float> Distance in connector.Distances)
            {
                if (Distance.Value < SmallestForConnector && FoundRoles.Contains(Distance.Key.BasisBoneTrackedRole) == false)
                {
                    SmallestForConnector = Distance.Value;
                    CalibrationConnector = Distance.Key;
                    HasFoundTarget = true;
                    Debug.Log("Found Con at distance " + Distance.Value + " " + Distance.Key.BasisBoneTrackedRole + "with bone name " + connector.Bone.name);
                }
            }
            if(HasFoundTarget)
            {
                FoundRoles.Add(CalibrationConnector.BasisBoneTrackedRole);
                connector.Bone.ApplyTrackerCalibration(CalibrationConnector.BasisBoneTrackedRole);
            }
        }
    }
}