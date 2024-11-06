using Basis.Scripts.Networking.NetworkedAvatar;
using DarkRift;

namespace Basis.Scripts.Networking.Compression
{
    public class BasisCompressionOfMuscles
    {
        public static void CompressMuscles(DarkRiftWriter Packer, float[] muscles, CompressionArraysRangedUshort CF)
        {
            BasisBitPackerExtensions.WriteUshortArrayFloat(Packer, muscles, CF);
        }
        public static void DecompressMuscles(DarkRiftReader Packer, ref BasisAvatarData BasisAvatarData, CompressionArraysRangedUshort CF)
        {
            BasisBitPackerExtensions.ReadUshortArrayFloat(Packer, CF, ref BasisAvatarData);
        }
    }
}