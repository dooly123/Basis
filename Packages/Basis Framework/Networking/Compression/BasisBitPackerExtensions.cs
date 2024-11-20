using DarkRift;
using System;
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
        public static void ReadUshortFloat(this DarkRiftReader bitPacker, BasisRangedUshortFloatData compressor, ref float Value)
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
            WriteUshortVectorFloat(packer, Input, compressor);
        }
        public static void DecompressUShortVector3(DarkRiftReader Packer, BasisRangedUshortFloatData CF, ref float3 Scale)
        {
            ReadUshortFloat(Packer, CF, ref Scale.x);
            ReadUshortFloat(Packer, CF, ref Scale.y);
            ReadUshortFloat(Packer, CF, ref Scale.z);
        }
        public static void CompressVector3(Vector3 Input, DarkRiftWriter packer)
        {
            WriteVectorFloat(packer, Input);
        }
        public static void DecompressVector3(DarkRiftReader Packer, ref float3 Position)
        {
            ReadVectorFloat(Packer, out Position.x, out Position.y, out Position.z);
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

        public static void WriteUshortFloatToBytes(float value, BasisRangedUshortFloatData compressor, ref byte[] bytes, ref int offset)
        {
            EnsureSize(ref bytes, offset + 2);
            ushort compressedValue = compressor.Compress(value);
            Buffer.BlockCopy(BitConverter.GetBytes(compressedValue), 0, bytes, offset, 2);
            offset += 2;
        }

        public static float ReadUshortFloatFromBytes(ref byte[] bytes, BasisRangedUshortFloatData compressor, ref int offset)
        {
            EnsureSize(bytes, offset + 2);
            ushort data = BitConverter.ToUInt16(bytes, offset);
            offset += 2;
            return compressor.Decompress(data);
        }

        public static void WriteUshortVectorFloatToBytes(Vector3 values, BasisRangedUshortFloatData compressor, ref byte[] bytes, ref int offset)
        {
            EnsureSize(ref bytes, offset + 6);
            ushort compressedX = compressor.Compress(values.x);
            ushort compressedY = compressor.Compress(values.y);
            ushort compressedZ = compressor.Compress(values.z);

            Buffer.BlockCopy(BitConverter.GetBytes(compressedX), 0, bytes, offset, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(compressedY), 0, bytes, offset + 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(compressedZ), 0, bytes, offset + 4, 2);
            offset += 6;
        }

        public static Vector3 ReadUshortVectorFloatFromBytes(ref byte[] bytes, BasisRangedUshortFloatData compressor, ref int offset)
        {
            EnsureSize(bytes, offset + 6);
            ushort compressedX = BitConverter.ToUInt16(bytes, offset);
            ushort compressedY = BitConverter.ToUInt16(bytes, offset + 2);
            ushort compressedZ = BitConverter.ToUInt16(bytes, offset + 4);
            offset += 6;

            return new Vector3(
                compressor.Decompress(compressedX),
                compressor.Decompress(compressedY),
                compressor.Decompress(compressedZ)
            );
        }

        public static void WriteVectorFloatToBytes(Vector3 values, ref byte[] bytes, ref int offset)
        {
            EnsureSize(ref bytes, offset + 12);
            Buffer.BlockCopy(BitConverter.GetBytes(values.x), 0, bytes, offset, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(values.y), 0, bytes, offset + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(values.z), 0, bytes, offset + 8, 4);
            offset += 12;
        }

        public static Vector3 ReadVectorFloatFromBytes(ref byte[] bytes, ref int offset)
        {
            EnsureSize(bytes, offset + 12);
            float x = BitConverter.ToSingle(bytes, offset);
            float y = BitConverter.ToSingle(bytes, offset + 4);
            float z = BitConverter.ToSingle(bytes, offset + 8);
            offset += 12;

            return new Vector3(x, y, z);
        }

        public static void WriteQuaternionToBytes(quaternion rotation, ref byte[] bytes, ref int offset, BasisRangedUshortFloatData compressor)
        {
            EnsureSize(ref bytes, offset + 14);
            ushort compressedW = compressor.Compress(rotation.value.w);

            Buffer.BlockCopy(BitConverter.GetBytes(rotation.value.x), 0, bytes, offset, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(rotation.value.y), 0, bytes, offset + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(rotation.value.z), 0, bytes, offset + 8, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(compressedW), 0, bytes, offset + 12, 2);
            offset += 14;
        }

        public static quaternion ReadQuaternionFromBytes(ref byte[] bytes, BasisRangedUshortFloatData compressor, ref int offset)
        {
            EnsureSize(bytes, offset + 14);
            float x = BitConverter.ToSingle(bytes, offset);
            float y = BitConverter.ToSingle(bytes, offset + 4);
            float z = BitConverter.ToSingle(bytes, offset + 8);
            ushort compressedW = BitConverter.ToUInt16(bytes, offset + 12);
            offset += 14;

            return new quaternion(x, y, z, compressor.Decompress(compressedW));
        }

        public static void WriteMusclesToBytes(float[] muscles, ref byte[] bytes, ref int offset)
        {
            int requiredLength = muscles.Length * sizeof(float);
            EnsureSize(ref bytes, offset + requiredLength);

            Buffer.BlockCopy(muscles, 0, bytes, offset, requiredLength);
            offset += requiredLength;
        }

        public static void ReadMusclesFromBytes(ref byte[] bytes, ref float[] muscles, ref int offset)
        {
            int requiredLength = bytes.Length - offset;
            if (muscles == null || muscles.Length * sizeof(float) != requiredLength)
                muscles = new float[requiredLength / sizeof(float)];

            EnsureSize(bytes, offset + requiredLength);
            Buffer.BlockCopy(bytes, offset, muscles, 0, requiredLength);
            offset += requiredLength;
        }

        private static void EnsureSize(ref byte[] bytes, int requiredSize)
        {
            if (bytes == null || bytes.Length < requiredSize)
            {
                Array.Resize(ref bytes, requiredSize);
            }
        }

        private static void EnsureSize(byte[] bytes, int requiredSize)
        {
            if (bytes == null || bytes.Length < requiredSize)
            {
                throw new ArgumentException("Byte array is too small for the required size.");
            }
        }
    }
}