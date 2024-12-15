using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    [System.Serializable]
    public struct AudioSegmentDataMessage
    {
        public byte[] buffer;
        public int LengthUsed;
        public void Deserialize(NetDataReader Writer)
        {
            if (Writer.EndOfData)
            {
                LengthUsed = 0;
            }
            else
            {
                buffer = Writer.GetRemainingBytes();
                LengthUsed = buffer.Length;
            }
        }
        public void Dispose()
        {
        }

        public void Serialize(NetDataWriter Writer)
        {
            if (LengthUsed != 0)
            {
                Writer.Put(buffer, 0, LengthUsed);
            }
        }
    }
}