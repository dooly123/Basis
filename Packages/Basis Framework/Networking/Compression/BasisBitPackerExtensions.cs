using Basis.Scripts.Networking.NetworkedAvatar;
using DarkRift;
using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using static Basis.Scripts.Networking.NetworkedAvatar.BasisNetworkSendBase;

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
        public static void ReadUshortFloat(this DarkRiftReader bitPacker, BasisRangedUshortFloatData compressor,ref float Value)
        {
            bitPacker.Read(out ushort data);
            Value = compressor.Decompress(data);
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
        public static void CompressUShortVector3(Vector3 Input, DarkRiftWriter packer, BasisRangedUshortFloatData compressor)
        {
            BasisBitPackerExtensions.WriteUshortVectorFloat(packer, Input, compressor);
        }
        public static void DecompressUShortVector3(DarkRiftReader Packer, BasisRangedUshortFloatData CF, ref float3 Scale)
        {
            BasisBitPackerExtensions.ReadUshortFloat(Packer, CF, ref Scale.x);
            BasisBitPackerExtensions.ReadUshortFloat(Packer, CF, ref Scale.y);
            BasisBitPackerExtensions.ReadUshortFloat(Packer, CF, ref Scale.z);
        }
        public static void CompressVector3(Vector3 Input, DarkRiftWriter packer)
        {
            BasisBitPackerExtensions.WriteVectorFloat(packer, Input);
        }
        public static void DecompressVector3(DarkRiftReader Packer, ref float3 Position)
        {
            BasisBitPackerExtensions.ReadVectorFloat(Packer, out Position.x, out Position.y, out Position.z);
        }
        public static BasisRangedUshortFloatData BasisRangedUshortFloatData = new BasisRangedUshortFloatData(-1f, 1f, 0.001f);
        public static void CompressQuaternion(DarkRiftWriter Packer, quaternion Rotation)
        {
            Packer.Write(Rotation.value.x);
            Packer.Write(Rotation.value.y);
            Packer.Write(Rotation.value.z);
            Packer.Write(BasisRangedUshortFloatData.Compress(Rotation.value.w));
        }
        public static void DecompressQuaternion(DarkRiftReader bitPacker, ref quaternion Quaternion)
        {
            //checked
            bitPacker.Read(out float x);
            bitPacker.Read(out float y);
            bitPacker.Read(out float z);
            bitPacker.Read(out ushort W);
            Quaternion.value.x = x;
            Quaternion.value.y = y;
            Quaternion.value.z = z;
            Quaternion.value.w = BasisRangedUshortFloatData.Decompress(W);
        }
        public static int LengthSize = 90;
        public static int LengthBytes = LengthSize * 4; // Initialize LengthBytes first
        public static byte[] StoredBytes = new byte[LengthBytes];
        // Compress the muscle data into the byte array
        public static void CompressMuscles(DarkRiftWriter Packer, float[] muscles)
        {
            // Convert the float array to bytes using Buffer.BlockCopy
            Buffer.BlockCopy(muscles, 0, StoredBytes, 0, LengthBytes);

            // Write the raw byte array to the Packer
            Packer.WriteRaw(StoredBytes, 0, LengthBytes);
        }
        // Decompress the byte array back into the muscle data
        public static void DecompressMuscles(DarkRiftReader Packer, ref AvatarBuffer BasisAvatarData)
        {
            // Read the raw byte array from the Packer
            Packer.ReadRaw(LengthBytes, ref StoredBytes);

            if (BasisAvatarData.Muscles == null)
            {
                BasisAvatarData.Muscles = new float[LengthSize];
            }
            // Convert the byte array back to the float array using Buffer.BlockCopy
            Buffer.BlockCopy(StoredBytes, 0, BasisAvatarData.Muscles, 0, LengthBytes);
        }
    }
}