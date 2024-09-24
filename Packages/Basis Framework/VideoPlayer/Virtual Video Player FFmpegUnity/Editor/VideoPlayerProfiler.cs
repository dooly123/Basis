using Unity.Profiling;
using Unity.Profiling.Editor;
[System.Serializable]
[ProfilerModuleMetadata("Basis Video Player")]
public class VideoPlayerProfiler : ProfilerModule
{
    static readonly ProfilerCounterDescriptor[] k_Counters = new ProfilerCounterDescriptor[]
    {
    new ProfilerCounterDescriptor(FFUnityFrameHelper.SaveFrameTexture2DGeneration, FFUnityFrameHelper.FrameGenerationCategory),
    new ProfilerCounterDescriptor(FFUnityFrameHelper.SaveFrameByteArray, FFUnityFrameHelper.FrameGenerationCategory),
    new ProfilerCounterDescriptor(FFUnityFrameHelper.SaveMain, FFUnityFrameHelper.FrameGenerationCategory),
    new ProfilerCounterDescriptor(FFUnityFrameHelper.ConvertFrame, FFUnityFrameHelper.FrameGenerationCategory),
    new ProfilerCounterDescriptor(FFUnityFrameHelper.LoadTexture, FFUnityFrameHelper.FrameGenerationCategory),
    };
    // Ensure that both ProfilerCategory.Scripts and ProfilerCategory.Memory categories are enabled when our module is active.
    static readonly string[] k_AutoEnabledCategoryNames = new string[]
    {
        ProfilerCategory.Scripts.Name,
         ProfilerCategory.Video.Name,
    };
    public VideoPlayerProfiler() : base(k_Counters, autoEnabledCategoryNames: k_AutoEnabledCategoryNames) { }
}