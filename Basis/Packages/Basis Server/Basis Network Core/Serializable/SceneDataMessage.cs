
using System;
using DarkRift;
using LiteNetLib.Utils;

public static partial class SerializableBasis
{
    public struct SceneDataMessage
    {
        public ushort messageIndex;

        public uint payloadSize;
        public ushort recipientsSize;

        public byte[] payload;

        /// <summary>
        /// If null, it's for everyone. Otherwise, send only to the listed entries.
        /// </summary>
        public ushort[] recipients;

        public void Deserialize(NetDataReader Writer)
        {
            // Read messageIndex
            Writer.Get(out messageIndex);

            Writer.Get(out recipientsSize);
            Writer.Get(out payloadSize);

            recipients = new ushort[recipientsSize];
            payload = new byte[payloadSize];

            for (int index = 0; index < recipientsSize; index++)
            {
                Writer.Get(out recipients[index]);
            }
            for (int index = 0; index < payloadSize; index++)
            {
                Writer.Get(out payload[index]);
            }
        }

        public void Dispose()
        {
        }

        public void Serialize(NetDataWriter Writer)
        {
            // Write the messageIndex and buffer
             Writer.Put(messageIndex);

            recipientsSize = (ushort)recipients.Length;
            payloadSize = (uint)payload.Length;

             Writer.Put(recipientsSize);
             Writer.Put(payloadSize);

            for (int index = 0; index < recipientsSize; index++)
            {
                 Writer.Put(recipients[index]);
            }
            for (int index = 0; index < payloadSize; index++)
            {
                 Writer.Put(payload[index]);
            }
        }
    }

    public struct ServerSceneDataMessage
    {
        public PlayerIdMessage playerIdMessage;
        public SceneDataMessage sceneDataMessage;

        public void Deserialize(NetDataReader Writer)
        {
            // Read the playerIdMessage
            playerIdMessage.Deserialize(Writer);
            sceneDataMessage.Deserialize(Writer);
        }

        public void Dispose()
        {
            playerIdMessage.Dispose();
            sceneDataMessage.Dispose();
        }

        public void Serialize(NetDataWriter Writer)
        {
            // Write the playerIdMessage and sceneDataMessage
            playerIdMessage.Serialize(Writer);
            sceneDataMessage.Serialize(Writer);
        }
    }
    public struct SceneDataMessage_NoRecipients
    {
        public ushort messageIndex;
        public byte[] payload;

        public void Deserialize(NetDataReader Writer)
        {
            // Read messageIndex and payload
            Writer.Get(out messageIndex);
            payload = Writer.GetRemainingBytes();
        }

        public void Dispose()
        {
        }

        public void Serialize(NetDataWriter Writer)
        {
            // Write the messageIndex and payload
             Writer.Put(messageIndex);
             Writer.Put(payload);
        }
    }

    public struct ServerSceneDataMessage_NoRecipients
    {
        public PlayerIdMessage playerIdMessage;
        public SceneDataMessage_NoRecipients sceneDataMessage;

        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
            sceneDataMessage.Deserialize(Writer);
        }

        public void Dispose()
        {
            playerIdMessage.Dispose();
            sceneDataMessage.Dispose();
        }

        public void Serialize(NetDataWriter Writer)
        {
            // Write the playerIdMessage and sceneDataMessage
            playerIdMessage.Serialize(Writer);
            sceneDataMessage.Serialize(Writer);
        }
    }
    public struct SceneDataMessage_NoRecipients_NoPayload
    {
        public ushort messageIndex;

        public void Deserialize(NetDataReader Writer)
        {
            // Read only messageIndex
            Writer.Get(out messageIndex);
        }

        public void Dispose()
        {
        }

        public void Serialize(NetDataWriter Writer)
        {
            // Write the messageIndex
             Writer.Put(messageIndex);
        }
    }

    public struct ServerSceneDataMessage_NoRecipients_NoPayload
    {
        public PlayerIdMessage playerIdMessage;
        public SceneDataMessage_NoRecipients_NoPayload sceneDataMessage;

        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
            sceneDataMessage.Deserialize(Writer);
        }

        public void Dispose()
        {
            playerIdMessage.Dispose();
            sceneDataMessage.Dispose();
        }

        public void Serialize(NetDataWriter Writer)
        {
            playerIdMessage.Serialize(Writer);
            sceneDataMessage.Serialize(Writer);
        }
    }
}
