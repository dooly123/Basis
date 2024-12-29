#nullable enable

using LiteNetLib.Utils;

namespace Basis.Network.Core.Serializable
{
    public static partial class SerializableBasis
    {
        /// Consistes of a ushort length, followed by byte array (of same length)
        [System.Serializable]
        public struct AuthenticationMessage
        {
            public byte[] bytes;
            public void Deserialize(NetDataReader Reader)
            {
                if (Reader.TryGetUShort(out ushort msgLength))
                {
                    bytes = new byte[msgLength];
                    Reader.GetBytes(bytes, msgLength);
                }
                else
                {
                    BNL.LogError("missing Message Length!");
                }
            }

            public readonly void Serialize(NetDataWriter Writer)
            {
                Writer.Put(checked((ushort)bytes.Length));
                Writer.Put(bytes);
            }
        }
    }
}
