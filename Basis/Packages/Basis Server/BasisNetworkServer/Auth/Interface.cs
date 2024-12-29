using AuthenticationMessage = Basis.Network.Core.Serializable.SerializableBasis.AuthenticationMessage;

namespace Basis.Network.Server.Auth
{
    public interface IAuth
    {
        public bool IsAuthenticated(AuthenticationMessage msg);
    }
}
