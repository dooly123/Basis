using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.Recievers;
using System.Collections.Concurrent;
using static Basis.Scripts.Networking.NetworkedAvatar.BasisNetworkSendBase;
using static SerializableDarkRift;
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
            AvatarBuffer avatarBuffer = new AvatarBuffer();
            int Offset = 0;
            avatarBuffer.Position = BasisUnityBitPackerExtensions.ReadVectorFloatFromBytes(ref syncMessage.avatarSerialization.array, ref Offset);
            avatarBuffer.rotation = BasisUnityBitPackerExtensions.ReadQuaternionFromBytes(ref syncMessage.avatarSerialization.array, BasisNetworkSendBase.RotationCompression, ref Offset);
            BasisUnityBitPackerExtensions.ReadMusclesFromBytes(ref syncMessage.avatarSerialization.array, ref avatarBuffer.Muscles, ref Offset);
            int length = syncMessage.avatarSerialization.array.Length;
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
            avatarBuffer.SecondsInterval = syncMessage.interval / 1000.0f;
            baseReceiver.EnQueueAvatarBuffer(ref avatarBuffer);
        }
    }
}