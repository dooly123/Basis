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
                BNL.Log("No Password Set");
                return true;
            }
            if(Auth.Message == null || Auth.Message.Length == 0)
            {
                UsedPassword = string.Empty;
                BNL.Log("Auth Was Empty");
                return false;
            }
            UsedPassword = Encoding.UTF8.GetString(Auth.Message);
            if (string.IsNullOrEmpty(UsedPassword))
            {
                BNL.Log("No Password in the Auth Message");
                return true;
            }
            return Configuration.Password == UsedPassword;
        }
    }
}