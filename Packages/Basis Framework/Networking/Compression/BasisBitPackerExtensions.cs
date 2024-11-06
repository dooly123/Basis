using Basis.Scripts.Networking.NetworkedAvatar;
using DarkRift;
using System;
using System.IO;
using Unity.Burst;
using UnityEngine;

namespace Basis.Scripts.Networking.Compression
{
    public static class BasisBitPackerExtensions
    {
        public static void WriteUshortFloat(DarkRiftWriter bitPacker, float value, BasisRangedUshortFloatData compressor)
        {
            bitPacker.Write(compressor.Compress(value));
        }
        public static float ReadUshortFloat(this DarkRiftReader bitPacker, BasisRangedUshortFloatData compressor)
        {
            bitPacker.Read(out ushort data);
            return compressor.Decompress(data);
        }
        [BurstCompile]
        public static void WriteUshortArrayFloat(DarkRiftWriter bitPacker, float[] values, CompressionArraysRangedUshort compressor, int arrayLength = 95)
        {
            // Compress the float array into a ushort array
            ushort[] compressedValues = compressor.CompressArray(values);

            // Allocate a byte array to hold the compressed ushort values
            byte[] byteArray = new byte[compressedValues.Length * 2];

            // Efficiently copy ushort values to the byte array
            for (int i = 0; i < compressedValues.Length; i++)
            {
                // Manually convert each ushort to 2 bytes (little-endian)
                byteArray[i * 2] = (byte)(compressedValues[i] & 0xFF);
                byteArray[i * 2 + 1] = (byte)((compressedValues[i] >> 8) & 0xFF);
            }

            // Write the byte array to the DarkRiftWriter
            bitPacker.WriteRaw(byteArray, 0, byteArray.Length);
        }
        [BurstCompile]
        public static void ReadUshortArrayFloat(this DarkRiftReader bitPacker, CompressionArraysRangedUshort compressor, ref BasisAvatarData BasisAvatarData, int ArrayLength = 95)
        {
            // Calculate the number of bytes required to store ArrayLength number of ushorts
            int ByteCount = ArrayLength * 2;

            // Read the raw byte array from the DarkRiftReader
            byte[] byteArray = bitPacker.ReadRaw(ByteCount);

            // Use Span to avoid array allocation overhead
            Span<byte> span = byteArray.AsSpan();

            // Create a Span for the ushort array
            Span<ushort> ushortSpan = new Span<ushort>(new ushort[ArrayLength]);

            // Use an unsafe context to efficiently convert bytes to ushorts in bulk
            unsafe
            {
                fixed (byte* bytePtr = span)
                fixed (ushort* ushortPtr = ushortSpan)
                {
                    for (int i = 0; i < ArrayLength; i++)
                    {
                        ushortPtr[i] = (ushort)(bytePtr[i * 2] | (bytePtr[i * 2 + 1] << 8));
                    }
                }
            }
            // Decompress the ushort array (assuming the decompression method works this way)
            BasisAvatarData.floatArray = compressor.DecompressArray(ushortSpan.ToArray());

            // Assuming that BasisAvatarData.Muscles is some structure that can copy from the float array
            BasisAvatarData.Muscles.CopyFrom(BasisAvatarData.floatArray);
        }
    public static void WriteUshortVectorFloat(DarkRiftWriter bitPacker, Vector3 values, BasisRangedUshortFloatData compressor)
        {
            ushort Compressx = compressor.Compress(values.x);
            ushort Compressy = compressor.Compress(values.y);
            ushort Compressz = compressor.Compress(values.z);
            bitPacker.Write(Compressx);
            bitPacker.Write(Compressy);
            bitPacker.Write(Compressz);
        }
        public static void ReadVectorFloat(this DarkRiftReader bitPacker, out float X, out float Y, out float Z)
        {
            bitPacker.Read(out X);
            bitPacker.Read(out Y);
            bitPacker.Read(out Z);
        }
        public static void WriteVectorFloat(DarkRiftWriter bitPacker, Vector3 values)
        {
            bitPacker.Write(values.x);
            bitPacker.Write(values.y);
            bitPacker.Write(values.z);
        }
    }
}