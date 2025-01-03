using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct CreateSingleRemoteMessage
    {
        public string password;
        public void Deserialize(NetDataReader Writer)
        {
            Writer.Get(out password);
        }
        public void Serialize(NetDataWriter Writer)
        {
             Writer.Put(password);
        }
    }
}
