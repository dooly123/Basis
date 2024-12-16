using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct LocalAvatarSyncMessage
    {
        public byte[] array;
        public void Deserialize(NetDataReader Writer)
        {
            Writer.GetBytes(array, 386);//360 for muscles, 3*4 for position 12, 4*4 for rotation 16-2
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
