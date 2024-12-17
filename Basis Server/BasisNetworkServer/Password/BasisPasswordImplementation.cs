using System.Text;
using static Basis.Network.Core.Serializable.SerializableBasis;

namespace Basis.Network.Server.Password
{
    public static class BasisPasswordImplementation
    {
        public static bool CheckPassword(AuthenticationMessage Auth, Configuration Configuration, out string UsedPassword)
        {
            if (string.IsNullOrEmpty(Configuration.Password))
            {
                UsedPassword = string.Empty;
                //we dont have a password configured!
                return true;
            }
            UsedPassword = Encoding.UTF8.GetString(Auth.Message);
            return Configuration.Password == UsedPassword;
        }
    }
}