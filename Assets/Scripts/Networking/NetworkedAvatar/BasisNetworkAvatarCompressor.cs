using DarkRift;
using DarkRift.Server.Plugins.Commands;
using UnityEngine;
public static class BasisNetworkAvatarCompressor
{
    public static void Compress(BasisNetworkSendBase NetworkSendBase, Animator Anim)
    {
        using (var writer = DarkRiftWriter.Create())
        {
            CompressAvatar(ref NetworkSendBase.Target, NetworkSendBase.HumanPose, NetworkSendBase.PoseHandler, Anim, out NetworkSendBase.LASM.array, NetworkSendBase.PositionRanged, NetworkSendBase.ScaleRanged);
            writer.Write(NetworkSendBase.LASM);
            BasisNetworkProfiler.AvatarUpdatePacket.Sample(writer.Length);
            using (var msg = Message.Create(BasisTags.PlayerUpdateTag, writer))
            {
                BasisNetworkConnector.Instance.Client.SendMessage(msg, SendMode.Unreliable);
            }
        }
    }
    public static void CompressAvatar(ref BasisAvatarData AvatarData, HumanPose CachedPose, HumanPoseHandler SenderPoseHandler, Animator Sender, out byte[] Bytes, BasisRangedFloatData PositionRanged, BasisRangedFloatData ScaleRanged)
    {
        SenderPoseHandler.GetHumanPose(ref CachedPose);
        AvatarData.BodyPosition = CachedPose.bodyPosition;
        AvatarData.PlayerPosition = Sender.transform.position;
        AvatarData.Scale = Sender.transform.localScale;
        AvatarData.Muscles = CachedPose.muscles;
        AvatarData.Rotation = CachedPose.bodyRotation;
        Bytes = CompressAvatarUpdate(AvatarData.PlayerPosition, AvatarData.Scale, AvatarData.BodyPosition, CachedPose.bodyRotation, CachedPose.muscles,PositionRanged,ScaleRanged);
    }
    public static byte[] CompressAvatarUpdate(Vector3 NewPosition, Vector3 Scale, Vector3 BodyPosition, Quaternion Rotation, float[] muscles, BasisRangedFloatData PositionRanged, BasisRangedFloatData ScaleRanged)
    {
        using (var Packer = DarkRiftWriter.Create())
        {
            CompressScaleAndPosition(Packer, NewPosition, BodyPosition, Scale, PositionRanged,ScaleRanged);

            BasisCompressionOfRotation.CompressQuaternion(Packer, Rotation);
            BasisCompressionOfMuscles.CompressMuscles(Packer, muscles);
            return Packer.ToArray();
        }
    }
    public static void CompressScaleAndPosition(DarkRiftWriter packer, Vector3 position, Vector3 bodyPosition, Vector3 scale, BasisRangedFloatData PositionRanged, BasisRangedFloatData ScaleRanged)
    {
        BasisCompressionOfPosition.CompressVector3(position, packer, PositionRanged);
        BasisCompressionOfPosition.CompressVector3(bodyPosition, packer, PositionRanged);

        BasisCompressionOfPosition.CompressVector3(scale, packer, ScaleRanged);
    }
}