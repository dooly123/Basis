public static unsafe partial class DynamicallyLinkedBindings
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    public const string avformat = "avformat-60";
    public const string avutil = "avutil-58";
    public const string avcodec = "avcodec-60";
    public const string avdevice = "avdevice-60";
    public const string avfilter = "avfilter-9";
    public const string swscale = "swscale-7";
    public const string swresample = "swresample-4";
#else
    public const string avformat = "avformat.so.60";
    public const string avutil = "avutil.so.58";
    public const string avcodec = "avcodec.so.60";
    public const string avdevice = "avdevice.so.60";
    public const string avfilter = "avfilter.so.9";
    public const string swscale = "swscale.so.7";
    public const string swresample = "swresample.so.4";
#endif
}