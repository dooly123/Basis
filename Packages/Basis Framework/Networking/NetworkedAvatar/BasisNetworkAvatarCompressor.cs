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
        public static void CompressIntoSendBase(BasisNetworkSendBase NetworkSendBase, Animator Anim)
        {
            CompressAvatar(ref NetworkSendBase.TargetData, ref NetworkSendBase.HumanPose, NetworkSendBase.PoseHandler, Anim, ref NetworkSendBase.LASM, NetworkSendBase.PositionRanged, NetworkSendBase.ScaleRanged);
        }
        /// <summary>
        /// 5
        /// </summary>
        /// <param name="AvatarData"></param>
        /// <param name="CachedPose"></param>
        /// <param name="SenderPoseHandler"></param>
        /// <param name="LocalPlayersAnimator"></param>
        /// <param name="Bytes"></param>
        /// <param name="PositionRanged"></param>
        /// <param name="ScaleRanged"></param>
        public static void CompressAvatar(ref BasisAvatarData AvatarData, ref HumanPose CachedPose, HumanPoseHandler SenderPoseHandler, Animator LocalPlayersAnimator, ref LocalAvatarSyncMessage Bytes, BasisRangedUshortFloatData PositionRanged, BasisRangedUshortFloatData ScaleRanged)
        {
            SenderPoseHandler.GetHumanPose(ref CachedPose);
            AvatarData.Vectors[0] = LocalPlayersAnimator.bodyPosition;
            AvatarData.Vectors[1] = LocalPlayersAnimator.transform.localScale; // scale
            // Copy muscles [0..14]
            Buffer.BlockCopy(CachedPose.muscles, 0, AvatarData.floatArray, 0,BasisNetworkSendBase.FirstBuffer);

            // Copy muscles [21..end]
            Buffer.BlockCopy(CachedPose.muscles, BasisNetworkSendBase.SecondBuffer, AvatarData.floatArray, BasisNetworkSendBase.FirstBuffer, BasisNetworkSendBase.SizeAfterGap);

            // Update the muscles in AvatarData
            AvatarData.Muscles.CopyFrom(AvatarData.floatArray); // muscles

            AvatarData.Rotation = LocalPlayersAnimator.bodyRotation; // hips rotation
            CompressAvatarUpdate(ref Bytes, AvatarData.Vectors[1], AvatarData.Vectors[0], AvatarData.Rotation, AvatarData.floatArray, PositionRanged, ScaleRanged);
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
            if (syncmessage.array == null)
            {
                syncmessage.array = new byte[212];
            }
            using (var Packer = DarkRiftWriter.Create(216))
            {
                CompressScaleAndPosition(Packer, HipsPosition, Scale, PositionRanged, ScaleRanged);//18

                BasisCompressionOfRotation.CompressQuaternion(Packer, Rotation);//4
                BasisCompressionOfMuscles.CompressMuscles(Packer, muscles);//190
                //disable in production
                if (Packer.Length != syncmessage.array.Length)
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