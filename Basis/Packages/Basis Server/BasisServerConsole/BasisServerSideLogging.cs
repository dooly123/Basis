using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Basis.Network
{
    public static class BasisServerSideLogging
    {
        private static readonly string LogDirectory;
        private static string CurrentLogFileName => Path.Combine(LogDirectory, $"{DateTime.UtcNow:yyyy-MM-dd}.log");

        private static CancellationTokenSource _cancellationTokenSource;
        private static Task _loggingTask;
        private static readonly BlockingCollection<string> LogQueue = new(new ConcurrentQueue<string>());
        private static readonly SemaphoreSlim FileWriteSemaphore = new(1, 1);

        static BasisServerSideLogging()
        {
            // Set the log directory to a folder named "Logs" in the executable's directory
            LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            // Ensure the logs directory exists
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }
        public static bool UseLogging;
        public static void Initialize(Configuration config)
        {
            UseLogging = config.UsingLoggingFile;
            BNL.LogOutput += Log;
            BNL.LogWarningOutput += LogWarning;
            BNL.LogErrorOutput += LogError;

            if (UseLogging)
            {
                Log("Logs are saved to " + CurrentLogFileName);
                StartLoggingTask();
            }
            else
            {
                Log("no logs will be saved");
            }
        }

        private static void StartLoggingTask()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            _loggingTask = Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested || !LogQueue.IsCompleted)
                    {
                        if (LogQueue.TryTake(out var logEntry, Timeout.Infinite))
                        {
                            await WriteToFileAsync(logEntry, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Task canceled, exit gracefully
                }
            }, cancellationToken);
        }

        private static async Task WriteToFileAsync(string logEntry, CancellationToken cancellationToken)
        {
            try
            {
                await FileWriteSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                using (var stream = new FileStream(CurrentLogFileName, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, true))
                {
                    var logData = Encoding.UTF8.GetBytes(logEntry + Environment.NewLine);
                    await stream.WriteAsync(logData, 0, logData.Length, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                FileWriteSemaphore.Release();
            }
        }

        public static async Task ShutdownAsync()
        {
            _cancellationTokenSource?.Cancel();
            LogQueue?.CompleteAdding();

            try
            {
                await _loggingTask.ConfigureAwait(false);
            }
            catch (AggregateException)
            {
                // Suppress exceptions caused by cancellation
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
            }
        }

        public static void Log(string message)
        {
            EnqueueLog("INFO", message);
        }

        public static void LogWarning(string message)
        {
            EnqueueLog("WARNING", message);
        }

        public static void LogError(string message)
        {
            EnqueueLog("ERROR", message);
        }

        private static void EnqueueLog(string level, string message)
        {
            var logEntry = $"[{level}] {message}";
            Console.WriteLine(logEntry);
            if (UseLogging)
            {
                LogQueue.Add(logEntry);

            }
        }
    }
}