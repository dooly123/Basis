using DarkRift;
public static class BasisBitPackerExtensions
{
    public static void WriteUshortFloat(DarkRiftWriter bitPacker, float value, BasisRangedUshortFloatData compressor)
    {
        bitPacker.Write(compressor.Compress(value));
    }
    public static float ReadUshortFloat(this DarkRiftReader bitPacker, BasisRangedUshortFloatData compressor)
    {
        bitPacker.Read(out ushort data);
        return compressor.Decompress(data);
    }
    public static void WriteFloat(DarkRiftWriter bitPacker, float value, BasisRangedUshortFloatData compressor)
    {
        bitPacker.Write(value);
    }
    public static float ReadFloat(this DarkRiftReader bitPacker, BasisRangedUshortFloatData compressor)
    {
        bitPacker.Read(out float data);
        return data;
    }
}