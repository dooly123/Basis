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

        Debug.Log("output is  " + Mode);

        ModeToTrackersMask(availableBoneControl, Mode, out List<BasisBoneControl> TrackedBoneControls, out List<BasisBoneTrackedRole> TrackedRoles);
        SequenceIndexes(ref TrackedBoneControls, ref TrackedRoles);
        AssignInputsToClosestControls(InputTrackers, TrackedBoneControls, TrackedRoles);
        BasisLocalPlayer.Instance.AvatarDriver.LocalCalibration();
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
        Debug.Log("Completed assigning input devices to closest bone controls.");
        bool[] assignedControls = new bool[availableBones.Count];
        Array.Fill(assignedControls, false);

        for (int inputIndex = 0; inputIndex < inputDevices.Count; inputIndex++)
        {
            BasisInput baseInput = inputDevices[inputIndex];
            BasisBoneControl closestControl = null;
            int selectedIndex = -1;
            float minDistance = float.MaxValue;

            for (int controlIndex = 0; controlIndex < availableBones.Count; controlIndex++)
            {
                BasisBoneControl control = availableBones[controlIndex];
                if (!assignedControls[controlIndex])
                {
                    float distance = Vector3.Distance(control.BoneTransform.position, baseInput.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestControl = control;
                        selectedIndex = controlIndex;
                    }
                }
            }

            if (closestControl != null && selectedIndex != -1 && !assignedControls[selectedIndex])
            {
                Debug.Log($"Closest tracker for {baseInput.name} is {closestControl.Name} at distance {minDistance}");
                if (BasisLocalPlayer.Instance.LocalBoneDriver.FindTrackedRole(closestControl, out BasisBoneTrackedRole role))
                {
                    ApplyToTarget(baseInput, role);
                }
                assignedControls[selectedIndex] = true;
            }
        }
    }
    public static void SequenceIndexes(ref List<BasisBoneControl> availableBones,ref List<BasisBoneTrackedRole> roles)
    {
        if (roles.Contains(BasisBoneTrackedRole.Hips))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.Hips);

            // Swap the Hips role and corresponding bone control to the front
            SwapAtIndex(roles, index, 0);
            SwapAtIndex(availableBones, index, 0);
        }
        if (roles.Contains(BasisBoneTrackedRole.LeftFoot))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.LeftFoot);

            // Swap the Hips role and corresponding bone control to the front
            SwapAtIndex(roles, index, 1);
            SwapAtIndex(availableBones, index, 1);
        }
        if (roles.Contains(BasisBoneTrackedRole.RightFoot))
        {
            int index = roles.IndexOf(BasisBoneTrackedRole.RightFoot);

            // Swap the Hips role and corresponding bone control to the front
            SwapAtIndex(roles, index, 2);
            SwapAtIndex(availableBones, index, 2);
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