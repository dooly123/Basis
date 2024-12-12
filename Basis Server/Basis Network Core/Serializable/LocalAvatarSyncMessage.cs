
using DarkRift;
using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct LocalAvatarSyncMessage
    {
        public byte[] array;
        public int size;
        public void Deserialize(NetDataReader Writer)
        {
            Writer.GetBytes(array,size);
        }

        public void Dispose()
        {
        }

        public void Serialize(NetDataWriter Writer)
        {
             Writer.Put(array);
        }
    }
}
