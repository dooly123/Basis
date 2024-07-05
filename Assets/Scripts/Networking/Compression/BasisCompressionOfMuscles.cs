using System;
using UnityEngine;
using DarkRift;
using Unity.Collections;
public class BasisCompressionOfMuscles
{
    public static BasisRangedUshortFloatData CompressMuscles(DarkRiftWriter Packer, float[] muscles)
    {
        BasisRangedUshortFloatData CF = new BasisRangedUshortFloatData(-180, 180, BasisNetworkConstants.MusclePrecision);
        for (int Index = 0; Index < 95; Index++)
        {
            BasisBitPackerExtensions.WriteUshortFloat(Packer, muscles[Index], CF);
        }
        return CF;
    }
    public static void DecompressMuscles(DarkRiftReader Packer, ref NativeArray<float> Muscles)
    {
        BasisRangedUshortFloatData CF = new BasisRangedUshortFloatData(-180, 180, BasisNetworkConstants.MusclePrecision);
        for (int Index = 0; Index < 95; Index++)
        {
            Muscles[Index] = BasisBitPackerExtensions.ReadUshortFloat(Packer, CF);
        }
    }
}