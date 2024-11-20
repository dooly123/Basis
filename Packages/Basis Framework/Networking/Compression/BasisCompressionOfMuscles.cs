using Basis.Scripts.Networking.NetworkedAvatar;
using DarkRift;
using System;

namespace Basis.Scripts.Networking.Compression
{
    public class BasisCompressionOfMuscles
    {
        public static byte[] StoredBytes = new byte[LengthBytes]; // 90 floats * 4 bytes
        public static int LengthBytes = 90 * 4;
        public static int LengthSize = 90;
        // Compress the muscle data into the byte array
        public static void CompressMuscles(DarkRiftWriter Packer, float[] muscles)
        {
            if (muscles.Length != LengthSize)
            {
                throw new ArgumentException("The 'muscles' array must contain exactly 90 elements.");
            }
            // Convert the float array to bytes using Buffer.BlockCopy
            Buffer.BlockCopy(muscles, 0, StoredBytes, 0, LengthBytes);

            // Write the raw byte array to the Packer
            Packer.WriteRaw(StoredBytes, 0, LengthBytes);
        }

        public static float[] decompressedMuscles = new float[LengthSize]; // 90 floats

        // Decompress the byte array back into the muscle data
        public static void DecompressMuscles(DarkRiftReader Packer, ref BasisAvatarData BasisAvatarData)
        {
            // Ensure the StoredBytes array has enough capacity
            if (StoredBytes.Length < LengthBytes)
            {
                throw new ArgumentException("The 'StoredBytes' array must have a length of at least 360 bytes.");
            }
            // Read the raw byte array from the Packer
            Packer.ReadRaw(LengthBytes, ref StoredBytes);

            // Convert the byte array back to the float array using Buffer.BlockCopy
            Buffer.BlockCopy(StoredBytes, 0, decompressedMuscles, 0, LengthBytes);

            // Copy the decompressed data back to the avatar's muscle array
            if (BasisAvatarData.Muscles.Length != LengthSize)
            {
                throw new ArgumentException("The 'Muscles' array in BasisAvatarData must contain exactly 90 elements.");
            }
            BasisAvatarData.Muscles.CopyFrom(decompressedMuscles);
        }
    }
}