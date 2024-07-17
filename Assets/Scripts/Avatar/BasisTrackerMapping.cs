using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class BasisAvatarIKStageCalibration
{
    [System.Serializable]
    public class BasisTrackerMapping
    {
        [SerializeField]
        public BasisBoneControl TargetControl;
        [SerializeField]
        public BasisBoneTrackedRole BasisBoneControlRole;
        [SerializeField]
        public List<CalibrationConnector> Candidates = new List<CalibrationConnector>();
        public List<Vector3> Stored = new List<Vector3>();
        public BasisTrackerMapping(BasisBoneControl Bone, BasisBoneTrackedRole Role, List<CalibrationConnector> calibration, float calibrationMaxDistance)
        {
            TargetControl = Bone;
            BasisBoneControlRole = Role;
            Candidates = new List<CalibrationConnector>();
            BasisLocalPlayer.Instance.SimulateHips();
            for (int Index = 0; Index < calibration.Count; Index++)
            {
                Vector3 Input = calibration[Index].BasisInput.transform.position;
                Vector3 BoneControl = TargetControl.BoneTransform.position;
                calibration[Index].Distance = Vector3.Distance(BoneControl, Input);

                if (calibration[Index].Distance < calibrationMaxDistance)
                {
                    Color randomColor = UnityEngine.Random.ColorHSV();
                    Debug.DrawLine(BoneControl, Input, randomColor, 40f);
                    Candidates.Add(calibration[Index]);
                }
                else
                {
                    // Debug.DrawLine(BoneControl, Input, Color.red, 40f);
                }
            }
            Candidates.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        }
    }
    public static GeneralLocation GetLocation(Vector3 Tracker, Vector3 Eye, Transform forward)
    {
        // Calculate the direction from Eye to Tracker
        Vector3 delta = (Tracker - Eye).normalized;

        // Calculate the right direction based on the forward direction
        Vector3 right = forward.forward;
        Debug.DrawLine(delta, delta + right * 3, Color.magenta, 12f);
        // Calculate the dot product between delta and right
        float dot = Vector3.Dot(delta, right);

        // Determine location based on dot product
        if (Mathf.Abs(dot) < Mathf.Epsilon)
        {
            // Target is straight ahead or directly behind
            return GeneralLocation.Center;
        }
        else if (dot > 0)
        {
            // Target is to the right
            return GeneralLocation.Right;
        }
        else
        {
            // Target is to the left
            return GeneralLocation.Left;
        }
    }
    public class CalibrationConnector
    {
        [SerializeField]
        public BasisInput BasisInput;
        public float Distance;
    }
    public static GeneralLocation FindGeneralLocation(BasisBoneTrackedRole Role)
    {

        switch (Role)
        {
            case BasisBoneTrackedRole.CenterEye:
                return BasisAvatarIKStageCalibration.GeneralLocation.Center;
            case BasisBoneTrackedRole.Head:
                return BasisAvatarIKStageCalibration.GeneralLocation.Center;
            case BasisBoneTrackedRole.Neck:
                return BasisAvatarIKStageCalibration.GeneralLocation.Center;
            case BasisBoneTrackedRole.Chest:
                return BasisAvatarIKStageCalibration.GeneralLocation.Center;
            case BasisBoneTrackedRole.Hips:
                return BasisAvatarIKStageCalibration.GeneralLocation.Center;
            case BasisBoneTrackedRole.Spine:
                return BasisAvatarIKStageCalibration.GeneralLocation.Center;
            case BasisBoneTrackedRole.Mouth:
                return BasisAvatarIKStageCalibration.GeneralLocation.Center;

            case BasisBoneTrackedRole.RightUpperLeg:
                return BasisAvatarIKStageCalibration.GeneralLocation.Right;
            case BasisBoneTrackedRole.RightLowerLeg:
                return BasisAvatarIKStageCalibration.GeneralLocation.Right;
            case BasisBoneTrackedRole.RightFoot:
                return BasisAvatarIKStageCalibration.GeneralLocation.Right;
            case BasisBoneTrackedRole.RightShoulder:
                return BasisAvatarIKStageCalibration.GeneralLocation.Right;
            case BasisBoneTrackedRole.RightUpperArm:
                return BasisAvatarIKStageCalibration.GeneralLocation.Right;
            case BasisBoneTrackedRole.RightLowerArm:
                return BasisAvatarIKStageCalibration.GeneralLocation.Right;
            case BasisBoneTrackedRole.RightHand:
                return BasisAvatarIKStageCalibration.GeneralLocation.Right;
            case BasisBoneTrackedRole.RightToes:
                return BasisAvatarIKStageCalibration.GeneralLocation.Right;

            case BasisBoneTrackedRole.LeftHand:
                return BasisAvatarIKStageCalibration.GeneralLocation.Left;
            case BasisBoneTrackedRole.LeftShoulder:
                return BasisAvatarIKStageCalibration.GeneralLocation.Left;
            case BasisBoneTrackedRole.LeftLowerArm:
                return BasisAvatarIKStageCalibration.GeneralLocation.Left;
            case BasisBoneTrackedRole.LeftFoot:
                return BasisAvatarIKStageCalibration.GeneralLocation.Left;
            case BasisBoneTrackedRole.LeftUpperLeg:
                return BasisAvatarIKStageCalibration.GeneralLocation.Left;
            case BasisBoneTrackedRole.LeftLowerLeg:
                return BasisAvatarIKStageCalibration.GeneralLocation.Left;
            case BasisBoneTrackedRole.LeftUpperArm:
                return BasisAvatarIKStageCalibration.GeneralLocation.Left;
            case BasisBoneTrackedRole.LeftToes:
                return BasisAvatarIKStageCalibration.GeneralLocation.Left;

            default:
                Console.WriteLine("Unknown role");
                return BasisAvatarIKStageCalibration.GeneralLocation.Center;
        }
    }
    public static BasisBoneTrackedRole[] desiredOrder = new BasisBoneTrackedRole[]
    {
        BasisBoneTrackedRole.Hips,
        BasisBoneTrackedRole.RightFoot,
        BasisBoneTrackedRole.LeftFoot,
        BasisBoneTrackedRole.LeftLowerLeg,
        BasisBoneTrackedRole.RightLowerLeg,
        BasisBoneTrackedRole.LeftLowerArm,
        BasisBoneTrackedRole.RightLowerArm,

        BasisBoneTrackedRole.CenterEye,
        BasisBoneTrackedRole.Chest,

        BasisBoneTrackedRole.Head,
        BasisBoneTrackedRole.Neck,

        BasisBoneTrackedRole.LeftHand,
        BasisBoneTrackedRole.RightHand,

        BasisBoneTrackedRole.LeftToes,
        BasisBoneTrackedRole.RightToes,

        BasisBoneTrackedRole.LeftUpperArm,
        BasisBoneTrackedRole.RightUpperArm,
        BasisBoneTrackedRole.LeftUpperLeg,
        BasisBoneTrackedRole.RightUpperLeg,
        BasisBoneTrackedRole.LeftShoulder,
        BasisBoneTrackedRole.RightShoulder,
    };
}