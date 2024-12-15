using Unity.Profiling;
using UnityEngine;
namespace Basis.Scripts.Profiler
{
    public class BasisNetworkProfiler : MonoBehaviour
    {
        public static readonly ProfilerCategory Category = ProfilerCategory.Network;

        public static ProfilerCounter<int> OutBoundAvatarUpdatePacket = new ProfilerCounter<int>(Category, OutBoundAvatarUpdatePacketText, ProfilerMarkerDataUnit.Bytes);
        public static ProfilerCounter<int> OutBoundAudioUpdatePacket = new ProfilerCounter<int>(Category, OutBoundAudioUpdatePacketText, ProfilerMarkerDataUnit.Bytes);
        public const string OutBoundAvatarUpdatePacketText = "OutBound Avatar Update Packet";
        public const string OutBoundAudioUpdatePacketText = "OutBound Audio Update Packet";


        public static ProfilerCounter<int> InBoundAvatarUpdatePacket = new ProfilerCounter<int>(Category, InBoundAvatarUpdatePacketText, ProfilerMarkerDataUnit.Bytes);
        public static ProfilerCounter<int> InBoundAudioUpdatePacket = new ProfilerCounter<int>(Category, InBoundAudioUpdatePacketText, ProfilerMarkerDataUnit.Bytes);
        public const string InBoundAvatarUpdatePacketText = "InBound Avatar Update Packet";
        public const string InBoundAudioUpdatePacketText = "InBound Audio Update Packet";
        public void Update()
        {
            OutBoundAvatarUpdatePacket.Sample(0);
            OutBoundAudioUpdatePacket.Sample(0);

            InBoundAvatarUpdatePacket.Sample(0);
            InBoundAudioUpdatePacket.Sample(0);
        }
    }
}