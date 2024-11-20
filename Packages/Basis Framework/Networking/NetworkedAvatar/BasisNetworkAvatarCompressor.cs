using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Profiler;
using DarkRift;
using DarkRift.Server.Plugins.Commands;
using System;
using UnityEngine;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    public static class BasisNetworkAvatarCompressor
    {
        public static void Compress(BasisNetworkSendBase NetworkSendBase, Animator Anim)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                CompressAvatarData(NetworkSendBase, Anim);
                writer.Write(NetworkSendBase.LASM);
                BasisNetworkProfiler.AvatarUpdatePacket.Sample(writer.Length);
                using (var msg = Message.Create(BasisTags.AvatarMuscleUpdateTag, writer))
                {
                    BasisNetworkManagement.Instance.Client.SendMessage(msg, BasisNetworking.MovementChannel, DeliveryMethod.Sequenced);
                }
            }
        }
        public static float[] FloatArray = new float[90];
        public static void CompressAvatarData(BasisNetworkSendBase NetworkSendBase, Animator Anim)
        {
            // Retrieve the human pose from the Animator
            NetworkSendBase.PoseHandler.GetHumanPose(ref NetworkSendBase.HumanPose);

            // Copy muscles [0..14]
            Array.Copy(NetworkSendBase.HumanPose.muscles, 0, FloatArray, 0, BasisNetworkSendBase.FirstBuffer);

            // Copy muscles [21..end]
            Array.Copy(NetworkSendBase.HumanPose.muscles, BasisNetworkSendBase.SecondBuffer, FloatArray, BasisNetworkSendBase.FirstBuffer, BasisNetworkSendBase.SizeAfterGap);

            int Offset = 0;
            BasisBitPackerExtensions.WriteVectorFloatToBytes(Anim.bodyPosition,ref NetworkSendBase.LASM.array,ref Offset);
            BasisBitPackerExtensions.WriteUshortVectorFloatToBytes(Anim.transform.localScale, BasisNetworkSendBase.ScaleRanged,ref NetworkSendBase.LASM.array, ref Offset);
            BasisBitPackerExtensions.WriteQuaternionToBytes(Anim.bodyRotation, ref NetworkSendBase.LASM.array, ref Offset, BasisNetworkSendBase.RotationCompression);
            BasisBitPackerExtensions.WriteMusclesToBytes(FloatArray, ref NetworkSendBase.LASM.array,ref Offset);
        }
    }
}