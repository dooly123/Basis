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
            //  int length = syncMessage.avatarSerialization.array.Length;
            avatarBuffer.Scale = Vector3.one;
            /*
            if (Offset == length)//we are at the end
            {
                avatarBuffer.Scale = Vector3.one;
            }
            else
            {
                if (length > Offset + 6)//we have 3 ushorts
                {
                    avatarBuffer.Scale = BasisUnityBitPackerExtensions.ReadUshortVectorFloatFromBytes(ref syncMessage.avatarSerialization.array, BasisNetworkReceiver.ScaleRanged, ref Offset);
                }
                else
                {
                    //we have just one
                    float Size = BasisUnityBitPackerExtensions.ReadUshortFloatFromBytes(ref syncMessage.avatarSerialization.array, BasisNetworkReceiver.ScaleRanged, ref Offset);
                    avatarBuffer.Scale = new Unity.Mathematics.float3(Size, Size, Size);
                }

          }
            */
            BasisNetworkProfiler.InBoundAvatarUpdatePacket.Sample(Length);
            avatarBuffer.SecondsInterval = syncMessage.interval / 1000.0f;
            baseReceiver.EnQueueAvatarBuffer(ref avatarBuffer);
        }
    }
}