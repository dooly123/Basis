using DarkRift;
using DarkRift.Server.Plugins.Commands;
using UnityEngine;
public static class BasisNetworkAvatarCompressor
{
    public static void Compress(BasisNetworkSendBase NetworkSendBase, Animator Anim)
    {
        if(NetworkSendBase.Target.Vectors.IsCreated == false)
        {
            ///NetworkSendBase.InitalizeAvatarStoredData();
          //  NetworkSendBase.InitalizeDataJobs();
        }
        using (var writer = DarkRiftWriter.Create())
        {
            CompressAvatar(ref NetworkSendBase.Target, NetworkSendBase.HumanPose, NetworkSendBase.PoseHandler, Anim, out NetworkSendBase.LASM.array, NetworkSendBase.PositionRanged, NetworkSendBase.ScaleRanged);
            writer.Write(NetworkSendBase.LASM);
            BasisNetworkProfiler.AvatarUpdatePacket.Sample(writer.Length);
            using (var msg = Message.Create(BasisTags.AvatarMuscleUpdateTag, writer))
            {
                BasisNetworkConnector.Instance.Client.SendMessage(msg, SendMode.Unreliable);
            }
        }
    }
    public static void CompressAvatar(ref BasisAvatarData AvatarData, HumanPose CachedPose, HumanPoseHandler SenderPoseHandler, Animator Sender, out byte[] Bytes, BasisRangedFloatData PositionRanged, BasisRangedFloatData ScaleRanged)
    {
        SenderPoseHandler.GetHumanPose(ref CachedPose);
        AvatarData.Vectors[1] = CachedPose.bodyPosition;
        AvatarData.Vectors[0] = Sender.transform.position;
        AvatarData.Vectors[2] = Sender.transform.localScale;
        AvatarData.Muscles.CopyFrom(CachedPose.muscles);
        AvatarData.Quaternions[0] = CachedPose.bodyRotation;
        Bytes = CompressAvatarUpdate(AvatarData.Vectors[0], AvatarData.Vectors[2], AvatarData.Vectors[1], CachedPose.bodyRotation, CachedPose.muscles,PositionRanged,ScaleRanged);
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