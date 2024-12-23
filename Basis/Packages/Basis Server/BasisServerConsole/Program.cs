using Basis.Network;
using Basis.Network.Server;
namespace Basis
{
    class Program
    {
        public static BasisNetworkHealthCheck Check;

        public static void Main(string[] args)
        {
            // Get the path to the config.xml file in the application's directory
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");

            // Load configuration from the XML file
            Configuration config = Configuration.LoadFromXml(configFilePath);
            //  BasisPrometheus.StartPrometheus(config.PromethusPort, config.PromethusUrl);
            // Initialize server-side logging
            BasisServerSideLogging.Initialize(config);

            BNL.Log("Server Booting");

            // Create a cancellation token source
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Initialize health check
            Check = new BasisNetworkHealthCheck(config);

            // Start the server in a background task and prevent it from exiting
            Task serverTask = Task.Run(() =>
            {
                try
                {
                    BasisNetworkServer.StartServer(config);
                }
                catch (Exception ex)
                {
                    BNL.LogError($"Server encountered an error: {ex.Message}");
                    // Optionally, handle server restart or log critical errors
                }
            }, cancellationToken);

            // Register a shutdown hook to clean up resources when the application is terminated
            AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) =>
            {
                BNL.Log("Shutting down server...");

                // Perform graceful shutdown of the server and logging
                cancellationTokenSource.Cancel();

                try
                {
                    await serverTask; // Wait for the server to finish
                }
                catch (Exception ex)
                {
                    BNL.LogError($"Error during server shutdown: {ex.Message}");
                }

                // BasisPrometheus.StopPrometheus();
                if (config.EnableStatistics)
                {
                    BasisStatistics.StopWorkerThread();
                }
                await BasisServerSideLogging.ShutdownAsync();
                BNL.Log("Server shut down successfully.");
            };
            while (true)
            {
                System.Threading.Thread.Sleep(15000);
            }
        }
    }
}
