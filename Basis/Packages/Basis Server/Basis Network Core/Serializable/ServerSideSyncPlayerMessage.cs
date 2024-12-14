using DarkRift;
using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct ServerSideSyncPlayerMessage
    {
        public PlayerIdMessage playerIdMessage;
        public LocalAvatarSyncMessage avatarSerialization;
        public byte interval;
        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
            avatarSerialization.Deserialize(Writer);
            Writer.Get(out interval);
        }

        public void Dispose()
        {
            playerIdMessage.Dispose();
            avatarSerialization.Dispose();
        }

        public void Serialize(NetDataWriter Writer)
        {
            playerIdMessage.Serialize(Writer);
            avatarSerialization.Serialize(Writer);
            Writer.Put(interval);
        }
    }
}