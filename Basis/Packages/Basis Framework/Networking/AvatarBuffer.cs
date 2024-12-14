namespace Basis.Scripts.Networking.NetworkedAvatar
{
    public struct AvatarBuffer
    {
        public Unity.Mathematics.quaternion rotation;
        public Unity.Mathematics.float3 Scale;
        public Unity.Mathematics.float3 Position;
        public float[] Muscles;
        public double SecondsInterval;
    }
}