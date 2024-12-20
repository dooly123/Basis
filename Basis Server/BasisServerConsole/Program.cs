using Basis.Network;
using Basis.Network.Server;
//using Basis.Network.Server.Prometheus;

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
            // Start the server on a background task
            Task serverTask = Task.Run(() => BasisNetworkServer.StartServer(config), cancellationToken);
            // Register a shutdown hook to clean up resources when the application is terminated
            AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) =>
            {
                BNL.Log("Shutting down server...");

                // Perform graceful shutdown of the server and logging
                cancellationTokenSource.Cancel();

                try
                {
                    serverTask.Wait(); // Wait for the server to finish
                }
                catch (AggregateException ex)
                {
                    foreach (var inner in ex.InnerExceptions)
                    {
                        BNL.LogError(inner.Message);
                    }
                }
                // BasisPrometheus.StopPrometheus();
                if (config.EnableStatistics)
                {
                    BasisStatistics.StopWorkerThread();
                }
                await BasisServerSideLogging.ShutdownAsync();
                BNL.Log("Server shut down successfully.");
            };
            serverTask.Wait();
            while (!Console.KeyAvailable)
            {
                BasisNetworkServer.server.PollEvents();
                Thread.Sleep(15);
            }
        }
    }
}
