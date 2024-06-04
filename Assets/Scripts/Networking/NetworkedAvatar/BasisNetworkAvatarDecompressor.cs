using DarkRift;
using UnityEngine;
using static SerializableDarkRift;
public static class BasisNetworkAvatarDecompressor
{
    public static void DeCompress(BasisNetworkSendBase Base, ServerSideSyncPlayerMessage ServerSideSyncPlayerMessage)
    {
        Base.LASM = ServerSideSyncPlayerMessage.avatarSerialization;
        DecompressAvatar(ref Base.Target, Base.LASM.array, Base.PositionRanged, Base.ScaleRanged);
    }
    public static void DecompressAvatar(ref BasisAvatarData AvatarData, byte[] AvatarUpdate, BasisRangedFloatData PositionRanged, BasisRangedFloatData ScaleRanged)
    {
        DecompressAvatarUpdate(AvatarUpdate, out Vector3 PlayerPosition, out Vector3 Scale, out Vector3 BodyPosition, out Quaternion Rotation, ref AvatarData.Muscles, PositionRanged, ScaleRanged);
        AvatarData.BodyPosition = BodyPosition;
        AvatarData.PlayerPosition = PlayerPosition;
        AvatarData.Scale = Scale;
        AvatarData.Rotation = Rotation;
    }
    public static void DecompressAvatarUpdate(byte[] compressedData, out Vector3 NewPosition, out Vector3 Scale, out Vector3 BodyPosition, out Quaternion Rotation, ref float[] muscles, BasisRangedFloatData PositionRanged, BasisRangedFloatData ScaleRanged)
    {
        using (var bitPacker = DarkRiftReader.CreateFromArray(compressedData, 0, compressedData.Length))
        {
            DecompressScaleAndPosition(bitPacker, out NewPosition, out BodyPosition, out Scale, PositionRanged, ScaleRanged);
            BasisCompressionOfRotation.DecompressQuaternion(bitPacker, out Rotation);
            BasisCompressionOfMuscles.DecompressMuscles(bitPacker, ref muscles);
        }
    }
    public static void DecompressScaleAndPosition(DarkRiftReader Packer, out Vector3 Position, out Vector3 BodyPosition, out Vector3 Scale, BasisRangedFloatData PositionRanged, BasisRangedFloatData ScaleRanged)
    {
        Position = BasisCompressionOfPosition.DecompressVector3(Packer, PositionRanged);
        BodyPosition = BasisCompressionOfPosition.DecompressVector3(Packer, PositionRanged);

        Scale = BasisCompressionOfPosition.DecompressVector3(Packer, ScaleRanged);
    }
}