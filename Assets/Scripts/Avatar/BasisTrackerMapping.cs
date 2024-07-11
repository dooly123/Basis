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
        public BasisTrackerMapping(BasisBoneControl Bone, BasisBoneTrackedRole Role, List<CalibrationConnector> calibration, float calibrationMaxDistance)
        {
            TargetControl = Bone;
            BasisBoneControlRole = Role;
            Candidates = new List<CalibrationConnector>();

            GeneralLocation BakedIn = FindGeneralLocation(BasisBoneControlRole);
           Vector3 CameraRotation = BasisLocalCameraDriver.Instance.Camera.transform.eulerAngles;
            Quaternion SanitisedRotation = Quaternion.Euler(0, CameraRotation.y, 0);
          Quaternion InversedSanitisedRotation =  Quaternion.Inverse(SanitisedRotation);
            foreach (CalibrationConnector connector in calibration)
            {
                connector.Distance = Vector3.Distance(InversedSanitisedRotation * connector.BasisInput.FinalPosition, TargetControl.TposeLocal.position);
                connector.BasisInput.GeneralLocation = GetLocation(connector.BasisInput.transform, BasisLocalCameraDriver.Instance.Camera.transform, BasisLocalCameraDriver.Instance.Camera.transform);

                if (connector.Distance < calibrationMaxDistance)
                {
                    Debug.DrawLine(InversedSanitisedRotation * connector.BasisInput.LocalRawPosition, TargetControl.TposeLocal.position, Color.blue, 40f);
                    Candidates.Add(connector);
                    /*
                    if (BakedIn == GeneralLocation.Center)
                    {
                        Candidates.Add(connector);
                    }
                    else
                    {

                        if (connector.Tracker.GeneralLocation == BakedIn || connector.Tracker.GeneralLocation == GeneralLocation.Center)
                        {
                            Candidates.Add(connector);

                        }
                        else
                        {
                            Debug.Log("Wrong side detected: " + connector.Tracker.GeneralLocation + " |" + BakedIn + " for bone " + connector.Tracker.name, connector.Tracker.gameObject);
                        }
                    }
                    */
                }
                else
                {
                    Debug.DrawLine(InversedSanitisedRotation * connector.BasisInput.FinalPosition,TargetControl.TposeLocal.position, Color.red, 40f);
                }
            }

            // Debug.Log("Bone " + TargetControl.Name + " has " + Candidates.Count + " Trackers Available");
        }
    }
    public static GeneralLocation GetLocation(Transform Tracker, Transform Eye, Transform forward)
    {
        // Calculate the direction from Eye to Tracker
        Vector3 delta = (Tracker.position - Eye.position).normalized;

        // Calculate the right direction based on the forward direction
        Vector3 right = forward.forward;
       Debug.DrawLine (delta, delta +  right * 3, Color.magenta,12f);
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
            case BasisBoneTrackedRole.UpperChest:
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
    // Define the desired order
    public static BasisBoneTrackedRole[] desiredOrder = new BasisBoneTrackedRole[]
    {
        BasisBoneTrackedRole.Hips,
        BasisBoneTrackedRole.LeftFoot,
        BasisBoneTrackedRole.RightFoot,
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