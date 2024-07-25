using DarkRift;
using Unity.Collections;
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