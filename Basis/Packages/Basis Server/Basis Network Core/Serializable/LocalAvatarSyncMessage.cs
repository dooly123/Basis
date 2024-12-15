using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct LocalAvatarSyncMessage
    {
        public byte[] array;
        public void Deserialize(NetDataReader Writer)
        {
            array = Writer.GetRemainingBytes();
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
