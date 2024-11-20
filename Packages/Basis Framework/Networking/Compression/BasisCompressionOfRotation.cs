using DarkRift;
using Unity.Mathematics;
namespace Basis.Scripts.Networking.Compression
{
    public static class BasisCompressionOfRotation
    {
        public static BasisRangedUshortFloatData BasisRangedUshortFloatData = new BasisRangedUshortFloatData(-1f, 1f, 0.001f);
        public static void CompressQuaternion(DarkRiftWriter Packer, quaternion Rotation)
        {
            Packer.Write(Rotation.value.x);
            Packer.Write(Rotation.value.y);
            Packer.Write(Rotation.value.z);
            Packer.Write(BasisRangedUshortFloatData.Compress(Rotation.value.w));
        }
        public static void DecompressQuaternion(DarkRiftReader bitPacker, ref quaternion Quaternion)
        {
            //checked
            bitPacker.Read(out float x);
            bitPacker.Read(out float y);
            bitPacker.Read(out float z);
            bitPacker.Read(out ushort W);
            Quaternion.value.x = x;
            Quaternion.value.y = y;
            Quaternion.value.z = z;
            Quaternion.value.w = BasisRangedUshortFloatData.Decompress(W);
        }
    }
}