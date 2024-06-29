using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public static class BasisAvatarIKStageCalibration
{
#if UNITY_EDITOR
    [MenuItem("Basis/CalibrateFB")]
    public static void CalibrateEditor()
    {
        Calibrate();
    }
#endif
    public static void Calibrate()
    {
        Debug.Log("running through");
        BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTpose();
        List<BasisBoneTrackedRole> rolesToDiscover = GetAllRoles();
        List<BasisInput> InputTrackers = GetUnassignedInputs(ref rolesToDiscover);
        List<BasisBoneControl> availableBoneControl = GetAvailableBoneControls(rolesToDiscover);

        int hasEnoughForFullBody = 3; // 3 point tracker
        bool makeSureFullBodyGoesFirst = InputTrackers.Count <= hasEnoughForFullBody;
        if (makeSureFullBodyGoesFirst)
        {
            Debug.Log("use 3Point");
        }
        BasisTrackingMode Mode = makeSureFullBodyGoesFirst ? BasisTrackingMode.HipAndFeet : BasisTrackingMode.EverythingGoes;
        ModeToTrackersMask(availableBoneControl, Mode, out List<BasisBoneControl> TrackedBoneControls, out List<BasisBoneTrackedRole> TrackedRoles);
       // Debug.Log("ModeToTrackersMask");
        SequenceIndexes(ref TrackedBoneControls, ref TrackedRoles);
      //  Debug.Log("SequenceIndexes");
        AssignInputsToClosestControls(InputTrackers, TrackedBoneControls, TrackedRoles);
       // Debug.Log("AssignInputsToClosestControls");
        BasisLocalPlayer.Instance.AvatarDriver.CalibrateRoles();
        //Debug.Log("CalibrateRoles");
        BasisLocalPlayer.Instance.AvatarDriver.ResetAvatarAnimator();
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
    private static List<BasisInput> GetUnassignedInputs(ref List<BasisBoneTrackedRole> rolesToDiscover)
    {
        List<BasisInput> TrackInput = new List<BasisInput>();
        foreach (BasisInput baseInput in BasisDeviceManagement.Instance.AllInputDevices)
        {
            if (!baseInput.hasRoleAssigned)
            {
                TrackInput.Add(baseInput);
            }
            else
            {
                rolesToDiscover.Remove(baseInput.TrackedRole);
            }
        }
        Debug.Log("Ran through additions");
        return TrackInput;
    }
    private static List<BasisBoneControl> GetAvailableBoneControls(List<BasisBoneTrackedRole> rolesToDiscover)
    {
        List<BasisBoneControl> availableBoneControl = new List<BasisBoneControl>();
        foreach (BasisBoneTrackedRole role in rolesToDiscover)
        {
            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindBone(out BasisBoneControl control, role))
            {
                availableBoneControl.Add(control);
            }
            else
            {
                Debug.LogError("Missing Role " + role);
            }
        }
        Debug.Log("Ran through Removals");
        return availableBoneControl;
    }
    public static void ModeToTrackersMask(List<BasisBoneControl> needsAHome, BasisTrackingMode TrackingMode, out List<BasisBoneControl> TrackedInputs, out List<BasisBoneTrackedRole> TrackedRoles)
    {
        TrackedInputs = new List<BasisBoneControl>();
        TrackedRoles = new List<BasisBoneTrackedRole>();

        foreach (BasisBoneControl baseInput in needsAHome)
        {
            if (BasisLocalPlayer.Instance.LocalBoneDriver.FindTrackedRole(baseInput, out BasisBoneTrackedRole role))
            {
                switch (TrackingMode)
                {
                    case BasisTrackingMode.JustUpperBody:
                        switch (role)
                        {
                            case BasisBoneTrackedRole.LeftLowerArm:
                            case BasisBoneTrackedRole.RightLowerArm:
                                TrackedInputs.Add(baseInput);
                                TrackedRoles.Add(role);
                                break;
                        }
                        break;

                    case BasisTrackingMode.HipAndFeet:
                        switch (role)
                        {
                            case BasisBoneTrackedRole.Hips:
                            case BasisBoneTrackedRole.LeftFoot:
                            case BasisBoneTrackedRole.RightFoot:
                                TrackedInputs.Add(baseInput);
                                TrackedRoles.Add(role);
                                break;
                        }
                        break;

                    case BasisTrackingMode.EverythingGoes:
                        TrackedInputs.Add(baseInput);
                        TrackedRoles.Add(role);
                        break;
                }
            }
        }
    }
    private static void AssignInputsToClosestControls(List<BasisInput> inputDevices, List<BasisBoneControl> availableBones, List<BasisBoneTrackedRole> roles)
    {
        List<Tuple<int, int>> optimalMatches = FindOptimalMatches(inputDevices, availableBones);
        foreach (var match in optimalMatches)
        {
            ApplyToTarget(inputDevices[match.Item1], roles[match.Item2]);
        }
    }
    public static List<Tuple<int, int>> FindOptimalMatches(List<BasisInput> inputDevices, List<BasisBoneControl> BasisBoneControls)
    {
        int inputDevicesCount = inputDevices.Count;
        int BasisBoneControlCount = BasisBoneControls.Count;
        Debug.Log("inputDevicesCount = " + inputDevicesCount + " BasisBoneControlCount = " + BasisBoneControlCount);
        // Create a cost matrix based on distances between points
        int[,] costMatrix = new int[inputDevicesCount, BasisBoneControlCount];

        for (int inputDevicesIndex = 0; inputDevicesIndex < inputDevicesCount; inputDevicesIndex++)
        {
            for (int BasisBoneControlIndex = 0; BasisBoneControlIndex < BasisBoneControlCount; BasisBoneControlIndex++)
            {
                // Ensure the positions are in the same coordinate space and not null
                if (inputDevices[inputDevicesIndex] == null || inputDevices[inputDevicesIndex].transform == null || BasisBoneControls[BasisBoneControlIndex] == null)
                {
                    throw new ArgumentNullException("One of the objects in the input lists is null.");
                }

                Vector3 inputPosition = inputDevices[inputDevicesIndex].transform.position;
                Vector3 arrayBPosition = BasisBoneControls[BasisBoneControlIndex].FinalisedWorldData.position;

                // Debugging log
                Debug.Log($"Input Device {inputDevicesIndex} Position: {inputPosition}");
                Debug.Log($"ArrayB {BasisBoneControlIndex} Position: {arrayBPosition}");

                // Compute the squared magnitude of the distance vector
                double squaredMagnitude = Vector3.SqrMagnitude(inputPosition - arrayBPosition);

                // Debugging log
                //Debug.Log($"Squared Magnitude between Input Device {i} and ArrayB {j}: {squaredMagnitude}");

                // Multiply by a scaling factor and cast to int
                costMatrix[inputDevicesIndex, BasisBoneControlIndex] = (int)(squaredMagnitude * 1000);
                Debug.Log($"Squared Magnitude between Input Device {inputDevicesIndex} and ArrayB {BasisBoneControlIndex}: {costMatrix[inputDevicesIndex, BasisBoneControlIndex]}");
            }
        }

        // Find the optimal assignments using the Hungarian Algorithm
        int[] matches = BasisHungarianAlgorithm.FindAssignments(costMatrix);

        // Convert matches to a list of tuples for easier usage
        List<Tuple<int, int>> optimalMatches = new List<Tuple<int, int>>();
        for (int i = 0; i < inputDevicesCount; i++)
        {
            if (matches[i] < BasisBoneControlCount)
            {
                optimalMatches.Add(new Tuple<int, int>(i, matches[i]));
            }
        }

        return optimalMatches;
    }
    public static void SequenceIndexes(ref List<BasisBoneControl> availableBones, ref List<BasisBoneTrackedRole> roles)
    {
        // Hips role and corresponding bone control to the front
        if (roles.Contains(BasisBoneTrackedRole.Hips))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.Hips);
            SwapAtIndex(roles, index, 0);
            SwapAtIndex(availableBones, index, 0);
        }

        // LeftFoot role and corresponding bone control to the second position
        if (roles.Contains(BasisBoneTrackedRole.LeftFoot))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.LeftFoot);
            SwapAtIndex(roles, index, 1);
            SwapAtIndex(availableBones, index, 1);
        }

        // RightFoot role and corresponding bone control to the third position
        if (roles.Contains(BasisBoneTrackedRole.RightFoot))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.RightFoot);
            SwapAtIndex(roles, index, 2);
            SwapAtIndex(availableBones, index, 2);
        }
        // RightFoot role and corresponding bone control to the third position
        if (roles.Contains(BasisBoneTrackedRole.Chest))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.Chest);
            SwapAtIndex(roles, index, 3);
            SwapAtIndex(availableBones, index, 3);
        }
        // RightFoot role and corresponding bone control to the third position
        if (roles.Contains(BasisBoneTrackedRole.LeftHand))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.LeftHand);
            SwapAtIndex(roles, index, 4);
            SwapAtIndex(availableBones, index, 4);
        }

        // RightFoot role and corresponding bone control to the third position
        if (roles.Contains(BasisBoneTrackedRole.RightHand))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.RightHand);
            SwapAtIndex(roles, index, 5);
            SwapAtIndex(availableBones, index, 5);
        }

        // RightFoot role and corresponding bone control to the third position
        if (roles.Contains(BasisBoneTrackedRole.UpperChest))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.UpperChest);
            SwapAtIndex(roles, index, 6);
            SwapAtIndex(availableBones, index, 6);
        }

        // Neck role and corresponding bone control to the end
        if (roles.Contains(BasisBoneTrackedRole.Neck))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.Neck);
            int lastIndex = roles.Count - 1;
            SwapAtIndex(roles, index, lastIndex);
            SwapAtIndex(availableBones, index, lastIndex);
        }
        // Neck role and corresponding bone control to the end
        if (roles.Contains(BasisBoneTrackedRole.Head))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.Head);
            int lastIndex = roles.Count - 2;
            SwapAtIndex(roles, index, lastIndex);
            SwapAtIndex(availableBones, index, lastIndex);
        }
        // Neck role and corresponding bone control to the end
        if (roles.Contains(BasisBoneTrackedRole.CenterEye))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.CenterEye);
            int lastIndex = roles.Count - 3;
            SwapAtIndex(roles, index, lastIndex);
            SwapAtIndex(availableBones, index, lastIndex);
        }
        // Neck role and corresponding bone control to the end
        if (roles.Contains(BasisBoneTrackedRole.Mouth))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.Mouth);
            int lastIndex = roles.Count - 4;
            SwapAtIndex(roles, index, lastIndex);
            SwapAtIndex(availableBones, index, lastIndex);
        }
    }

    // Helper method to swap elements in a list
    private static void SwapAtIndex<T>(List<T> list, int indexA, int indexB)
    {
        T temp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = temp;
    }
    public static void ApplyToTarget(BasisInput Input, BasisBoneTrackedRole Role)
    {
        // Assign the tracked role and apply it
        Input.TrackedRole = Role;
        Input.ApplyRole();
        Debug.Log($"Tracker role assigned for {Input.name}: {Input.TrackedRole}");
    }
    public enum BasisTrackingMode
    {
        HipAndFeet,
        JustUpperBody,
        EverythingGoes,
    }

}
