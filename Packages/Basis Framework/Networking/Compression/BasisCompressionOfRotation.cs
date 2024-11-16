using DarkRift;
using UnityEngine;

namespace Basis.Scripts.Networking.Compression
{
    public static class BasisCompressionOfRotation
    {
        public static BasisRangedUshortFloatData BasisRangedUshortFloatData = new BasisRangedUshortFloatData(-1f, 1f, 0.001f);
        public static void CompressQuaternion(DarkRiftWriter Packer, Quaternion Rotation)
        {
            Packer.Write(Rotation.x);
            Packer.Write(Rotation.y);
            Packer.Write(Rotation.z);
            Packer.Write(BasisRangedUshortFloatData.Compress(Rotation.w));
        }
        public static void DecompressQuaternion(DarkRiftReader bitPacker, ref Quaternion Quaternion)
        {
            //checked
            bitPacker.Read(out float x);
            bitPacker.Read(out float y);
            bitPacker.Read(out float z);
            bitPacker.Read(out ushort W);
            Quaternion.x = x;
            Quaternion.y = y;
            Quaternion.z = z;
            Quaternion.w = BasisRangedUshortFloatData.Decompress(W);
        }
    }
}