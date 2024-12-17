using System.Text;
using static Basis.Network.Core.Serializable.SerializableBasis;

namespace Basis.Network.Server.Password
{
    public static class BasisPasswordImplementation
    {
        public static bool CheckPassword(AuthenticationMessage Auth, Configuration Configuration)
        {
            if (string.IsNullOrEmpty(Configuration.Password))
            {
                //we dont have a password configured!
                return true;
            }
            string result = Encoding.UTF8.GetString(Auth.Message);
            return Configuration.Password == result;
        }
    }
}