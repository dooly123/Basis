using System;
using System.Collections.Generic;
using UnityEngine;
using static BasisAvatarIKStageCalibration.BasisTrackerMapping;
public static partial class BasisAvatarIKStageCalibration
{
    public static float MaxDistanceBeforeMax = 0.2f;
    public static void FullBodyCalibration()
    {
        BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTPose();

        List<BasisBoneTrackedRole> rolesToDiscover = GetAllRoles();
        List<BasisInput> Trackers = GetAllInputsExcludingEyeAndHands(ref rolesToDiscover);
        List<CalibrationConnector> availableBoneControl = GetAvailableBoneControls(rolesToDiscover);
        DebugOptimalRoles(availableBoneControl);
        FindOptimalMatches(Trackers, availableBoneControl);

        BasisLocalPlayer.Instance.AvatarDriver.CalibrateRoles();
        BasisLocalPlayer.Instance.AvatarDriver.ResetAvatarAnimator();
    }
    private static void DebugOptimalRoles(List<CalibrationConnector> CC)
    {
        foreach (CalibrationConnector CalibrationConnector in CC)
        {
            //  Debug.Log("Order " + CalibrationConnector.BasisBoneTrackedRole);
        }
    }
    #region DiscoverWhatsPossible
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
                    Debug.Log("excluding role " + baseInput.TrackedRole + " from being used during FB");
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
                    BasisBoneControlRole = role,
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
    private static void FindOptimalMatches(List<BasisInput> inputDevices, List<CalibrationConnector> connectors)
    {
        List<BasisTrackerMapping> boneTransformMappings = new List<BasisTrackerMapping>();
        float scaler = BasisAvatarIKStageCalibration.MaxDistanceBeforeMax * BasisLocalPlayer.Instance.RatioAvatarToAvatarEyeDefaultScale;
        Debug.Log("Using a Scaled max Distance for trackers of " + scaler);

        // Create tracker mappings
        foreach (var tracker in inputDevices)
        {
            if (tracker == null)
            {
                Debug.LogError("missing tracker");
            }
            else
            {
                BasisTrackerMapping mapping = new BasisTrackerMapping(tracker, connectors, scaler);
                boneTransformMappings.Add(mapping);
            }
        }
        IterateOver(boneTransformMappings);
    }
    public static List<BasisBoneTrackedRole> roles = new List<BasisBoneTrackedRole>();
    public static List<BasisTrackerMapping> maps = new List<BasisTrackerMapping>();
    private static void IterateOver(List<BasisTrackerMapping> boneTransformMappings)
    {
        roles.Clear();
        maps.Clear();
        // Find optimal matches
        foreach (BasisTrackerMapping mapping in boneTransformMappings)
        {
            if (mapping.Tracker != null && mapping.Candidates.Count > 0)
            {
                // Find the closest candidate
                for (int Index = 0; Index < mapping.Candidates.Count; Index++)
                {
                    CalibrationConnector candidate = mapping.Candidates[Index];
                    if (roles.Contains(candidate.BasisBoneControlRole) == false && maps.Contains(mapping) == false)
                    {
                        ApplyToTarget(candidate, mapping);
                    }
                }
            }
            else
            {
                Debug.LogError("Missing Tracker for index " + boneTransformMappings.IndexOf(mapping) + " with ID " + mapping.Tracker.name);
            }
        }

    }
    public static void ApplyToTarget(CalibrationConnector candidate, BasisTrackerMapping mapping)
    {
        mapping.Tracker.ApplyTrackerCalibration(candidate.BasisBoneControlRole);
        roles.Add(candidate.BasisBoneControlRole);
        maps.Add(mapping);
    }
}