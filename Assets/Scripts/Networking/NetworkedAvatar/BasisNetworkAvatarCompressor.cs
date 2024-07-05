using DarkRift;
using DarkRift.Server.Plugins.Commands;
using UnityEngine;
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
                BasisNetworkConnector.Instance.Client.SendMessage(msg, DeliveryMethod.Unreliable);
            }
        }
    }
    public static void CompressIntoSendBase(BasisNetworkSendBase NetworkSendBase, Animator Anim)
    {
        CompressAvatar(ref NetworkSendBase.Target, NetworkSendBase.HumanPose, NetworkSendBase.PoseHandler, Anim, out NetworkSendBase.LASM.array, NetworkSendBase.PositionRanged, NetworkSendBase.ScaleRanged);
    }
    public static void CompressAvatar(ref BasisAvatarData AvatarData, HumanPose CachedPose, HumanPoseHandler SenderPoseHandler, Animator Sender, out byte[] Bytes, BasisRangedUshortFloatData PositionRanged, BasisRangedUshortFloatData ScaleRanged)
    {
        SenderPoseHandler.GetHumanPose(ref CachedPose);
        AvatarData.Vectors[1] = CachedPose.bodyPosition;
        AvatarData.Vectors[0] = Sender.transform.position;
        AvatarData.Vectors[2] = Sender.transform.localScale;
        AvatarData.Muscles.CopyFrom(CachedPose.muscles);
        AvatarData.Quaternions[0] = CachedPose.bodyRotation;
        Bytes = CompressAvatarUpdate(AvatarData.Vectors[0], AvatarData.Vectors[2], AvatarData.Vectors[1], CachedPose.bodyRotation, CachedPose.muscles, PositionRanged, ScaleRanged);
    }
    public static byte[] CompressAvatarUpdate(Vector3 NewPosition, Vector3 Scale, Vector3 BodyPosition, Quaternion Rotation, float[] muscles, BasisRangedUshortFloatData PositionRanged, BasisRangedUshortFloatData ScaleRanged)
    {
        using (var Packer = DarkRiftWriter.Create(216))
        {
            CompressScaleAndPosition(Packer, NewPosition, BodyPosition, Scale, PositionRanged, ScaleRanged);//3 ushorts atm needs to be uints (3*4)*3

            BasisCompressionOfRotation.CompressQuaternion(Packer, Rotation);//uint
            BasisCompressionOfMuscles.CompressMuscles(Packer, muscles);//95 ushorts 95*4
            return Packer.ToArray();
        }
    }
    public static void CompressScaleAndPosition(DarkRiftWriter packer, Vector3 position, Vector3 bodyPosition, Vector3 scale, BasisRangedUshortFloatData PositionRanged, BasisRangedUshortFloatData ScaleRanged)
    {
        BasisCompressionOfPosition.CompressVector3(position, packer, PositionRanged);
        BasisCompressionOfPosition.CompressVector3(bodyPosition, packer, PositionRanged);

        BasisCompressionOfPosition.CompressUShortVector3(scale, packer, ScaleRanged);
    }
}