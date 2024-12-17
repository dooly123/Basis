using LiteNetLib.Utils;
namespace Basis.Network.Core.Serializable
{
    public static partial class SerializableBasis
    {
        [System.Serializable]
        public struct AuthenticationMessage
        {
            public ushort MessageLength;
            public byte[] Message;
            public void Deserialize(NetDataReader Writer)
            {
                if (Writer.TryGetUShort(out MessageLength))
                {
                    Message = new byte[MessageLength];
                    Writer.GetBytes(Message, MessageLength);
                }
                else
                {
                    BNL.LogError("missing Message Length!");
                }
            }
            public void Dispose()
            {
            }
            public void Serialize(NetDataWriter Writer)
            {
                Writer.Put(MessageLength);
                Writer.Put(Message);
            }
        }
    }
}