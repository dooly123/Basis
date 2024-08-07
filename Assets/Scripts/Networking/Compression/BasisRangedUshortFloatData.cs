using System;

namespace Assets.Scripts.Networking.Compression
{
[System.Serializable]
public class BasisRangedUshortFloatData
{
    public readonly float precision;
    public readonly float inversePrecision;

    public readonly float minValue;
    public readonly float maxValue;

    public readonly int requiredBits;
    public readonly ushort mask;

    public BasisRangedUshortFloatData(float minValue, float maxValue, float precision)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.precision = precision;

        inversePrecision = 1.0f / precision;
        requiredBits = CalculateRequiredBits();
        mask = (ushort)((1 << requiredBits) - 1);


    }

    public ushort Compress(float value)
    {
        if (value < minValue)
        {
            Console.WriteLine($"Clamping value {value} to minimum value {minValue}");
            value = minValue;
        }
        else if (value > maxValue)
        {
            Console.WriteLine($"Clamping value {value} to maximum value {maxValue}");
            value = maxValue;
        }

        float normalizedValue = (value - minValue) * inversePrecision;
        return (ushort)((ushort)(normalizedValue + 0.5f) & mask);
    }

    public float Decompress(ushort compressedValue)
    {
        float decompressedValue = ((float)compressedValue * precision) + minValue;
        if (decompressedValue < minValue)
        {
            Console.WriteLine($"Clamping value {decompressedValue} to minimum value {minValue}");
            decompressedValue = minValue;
        }
        else if (decompressedValue > maxValue)
        {
            Console.WriteLine($"Clamping value {decompressedValue} to maximum value {maxValue}");
            decompressedValue = maxValue;
        }
        return decompressedValue;
    }

    private int CalculateRequiredBits()
    {
        float range = maxValue - minValue;
        float maxValueInRange = range * inversePrecision;
        return FastLog2((uint)(maxValueInRange + 0.5f)) + 1;
    }

    private static readonly int[] deBruijnLookup = new int[32]
    {
        0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
        8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
    };

    public static int FastLog2(uint value)
    {
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;

        return deBruijnLookup[(value * 0x07C4ACDDU) >> 27];
    }
}
}