using DarkRift;
using System;
using static Basis.Scripts.Networking.NetworkedAvatar.BasisNetworkSendBase;

namespace Basis.Scripts.Networking.Compression
{
    public class BasisCompressionOfMuscles
    {
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

            // Convert the byte array back to the float array using Buffer.BlockCopy
            Buffer.BlockCopy(StoredBytes, 0, BasisAvatarData.Muscles, 0, LengthBytes);
        }
    }
}