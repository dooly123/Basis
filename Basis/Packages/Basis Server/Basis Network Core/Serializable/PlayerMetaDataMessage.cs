
using DarkRift;
using LiteNetLib.Utils;

public static partial class SerializableBasis
{
    public struct PlayerMetaDataMessage
    {
        public string playerUUID;
        public string playerDisplayName;
        public void Deserialize(NetDataReader Writer)
        {
            Writer.Get(out playerUUID);
            Writer.Get(out playerDisplayName);
        }

        public void Dispose()
        {
        }

        public void Serialize(NetDataWriter Writer)
        {
             Writer.Put(playerUUID);
             Writer.Put(playerDisplayName);
        }
    }
}
