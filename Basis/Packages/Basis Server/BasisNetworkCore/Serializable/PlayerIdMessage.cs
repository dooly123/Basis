using DarkRift;
using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct PlayerIdMessage
    {
        public ushort playerID;
        public void Deserialize(NetDataReader Writer)
        {
            Writer.Get(out playerID);
        }

        public void Dispose()
        {
        }

        public void Serialize(NetDataWriter Writer)
        {
            Writer.Put(playerID);
        }
    }
}
