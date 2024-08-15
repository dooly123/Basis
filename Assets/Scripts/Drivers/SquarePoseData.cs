public partial class BasisMuscleDriver
{
    [System.Serializable]
    public struct SquarePoseData
    {
        public PoseData TopLeft;
        public PoseData TopRight;

        public PoseData BottomLeft;
        public PoseData BottomRight;
    }
}