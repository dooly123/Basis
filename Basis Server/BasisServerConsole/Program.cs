using Basis.Network;
using Basis.Network.Server;
namespace Basis
{
    class Program
    {
        public static BasisNetworkHealthCheck Check;
        public static void Main(string[] args)
        {
            BasisServerSideLogging.Initalize();
            BNL.Log($"Server Booting");
            Check = new BasisNetworkHealthCheck();
            // Create a cancellation token source, which will never be cancelled
            //there is work on other threads it should never be stopped unless we kill the application
            // Start the server on a background task
            var serverTask = Task.Run(() => BasisNetworkServer.StartServer());
            System.Threading.Thread.Sleep(-1);
        }
    }
}