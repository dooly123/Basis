using System;
using UnityEngine;
using DarkRift;
using Unity.Collections;
public class BasisCompressionOfMuscles
{
    public static BasisRangedFloatData CompressMuscles(DarkRiftWriter Packer, float[] muscles)
    {
        BasisRangedFloatData CF = new BasisRangedFloatData(-180, 180, BasisNetworkConstants.MusclePrecision);
        for (int Index = 0; Index < 95; Index++)
        {
            BasisBitPackerExtensions.WriteFloat(Packer, muscles[Index], CF);
        }
        return CF;
    }
    public static void DecompressMuscles(DarkRiftReader Packer, ref NativeArray<float> Muscles)
    {
        BasisRangedFloatData CF = new BasisRangedFloatData(-180, 180, BasisNetworkConstants.MusclePrecision);
        for (int Index = 0; Index < 95; Index++)
        {
            Muscles[Index] = BasisBitPackerExtensions.ReadFloat(Packer, CF);
        }
    }
}