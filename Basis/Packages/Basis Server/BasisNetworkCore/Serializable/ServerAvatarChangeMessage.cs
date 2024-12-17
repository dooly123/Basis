using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct ServerAvatarChangeMessage
    {
        public PlayerIdMessage uShortPlayerId;
        public ClientAvatarChangeMessage clientAvatarChangeMessage;
        public void Deserialize(NetDataReader Writer)
        {
            uShortPlayerId.Deserialize(Writer);
            clientAvatarChangeMessage.Deserialize(Writer);
        }

        public void Dispose()
        {
            uShortPlayerId.Dispose();
            clientAvatarChangeMessage.Dispose();
        }

        public void Serialize(NetDataWriter Writer)
        {
            uShortPlayerId.Serialize(Writer);
            clientAvatarChangeMessage.Serialize(Writer);
        }
    }
    public struct ClientAvatarChangeMessage
    {
        // Downloading - attempts to download from a URL, make sure a hash also exists.
        // BuiltIn - loads as an addressable in Unity.
        public byte loadMode;
        public ushort byteLength;
        public byte[] byteArray;

        public void Deserialize(NetDataReader Writer)
        {
            // Read the load mode
            loadMode = Writer.GetByte();

            // Read the byte length
            byteLength = Writer.GetUShort();

            // Initialize the byte array with the specified length
            byteArray = new byte[byteLength];

            // Read each byte manually into the array
            Writer.GetBytes(byteArray, byteLength);
        }

        public void Dispose()
        {
        }

        public void Serialize(NetDataWriter Writer)
        {
            // Write the load mode
            Writer.Put(loadMode);

            // Update and write the byte length
            byteLength = (ushort)byteArray.Length;
            Writer.Put(byteLength);
            // Write each byte manually from the array
            for (int index = 0; index < byteLength; index++)
            {
                Writer.Put(byteArray[index]);
            }
        }
    }
}