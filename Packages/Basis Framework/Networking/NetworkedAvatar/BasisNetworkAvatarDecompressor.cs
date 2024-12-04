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
            avatarBuffer.Position = BasisUnityBitPackerExtensions.ReadVectorFloatFromBytes(ref syncMessage.avatarSerialization.array, ref Offset);
            avatarBuffer.Scale = BasisUnityBitPackerExtensions.ReadUshortVectorFloatFromBytes(ref syncMessage.avatarSerialization.array, BasisNetworkReceiver.ScaleRanged, ref Offset);
            avatarBuffer.rotation = BasisUnityBitPackerExtensions.ReadQuaternionFromBytes(ref syncMessage.avatarSerialization.array, BasisNetworkSendBase.RotationCompression, ref Offset);
            BasisUnityBitPackerExtensions.ReadMusclesFromBytes(ref syncMessage.avatarSerialization.array, ref avatarBuffer.Muscles, ref Offset);
            avatarBuffer.timestamp = Time.timeAsDouble;
            avatarBuffer.SendRate = 0.1;

            // Only compute the sendRate if data is available
            if (baseReceiver.PayloadQueue.Count > 0)
            {
                double lastPayloadTime = baseReceiver.PayloadQueue.Peek().timestamp;
                baseReceiver.sendRate = avatarBuffer.timestamp - lastPayloadTime;
                baseReceiver.smoothedSendRate = baseReceiver.smoothedSendRate * (1 - baseReceiver.smoothingFactor) + baseReceiver.sendRate * baseReceiver.smoothingFactor;
                avatarBuffer.SendRate = baseReceiver.smoothedSendRate;
            }
            avatarBuffer.IsInitalized = true;
            baseReceiver.PayloadQueue.Enqueue(avatarBuffer);
        }
    }
}