using DarkRift;
public static class BasisBitPackerExtensions
{
    public static void WriteInt(DarkRiftWriter bitPacker, int value, BasisRangedIntData compressor)
    {
        bitPacker.Write(compressor.Compress(value));
    }
    public static int ReadInt(DarkRiftReader bitPacker, BasisRangedIntData compressor)
    {
        bitPacker.Read(out uint data);
        return compressor.Decompress(data);
    }

    public static void WriteFloat(DarkRiftWriter bitPacker, float value, BasisRangedFloatData compressor)
    {
        bitPacker.Write(compressor.Compress(value));
    }
    public static float ReadFloat(this DarkRiftReader bitPacker, BasisRangedFloatData compressor)
    {
        bitPacker.Read(out ushort data);
        return compressor.Decompress(data);
    }
}