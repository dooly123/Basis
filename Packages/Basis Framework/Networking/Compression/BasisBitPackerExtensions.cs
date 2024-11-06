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
           compressor.CompressArray(values,arrayLength,ref compressor.ushortArray);

            // Efficiently copy ushort values to the byte array
            for (int Index = 0; Index < arrayLength; Index++)
            {
                // Manually convert each ushort to 2 bytes (little-endian)
                compressor.byteArray[Index * 2] = (byte)(compressor.ushortArray[Index] & 0xFF);
                compressor.byteArray[Index * 2 + 1] = (byte)((compressor.ushortArray[Index] >> 8) & 0xFF);
            }

            // Write the byte array to the DarkRiftWriter
            bitPacker.WriteRaw(compressor.byteArray, 0, compressor.ByteCount);
        }
        [BurstCompile]
        public static void ReadUshortArrayFloat(this DarkRiftReader bitPacker, CompressionArraysRangedUshort compressor, ref BasisAvatarData BasisAvatarData, int ArrayLength = 95)
        {

            // Read the raw byte array from the DarkRiftReader
            byte[] byteArray = bitPacker.ReadRaw(compressor.ByteCount);
            // Convert bytes to ushorts
            for (int Index = 0; Index < ArrayLength; Index++)
            {
                compressor.ushortArray[Index] = (ushort)(byteArray[Index * 2] | (byteArray[Index * 2 + 1] << 8));
            }

            // Decompress the ushort array to obtain a float array
            compressor.DecompressArray(compressor.ushortArray, ArrayLength, ref BasisAvatarData.floatArray);

            // Copy the decompressed float array into the Muscles structure
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