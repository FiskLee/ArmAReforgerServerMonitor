using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;

namespace ArmaLogFrontend
{
    public partial class LogViewerWindow : Window
    {
        private string _serverUrl;
        private HttpClient _httpClient;
        private DispatcherTimer _timer;

        public LogViewerWindow(string serverUrl, HttpClient httpClient)
        {
            try
            {
                InitializeComponent();
                _serverUrl = serverUrl;
                _httpClient = httpClient;
            }
            catch (Exception ex)
            {
                FrontendLogger.LogCrash($"LogViewerWindow constructor => {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a timer that fetches logs every 2 seconds
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(2);
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
            catch (Exception ex)
            {
                FrontendLogger.LogError($"Window_Loaded => {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                StatusTextBlock.Text = "Status: Fetching logs...";
                // Example => fetch last 100 lines of "errors" logs from the backend
                var logsJson = await _httpClient.GetStringAsync($"{_serverUrl}/api/logs/errors");
                var lines = JsonConvert.DeserializeObject<List<string>>(logsJson);
                if (lines == null)
                {
                    lines = new List<string> { "(No logs or parse error)" };
                }
                LogTextBox.Text = string.Join("\r\n", lines);
                StatusTextBlock.Text = $"Status: Updated {DateTime.Now:T}";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error => {ex.Message}";
                FrontendLogger.LogError($"LogViewerWindow => Timer_Tick => {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void StopLogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }
                StatusTextBlock.Text = "Status: Stopped";
            }
            catch (Exception ex)
            {
                FrontendLogger.LogError($"StopLogsButton_Click => {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer = null;
                }
                base.OnClosed(e);
            }
            catch (Exception ex)
            {
                FrontendLogger.LogError($"OnClosed => {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// FrontendLogger is placed in the SAME file & namespace to fix “does not exist in the current context” errors.
    /// </summary>
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
                System.IO.File.AppendAllText(ErrorsFile, line + "\n");
            }
        }

        public static void LogCrash(string msg)
        {
            var line = $"[CRASH {DateTime.UtcNow:O}] {msg}";
            Console.WriteLine(line);
            lock (fileLock)
            {
                System.IO.File.AppendAllText(CrashesFile, line + "\n");
            }
        }

        public static List<string> GetLast100Errors()
        {
            lock (fileLock)
            {
                if (!System.IO.File.Exists(ErrorsFile))
                {
                    return new List<string> { "(No FrontendErrors.log yet)" };
                }
                var all = System.IO.File.ReadAllLines(ErrorsFile);
                if (all.Length <= 100) return new List<string>(all);
                return new List<string>(all[(all.Length - 100)..]);
            }
        }

        public static List<string> GetLast100Crashes()
        {
            lock (fileLock)
            {
                if (!System.IO.File.Exists(CrashesFile))
                {
                    return new List<string> { "(No FrontendCrashes.log yet)" };
                }
                var all = System.IO.File.ReadAllLines(CrashesFile);
                if (all.Length <= 100) return new List<string>(all);
                return new List<string>(all[(all.Length - 100)..]);
            }
        }
    }
}
