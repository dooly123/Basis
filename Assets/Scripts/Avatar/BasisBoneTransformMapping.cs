using System.Collections.Generic;
using UnityEngine;

public static partial class BasisAvatarIKStageCalibration
{
    [System.Serializable]
    public class BasisBoneTransformMapping
    {
        [SerializeField]
        public BasisInput Bone;
        [SerializeField]
        public Dictionary<CalibrationConnector, float> Distances = new Dictionary<CalibrationConnector, float>();
        public BasisBoneTransformMapping(BasisInput bone, List<CalibrationConnector> transformsToMatch, float CalibrationMaxDistance)
        {
            Bone = bone;
            for (int Index = 0; Index < transformsToMatch.Count; Index++)
            {
                float Distance = Vector3.Distance(bone.transform.position, transformsToMatch[Index].BasisBoneControl.BoneModelTransform.position);
                if (Distance < CalibrationMaxDistance)
                {
                    Distances.TryAdd(transformsToMatch[Index], Distance);
                }
            }
        }
    }
}