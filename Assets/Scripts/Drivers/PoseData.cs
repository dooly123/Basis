using UnityEngine;

[System.Serializable]
public struct PoseData
{
    [SerializeField]
    public MuscleLocalPose[] LeftThumb;
    [SerializeField]
    public MuscleLocalPose[] LeftIndex;
    [SerializeField]
    public MuscleLocalPose[] LeftMiddle;
    [SerializeField]
    public MuscleLocalPose[] LeftRing;
    [SerializeField]
    public MuscleLocalPose[] LeftLittle;
    [SerializeField]
    public MuscleLocalPose[] RightThumb;
    [SerializeField]
    public MuscleLocalPose[] RightIndex;
    [SerializeField]
    public MuscleLocalPose[] RightMiddle;
    [SerializeField]
    public MuscleLocalPose[] RightRing;
    [SerializeField]
    public MuscleLocalPose[] RightLittle;
}