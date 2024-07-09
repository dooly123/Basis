using System.Collections.Generic;
using UnityEngine;

public static partial class BasisAvatarIKStageCalibration
{
    [System.Serializable]
    public class BasisTrackerMapping
    {
        [SerializeField]
        public BasisInput Tracker;
        [SerializeField]
        public List<CalibrationConnector> Candidates = new List<CalibrationConnector>();

        public BasisTrackerMapping(BasisInput bone, List<CalibrationConnector> transformsToMatch, float CalibrationMaxDistance)
        {
            Tracker = bone;
            for (int Index = 0; Index < transformsToMatch.Count; Index++)
            {
                transformsToMatch[Index].Distance = Vector3.Distance(bone.transform.position, transformsToMatch[Index].BasisBoneControl.BoneModelTransform.position);
                if (transformsToMatch[Index].Distance < CalibrationMaxDistance)
                {
                    Debug.DrawLine(bone.transform.position, transformsToMatch[Index].BasisBoneControl.BoneModelTransform.position, Color.blue, 8f);
                    Candidates.Add(transformsToMatch[Index]);
                }
                else
                {
                    Debug.DrawLine(bone.transform.position, transformsToMatch[Index].BasisBoneControl.BoneModelTransform.position, Color.red, 8f);
                }
            }

            // Sort the Candidates list based on the desired order
            Candidates.Sort(new CalibrationConnectorComparer(desiredOrder));

            Debug.Log("Bone " + Tracker.UniqueID + " has " + Candidates.Count + " Trackers Available");
        }

        public class CalibrationConnector
        {
            public BasisBoneControl BasisBoneControl;
            public BasisBoneTrackedRole BasisBoneControlRole;
            public float Distance;
        }

        // Comparer for sorting CalibrationConnector based on desired order
        private class CalibrationConnectorComparer : IComparer<CalibrationConnector>
        {
            private Dictionary<BasisBoneTrackedRole, int> orderDict;

            public CalibrationConnectorComparer(BasisBoneTrackedRole[] desiredOrder)
            {
                orderDict = new Dictionary<BasisBoneTrackedRole, int>();
                for (int i = 0; i < desiredOrder.Length; i++)
                {
                    orderDict[desiredOrder[i]] = i;
                }
            }

            public int Compare(CalibrationConnector x, CalibrationConnector y)
            {
                if (x == null || y == null)
                {
                    return 0;
                }

                int xIndex = orderDict.ContainsKey(x.BasisBoneControlRole) ? orderDict[x.BasisBoneControlRole] : int.MaxValue;
                int yIndex = orderDict.ContainsKey(y.BasisBoneControlRole) ? orderDict[y.BasisBoneControlRole] : int.MaxValue;

                return xIndex.CompareTo(yIndex);
            }
        }
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