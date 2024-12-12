namespace Basis.Network
{
    public static class BasisServerSideLogging
    {
        public static void Initalize()
        {
            BNL.LogOutput += Log;
            BNL.LogWarningOutput += LogWarning;
            BNL.LogErrorOutput += LogError;
        }
        public static void Log(string message)
        {
            Console.WriteLine(message);
        }

        public static void LogWarning(string message)
        {
            Console.WriteLine($"WARNING: {message}");
        }

        public static void LogError(string message)
        {
            Console.WriteLine($"ERROR: {message}");
        }
    }
}