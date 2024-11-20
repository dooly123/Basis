using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.Recievers;
using DarkRift;
using Unity.Mathematics;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.UIElements;
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
            using (var bitPacker = DarkRiftReader.CreateFromArray(syncMessage.avatarSerialization.array, 0, syncMessage.avatarSerialization.array.Length))
            {

                avatarBuffer.Position = BasisCompressionOfPosition.DecompressVector3(bitPacker);
                avatarBuffer.Scale = BasisCompressionOfPosition.DecompressUShortVector3(bitPacker, baseReceiver.ScaleRanged);
                BasisCompressionOfRotation.DecompressQuaternion(bitPacker, ref avatarBuffer.rotation);
                BasisCompressionOfMuscles.DecompressMuscles(bitPacker, ref avatarBuffer);
            }
            baseReceiver.TimeAsDoubleWhenLastSync = Time.realtimeSinceStartupAsDouble;
            avatarBuffer.timestamp = baseReceiver.TimeAsDoubleWhenLastSync;
            baseReceiver.AvatarDataBuffer.Add(avatarBuffer);

            // Sort buffer by timestamp
            baseReceiver.AvatarDataBuffer.Sort((a, b) => a.timestamp.CompareTo(b.timestamp));
        }
    }
}