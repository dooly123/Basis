using Basis.Scripts.Networking.Compression;
using Basis.Scripts.Networking.Transmitters;
using Basis.Scripts.Profiler;
using DarkRift;
using DarkRift.Server.Plugins.Commands;
using System;
using UnityEngine;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    public static class BasisNetworkAvatarCompressor
    {
        public static float[] FloatArray = new float[90];
        public static void Compress(BasisNetworkTransmitter NetworkSendBase, Animator Anim)
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
        public static void CompressAvatarData(BasisNetworkTransmitter NetworkSendBase, Animator Anim)
        {
            // Retrieve the human pose from the Animator
            NetworkSendBase.PoseHandler.GetHumanPose(ref NetworkSendBase.HumanPose);

            // Copy muscles [0..14]
            Array.Copy(NetworkSendBase.HumanPose.muscles, 0, FloatArray, 0, BasisNetworkSendBase.FirstBuffer);

            // Copy muscles [21..end]
            Array.Copy(NetworkSendBase.HumanPose.muscles, BasisNetworkSendBase.SecondBuffer, FloatArray, BasisNetworkSendBase.FirstBuffer, BasisNetworkSendBase.SizeAfterGap);

            int Offset = 0;
            //we write position first so we can use that on the server
            BasisUnityBitPackerExtensions.WriteVectorFloatToBytes(Anim.bodyPosition, ref NetworkSendBase.LASM.array, ref Offset);
            BasisUnityBitPackerExtensions.WriteQuaternionToBytes(Anim.bodyRotation, ref NetworkSendBase.LASM.array, ref Offset, BasisNetworkSendBase.RotationCompression);
            BasisUnityBitPackerExtensions.WriteMusclesToBytes(FloatArray, ref NetworkSendBase.LASM.array, ref Offset);

            UnityEngine.Vector3 Scale = Anim.transform.localScale;
            //we decode localscale last so we can optimize it.
            if (Scale == UnityEngine.Vector3.one)
            {
                //dont send anything as the remote can assume its one
            }
            else
            {
                if (Scale.x == Scale.y && Scale.y == Scale.z)
                {
                    //we write a single ushort
                    BasisUnityBitPackerExtensions.WriteUshortToBytes(Scale.x, BasisNetworkSendBase.ScaleRanged, ref NetworkSendBase.LASM.array, ref Offset);
                }
                else
                {
                    //we write 3 ushorts
                    BasisUnityBitPackerExtensions.WriteUshortVectorFloatToBytes(Scale, BasisNetworkSendBase.ScaleRanged, ref NetworkSendBase.LASM.array, ref Offset);
                }
            }
        }
    }
}