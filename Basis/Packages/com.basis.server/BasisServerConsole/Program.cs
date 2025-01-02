using Basis.Network;
using Basis.Network.Server;

namespace Basis
{
    class Program
    {
        public static BasisNetworkHealthCheck Check;

        public static void Main(string[] args)
        {
            // Set up global exception handlers
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // Get the path to the config.xml file in the application's directory
            string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");

            // Load configuration from the XML file
            Configuration config = Configuration.LoadFromXml(configFilePath);

            // Initialize server-side logging
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            BasisServerSideLogging.Initialize(config, folderPath);

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

            // Keep the application running
            while (true)
            {
                Thread.Sleep(15000);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                BNL.LogError($"Fatal exception: {exception.Message}");
                BNL.LogError($"Stack trace: {exception.StackTrace}");
            }
            else
            {
                BNL.LogError("An unknown fatal exception occurred.");
            }
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            foreach (var exception in e.Exception.InnerExceptions)
            {
                BNL.LogError($"Unobserved task exception: {exception.Message}");
                BNL.LogError($"Stack trace: {exception.StackTrace}");
            }
            e.SetObserved(); // Prevents the application from crashing
        }
    }
}
