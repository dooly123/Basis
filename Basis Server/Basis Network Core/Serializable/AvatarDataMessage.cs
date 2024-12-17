using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct AvatarDataMessage
    {
        public PlayerIdMessage PlayerIdMessage;
        public byte messageIndex;
        public ushort recipientsSize;
        /// <summary>
        /// If null, it's for everyone. Otherwise, send only to the listed entries.
        /// </summary>
        public ushort[] recipients;
        public byte[] payload;

        public void Deserialize(NetDataReader Writer)
        {
            PlayerIdMessage = new PlayerIdMessage();
            PlayerIdMessage.Deserialize(Writer);
            Writer.Get(out messageIndex);
            if (Writer.TryGetUShort(out recipientsSize))
            {
                recipients = new ushort[recipientsSize];
                for (int index = 0; index < recipientsSize; index++)
                {
                    Writer.Get(out recipients[index]);
                }
                if (Writer.AvailableBytes != 0)
                {
                    payload = Writer.GetRemainingBytes();
                }
            }
        }
        public void Serialize(NetDataWriter Writer)
        {
            PlayerIdMessage.Serialize(Writer);
            // Write the messageIndex and buffer
            Writer.Put(messageIndex);

            if (recipients == null || recipients.Length == 0 && (payload == null || payload.Length == 0))
            {
                //this is the end of the message! its just a simple RPC
            }
            else
            {
                if (recipients == null)//no recipients but we have data so set the size to zero
                {
                    recipientsSize = 0;
                }
                else
                {
                    recipientsSize = (ushort)recipients.Length;
                }
                Writer.Put(recipientsSize);
                Writer.PutArray(recipients);
                if (payload != null && payload.Length != 0)
                {
                    Writer.Put(payload);
                }
            }
        }
        public void Dispose()
        {
        }
    }
    public struct ServerAvatarDataMessage
    {
        public PlayerIdMessage playerIdMessage;
        public AvatarDataMessage avatarDataMessage;
        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
            avatarDataMessage.Deserialize(Writer);
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
            avatarDataMessage.Dispose();
        }

        public void Serialize(NetDataWriter Writer)
        {
            playerIdMessage.Serialize(Writer);
            avatarDataMessage.Serialize(Writer);
        }
    }
}