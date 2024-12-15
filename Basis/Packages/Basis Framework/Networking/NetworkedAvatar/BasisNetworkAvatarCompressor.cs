using Basis.Network.Core;
using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.Transmitters;
using Basis.Scripts.Profiler;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using UnityEngine;
using static SerializableBasis;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    public static class BasisNetworkAvatarCompressor
    {
        public static void Compress(BasisNetworkTransmitter NetworkSendBase, Animator Anim)
        {
            NetDataWriter Writer = new NetDataWriter();
            CompressAvatarData(ref NetworkSendBase.Offset, ref NetworkSendBase.FloatArray,ref NetworkSendBase.LASM,NetworkSendBase.PoseHandler,NetworkSendBase.HumanPose, Anim);
            NetworkSendBase.LASM.Serialize(Writer);
            BasisNetworkProfiler.AvatarUpdatePacket.Sample(Writer.Length);
            BasisNetworkManagement.LocalPlayerPeer.Send(Writer, BasisNetworkCommons.MovementChannel, DeliveryMethod.Sequenced);
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
        public static void CompressAvatarData(ref int Offset,ref float[] FloatArray, ref LocalAvatarSyncMessage LocalAvatarSyncMessage,HumanPoseHandler Handler, HumanPose PoseHandler, Animator Anim)
        {
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
            /*
            UnityEngine.Vector3 Scale = Anim.transform.localScale;
            //we decode localscale last so we can optimize it.
            const float EPSILON = 0.0001f; // Define a small value for approximate comparison

            // We decode local scale last so we can optimize it.
            if (Scale == UnityEngine.Vector3.one)
            {
                // Don't send anything as the remote can assume it's one
            }
            else
            {
                if (Mathf.Abs(Scale.x - Scale.y) < EPSILON && Mathf.Abs(Scale.y - Scale.z) < EPSILON)
                {
                    // The scale is uniform; write a single ushort
                    BasisUnityBitPackerExtensions.WriteUshortToBytes(Scale.x, BasisNetworkSendBase.ScaleRanged, ref LocalAvatarSyncMessage.array, ref Offset);
                }
                else
                {
                    // The scale is non-uniform; write 3 ushorts
                    BasisUnityBitPackerExtensions.WriteUshortVectorFloatToBytes(Scale, BasisNetworkSendBase.ScaleRanged, ref LocalAvatarSyncMessage.array, ref Offset);
                }
            }
            */
        }
    }
}