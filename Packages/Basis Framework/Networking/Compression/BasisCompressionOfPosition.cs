using DarkRift;
using Unity.Mathematics;
using UnityEngine;

namespace Basis.Scripts.Networking.Compression
{
    public static class BasisCompressionOfPosition
    {
        public static void CompressUShortVector3(Vector3 Input, DarkRiftWriter packer, BasisRangedUshortFloatData compressor)
        {
            BasisBitPackerExtensions.WriteUshortVectorFloat(packer, Input, compressor);
        }
        public static void DecompressUShortVector3(DarkRiftReader Packer, BasisRangedUshortFloatData CF,ref float3 Scale)
        {
            BasisBitPackerExtensions.ReadUshortFloat(Packer, CF,ref Scale.x);
            BasisBitPackerExtensions.ReadUshortFloat(Packer, CF, ref Scale.y);
            BasisBitPackerExtensions.ReadUshortFloat(Packer, CF, ref Scale.z);
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
        public static void DecompressVector3(DarkRiftReader Packer,ref float3 Position)
        {
            BasisBitPackerExtensions.ReadVectorFloat(Packer, out Position.x, out Position.y, out Position.z);
        }
    }
}