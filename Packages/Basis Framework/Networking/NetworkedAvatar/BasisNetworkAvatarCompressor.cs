using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Profiler;
using DarkRift;
using DarkRift.Server.Plugins.Commands;
using System;
using UnityEngine;
using static SerializableDarkRift;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    public static class BasisNetworkAvatarCompressor
    {
        public static void Compress(BasisNetworkSendBase NetworkSendBase, Animator Anim)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                CompressIntoSendBase(NetworkSendBase, Anim);
                writer.Write(NetworkSendBase.LASM);
                BasisNetworkProfiler.AvatarUpdatePacket.Sample(writer.Length);
                using (var msg = Message.Create(BasisTags.AvatarMuscleUpdateTag, writer))
                {
                    BasisNetworkManagement.Instance.Client.SendMessage(msg, BasisNetworking.MovementChannel, DeliveryMethod.Sequenced);
                }
            }
        }
        public static float[] FloatArray = new float[90];
        public static void CompressIntoSendBase(BasisNetworkSendBase NetworkSendBase, Animator Anim)
        {
            // Extract the necessary components
            HumanPoseHandler SenderPoseHandler = NetworkSendBase.PoseHandler;

            // Retrieve the human pose from the Animator
            SenderPoseHandler.GetHumanPose(ref NetworkSendBase.HumanPose);

            // Copy muscles [0..14]
            Buffer.BlockCopy(NetworkSendBase.HumanPose.muscles, 0, FloatArray, 0, BasisNetworkSendBase.FirstBufferBytes);

            // Copy muscles [21..end]
            Buffer.BlockCopy(NetworkSendBase.HumanPose.muscles, BasisNetworkSendBase.SecondBufferBytes, FloatArray, BasisNetworkSendBase.FirstBufferBytes, BasisNetworkSendBase.SizeAfterGapBytes);

            // Compress the avatar data
            CompressAvatarUpdate(ref NetworkSendBase.LASM, Anim.transform.localScale, Anim.bodyPosition, Anim.bodyRotation, FloatArray, NetworkSendBase.PositionRanged, NetworkSendBase.ScaleRanged);
        }
        /// <summary>
        /// 212
        /// </summary>
        /// <param name="syncmessage"></param>
        /// <param name="Scale"></param>
        /// <param name="HipsPosition"></param>
        /// <param name="Rotation"></param>
        /// <param name="muscles"></param>
        /// <param name="PositionRanged"></param>
        /// <param name="ScaleRanged"></param>
        public static void CompressAvatarUpdate(ref LocalAvatarSyncMessage syncmessage, Vector3 Scale, Vector3 HipsPosition, Quaternion Rotation, float[] muscles, BasisRangedUshortFloatData PositionRanged, BasisRangedUshortFloatData ScaleRanged)
        {
            using (var Packer = DarkRiftWriter.Create(216))
            {
                CompressScaleAndPosition(Packer, HipsPosition, Scale, PositionRanged, ScaleRanged);//18

                BasisCompressionOfRotation.CompressQuaternion(Packer, Rotation);//4
                BasisCompressionOfMuscles.CompressMuscles(Packer, muscles);//190
                //disable in production
                if (syncmessage.array == null || Packer.Length != syncmessage.array.Length)
                {
                    syncmessage.array = new byte[Packer.Length];
                }
                Packer.CopyTo(syncmessage.array, 0);
            }
        }
        public static void CompressScaleAndPosition(DarkRiftWriter packer, Vector3 bodyPosition, Vector3 scale, BasisRangedUshortFloatData PositionRanged, BasisRangedUshortFloatData ScaleRanged)
        {
            BasisCompressionOfPosition.CompressVector3(bodyPosition, packer); //4+4+4 = 12

            BasisCompressionOfPosition.CompressUShortVector3(scale, packer, ScaleRanged);//2+2+2 6
        }
    }
}