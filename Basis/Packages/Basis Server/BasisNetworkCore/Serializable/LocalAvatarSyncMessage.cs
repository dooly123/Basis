using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct LocalAvatarSyncMessage
    {
        public byte[] array;
        public void Deserialize(NetDataReader Writer)
        {
            int Bytes = Writer.AvailableBytes;
            if (Bytes >= 386)
            {
                if (array == null)
                {
                    array = new byte[386];
                }
                Writer.GetBytes(array, 386);//360 for muscles, 3*4 for position 12, 4*4 for rotation 16-2
            }
            else
            {
                BNL.LogError($"Unable to read Remaing bytes where {Bytes}");
            }
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
