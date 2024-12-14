using DarkRift;
using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct ServerSideSyncPlayerMessage
    {
        public PlayerIdMessage playerIdMessage;
        public byte interval;
        public LocalAvatarSyncMessage avatarSerialization;
        public void Deserialize(NetDataReader Writer)
        {
            Writer.Get(out interval);
            playerIdMessage.Deserialize(Writer);
            avatarSerialization.Deserialize(Writer);
        }

        public void Dispose()
        {
            playerIdMessage.Dispose();
            avatarSerialization.Dispose();
        }

        public void Serialize(NetDataWriter Writer)
        {
            Writer.Put(interval);
            playerIdMessage.Serialize(Writer);
            avatarSerialization.Serialize(Writer);
        }
    }
}