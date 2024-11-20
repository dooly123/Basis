using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.Recievers;
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
            int Offset = 0;
            avatarBuffer.Position = BasisBitPackerExtensions.ReadVectorFloatFromBytes(ref syncMessage.avatarSerialization.array, ref Offset);
            avatarBuffer.Scale = BasisBitPackerExtensions.ReadUshortVectorFloatFromBytes(ref syncMessage.avatarSerialization.array, BasisNetworkReceiver.ScaleRanged, ref Offset);
            avatarBuffer.rotation = BasisBitPackerExtensions.ReadQuaternionFromBytes(ref syncMessage.avatarSerialization.array, BasisNetworkSendBase.RotationCompression, ref Offset);
            BasisBitPackerExtensions.ReadMusclesFromBytes(ref syncMessage.avatarSerialization.array, ref avatarBuffer.Muscles, ref Offset);
            avatarBuffer.timestamp = Time.realtimeSinceStartupAsDouble;
            baseReceiver.AvatarDataBuffer.Add(avatarBuffer);
        }
    }
}