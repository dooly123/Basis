using DarkRift;
using UnityEngine;

public static class BasisCompressionOfPosition 
{
    public static void CompressUShortVector3(Vector3 Input, DarkRiftWriter packer, BasisRangedUshortFloatData compressor)
    {
        BasisBitPackerExtensions.WriteUshortFloat(packer, Input.x, compressor);
        BasisBitPackerExtensions.WriteUshortFloat(packer, Input.y, compressor);
        BasisBitPackerExtensions.WriteUshortFloat(packer, Input.z, compressor);
    }
    public static Vector3 DecompressUShortVector3(DarkRiftReader Packer, BasisRangedUshortFloatData CF)
    {
        float x = BasisBitPackerExtensions.ReadUshortFloat(Packer, CF);
        float y = BasisBitPackerExtensions.ReadUshortFloat(Packer, CF);
        float z = BasisBitPackerExtensions.ReadUshortFloat(Packer, CF);
        return new Vector3(x, y, z);
    }
    public static void CompressVector3(Vector3 Input, DarkRiftWriter packer, BasisRangedUshortFloatData compressor)
    {
        BasisBitPackerExtensions.WriteFloat(packer, Input.x, compressor);
        BasisBitPackerExtensions.WriteFloat(packer, Input.y, compressor);
        BasisBitPackerExtensions.WriteFloat(packer, Input.z, compressor);
    }
    public static Vector3 DecompressVector3(DarkRiftReader Packer, BasisRangedUshortFloatData CF)
    {
        float x = BasisBitPackerExtensions.ReadFloat(Packer, CF);
        float y = BasisBitPackerExtensions.ReadFloat(Packer, CF);
        float z = BasisBitPackerExtensions.ReadFloat(Packer, CF);
        return new Vector3(x, y, z);
    }
}
