using LiteNetLib.Utils;
using System;
public static partial class SerializableBasis
{
    public struct RemoteAvatarDataMessage
    {
        public PlayerIdMessage PlayerIdMessage;
        public byte messageIndex;
        public byte[] payload;

        public void Deserialize(NetDataReader Writer)
        {
            PlayerIdMessage.Deserialize(Writer);
            // Read the messageIndex safely
            if (!Writer.TryGetByte(out messageIndex))
            {
                throw new ArgumentException("Failed to read messageIndex.");
            }
            if (Writer.AvailableBytes > 0)
            {
                payload = Writer.GetRemainingBytes();
            }
        }

        public void Serialize(NetDataWriter Writer)
        {
            PlayerIdMessage.Serialize(Writer);
            // Write the messageIndex
            Writer.Put(messageIndex);
            // Write the payload if present
            if (payload != null && payload.Length > 0)
            {
                Writer.Put(payload);
            }
        }
    }
}
