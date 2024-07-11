using System;
using System.Collections.Generic;
using UnityEngine;
public static partial class BasisAvatarIKStageCalibration
{
    /// <summary>
    ///  = 0.4f;
    /// </summary>
    public static float MaxDistanceBeforeMax(BasisBoneTrackedRole role)
    {

        switch (role)
        {
            case BasisBoneTrackedRole.CenterEye:
                return 0.6f;
            case BasisBoneTrackedRole.Head:
                return 0.6f;
            case BasisBoneTrackedRole.Neck:
                return 0.6f;
            case BasisBoneTrackedRole.Chest:
                return 0.8f;
            case BasisBoneTrackedRole.Hips:
                return 0.8f;
            case BasisBoneTrackedRole.Spine:
                return 0.8f;

            case BasisBoneTrackedRole.LeftUpperLeg:
                return 0.8f;
            case BasisBoneTrackedRole.RightUpperLeg:
                return 0.8f;

            case BasisBoneTrackedRole.LeftLowerLeg:
                return 0.8f;
            case BasisBoneTrackedRole.RightLowerLeg:
                return 0.8f;

            case BasisBoneTrackedRole.LeftFoot:
                return 0.6f;
            case BasisBoneTrackedRole.RightFoot:
                return 0.6f;

            case BasisBoneTrackedRole.UpperChest:
                return 0.8f;

            case BasisBoneTrackedRole.LeftShoulder:
                return 0.6f;
            case BasisBoneTrackedRole.RightShoulder:
                return 0.6f;

            case BasisBoneTrackedRole.LeftUpperArm:
                return 0.8f;
            case BasisBoneTrackedRole.RightUpperArm:
                return 0.8f;

            case BasisBoneTrackedRole.LeftLowerArm:
                return 0.8f;
            case BasisBoneTrackedRole.RightLowerArm:
                return 0.8f;

            case BasisBoneTrackedRole.LeftHand:
                return 0.6f;
            case BasisBoneTrackedRole.RightHand:
                return 0.6f;

            case BasisBoneTrackedRole.LeftToes:
                return 0.6f;
            case BasisBoneTrackedRole.RightToes:
                return 0.6f;

            case BasisBoneTrackedRole.Mouth:
                return 0.6f;
            default:
                Console.WriteLine("Unknown role");
                return 0.8f;
        }
    }
    private static List<BasisBoneTrackedRole> GetAllRoles()
    {
        List<BasisBoneTrackedRole> rolesToDiscover = new List<BasisBoneTrackedRole>();
        foreach (BasisBoneTrackedRole role in Enum.GetValues(typeof(BasisBoneTrackedRole)))
        {
            rolesToDiscover.Add(role);
        }
        // Create a dictionary for quick index lookup
        Dictionary<BasisBoneTrackedRole, int> orderLookup = new Dictionary<BasisBoneTrackedRole, int>();
        for (int i = 0; i < desiredOrder.Length; i++)
        {
            orderLookup[desiredOrder[i]] = i;
        }

        // Assign a large index value to roles not in the desired order
        int largeIndex = desiredOrder.Length;

        // Sort the list based on the desired order
        rolesToDiscover.Sort((x, y) =>
        {
            int indexX = orderLookup.ContainsKey(x) ? orderLookup[x] : largeIndex;
            int indexY = orderLookup.ContainsKey(y) ? orderLookup[y] : largeIndex;
            return indexX.CompareTo(indexY);
        });

        return rolesToDiscover;
    }
    /*
    public void LockAllBonesToActualTargets(List<BasisBoneTrackedRole> rolesToDiscover)
    {
        foreach(BasisBoneTrackedRole Role in rolesToDiscover)
        {
            BasisLocalPlayer Local = BasisLocalPlayer.Instance;
            Local.AvatarDriver.GetBoneRotAndPos(Local.LocalBoneDriver, Local.Avatar.Animator,);
        }
    }
    */
    public static void FullBodyCalibration()
    {
        BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTPose();
        List<BasisBoneTrackedRole> rolesToDiscover = GetAllRoles();
        List<BasisBoneTrackedRole> trackInputRoles = new List<BasisBoneTrackedRole>();
        for (int Index = 0; Index < rolesToDiscover.Count; Index++)
        {
            BasisBoneTrackedRole Role = rolesToDiscover[Index];
            if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(Role))
            {
                trackInputRoles.Add(Role);
            }
        }
        List<CalibrationConnector> connectors = new List<CalibrationConnector>();
        for (int Index = 0; Index < BasisDeviceManagement.Instance.AllInputDevices.Count; Index++)
        {
            BasisInput baseInput = BasisDeviceManagement.Instance.AllInputDevices[Index];
            if (baseInput.TryGetRole(out BasisBoneTrackedRole role))
            {
                if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(role))
                {
                    //in use un assign first
                    baseInput.UnAssignFullBodyTrackers();
                    CalibrationConnector calibrationConnector = new CalibrationConnector
                    {
                        BasisInput = baseInput,
                        Distance = float.MaxValue
                    };
                    connectors.Add(calibrationConnector);
                }
            }
            else//no assigned role
            {
                CalibrationConnector calibrationConnector = new CalibrationConnector
                {
                    BasisInput = baseInput,
                    Distance = float.MaxValue
                };
                //tracker was a uncalibrated type
                connectors.Add(calibrationConnector);
            }
        }
        List<BasisTrackerMapping> boneTransformMappings = new List<BasisTrackerMapping>();
        foreach (BasisBoneTrackedRole role in trackInputRoles)
        {
            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl control, role))
            {
               float ScaledDistance = MaxDistanceBeforeMax(role) * BasisLocalPlayer.Instance.RatioAvatarToAvatarEyeDefaultScale;
                Debug.Log("Using a scaler of  " + BasisLocalPlayer.Instance.RatioAvatarToAvatarEyeDefaultScale + " leading to a scaled Distance of " + ScaledDistance);
                BasisTrackerMapping mapping = new BasisTrackerMapping(control, role, connectors, ScaledDistance);
                boneTransformMappings.Add(mapping);
            }
            else
            {
                Debug.LogError("Missing bone control for role " + role);
            }
        }
        List<BasisBoneTrackedRole> roles = new List<BasisBoneTrackedRole>();
        List<BasisInput> BasisInputs = new List<BasisInput>();
        // Find optimal matches
        for (int Index = 0; Index < boneTransformMappings.Count; Index++)
        {
            BasisTrackerMapping mapping = boneTransformMappings[Index];
            if (mapping.TargetControl != null)
            {
                RunThroughConnectors(mapping, ref BasisInputs, ref roles);
            }
            else
            {
                Debug.LogError("Missing Tracker for index " + Index + " with ID " + mapping);
            }
        }
        BasisLocalPlayer.Instance.AvatarDriver.ResetAvatarAnimator();
        //do the roles after to stop the animator switch issue
        BasisLocalPlayer.Instance.AvatarDriver.CalibrateRoles();
    }
    public static void RunThroughConnectors(BasisTrackerMapping mapping, ref List<BasisInput> BasisInputs, ref List<BasisBoneTrackedRole> roles)
    {
        int CandidateCount = mapping.Candidates.Count;
        for (int Index = 0; Index < CandidateCount; Index++)
        {
            CalibrationConnector Connector = mapping.Candidates[Index];
            if (BasisInputs.Contains(Connector.BasisInput) == false)
            {
                if (roles.Contains(mapping.BasisBoneControlRole) == false)
                {
                    //  Debug.Log("Apply Tracked Data for " + mapping.BasisBoneControlRole + " attached to tracker " + Connector.Tracker.UniqueDeviceIdentifier);
                    Connector.BasisInput.ApplyTrackerCalibration(mapping.BasisBoneControlRole);
                    roles.Add(mapping.BasisBoneControlRole);
                    BasisInputs.Add(Connector.BasisInput);
                    break;
                }
                else
                {
                    //Debug.Log("we have already assigned role " + mapping.BasisBoneControlRole);
                }
            }
            else
            {
             //   Debug.Log("Already assigned " + Connector.Tracker);
            }
        }
    }
}