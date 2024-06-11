using System;
using UnityEngine;
using DarkRift;
using Unity.Collections;
public class BasisCompressionOfMuscles
{
    public static BasisRangedFloatData CompressMuscles(DarkRiftWriter Packer, float[] muscles)
    {
        byte LASV = ByteAbsLargestValue(muscles);
        Packer.Write(LASV);
        BasisRangedFloatData CF = new BasisRangedFloatData(-LASV, LASV, BasisNetworkConstants.MusclePrecision);
        for (int Index = 0; Index < 95; Index++)
        {
            BasisBitPackerExtensions.WriteFloat(Packer, muscles[Index], CF);
        }
        return CF;
    }
    public static void DecompressMuscles(DarkRiftReader Packer,ref NativeArray<float> Muscles)
    {
        Packer.Read(out byte LASV);
        BasisRangedFloatData CF = new BasisRangedFloatData(-LASV, LASV, BasisNetworkConstants.MusclePrecision);
        for (int Index = 0; Index < 95; Index++)
        {
            Muscles[Index] = BasisBitPackerExtensions.ReadFloat(Packer, CF);
        }
    }
    public static byte ByteAbsLargestValue(float[] Values)
    {
        float largest = float.MinValue;
        foreach (float value in Values)
        {
            float Abs = Math.Abs(value);
            if (Abs > largest)
            {
                largest = Abs;
            }
        }
        // Rounding up if the largest value has a decimal part
        byte result = (byte)Math.Ceiling(largest);
        // Handling the case where the result exceeds the maximum byte value
        if (result > byte.MaxValue)
        {
            Debug.LogError("Maxed Value ");
            result = byte.MaxValue;
        }
        return result;
    }
}
