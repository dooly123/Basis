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
            if (Writer.AvailableBytes == 0)
            {
                LengthUsed = 0;
            }
            else
            {
                buffer = Writer.GetRemainingBytes();
                LengthUsed = buffer.Length;
               // BNL.Log("Get Length was " + LengthUsed);
            }
        }
        public void Serialize(NetDataWriter Writer)
        {
            if (LengthUsed != 0)
            {
                Writer.Put(buffer, 0, LengthUsed);
              //  BNL.Log("Put Length was " + LengthUsed);
            }
        }
    }
}
