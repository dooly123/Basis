using System;

/// <summary>
/// Compresses integers to a given range
/// </summary>
public class BasisRangedIntData
{
    private readonly int minValue;
    private readonly int maxValue;

    private readonly int requiredBits;
    private readonly uint mask;

    /// <summary>
    /// Initializes a new instance of the RangedIntCompressor class.
    /// </summary>
    /// <param name="minValue">The minimum value of the range.</param>
    /// <param name="maxValue">The maximum value of the range.</param>
    public BasisRangedIntData(int minValue, int maxValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;

        requiredBits = CalculateRequiredBits();
        mask = (uint)((1L << requiredBits) - 1);
    }

    /// <summary>
    /// Compresses the specified value to the defined range.
    /// </summary>
    /// <param name="value">The value to compress.</param>
    /// <returns>The compressed value as a uint.</returns>
    public uint Compress(int value)
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

        return (uint)(value - minValue) & mask;
    }

    /// <summary>
    /// Decompresses the specified data back to its original value.
    /// </summary>
    /// <param name="compressedValue">The compressed value to decompress.</param>
    /// <returns>The decompressed integer value.</returns>
    public int Decompress(uint compressedValue)
    {
        return (int)(compressedValue + minValue);
    }

    /// <summary>
    /// Calculates the number of bits required to represent the range.
    /// </summary>
    /// <returns>The number of bits required.</returns>
    private int CalculateRequiredBits()
    {
        if (minValue > maxValue)
        {
            return 0;
        }

        long range = (long)maxValue - minValue;
        return Log2Fast((uint)range) + 1;
    }
    // http://stackoverflow.com/questions/15967240/fastest-implementation-of-log2int-and-log2float
    private static readonly int[] deBruijnLookup = new int[32]
    {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
    };
    public static int Log2Fast(uint v)
    {
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;

        return deBruijnLookup[(v * 0x07C4ACDDU) >> 27];
    }
}