
using UnityEngine;

public static partial class BasisAvatarIKStageCalibration
{
    [System.Serializable]
    public class BasisBoneTransformMapping
    {
        public BasisInput Bone;
        [SerializeField]
        public float[] Distances;
        [SerializeField]
        public CalibrationConnector Closest;

        public BasisBoneTransformMapping(BasisInput bone, CalibrationConnector[] transformsToMatch,float CalibrationMaxDistance)
        {
            Bone = bone;
            Distances = new float[transformsToMatch.Length];
            for (int Index = 0; Index < transformsToMatch.Length; Index++)
            {
                Distances[Index] = Vector3.Distance(bone.transform.position, transformsToMatch[Index].BasisBoneControl.BoneModelTransform.position);
                if(Distances[Index] > CalibrationMaxDistance)
                {
                    Distances[Index] = float.MaxValue;
                }
            }
        }
    }
}