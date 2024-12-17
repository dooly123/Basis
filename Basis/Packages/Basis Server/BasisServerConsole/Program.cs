using System;
using System.Threading;
using System.Threading.Tasks;
using Basis.Network;
using Basis.Network.Server;

namespace Basis
{
    class Program
    {
        public static BasisNetworkHealthCheck Check;

        public static void Main(string[] args)
        {
            // Initialize server-side logging
            BasisServerSideLogging.Initialize();

            BNL.Log("Server Booting");

            // Create a cancellation token source
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Initialize health check
            Check = new BasisNetworkHealthCheck();

            // Start the server on a background task
            var serverTask = Task.Run(() => BasisNetworkServer.StartServer(), cancellationToken);

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

                await BasisServerSideLogging.ShutdownAsync();
                BNL.Log("Server shut down successfully.");
            };

            // Keep the main thread alive indefinitely
            Thread.Sleep(Timeout.Infinite);
        }
    }
}