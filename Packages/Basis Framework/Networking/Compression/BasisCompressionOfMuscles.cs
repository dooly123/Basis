using Basis.Scripts.Networking.NetworkedAvatar;
using DarkRift;

namespace Basis.Scripts.Networking.Compression
{
    public class BasisCompressionOfMuscles
    {
        public static void CompressMuscles(DarkRiftWriter Packer, float[] muscles, CompressionArraysRangedUshort CF)
        {
            for (int Index = 0; Index < 95; Index++)
            {
                float Muscle = muscles[Index];
                Packer.Write(Muscle);
            }
            //     BasisBitPackerExtensions.WriteUshortArrayFloat(Packer, muscles, CF);
        }
        public static void DecompressMuscles(DarkRiftReader Packer, ref BasisAvatarData BasisAvatarData, CompressionArraysRangedUshort CF)
        {
            //    BasisBitPackerExtensions.ReadUshortArrayFloat(Packer, CF, ref BasisAvatarData);
            for (int Index = 0; Index < 95; Index++)
            {
                Packer.Read(out float Value);
                BasisAvatarData.Muscles[Index] = Value;
            }
        }
    }
}