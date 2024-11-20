using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.Recievers;
using DarkRift;
using UnityEngine;
using static Basis.Scripts.Networking.NetworkedAvatar.BasisNetworkSendBase;
using static SerializableDarkRift;
namespace Basis.Scripts.Networking.NetworkedAvatar
{
    public static class BasisNetworkAvatarDecompressor
    {
        /// <summary>
        /// Single API to handle all avatar decompression tasks.
        /// </summary>
        public static void DecompressAndProcessAvatar(BasisNetworkReceiver baseReceiver, ServerSideSyncPlayerMessage syncMessage)
        {
            // Update receiver state
            baseReceiver.LASM = syncMessage.avatarSerialization;

            // Initialize AvatarBuffer
            AvatarBuffer avatarBuffer = new AvatarBuffer();
            using (var bitPacker = DarkRiftReader.CreateFromArray(syncMessage.avatarSerialization.array, 0, baseReceiver.LASM.array.Length))
            {
                BasisCompressionOfPosition.DecompressVector3(bitPacker, ref avatarBuffer.Position);
                BasisCompressionOfPosition.DecompressUShortVector3(bitPacker, BasisNetworkReceiver.ScaleRanged, ref avatarBuffer.Scale);
                BasisCompressionOfRotation.DecompressQuaternion(bitPacker, ref avatarBuffer.rotation);
                BasisCompressionOfMuscles.DecompressMuscles(bitPacker, ref avatarBuffer);
            }
            avatarBuffer.timestamp = Time.realtimeSinceStartupAsDouble;
            baseReceiver.AvatarDataQueue.Enqueue(avatarBuffer);
        }
    }
}