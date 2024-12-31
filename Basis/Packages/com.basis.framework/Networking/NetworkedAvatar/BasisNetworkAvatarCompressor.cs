using Basis.Network.Core;
using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.Transmitters;
using Basis.Scripts.Profiler;
using LiteNetLib;
using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using static SerializableBasis;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    public static class BasisNetworkAvatarCompressor
    {
        public static void Compress(BasisNetworkTransmitter NetworkSendBase, Animator Anim)
        {
            CompressAvatarData(ref NetworkSendBase.Offset, ref NetworkSendBase.FloatArray, ref NetworkSendBase.UshortArray, ref NetworkSendBase.LASM, NetworkSendBase.PoseHandler, NetworkSendBase.HumanPose, Anim);
            NetworkSendBase.LASM.Serialize(NetworkSendBase.AvatarSendWriter);
            BasisNetworkProfiler.LocalAvatarSyncMessageCounter.Sample(NetworkSendBase.AvatarSendWriter.Length);
            BasisNetworkManagement.LocalPlayerPeer.Send(NetworkSendBase.AvatarSendWriter, BasisNetworkCommons.MovementChannel, DeliveryMethod.Sequenced);
            NetworkSendBase.AvatarSendWriter.Reset();
        }
        public static void InitalAvatarData(Animator Anim, out LocalAvatarSyncMessage LocalAvatarSyncMessage)
        {
            HumanPoseHandler PoseHandler = new HumanPoseHandler(Anim.avatar, Anim.transform);
            HumanPose HumanPose = new HumanPose();
            PoseHandler.GetHumanPose(ref HumanPose);
            float[] FloatArray = new float[LocalAvatarSyncMessage.StoredBones];
            ushort[] UshortArray = new ushort[LocalAvatarSyncMessage.StoredBones];
            int Offset = 0;
            LocalAvatarSyncMessage = new LocalAvatarSyncMessage();
            CompressAvatarData(ref Offset, ref FloatArray, ref UshortArray, ref LocalAvatarSyncMessage, PoseHandler, HumanPose, Anim);
        }
        public static void CompressAvatarData(ref int Offset, ref float[] FloatArray, ref ushort[] NetworkSend, ref LocalAvatarSyncMessage LocalAvatarSyncMessage, HumanPoseHandler Handler, HumanPose PoseHandler, Animator Anim)
        {
            if (Handler == null)
            {
                Handler = new HumanPoseHandler(Anim.avatar, Anim.transform);
            }
            Offset = 0;
            // Retrieve the human pose from the Animator
            Handler.GetHumanPose(ref PoseHandler);

            // Copy muscles [0..14]
            Array.Copy(PoseHandler.muscles, 0, FloatArray, 0, BasisNetworkSendBase.FirstBuffer);

            // Copy muscles [21..end]
            Array.Copy(PoseHandler.muscles, BasisNetworkSendBase.SecondBuffer, FloatArray, BasisNetworkSendBase.FirstBuffer, BasisNetworkSendBase.SizeAfterGap);
            //we write position first so we can use that on the server
            BasisUnityBitPackerExtensions.WriteVectorFloatToBytes(Anim.bodyPosition, ref LocalAvatarSyncMessage.array, ref Offset);
            BasisUnityBitPackerExtensions.WriteQuaternionToBytes(Anim.bodyRotation, ref LocalAvatarSyncMessage.array, ref Offset, BasisNetworkSendBase.RotationCompression);

            if(NetworkSend == null)
            {
                BasisDebug.LogError("Network send was null!");
                NetworkSend = new ushort[LocalAvatarSyncMessage.StoredBones];
            }
            if (FloatArray == null)
            {
                BasisDebug.LogError("FloatArray send was null!");
                FloatArray = new float[LocalAvatarSyncMessage.StoredBones];
            }

            var NetworkOutData = NetworkSend;
            var FloatArrayCopy = FloatArray;
            Parallel.For(0, LocalAvatarSyncMessage.StoredBones, Index =>
            {
                NetworkOutData[Index] = Compress(FloatArrayCopy[Index], BasisNetworkSendBase.MinMuscle[Index], BasisNetworkSendBase.MaxMuscle[Index], BasisNetworkSendBase.RangeMuscle[Index]);
            });

            BasisUnityBitPackerExtensions.WriteUShortsToBytes(NetworkOutData, ref LocalAvatarSyncMessage.array, ref Offset);
        }
        public static ushort Compress(float value, float MinValue, float MaxValue,float valueDiffence)
        {
            // Clamp the value to ensure it's within the specified range
            value = math.clamp(value, MinValue, MaxValue);

            // Map the float value to the ushort range
            float normalized = (value - MinValue) / (valueDiffence); // 0..1
            return (ushort)(normalized * ushortRangeDifference);//+ UShortMin (its always zero)
        }
        private const ushort UShortMin = ushort.MinValue; // 0
        private const ushort UShortMax = ushort.MaxValue; // 65535
        private const ushort ushortRangeDifference = UShortMax - UShortMin;
    }
}
