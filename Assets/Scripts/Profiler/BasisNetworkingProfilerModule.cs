using Unity.Profiling;
using UnityEngine;
#if UNITY_EDITOR
using Unity.Profiling.Editor;

namespace Assets.Scripts.Profiler
{
[System.Serializable]
[ProfilerModuleMetadata("Byte Array Count")]
public class BasisNetworkingProfilerModule : ProfilerModule
{
    static readonly ProfilerCounterDescriptor[] k_Counters = new ProfilerCounterDescriptor[]
    {
        new ProfilerCounterDescriptor(BasisNetworkProfiler.AvatarUpdatePacketText, "Avatar Packet Size"),
        new ProfilerCounterDescriptor(BasisNetworkProfiler.AudioUpdatePacketText, "Audio Packet Size"),
       //new ProfilerCounterDescriptor(NetworkProfiler.AvatarMusclesCountText,"Muscles"),
        //new ProfilerCounterDescriptor(NetworkProfiler.PositionsCountText, "Positions"),
         //  new ProfilerCounterDescriptor(NetworkProfiler.RotationCountText, "Rotations"),
    };

    // Ensure that both ProfilerCategory.Scripts and ProfilerCategory.Memory categories are enabled when our module is active.
    static readonly string[] k_AutoEnabledCategoryNames = new string[]
    {
        ProfilerCategory.Scripts.Name,
         ProfilerCategory.Network.Name,
    };


    // Pass the auto-enabled category names to the base constructor.
    public BasisNetworkingProfilerModule() : base(k_Counters, autoEnabledCategoryNames: k_AutoEnabledCategoryNames) { }
}
#endif
public class BasisNetworkProfiler : MonoBehaviour
{
    public static readonly ProfilerCategory Category = ProfilerCategory.Network;

    public static ProfilerCounter<int> AvatarUpdatePacket = new ProfilerCounter<int>(Category, AvatarUpdatePacketText, ProfilerMarkerDataUnit.Bytes);
    public static ProfilerCounter<int> AudioUpdatePacket = new ProfilerCounter<int>(Category, AudioUpdatePacketText, ProfilerMarkerDataUnit.Bytes);
    //public static ProfilerCounter<int> AvatarMusclesCount = new ProfilerCounter<int>(Category, AvatarMusclesCountText, ProfilerMarkerDataUnit.Bytes);
    // public static ProfilerCounter<int> PositionsCount = new ProfilerCounter<int>(Category, PositionsCountText, ProfilerMarkerDataUnit.Bytes);
    // public static ProfilerCounter<int> RotationCount = new ProfilerCounter<int>(Category, RotationCountText, ProfilerMarkerDataUnit.Bytes);

    public const string AvatarUpdatePacketText = "Avatar Update Packet";
    public const string AudioUpdatePacketText = "Audio Update Packet";
    // public const string AvatarMusclesCountText = "Avatar Muscles Byte Count";
    // public const string PositionsCountText = "Position Byte Count";
    // public const string RotationCountText = "Rotation Byte Count";
    public void Update()
    {
        AvatarUpdatePacket.Sample(0);
        AudioUpdatePacket.Sample(0);
    }
}
}