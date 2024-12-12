using LiteNetLib.Utils;
using System;
public static partial class SerializableBasis
{
    public struct AudioSegmentDataMessage
    {
        public ArraySegment<byte> buffer;
        public int size;
        public void Deserialize(NetDataReader Writer)
        {
            buffer = Writer.GetRemainingBytesSegment();
            size = buffer.Count;
        }
        public void Dispose()
        {
        }

        public void Serialize(NetDataWriter Writer)
        {
            Writer.PutArray(buffer.Array, size);
        }
    }
}
