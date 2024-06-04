using DarkRift;
using UnityEngine;

public static class BasisCompressionOfPosition 
{
    public static void CompressVector3(Vector3 Input, DarkRiftWriter packer, BasisRangedFloatData compressor)
    {
        BasisBitPackerExtensions.WriteFloat(packer, Input.x, compressor);
        BasisBitPackerExtensions.WriteFloat(packer, Input.y, compressor);
        BasisBitPackerExtensions.WriteFloat(packer, Input.z, compressor);
    }
    public static Vector3 DecompressVector3(DarkRiftReader Packer, BasisRangedFloatData CF)
    {
        float x = BasisBitPackerExtensions.ReadFloat(Packer, CF);
        float y = BasisBitPackerExtensions.ReadFloat(Packer, CF);
        float z = BasisBitPackerExtensions.ReadFloat(Packer, CF);
        return new Vector3(x, y, z);
    }
}
