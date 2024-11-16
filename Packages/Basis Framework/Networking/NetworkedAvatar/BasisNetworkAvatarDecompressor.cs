using Basis.Scripts.Networking.Compression;
using DarkRift;
using Unity.Mathematics;
using UnityEngine;
using static SerializableDarkRift;

namespace Basis.Scripts.Networking.NetworkedAvatar
{
    public static class BasisNetworkAvatarDecompressor
    {
        public static void DeCompress(BasisNetworkSendBase Base, ServerSideSyncPlayerMessage ServerSideSyncPlayerMessage)
        {
            Base.LASM = ServerSideSyncPlayerMessage.avatarSerialization;
            DecompressAvatar(Base.CompressionArraysRangedUshort, ref Base.Target, Base.LASM.array, Base.PositionRanged, Base.ScaleRanged);
        }
        public static void DeCompress(BasisNetworkSendBase Base, LocalAvatarSyncMessage ServerSideSyncPlayerMessage)
        {
            Base.LASM = ServerSideSyncPlayerMessage;
            DecompressAvatar(Base.CompressionArraysRangedUshort,ref Base.Target, Base.LASM.array, Base.PositionRanged, Base.ScaleRanged);
        }
        public static void DecompressAvatar(CompressionArraysRangedUshort CompressionArraysRangedUshort, ref BasisAvatarData AvatarData, byte[] AvatarUpdate, BasisRangedUshortFloatData PositionRanged, BasisRangedUshortFloatData ScaleRanged)
        {
            DecompressAvatarUpdate(CompressionArraysRangedUshort, AvatarUpdate, out Vector3 Scale, out Vector3 BodyPosition, ref AvatarData.Rotation, ref AvatarData, PositionRanged, ScaleRanged);
            AvatarData.Vectors[1] = BodyPosition;
            AvatarData.Vectors[0] = Scale;
        }
        public static void DecompressAvatarUpdate(CompressionArraysRangedUshort CompressionArraysRangedUshort, byte[] compressedData, out Vector3 Scale, out Vector3 BodyPosition, ref Quaternion Rotation, ref BasisAvatarData BasisAvatarData, BasisRangedUshortFloatData PositionRanged, BasisRangedUshortFloatData ScaleRanged)
        {
            if (compressedData != null && compressedData.Length != 0)
            {
                using (var bitPacker = DarkRiftReader.CreateFromArray(compressedData, 0, compressedData.Length))
                {
                    DecompressScaleAndPosition(bitPacker, out BodyPosition, out Scale, PositionRanged, ScaleRanged);
                    BasisCompressionOfRotation.DecompressQuaternion(bitPacker, ref Rotation);
                    BasisCompressionOfMuscles.DecompressMuscles(bitPacker, ref BasisAvatarData, CompressionArraysRangedUshort);
                }
            }
            else
            {
                Debug.LogError("Array was null or empty!");
                Scale = new Vector3();
                BodyPosition = new Vector3();
                Rotation = new Quaternion();
            }
        }
        public static void DecompressScaleAndPosition(DarkRiftReader Packer, out Vector3 BodyPosition, out Vector3 Scale, BasisRangedUshortFloatData PositionRanged, BasisRangedUshortFloatData ScaleRanged)
        {
            BodyPosition = BasisCompressionOfPosition.DecompressVector3(Packer);

            Scale = BasisCompressionOfPosition.DecompressUShortVector3(Packer, ScaleRanged);
        }
    }
}