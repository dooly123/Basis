using DarkRift;
using LiteNetLib.Utils;
public static partial class SerializableBasis
{
    public struct Vector3Message
    {
        public float x;
        public float y;
        public float z;
        public void SetValue(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public void Deserialize(NetDataReader Writer)
        {
            Writer.Get(out x);
            Writer.Get(out y);
            Writer.Get(out z);
        }
        public void Serialize(NetDataWriter Writer)
        {
             Writer.Put(x);
             Writer.Put(y);
             Writer.Put(z);
        }

        public void Dispose()
        {
        }
    }
}
