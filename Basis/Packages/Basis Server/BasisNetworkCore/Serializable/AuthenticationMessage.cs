using LiteNetLib.Utils;
namespace Basis.Network.Core.Serializable
{
    public static partial class SerializableBasis
    {
        [System.Serializable]
        public struct AuthenticationMessage
        {
            public bool HasAuthMessage;
            public ushort MessageLength;
            public byte[] Message;
            public void Deserialize(NetDataReader Writer)
            {
                if (Writer.TryGetUShort(out MessageLength))
                {
                    if (MessageLength != 0)
                    {
                        HasAuthMessage = true;
                        Writer.GetBytes(Message, MessageLength);
                    }
                    else
                    {
                        HasAuthMessage = false;
                    }
                }
                else
                {
                    BNL.LogError("missing Message Length!");
                    HasAuthMessage = false;
                }
            }
            public void Dispose()
            {
            }
            public void Serialize(NetDataWriter Writer)
            {
                Writer.Put(MessageLength);
                Writer.Put(Message);
                HasAuthMessage = true;
            }
        }
    }
}