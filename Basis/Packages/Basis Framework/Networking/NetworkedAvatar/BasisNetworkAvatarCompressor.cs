using Basis.Network.Core;
using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.Transmitters;
using Basis.Scripts.Profiler;
using LiteNetLib;
using System;
using UnityEngine;
using static SerializableBasis;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    public static class BasisNetworkAvatarCompressor
    {
        public static void Compress(BasisNetworkTransmitter NetworkSendBase, Animator Anim)
        {
            CompressAvatarData(ref NetworkSendBase.Offset, ref NetworkSendBase.FloatArray, ref NetworkSendBase.LASM, NetworkSendBase.PoseHandler, NetworkSendBase.HumanPose, Anim);
            NetworkSendBase.LASM.Serialize(NetworkSendBase.AvatarSendWriter);
            BasisNetworkProfiler.LocalAvatarSyncMessageCounter.Sample(NetworkSendBase.AvatarSendWriter.Length);
            BasisNetworkManagement.LocalPlayerPeer.Send(NetworkSendBase.AvatarSendWriter, BasisNetworkCommons.MovementChannel, DeliveryMethod.Sequenced);
            NetworkSendBase.AvatarSendWriter.Reset();
        }
        public static LocalAvatarSyncMessage InitalAvatarData(Animator Anim)
        {
            HumanPoseHandler PoseHandler = new HumanPoseHandler(Anim.avatar, Anim.transform);
            HumanPose HumanPose = new HumanPose();
            PoseHandler.GetHumanPose(ref HumanPose);
            float[] FloatArray = new float[90];
            int Offset = 0;
            LocalAvatarSyncMessage LocalAvatarSyncMessage = new LocalAvatarSyncMessage();
            CompressAvatarData(ref Offset, ref FloatArray, ref LocalAvatarSyncMessage, PoseHandler, HumanPose, Anim);
            return LocalAvatarSyncMessage;
        }
        public static void CompressAvatarData(ref int Offset, ref float[] FloatArray, ref LocalAvatarSyncMessage LocalAvatarSyncMessage, HumanPoseHandler Handler, HumanPose PoseHandler, Animator Anim)
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
            BasisUnityBitPackerExtensions.WriteMusclesToBytes(FloatArray, ref LocalAvatarSyncMessage.array, ref Offset);
        }
    }
}
