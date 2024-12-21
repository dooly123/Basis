using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.Recievers;
using Basis.Scripts.Profiler;
using System;
using static SerializableBasis;
using Vector3 = UnityEngine.Vector3;
namespace Basis.Scripts.Networking.NetworkedAvatar
{
    public static class BasisNetworkAvatarDecompressor
    {
        /// <summary>
        /// Single API to handle all avatar decompression tasks.
        /// </summary>
        public static void DecompressAndProcessAvatar(BasisNetworkReceiver baseReceiver, ServerSideSyncPlayerMessage syncMessage)
        {
            if (syncMessage.avatarSerialization.array == null)
            {
                throw new ArgumentException("Cant Serialize Avatar Data");
            }
            int Length = syncMessage.avatarSerialization.array.Length;
            baseReceiver.Offset = 0;
            AvatarBuffer avatarBuffer = new AvatarBuffer
            {
                Position = BasisUnityBitPackerExtensions.ReadVectorFloatFromBytes(ref syncMessage.avatarSerialization.array, ref baseReceiver.Offset),
                rotation = BasisUnityBitPackerExtensions.ReadQuaternionFromBytes(ref syncMessage.avatarSerialization.array, BasisNetworkSendBase.RotationCompression, ref baseReceiver.Offset)
            };
            BasisUnityBitPackerExtensions.ReadMusclesFromBytes(ref syncMessage.avatarSerialization.array, ref avatarBuffer.Muscles, ref baseReceiver.Offset);
            avatarBuffer.Scale = Vector3.one;
            BasisNetworkProfiler.ServerSideSyncPlayerMessageCounter.Sample(Length);
            avatarBuffer.SecondsInterval = syncMessage.interval / 1000.0f;
            baseReceiver.EnQueueAvatarBuffer(ref avatarBuffer);
        }
        /// <summary>
        /// Inital Payload
        /// </summary>
        /// <param name="baseReceiver"></param>
        /// <param name="syncMessage"></param>
        /// <exception cref="ArgumentException"></exception>
        public static void DecompressAndProcessAvatar(BasisNetworkReceiver baseReceiver, LocalAvatarSyncMessage syncMessage)
        {
            if (syncMessage.array == null)
            {
                throw new ArgumentException("Cant Serialize Avatar Data");
            }
            int Length = syncMessage.array.Length;
            baseReceiver.Offset = 0;
            AvatarBuffer avatarBuffer = new AvatarBuffer
            {
                Position = BasisUnityBitPackerExtensions.ReadVectorFloatFromBytes(ref syncMessage.array, ref baseReceiver.Offset),
                rotation = BasisUnityBitPackerExtensions.ReadQuaternionFromBytes(ref syncMessage.array, BasisNetworkSendBase.RotationCompression, ref baseReceiver.Offset)
            };
            BasisUnityBitPackerExtensions.ReadMusclesFromBytes(ref syncMessage.array, ref avatarBuffer.Muscles, ref baseReceiver.Offset);
            avatarBuffer.Scale = Vector3.one;
            BasisNetworkProfiler.ServerSideSyncPlayerMessageCounter.Sample(Length);
            avatarBuffer.SecondsInterval = 0.01f;
            baseReceiver.EnQueueAvatarBuffer(ref avatarBuffer);
        }
    }
}
