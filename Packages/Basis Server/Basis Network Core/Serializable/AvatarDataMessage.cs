using DarkRift;
using LiteNetLib.Utils;
using System.IO;
public static partial class SerializableBasis
{
    public struct AvatarDataMessage
    {
        public PlayerIdMessage playerIdMessage;
        public byte messageIndex;

        public uint payloadSize;
        public ushort recipientsSize;


        public byte[] payload;
        public ushort[] recipients;
        public void Deserialize(NetDataReader Writer)
        {
            try
            {
                // Read the playerIdMessage and messageIndex first
                playerIdMessage.Deserialize(Writer);
                Writer.Get(out messageIndex);
                Writer.Get(out recipientsSize);
                Writer.Get(out payloadSize);

                // Validate if the reader has enough data to read the expected sizes
                if (Writer.AvailableBytes < recipientsSize * sizeof(ushort) + payloadSize * sizeof(byte))
                {
                    throw new EndOfStreamException("Insufficient data in stream for recipients and payload.");
                }

                // Allocate the arrays
                recipients = new ushort[recipientsSize];
                payload = new byte[payloadSize];

                // Read the recipients
                for (int index = 0; index < recipientsSize; index++)
                {
                    Writer.Get(out recipients[index]);
                }

                // Read the payload
                for (int index = 0; index < payloadSize; index++)
                {
                    Writer.Get(out payload[index]);
                }
            }
            catch (EndOfStreamException ex)
            {
                // Log or handle stream read error
                throw new EndOfStreamException("Error deserializing AvatarDataMessage: " + ex.Message, ex);
            }
        }

        public void Dispose()
        {

            playerIdMessage.Dispose();
        }

        public void Serialize(NetDataWriter Writer)
        {
            // Write the playerIdMessage and messageIndex first
            playerIdMessage.Serialize(Writer);
             Writer.Put(messageIndex);

            // Prepare sizes
            recipientsSize = (ushort)(recipients?.Length ?? 0);
            payloadSize = (uint)(payload?.Length ?? 0);

            // Write sizes
             Writer.Put(recipientsSize);
             Writer.Put(payloadSize);

            // Write the recipients
            if (recipients != null)
            {
                for (int index = 0; index < recipientsSize; index++)
                {
                     Writer.Put(recipients[index]);
                }
            }

            // Write the payload
            if (payload != null)
            {
                for (int index = 0; index < payloadSize; index++)
                {
                     Writer.Put(payload[index]);
                }
            }
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
    public struct AvatarDataMessage_NoRecipients
    {
        public PlayerIdMessage playerIdMessage;
        public byte messageIndex;
        public byte[] payload;

        public void Deserialize(NetDataReader Writer)
        {
            // Read the assignedAvatarPlayer, messageIndex, and payload
            playerIdMessage.Deserialize(Writer);
            Writer.Get(out messageIndex);
            payload = Writer.GetRemainingBytes();
        }

        public void Serialize(NetDataWriter Writer)
        {
            // Write the assignedAvatarPlayer, messageIndex, and payload
            playerIdMessage.Serialize(Writer);
             Writer.Put(messageIndex);
             Writer.Put(payload);
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
        }
    }

    public struct ServerAvatarDataMessage_NoRecipients
    {
        public PlayerIdMessage playerIdMessage;
        public AvatarDataMessage_NoRecipients avatarDataMessage;

        public void Deserialize(NetDataReader Writer)
        {
            // Read the playerIdMessage and avatarDataMessage
            playerIdMessage.Deserialize(Writer);
            avatarDataMessage.Deserialize(Writer);
        }

        public void Serialize(NetDataWriter Writer)
        {
            // Write the playerIdMessage and avatarDataMessage
            playerIdMessage.Serialize(Writer);
            avatarDataMessage.Serialize(Writer);
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
            avatarDataMessage.Dispose();
        }

    }
    public struct AvatarDataMessage_NoRecipients_NoPayload
    {
        public PlayerIdMessage playerIdMessage;
        public byte messageIndex;

        public void Deserialize(NetDataReader Writer)
        {
            // Read the assignedAvatarPlayer and messageIndex only
            playerIdMessage.Deserialize(Writer);
            Writer.Get(out messageIndex);
        }
        public void Serialize(NetDataWriter Writer)
        {
            // Write the assignedAvatarPlayer and messageIndex
            playerIdMessage.Serialize(Writer);
             Writer.Put(messageIndex);
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
        }

    }

    public struct ServerAvatarDataMessage_NoRecipients_NoPayload
    {
        public PlayerIdMessage playerIdMessage;
        public AvatarDataMessage_NoRecipients_NoPayload avatarDataMessage;

        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
            avatarDataMessage.Deserialize(Writer);
        }

        public void Serialize(NetDataWriter Writer)
        {
            // Write the playerIdMessage and avatarDataMessage
             playerIdMessage.Serialize(Writer);
             avatarDataMessage.Serialize(Writer);
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
            avatarDataMessage.Dispose();
        }

    }
    public struct SceneDataMessage_Recipients_NoPayload
    {
        public PlayerIdMessage playerIdMessage;
        public byte messageIndex;
        public ushort recipientsSize;
        public ushort[] recipients;
        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
            Writer.Get(out messageIndex);
            Writer.Get(out recipientsSize);
            recipients = new ushort[recipientsSize];
            for (int index = 0; index < recipientsSize; index++)
            {
                Writer.Get(out recipients[index]);
            }
        }

        public void Serialize(NetDataWriter Writer)
        {
            playerIdMessage.Serialize(Writer);
            Writer.Put(messageIndex);

            recipientsSize = (ushort)recipients.Length;
            Writer.Put(recipientsSize);

            for (int index = 0; index < recipientsSize; index++)
            {
                ushort recipient = recipients[index];
                Writer.Put(recipient);
            }
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
        }

    }
    public struct ServerSceneDataMessage_Recipients_NoPayload
    {
        public PlayerIdMessage playerIdMessage;
        public SceneDataMessage_Recipients_NoPayload sceneDataMessage;

        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
            sceneDataMessage.Deserialize(Writer);
        }

        public void Serialize(NetDataWriter Writer)
        {
            playerIdMessage.Serialize(Writer);
            sceneDataMessage.Serialize(Writer);
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
            sceneDataMessage.Dispose();
        }

    }

    public struct AvatarDataMessage_Recipients_NoPayload
    {
        public PlayerIdMessage playerIdMessage;
        public byte messageIndex;
        public ushort recipientsSize;
        public ushort[] recipients;
        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
            Writer.Get(out messageIndex);

            Writer.Get(out recipientsSize);

            recipients = new ushort[recipientsSize];

            for (int index = 0; index < recipients.Length; index++)
            {
                Writer.Get(out recipients[index]);
            }
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
        }


        public void Serialize(NetDataWriter Writer)
        {
            playerIdMessage.Serialize(Writer);
             Writer.Put(messageIndex);

            recipientsSize = (ushort)recipients.Length;

             Writer.Put(recipientsSize);

            for (int index = 0; index < recipients.Length; index++)
            {
                 Writer.Put(recipients[index]);
            }
        }
    }

    public struct ServerAvatarDataMessage_Recipients_NoPayload
    {
        public PlayerIdMessage playerIdMessage;
        public AvatarDataMessage_Recipients_NoPayload avatarDataMessage;
        public void Dispose()
        {
            playerIdMessage.Dispose();
            avatarDataMessage.Dispose();
        }

        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
            avatarDataMessage.Deserialize(Writer);
        }

        public void Serialize(NetDataWriter Writer)
        {
            playerIdMessage.Serialize(Writer);
            avatarDataMessage.Serialize(Writer);
        }
    }
}
