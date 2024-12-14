namespace Basis.Network.Core
{
    public struct BasisMessageReceivedEventArgs
    {
        public LiteNetLib.DeliveryMethod SendMode;
        public byte Tag;
        public ushort ClientId;
    }
}