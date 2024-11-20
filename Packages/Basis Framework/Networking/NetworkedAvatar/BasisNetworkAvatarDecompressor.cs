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
            AvatarBuffer avatarBuffer = BasisAvatarBufferPool.Rent();
            using (var bitPacker = DarkRiftReader.CreateFromArray(syncMessage.avatarSerialization.array, 0, baseReceiver.LASM.array.Length))
            {
                BasisBitPackerExtensions.DecompressVector3(bitPacker, ref avatarBuffer.Position);
                BasisBitPackerExtensions.DecompressUShortVector3(bitPacker, BasisNetworkReceiver.ScaleRanged, ref avatarBuffer.Scale);
                BasisBitPackerExtensions.DecompressQuaternion(bitPacker, ref avatarBuffer.rotation);
                BasisBitPackerExtensions.DecompressMuscles(bitPacker, ref avatarBuffer);
            }
            avatarBuffer.timestamp = Time.realtimeSinceStartupAsDouble;
            baseReceiver.AvatarDataBuffer.Add(avatarBuffer);
        }
    }
}