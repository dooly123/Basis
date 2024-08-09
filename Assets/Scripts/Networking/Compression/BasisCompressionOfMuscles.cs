using Basis.Scripts.Networking.NetworkedAvatar;
using DarkRift;
using Unity.Collections;

namespace Basis.Scripts.Networking.Compression
{
public class BasisCompressionOfMuscles
{
    public static void CompressMuscles(DarkRiftWriter Packer, float[] muscles, BasisRangedUshortFloatData CF)
    {
        BasisBitPackerExtensions.WriteUshortArrayFloat(Packer, muscles, CF);
    }
    public static void DecompressMuscles(DarkRiftReader Packer,  ref BasisAvatarData BasisAvatarData, BasisRangedUshortFloatData CF)
    {
        BasisBitPackerExtensions.ReadUshortArrayFloat(Packer, CF, ref BasisAvatarData);
    }
}
}