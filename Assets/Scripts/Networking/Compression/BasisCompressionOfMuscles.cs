using DarkRift;
using Unity.Collections;
public class BasisCompressionOfMuscles
{
    public static BasisRangedUshortFloatData CompressMuscles(DarkRiftWriter Packer, float[] muscles, BasisRangedUshortFloatData CF)
    {
        for (int Index = 0; Index < 95; Index++)
        {
            BasisBitPackerExtensions.WriteUshortFloat(Packer, muscles[Index], CF);
        }
        return CF;
    }
    public static void DecompressMuscles(DarkRiftReader Packer, ref NativeArray<float> Muscles, BasisRangedUshortFloatData CF)
    {
        for (int Index = 0; Index < 95; Index++)
        {
            Muscles[Index] = BasisBitPackerExtensions.ReadUshortFloat(Packer, CF);
        }
    }
}