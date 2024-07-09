using System.Collections.Generic;
using System.Linq;
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
        public BasisTrackerMapping(BasisBoneControl Bone, BasisBoneTrackedRole Role, List<CalibrationConnector> Calibration, float CalibrationMaxDistance)
        {
            TargetControl = Bone;
            BasisBoneControlRole = Role;

            for (int Index = 0; Index < Calibration.Count; Index++)
            {
                Calibration[Index].Distance = Vector3.Distance(Calibration[Index].Tracker.transform.position, TargetControl.BoneModelTransform.position);
                if (Calibration[Index].Distance < CalibrationMaxDistance)
                {
                    Debug.DrawLine(Calibration[Index].Tracker.transform.position, TargetControl.BoneModelTransform.position, Color.blue, 8f);

                    Candidates.Add(Calibration[Index]);
                }
                else
                {
                    Debug.DrawLine(Calibration[Index].Tracker.transform.position, TargetControl.BoneModelTransform.position, Color.red, 8f);
                }
            }

            Debug.Log("Bone " + TargetControl.Name + " has " + Candidates.Count + " Trackers Available");

            FindClosestTargetPerInput();
        }
        public void FindClosestTargetPerInput()
        {
            if (Candidates.Count == 0)
            {
                return;
            }

            // Reorder Candidates by Distance
            Candidates = Candidates.OrderBy(candidate => candidate.Distance).ToList();
        }
    }

    public class CalibrationConnector
    {
        [SerializeField]
        public BasisInput Tracker;
        public float Distance;
    }

    // Define the desired order
    public static BasisBoneTrackedRole[] desiredOrder = new BasisBoneTrackedRole[]
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
}