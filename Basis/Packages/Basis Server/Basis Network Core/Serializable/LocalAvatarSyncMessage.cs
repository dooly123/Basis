
using DarkRift;
using LiteNetLib.Utils;
using System;
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
