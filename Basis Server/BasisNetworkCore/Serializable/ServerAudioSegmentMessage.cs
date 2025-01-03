using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct ServerAudioSegmentMessage
    {
        public PlayerIdMessage playerIdMessage;
        public AudioSegmentDataMessage audioSegmentData;
        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
            audioSegmentData.Deserialize(Writer);
        }
        public void Serialize(NetDataWriter Writer)
        {
            playerIdMessage.Serialize(Writer);
            audioSegmentData.Serialize(Writer);
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
        public void Serialize(NetDataWriter Writer)
        {
            int Count = users.Length;
            for (int Index = 0; Index < Count; Index++)
            {
                Writer.Put(users[Index]);
            }
        }
    }
}
