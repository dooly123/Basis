using DarkRift;
using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct AudioSegmentMessage
    {
        public PlayerIdMessage playerIdMessage;

        public AudioSilentSegmentDataMessage silentData;

        public AudioSegmentDataMessage audioSegmentData;
        /// <summary>
        /// the goal here is to reuse this message but drop the AudioSegmentData when its not.
        /// this forces the queue to remain correct.
        /// </summary>
        public bool wasSilentData;
        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
            if (Writer.EndOfData)
            {
                wasSilentData = true;
                silentData.Deserialize(Writer);
            }
            else
            {
                wasSilentData = false;
                audioSegmentData.Deserialize(Writer);
            }
        }

        public void Dispose()
        {
            playerIdMessage.Dispose();
            silentData.Dispose();
            audioSegmentData.Dispose();
        }

        public void Serialize(NetDataWriter Writer)
        {
            playerIdMessage.Serialize(Writer);
            if (wasSilentData)
            {
                silentData.Serialize(Writer);
            }
            else
            {
                audioSegmentData.Serialize(Writer);
            }
        }
    }

    public struct VoiceReceiversMessage
    {
        public ushort[] users;

        public void Deserialize(NetDataReader Writer)
        {
            // Calculate the number of ushorts based on the remaining bytes
            int remainingBytes = Writer.AvailableBytes;
            int ushortCount = remainingBytes / sizeof(ushort);

            // Initialize the array with the calculated size
            users = new ushort[ushortCount];

            // Read each ushort value into the array
            for (int index = 0; index < ushortCount; index++)
            {
                users[index] = Writer.GetUShort();
            }
        }

        public void Dispose()
        {
        }

        public void Serialize(NetDataWriter Writer)
        {
            foreach (ushort v in users)
            {
                 Writer.Put(v);
            }
        }
    }
}
