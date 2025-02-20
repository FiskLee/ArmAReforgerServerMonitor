using System;
using System.Collections.Generic;
using System.IO;

namespace ArmaLogFrontend
{
    public static class FrontendLogger
    {
        private static readonly object fileLock = new object();
        private const string ErrorsFile = "FrontendErrors.log";
        private const string CrashesFile = "FrontendCrashes.log";

        public static void LogError(string msg)
        {
            var line = $"[ERROR {DateTime.UtcNow:O}] {msg}";
            Console.WriteLine(line);
            lock (fileLock)
            {
                File.AppendAllText(ErrorsFile, line + "\n");
            }
        }

        public static void LogCrash(string msg)
        {
            var line = $"[CRASH {DateTime.UtcNow:O}] {msg}";
            Console.WriteLine(line);
            lock (fileLock)
            {
                File.AppendAllText(CrashesFile, line + "\n");
            }
        }

        public static List<string> GetLast100Errors()
        {
            lock (fileLock)
            {
                if (!File.Exists(ErrorsFile))
                {
                    return new List<string> { "(No FrontendErrors.log yet)" };
                }
                var all = File.ReadAllLines(ErrorsFile);
                if (all.Length <= 100) return new List<string>(all);
                return new List<string>(all[(all.Length - 100)..]);
            }
        }

        public static List<string> GetLast100Crashes()
        {
            lock (fileLock)
            {
                if (!File.Exists(CrashesFile))
                {
                    return new List<string> { "(No FrontendCrashes.log yet)" };
                }
                var all = File.ReadAllLines(CrashesFile);
                if (all.Length <= 100) return new List<string>(all);
                return new List<string>(all[(all.Length - 100)..]);
            }
        }
    }
}
