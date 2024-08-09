using DarkRift;
using UnityEngine;

namespace Basis.Scripts.Networking.Compression
{
public static class BasisCompressionOfPosition
{
    public static void CompressUShortVector3(Vector3 Input, DarkRiftWriter packer, BasisRangedUshortFloatData compressor)
    {
        BasisBitPackerExtensions.WriteUshortVectorFloat(packer, Input, compressor);
    }
    public static Vector3 DecompressUShortVector3(DarkRiftReader Packer, BasisRangedUshortFloatData CF)
    {
        float x = BasisBitPackerExtensions.ReadUshortFloat(Packer, CF);
        float y = BasisBitPackerExtensions.ReadUshortFloat(Packer, CF);
        float z = BasisBitPackerExtensions.ReadUshortFloat(Packer, CF);
        return new Vector3(x, y, z);
    }
    public static void CompressVector3(Vector3 Input, DarkRiftWriter packer)
    {
        BasisBitPackerExtensions.WriteVectorFloat(packer, Input);
    }
    public static Vector3 DecompressVector3(DarkRiftReader Packer)
    {
        BasisBitPackerExtensions.ReadVectorFloat(Packer, out float x, out float y, out float z);
        return new Vector3(x, y, z);
    }
}
}