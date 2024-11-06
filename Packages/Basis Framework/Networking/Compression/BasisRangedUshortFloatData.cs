using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
namespace Basis.Scripts.Networking.Compression
{
    [Serializable]
    public class BasisRangedUshortFloatData
    {
        public readonly float Precision;
        public readonly float InversePrecision;
        public readonly float MinValue;
        public readonly float MaxValue;
        public readonly int RequiredBits;
        public readonly ushort Mask;
        public BasisRangedUshortFloatData(float minValue, float maxValue, float precision)
        {
            if (precision <= 0) throw new ArgumentException("Precision must be greater than zero.", nameof(precision));
            if (minValue >= maxValue) throw new ArgumentException("MinValue must be less than MaxValue.");

            MinValue = minValue;
            MaxValue = maxValue;
            Precision = precision;
            InversePrecision = 1.0f / precision;
            RequiredBits = CalculateRequiredBits();
            Mask = (ushort)((1 << RequiredBits) - 1);
        }
        public ushort Compress(float value)
        {
            value = Mathf.Clamp(value, MinValue, MaxValue);
            float normalizedValue = (value - MinValue) * InversePrecision;
            return (ushort)((ushort)(normalizedValue + 0.5f) & Mask);
        }
        public float Decompress(ushort compressedValue)
        {
            float decompressedValue = ((float)compressedValue * Precision) + MinValue;
            return Mathf.Clamp(decompressedValue, MinValue, MaxValue);
        }
        private int CalculateRequiredBits()
        {
            float range = MaxValue - MinValue;
            float maxValueInRange = range * InversePrecision;
            return math.ceilpow2((int)(maxValueInRange + 0.5f));
        }
    }
}
public class CompressionArraysRangedUshort : IDisposable
{
    public NativeArray<float> Floats;
    public NativeArray<ushort> Ushorts;
    public readonly float Precision;
    public readonly float InversePrecision;
    public readonly float MinValue;
    public readonly float MaxValue;
    public readonly int RequiredBits;
    public readonly ushort Mask;
    public DecompressJob Decompression;
    public CompressJob Compression;
    public JobHandle handle;
    public int InnerBatchCount = 64;
    public CompressionArraysRangedUshort(int length, float minValue, float maxValue, float precision, bool isDecompression)
    {
        Floats = new NativeArray<float>(length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        Ushorts = new NativeArray<ushort>(length, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        MinValue = minValue;
        MaxValue = maxValue;
        Precision = precision;
        InversePrecision = 1.0f / precision;
        RequiredBits = CalculateRequiredBits();
        Mask = (ushort)((1 << RequiredBits) - 1);

        if (isDecompression)
        {
            Decompression = new DecompressJob
            {
                InputValues = Ushorts,
                OutputValues = Floats,
                MinValue = MinValue,
                Precision = Precision
            };
        }
        else
        {
            Compression = new CompressJob
            {
                InputValues = Floats,
                OutputValues = Ushorts,
                MinValue = MinValue,
                InversePrecision = InversePrecision,
                Mask = Mask
            };
        }
    }

    private int CalculateRequiredBits()
    {
        float range = MaxValue - MinValue;
        float maxValueInRange = range * InversePrecision;
        return math.ceilpow2((int)(maxValueInRange + 0.5f));
    }

    public ushort[] CompressArray(float[] values)
    {
        Compression.InputValues.CopyFrom(values);
        handle = Compression.Schedule(values.Length, InnerBatchCount);
        handle.Complete();

        return Compression.OutputValues.ToArray();
    }

    public float[] DecompressArray(ushort[] compressedValues)
    {
        Decompression.InputValues.CopyFrom(compressedValues);
        handle = Decompression.Schedule(compressedValues.Length, InnerBatchCount);
        handle.Complete();

        return Decompression.OutputValues.ToArray();
    }

    public void Dispose()
    {
        if (Floats.IsCreated) Floats.Dispose();
        if (Ushorts.IsCreated) Ushorts.Dispose();
    }

    [BurstCompile]
    public struct CompressJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> InputValues;
        [WriteOnly] public NativeArray<ushort> OutputValues;
        public float MinValue;
        public float InversePrecision;
        public ushort Mask;

        public void Execute(int index)
        {
            float normalizedValue = (InputValues[index] - MinValue) * InversePrecision;
            OutputValues[index] = (ushort)(math.min((int)normalizedValue, Mask));
        }
    }

    [BurstCompile]
    public struct DecompressJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ushort> InputValues;
        [WriteOnly] public NativeArray<float> OutputValues;
        public float MinValue;
        public float Precision;

        public void Execute(int index)
        {
            OutputValues[index] = InputValues[index] * Precision + MinValue;
        }
    }
}