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
        FullBodyCalibration();
    }
#endif

    public static void FullBodyCalibration()
    {
        BasisLocalPlayer.Instance.AvatarDriver.PutAvatarIntoTpose();

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

            if (AvaliableTrackersCount <= 3)
            {
                // Case for 3 or fewer trackers
                if (CheckFor3Point(Connector.BasisBoneTrackedRole))
                {
                    LatestConnectors.Add(Connector);
                }
            }
            /*
            else if (AvaliableTrackersCount >= 4 && AvaliableTrackersCount <= 5)
            {
                // Case for 4 or 5 trackers
                if (CheckFor3Point(Connector.BasisBoneTrackedRole) && CheckForNextPriority6Point(Connector.BasisBoneTrackedRole))
                {
                    LatestConnectors.Add(Connector);
                }
            }
            else if (AvaliableTrackersCount >= 6 && AvaliableTrackersCount <= 8)
            {
                // Case for 6 to 8 trackers
                if (CheckFor3Point(Connector.BasisBoneTrackedRole) && CheckForNextPriority6Point(Connector.BasisBoneTrackedRole) && CheckForNextPriority9Point(Connector.BasisBoneTrackedRole))
                {
                    LatestConnectors.Add(Connector);
                }
            }
            else if (AvaliableTrackersCount >= 9 && AvaliableTrackersCount <= 10)
            {
                // Case for 9 or 10 trackers
                if (CheckFor3Point(Connector.BasisBoneTrackedRole) && CheckForNextPriority6Point(Connector.BasisBoneTrackedRole) && CheckForNextPriority9Point(Connector.BasisBoneTrackedRole) && CheckForNextPriority11Point(Connector.BasisBoneTrackedRole))
                {
                    LatestConnectors.Add(Connector);
                }
            }
            */
            else// if (AvaliableTrackersCount >= 11)
            {
                // Case for 11 or more trackers
                //  if (CheckForNextPriority13Point(Connector.BasisBoneTrackedRole) && ReplaceMeOnceyouhaveTime(Connector.BasisBoneTrackedRole) && DisableAsIhaveNotImplemented(Connector.BasisBoneTrackedRole))
                // {
                //    LatestConnectors.Add(Connector);
                // }
            }
        }
    }
    public static bool CheckFor3Point(BasisBoneTrackedRole Role)
    {
        return (Role == BasisBoneTrackedRole.Hips || Role == BasisBoneTrackedRole.LeftFoot || Role == BasisBoneTrackedRole.RightFoot);
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

    private static List<BasisInput> GetUnassignedInputs(ref List<BasisBoneTrackedRole> rolesToDiscover)
    {
        List<BasisInput> trackInput = new List<BasisInput>();
        foreach (BasisInput baseInput in BasisDeviceManagement.Instance.AllInputDevices)
        {
            if (!baseInput.hasRoleAssigned)
            {
                trackInput.Add(baseInput);
            }
            else
            {
                Debug.Log("Removing role " + baseInput.TrackedRole);
                rolesToDiscover.Remove(baseInput.TrackedRole);
            }
        }
        Debug.Log("Completed input tracking");
        return trackInput;
    }
    private static List<BasisInput> GetAllInputsExcludingEyeAndHands(ref List<BasisBoneTrackedRole> rolesToDiscover)
    {
        List<BasisInput> trackInput = new List<BasisInput>();
        foreach (BasisInput baseInput in BasisDeviceManagement.Instance.AllInputDevices)
        {
            if (!baseInput.hasRoleAssigned)
            {
                trackInput.Add(baseInput);
            }
            else
            {
                // Debug.Log("Removing role " + baseInput.TrackedRole);
                // rolesToDiscover.Remove(baseInput.TrackedRole);
                if (baseInput.TrackedRole != BasisBoneTrackedRole.CenterEye && baseInput.TrackedRole != BasisBoneTrackedRole.LeftHand && baseInput.TrackedRole != BasisBoneTrackedRole.RightHand)
                {
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
    public struct CalibrationConnector
    {
        public BasisBoneControl BasisBoneControl;
        public BasisBoneTrackedRole BasisBoneTrackedRole;
        public GeneralLocation GeneralLocation;
    }
    public enum GeneralLocation
    {
        Middle, Left, Right
    }
    private static void FindOptimalMatches(List<BasisInput> inputDevices, List<CalibrationConnector> Connectors)
    {
        List<BoneTransformMapping> boneTransformMappings = new List<BoneTransformMapping>();
        foreach (BasisInput bone in inputDevices)
        {
            BoneTransformMapping mapping = new BoneTransformMapping(bone, Connectors.ToArray());
            boneTransformMappings.Add(mapping);
        }
        foreach (BoneTransformMapping mapping in boneTransformMappings)
        {
            for (int topDistanceIndex = 0; topDistanceIndex < mapping.Distances.Length; topDistanceIndex++)
            {
                float topdistance = mapping.Distances[topDistanceIndex];
                if (WasThereASmallerIndex(boneTransformMappings, mapping, topDistanceIndex, topdistance) == false)
                {
                    mapping.Closest = Connectors[topDistanceIndex];
                    ApplyToTarget(mapping.Bone, mapping.Closest.BasisBoneTrackedRole);
                }
            }
        }
    }
    public static void ApplyToTarget(BasisInput Input, BasisBoneTrackedRole Role)
    {
        // Assign the tracked role and apply it
        Input.TrackedRole = Role;
        Input.ApplyRole();
        Debug.Log($"Tracker role assigned for {Input.name}: {Input.TrackedRole}");
    }
    public static bool WasThereASmallerIndex(List<BoneTransformMapping> boneTransformMappings, BoneTransformMapping mapping, int topDistanceIndex, float topdistance)
    {
        foreach (BoneTransformMapping secondPass in boneTransformMappings)
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
    [System.Serializable]
    public class BoneTransformMapping
    {
        public BasisInput Bone;
        [SerializeField]
        public float[] Distances;
        [SerializeField]
        public CalibrationConnector Closest;

        public BoneTransformMapping(BasisInput bone, CalibrationConnector[] transformsToMatch)
        {
            Bone = bone;
            Distances = new float[transformsToMatch.Length];
            for (int i = 0; i < transformsToMatch.Length; i++)
            {
                Distances[i] = Vector3.Distance(bone.transform.position, transformsToMatch[i].BasisBoneControl.BoneModelTransform.position);
            }
        }
    }
}