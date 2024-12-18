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
        public byte[] byteArray;

        public void Deserialize(NetDataReader Writer)
        {
            // Read the load mode
            loadMode = Writer.GetByte();
            // Initialize the byte array with the specified length
            byteArray = new byte[Writer.GetUShort()];

            // Read each byte manually into the array
            Writer.GetBytes(byteArray, 0, byteArray.Length);
        }

        public void Dispose()
        {
        }

        public void Serialize(NetDataWriter Writer)
        {
            // Write the load mode
            Writer.Put(loadMode);
            Writer.Put((ushort)byteArray.Length);
            Writer.Put(byteArray);
        }
    }
}