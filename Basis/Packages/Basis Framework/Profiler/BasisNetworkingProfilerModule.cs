using Unity.Profiling;
using UnityEngine;
namespace Basis.Scripts.Profiler
{
    public class BasisNetworkProfiler : MonoBehaviour
    {
        public static readonly ProfilerCategory Category = ProfilerCategory.Network;

        public static ProfilerCounter<int> AvatarUpdatePacket = new ProfilerCounter<int>(Category, AvatarUpdatePacketText, ProfilerMarkerDataUnit.Bytes);
        public static ProfilerCounter<int> AudioUpdatePacket = new ProfilerCounter<int>(Category, AudioUpdatePacketText, ProfilerMarkerDataUnit.Bytes);
        public const string AvatarUpdatePacketText = "Avatar Update Packet";
        public const string AudioUpdatePacketText = "Audio Update Packet";
        public void Update()
        {
            AvatarUpdatePacket.Sample(0);
            AudioUpdatePacket.Sample(0);
        }
    }
}