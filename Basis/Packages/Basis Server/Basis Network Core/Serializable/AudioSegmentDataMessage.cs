using LiteNetLib.Utils;
using System;
public static partial class SerializableBasis
{
    [System.Serializable]
    public struct AudioSegmentDataMessage
    {
        public byte[] buffer;
        public int LengthUsed;
        public void Deserialize(NetDataReader Writer)
        {
            buffer = Writer.GetRemainingBytes();
            LengthUsed = buffer.Length;
        }
        public void Dispose()
        {
        }

        public void Serialize(NetDataWriter Writer)
        {
            Writer.Put(buffer, 0, LengthUsed);
        }
    }
}