using System;
using System.Collections.Generic;
using static BasisNetworkPrimitiveCompression;

namespace Basis.Scripts.Networking.Compression
{
    public static class BasisUnityBitPackerExtensions
    {
        public static BasisRangedUshortFloatData BasisRangedUshortFloatData = new BasisRangedUshortFloatData(-1f, 1f, 0.001f);
        public static int LengthSize = 90;
        public static int LengthBytes = LengthSize * 4; // Initialize LengthBytes first
        public static byte[] StoredBytes = new byte[LengthBytes];

        // Object pool for byte arrays to avoid allocation during runtime
        private static readonly ObjectPool<byte[]> byteArrayPool = new ObjectPool<byte[]>(() => new byte[LengthBytes]);

        // Manual conversion of ushort to bytes (without BitConverter)
        public static void WriteUshortVectorFloatToBytes(UnityEngine.Vector3 values, BasisRangedUshortFloatData compressor, ref byte[] bytes, ref int offset)
        {
            EnsureSize(ref bytes, offset + 6);
            ushort compressedX = compressor.Compress(values.x);
            ushort compressedY = compressor.Compress(values.y);
            ushort compressedZ = compressor.Compress(values.z);

            // Manually write the compressed values to the byte array (no BitConverter)
            bytes[offset] = (byte)(compressedX & 0xFF);           // Low byte
            bytes[offset + 1] = (byte)((compressedX >> 8) & 0xFF); // High byte

            bytes[offset + 2] = (byte)(compressedY & 0xFF);           // Low byte
            bytes[offset + 3] = (byte)((compressedY >> 8) & 0xFF);    // High byte

            bytes[offset + 4] = (byte)(compressedZ & 0xFF);           // Low byte
            bytes[offset + 5] = (byte)((compressedZ >> 8) & 0xFF);    // High byte

            offset += 6;
        }
        // Manual conversion of ushort to bytes (without BitConverter) for a single ushort
        public static void WriteUshortToBytes(float input, BasisRangedUshortFloatData compressor, ref byte[] bytes, ref int offset)
        {
            ushort value = compressor.Compress(input);
            EnsureSize(ref bytes, offset + 2); // Ensure enough space for 2 bytes

            // Manually write the ushort value to the byte array
            bytes[offset] = (byte)(value & 0xFF);           // Low byte
            bytes[offset + 1] = (byte)((value >> 8) & 0xFF); // High byte

            offset += 2; // Increment offset by 2 bytes
        }
        // Manual conversion of bytes to ushort (without BitConverter)
        public static Unity.Mathematics.float3 ReadUshortVectorFloatFromBytes(ref byte[] bytes, BasisRangedUshortFloatData compressor, ref int offset)
        {
            EnsureSize(bytes, offset + 6);

            ushort compressedX = (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
            ushort compressedY = (ushort)(bytes[offset + 2] | (bytes[offset + 3] << 8));
            ushort compressedZ = (ushort)(bytes[offset + 4] | (bytes[offset + 5] << 8));

            offset += 6;

            return new Unity.Mathematics.float3(
                compressor.Decompress(compressedX),
                compressor.Decompress(compressedY),
                compressor.Decompress(compressedZ)
            );
        }
        public static float ReadUshortFloatFromBytes(ref byte[] bytes, BasisRangedUshortFloatData compressor, ref int offset)
        {
            EnsureSize(bytes, offset + 2);  // Only 2 bytes needed for one ushort value

            // Read the single compressed ushort value
            ushort compressedValue = (ushort)(bytes[offset] | (bytes[offset + 1] << 8));

            offset += 2;

            // Decompress the single value and return it
            return compressor.Decompress(compressedValue);
        }
        // Manual conversion of Vector3 to bytes (without BitConverter)
        public static void WriteEnsureFloatToBytes(float value, ref byte[] bytes, ref int offset)
        {
            EnsureSize(ref bytes, offset + 4);

            // Manually write the float values to bytes (no BitConverter)
            WriteFloatToBytes(value, ref bytes, ref offset);
        }
        // Manual conversion of Vector3 to bytes (without BitConverter)
        public static void WriteVectorFloatToBytes(UnityEngine. Vector3 values, ref byte[] bytes, ref int offset)
        {
            EnsureSize(ref bytes, offset + 12);

            // Manually write the float values to bytes (no BitConverter)
            WriteFloatToBytes(values.x, ref bytes, ref offset);
            WriteFloatToBytes(values.y, ref bytes, ref offset);
            WriteFloatToBytes(values.z, ref bytes, ref offset);
        }

        // Manual conversion of bytes to Vector3 (without BitConverter)
        public static Unity.Mathematics.float3 ReadVectorFloatFromBytes(ref byte[] bytes, ref int offset)
        {
            EnsureSize(bytes, offset + 12);

            float x = ReadFloatFromBytes(ref bytes, ref offset);//4
            float y = ReadFloatFromBytes(ref bytes, ref offset);//8
            float z = ReadFloatFromBytes(ref bytes, ref offset);//12

            return new Unity.Mathematics.float3(x, y, z);
        }

        // Manual conversion of quaternion to bytes (without BitConverter)
        public static void WriteQuaternionToBytes(Unity.Mathematics.quaternion rotation, ref byte[] bytes, ref int offset, BasisRangedUshortFloatData compressor)
        {
            EnsureSize(ref bytes, offset + 14);
            ushort compressedW = compressor.Compress(rotation.value.w);

            // Write the quaternion's components
            WriteFloatToBytes(rotation.value.x, ref bytes, ref offset);
            WriteFloatToBytes(rotation.value.y, ref bytes, ref offset);
            WriteFloatToBytes(rotation.value.z, ref bytes, ref offset);

            // Write the compressed 'w' component
            bytes[offset] = (byte)(compressedW & 0xFF);           // Low byte
            bytes[offset + 1] = (byte)((compressedW >> 8) & 0xFF); // High byte
            offset += 2;
        }

        // Manual conversion of bytes to quaternion (without BitConverter)
        public static Unity.Mathematics.quaternion ReadQuaternionFromBytes(ref byte[] bytes, BasisRangedUshortFloatData compressor, ref int offset)
        {
            EnsureSize(bytes, offset + 14);

            float x = ReadFloatFromBytes(ref bytes, ref offset);//4
            float y = ReadFloatFromBytes(ref bytes, ref offset);//8
            float z = ReadFloatFromBytes(ref bytes, ref offset);//12

            ushort compressedW = (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
            offset += 2;//2

            return new Unity.Mathematics.quaternion(x, y, z, compressor.Decompress(compressedW));
        }

        // Write muscles to bytes (no BitConverter)
        public static void WriteMusclesToBytes(float[] muscles, ref byte[] bytes, ref int offset)
        {
            int Length = muscles.Length;
            int requiredLength = Length * sizeof(float);
            EnsureSize(ref bytes, offset + requiredLength);

            // Manually copy float values as bytes
            for (int Index = 0; Index < Length; Index++)
            {
                WriteFloatToBytes(muscles[Index], ref bytes, ref offset);
            }
        }

        // Read muscles from bytes (no BitConverter)
        public static void ReadMusclesFromBytes(ref byte[] bytes, ref float[] muscles, ref int offset)
        {
            if (muscles == null || muscles.Length != 90)
            {
                muscles = new float[90];
            }
            EnsureSize(bytes, offset + 360);

            // Manually read float values from bytes
            for (int Index = 0; Index < 90; Index++)
            {
                muscles[Index] = ReadFloatFromBytes(ref bytes, ref offset);
               // UnityEngine.Debug.Log("" + muscles[i]);
            }
        }

        // Manual float to bytes conversion
        // Manual float to bytes conversion (without BitConverter)
        private unsafe static void WriteFloatToBytes(float value, ref byte[] bytes, ref int offset)
        {
            // Convert the float to a uint using its bitwise representation
            uint intValue = *((uint*)&value);

            // Manually write the bytes
            bytes[offset] = (byte)(intValue & 0xFF);
            bytes[offset + 1] = (byte)((intValue >> 8) & 0xFF);
            bytes[offset + 2] = (byte)((intValue >> 16) & 0xFF);
            bytes[offset + 3] = (byte)((intValue >> 24) & 0xFF);
            offset += 4;
        }

        // Manual bytes to float conversion (without BitConverter)
        private unsafe static float ReadFloatFromBytes(ref byte[] bytes, ref int offset)
        {
            // Reconstruct the uint from the byte array
            uint intValue = (uint)(bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24));

            // Convert the uint back to float using a pointer cast
            float result = *((float*)&intValue);
            offset += 4;
            return result;
        }

        // Ensure the byte array is large enough to hold the data
        private static void EnsureSize(ref byte[] bytes, int requiredSize)
        {
            if (bytes == null || bytes.Length < requiredSize)
            {
                // Reuse pooled byte arrays
                bytes = byteArrayPool.Get();
                Array.Resize(ref bytes, requiredSize);
            }
        }

        // Ensure the byte array is large enough for reading
        private static void EnsureSize(byte[] bytes, int requiredSize)
        {
            if (bytes.Length < requiredSize)
            {
                throw new ArgumentException("Byte array is too small for the required size. Current Size is " + bytes.Length + " But Required " + requiredSize);
            }
        }

        // Object pool for byte arrays to avoid allocation during runtime
        private class ObjectPool<T>
        {
            private readonly Func<T> createFunc;
            private readonly Stack<T> pool;

            public ObjectPool(Func<T> createFunc)
            {
                this.createFunc = createFunc;
                this.pool = new Stack<T>();
            }

            public T Get()
            {
                return pool.Count > 0 ? pool.Pop() : createFunc();
            }

            public void Return(T item)
            {
                pool.Push(item);
            }
        }
    }
}