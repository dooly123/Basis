
namespace Basis.Network.Server
{
    public struct BasisMessageReceivedEventArgs
    {
        public LiteNetLib.DeliveryMethod SendMode;
        public byte Tag;
        public ushort ClientId;
    }
}