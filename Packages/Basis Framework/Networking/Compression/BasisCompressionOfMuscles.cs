using Basis.Scripts.Networking.NetworkedAvatar;
using DarkRift;
using System;

namespace Basis.Scripts.Networking.Compression
{
    public class BasisCompressionOfMuscles
    {
        public static byte[] StoredBytes = new byte[95 * 4];  // 95 floats, 4 bytes per float
        public static int Size = 95 * 4;

        // Compress the muscle data into the byte array
        public static void CompressMuscles(DarkRiftWriter Packer, float[] muscles)
        {
            // Convert the float array to bytes using Buffer.BlockCopy
            Buffer.BlockCopy(muscles, 0, StoredBytes, 0, Size);

            // Write the raw byte array to the Packer
            Packer.WriteRaw(StoredBytes, 0, Size);
        }

        // Decompress the byte array back into the muscle data
        public static void DecompressMuscles(DarkRiftReader Packer, ref BasisAvatarData BasisAvatarData)
        {
            // Read the raw byte array from the Packer
            Packer.ReadRaw(Size, ref StoredBytes);

            // Convert the byte array back to the float array using Buffer.BlockCopy
            float[] decompressedMuscles = new float[95];
            Buffer.BlockCopy(StoredBytes, 0, decompressedMuscles, 0, Size);

            // Assign the decompressed muscles data to the BasisAvatarData object
            for (int Index = 0; Index < 95; Index++)
            {
                BasisAvatarData.Muscles[Index] = decompressedMuscles[Index];
            }
        }
    }
}