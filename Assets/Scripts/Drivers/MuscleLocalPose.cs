using UnityEngine;

public partial class BasisMuscleDriver
{
    [System.Serializable]
    public struct MuscleLocalPose
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}