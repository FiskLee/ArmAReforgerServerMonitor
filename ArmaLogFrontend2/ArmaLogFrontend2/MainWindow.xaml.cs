using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ArmaLogFrontend
{
    /// <summary>
    /// MainWindow is the primary UI for the Arma Reforger Server Monitor.
    /// Includes multiple bar charts, raw data, logs, players, login fields, etc.
    /// </summary>
    public partial class MainWindow : Window
    {
        // Timer that polls performance data every 2 seconds
        private Timer? _pollTimer;

        // Toggle for raw data tab
        private bool _rawDataPaused = true;

        // Toggle for performance data
        private bool _performancePaused = false;
        private string _timeRange = "Last 10 min"; // default time range
        private bool _useGb = true;                // if true => memory usage in GB, else MB

        // We store the backend URL from the top bar
        private string _backendUrl = "http://localhost:5000";

        // Chart values => each chart is a ColumnSeries with data labels
        public ChartValues<double> FpsValues { get; set; } = new ChartValues<double>();
        public ChartValues<double> FrameTimeValues { get; set; } = new ChartValues<double>();
        public ChartValues<double> PlayerCountValues { get; set; } = new ChartValues<double>();
        public ChartValues<double> CpuUsageValues { get; set; } = new ChartValues<double>();
        public ChartValues<double> CoreUsageValues { get; set; } = new ChartValues<double>();

        /// <summary>
        /// Constructor initializes the UI, sets up bar charts, sets up the poll timer, etc.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Set up bar charts => define each series with data labels
            SetupBarCharts();

            // "No logs yet" label is visible until logs appear
            NoLogsLabel.Visibility = Visibility.Visible;

            // Lock scroll by default in logs
            ScrollLockCheckBox.IsChecked = true;

            // Start a poll timer => calls OnPollTimer every 2 seconds
            _pollTimer = new Timer(OnPollTimer, null, 2000, 2000);
        }

        /// <summary>
        /// We define the bar charts with data labels and different colors.
        /// Each chart references the ChartValues we declared above.
        /// </summary>
        private void SetupBarCharts()
        {
            FpsChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "FPS",
                    Values = FpsValues,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("F0"),
                    Fill = System.Windows.Media.Brushes.MediumPurple
                }
            };

            FrameTimeChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "FrameTime",
                    Values = FrameTimeValues,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("F1"),
                    Fill = System.Windows.Media.Brushes.Orange
                }
            };

            PlayerCountChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Players",
                    Values = PlayerCountValues,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("F0"),
                    Fill = System.Windows.Media.Brushes.DarkCyan
                }
            };

            CpuUsageChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "CPU Usage",
                    Values = CpuUsageValues,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("F1"),
                    Fill = System.Windows.Media.Brushes.Green
                }
            };

            CpuCoresChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Cores",
                    Values = CoreUsageValues,
                    DataLabels = true,
                    LabelPoint = point => point.Y.ToString("F1"),
                    Fill = System.Windows.Media.Brushes.BlueViolet
                }
            };
        }

        /// <summary>
        /// The poll timer calls this method every 2 seconds.
        /// We only fetch performance data if we haven't paused it.
        /// We could also fetch daily usage or players if needed.
        /// </summary>
        private void OnPollTimer(object? state)
        {
            if (!_performancePaused)
            {
                FetchPerformanceData();
            }
            // We might also fetch daily usage if there's a daily usage tab, etc.
        }

        // ==========================
        // LOGIN / DISCONNECT
        // ==========================
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var username = UsernameTextBox.Text.Trim();
                var password = PasswordBox.Password.Trim();
                var serverUrl = ServerUrlTextBox.Text.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(serverUrl))
                {
                    ConnectionStatusText.Text = "Status: Invalid credentials/URL";
                    ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                // store new URL
                _backendUrl = serverUrl;

                // Example => POST /api/login
                using var client = new HttpClient();
                var payload = new { Username = username, Password = password };
                var resp = await client.PostAsync($"{_backendUrl}/api/login",
                    new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
                if (resp.IsSuccessStatusCode)
                {
                    ConnectionStatusText.Text = "Status: Connected";
                    ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    ConnectionStatusText.Text = "Status: Login failed";
                    ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                ConnectionStatusText.Text = $"Status: Error => {ex.Message}";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Red;
                FrontendLogger.LogError($"ConnectButton_Click => {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var client = new HttpClient();
                var resp = await client.PostAsync($"{_backendUrl}/api/disconnect", null);
                ConnectionStatusText.Text = "Status: Disconnected";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
            catch (Exception ex)
            {
                ConnectionStatusText.Text = $"Status: Error => {ex.Message}";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Red;
                FrontendLogger.LogError($"DisconnectButton_Click => {ex.Message}\n{ex.StackTrace}");
            }
        }

        // ==========================
        // TIME RANGE / PAUSE / USE GB
        // ==========================
        private void TimeRangeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TimeRangeComboBox.SelectedItem is ComboBoxItem item)
            {
                _timeRange = item.Content.ToString() ?? "Last 10 min";
            }
        }

        private void PausePerformanceButton_Click(object sender, RoutedEventArgs e)
        {
            _performancePaused = !_performancePaused;
            PausePerformanceButton.Content = _performancePaused ? "Resume Performance" : "Pause Performance";
        }

        private void UseGbCheckBox_Click(object sender, RoutedEventArgs e)
        {
            _useGb = (UseGbCheckBox.IsChecked == true);
        }

        // ==========================
        // FETCH PERFORMANCE
        // ==========================
        private async void FetchPerformanceData()
        {
            try
            {
                // We call GET /api/performance?timeRange=...
                using var client = new HttpClient();
                var url = $"{_backendUrl}/api/performance?timeRange={_timeRange}";
                var json = await client.GetStringAsync(url);
                var records = JsonConvert.DeserializeObject<List<PerformanceRecord>>(json) ?? new();
                if (records.Count == 0) return;

                // We take the last record as the "latest"
                var latest = records[0];

                // memory usage => MB or GB
                double memVal = latest.MemoryUsage; 
                if (_useGb) memVal /= 1024.0; // MB -> GB

                // update summary
                Dispatcher.Invoke(() =>
                {
                    PerformanceSummaryText.Text =
                        $"CPU: {latest.CpuUsage:F1}% | Active Memory: {memVal:F2} {(_useGb ? "GB" : "MB")} | " +
                        $"DiskR:{latest.DiskReadMBs:F2}MB/s | DiskW:{latest.DiskWriteMBs:F2}MB/s | " +
                        $"NetIn:{latest.NetworkInMBs:F2}MB/s | NetOut:{latest.NetworkOutMBs:F2}MB/s";
                });

                // We also parse a "raw performance data" snippet
                var snippet = $"FPS: 60.0, frame time (avg: 16.7 ms), Mem: {latest.MemoryUsage} MB, Player: 11, etc.";
                Dispatcher.Invoke(() =>
                {
                    RawPerformanceDataTextBox.Text = snippet;
                });

                // update bar charts
                Dispatcher.Invoke(() =>
                {
                    // FPS chart => pretend CPU usage is fps
                    if (FpsValues.Count > 20) FpsValues.RemoveAt(0);
                    FpsValues.Add(latest.CpuUsage);

                    // FrameTime => pretend 16.7
                    if (FrameTimeValues.Count > 20) FrameTimeValues.RemoveAt(0);
                    FrameTimeValues.Add(16.7);

                    // PlayerCount => pretend 5
                    if (PlayerCountValues.Count > 20) PlayerCountValues.RemoveAt(0);
                    PlayerCountValues.Add(5);

                    // CPU usage
                    if (CpuUsageValues.Count > 20) CpuUsageValues.RemoveAt(0);
                    CpuUsageValues.Add(latest.CpuUsage);

                    // Core usage => pretend half the CPU usage for a single "core"
                    if (CoreUsageValues.Count > 20) CoreUsageValues.RemoveAt(0);
                    CoreUsageValues.Add(latest.CpuUsage / 2.0);

                    // Update stats updated label
                    StatsUpdatedLabel.Text = $"Stats updated: {DateTime.Now:T}";
                });

                // We can also guess the "active players" from the record => pretend 11
                Dispatcher.Invoke(() =>
                {
                    ActivePlayersLabel.Text = $"Active Players: 11";
                });
            }
            catch (Exception ex)
            {
                AppendLog($"[Error PerfData] {ex.Message}");
                FrontendLogger.LogError($"FetchPerformanceData => {ex.Message}\n{ex.StackTrace}");
            }
        }

        // ==========================
        // PLAYERS TAB
        // ==========================
        private async void ShowPlayersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var client = new HttpClient();
                var json = await client.GetStringAsync($"{_backendUrl}/api/players/leaderboard");
                var players = JsonConvert.DeserializeObject<List<PlayerData>>(json) ?? new();
                Dispatcher.Invoke(() =>
                {
                    PlayersListBox.Items.Clear();
                    foreach (var p in players)
                    {
                        PlayersListBox.Items.Add($"{p.Name} - LastSeen: {p.LastSeen}, Country: {p.Country}");
                    }
                });
            }
            catch (Exception ex)
            {
                AppendLog($"[Error ShowPlayers] {ex.Message}");
                FrontendLogger.LogError($"ShowPlayersButton_Click => {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void RefreshPlayersButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPlayersButton_Click(sender, e);
        }

        // ==========================
        // RAW DATA TAB
        // ==========================
        private async void ShowRawDataButton_Click(object sender, RoutedEventArgs e)
        {
            _rawDataPaused = false;
            RawDataTextBox.Text = "(Loading raw data...)";
            try
            {
                using var client = new HttpClient();
                var resp = await client.GetStringAsync($"{_backendUrl}/api/rawdata");
                var lines = JsonConvert.DeserializeObject<string[]>(resp) ?? new string[0];
                RawDataTextBox.Text = string.Join("\n", lines);
            }
            catch (Exception ex)
            {
                AppendLog($"[Error ShowRawData] {ex.Message}");
                FrontendLogger.LogError($"ShowRawDataButton_Click => {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void PauseRawDataButton_Click(object sender, RoutedEventArgs e)
        {
            _rawDataPaused = !_rawDataPaused;
            PauseRawDataButton.Content = _rawDataPaused ? "Resume Raw Data" : "Pause Raw Data";
            if (_rawDataPaused)
            {
                RawDataTextBox.Text += "\n(Raw data paused.)";
            }
        }

        // ==========================
        // LOGS TAB
        // ==========================
        private async void FetchBackendLogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new HttpClient();
                var logsJson = await client.GetStringAsync($"{_backendUrl}/api/logs/backend");
                var lines = JsonConvert.DeserializeObject<List<string>>(logsJson) ?? new List<string>();
                Dispatcher.Invoke(() =>
                {
                    LogsTextBox.Text = string.Join("\r\n", lines);
                    NoLogsLabel.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                AppendLog($"[Error FetchBackendLogs] {ex.Message}");
                FrontendLogger.LogError($"FetchBackendLogsButton_Click => {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ShowFrontendLogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var errors = FrontendLogger.GetLast100Errors();
                var crashes = FrontendLogger.GetLast100Crashes();
                LogsTextBox.Text = "--- Last 100 Frontend Errors ---\r\n" +
                                   string.Join("\r\n", errors) +
                                   "\r\n\r\n--- Last 100 Frontend Crashes ---\r\n" +
                                   string.Join("\r\n", crashes);
                NoLogsLabel.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                AppendLog($"[Error ShowFrontendLogs] {ex.Message}");
                FrontendLogger.LogError($"ShowFrontendLogsButton_Click => {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void LogsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ScrollLockCheckBox.IsChecked == false)
            {
                LogsScrollViewer.ScrollToEnd();
            }
            if (!string.IsNullOrEmpty(LogsTextBox.Text))
            {
                NoLogsLabel.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Helper method to append a line to the logs text box.
        /// </summary>
        private void AppendLog(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                LogsTextBox.AppendText(msg + "\n");
                if (!string.IsNullOrEmpty(LogsTextBox.Text))
                {
                    NoLogsLabel.Visibility = Visibility.Collapsed;
                }
                if (ScrollLockCheckBox.IsChecked == false)
                {
                    LogsScrollViewer.ScrollToEnd();
                }
            });
        }
    }

    /// <summary>
    /// This class matches the JSON structure returned by the backend for performance records.
    /// </summary>
    public class PerformanceRecord
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; } // MB
        public double DiskReadMBs { get; set; }
        public double DiskWriteMBs { get; set; }
        public double NetworkInMBs { get; set; }
        public double NetworkOutMBs { get; set; }
    }

    /// <summary>
    /// This class matches the JSON structure for players from /api/players/leaderboard.
    /// </summary>
    public class PlayerData
    {
        public string Name { get; set; } = "";
        public DateTime LastSeen { get; set; }
        public string Country { get; set; } = "Unknown";
        // Additional fields if needed
    }

    /// <summary>
    /// For daily usage (optional).
    /// </summary>
    public class DailyUsage
    {
        public string Date { get; set; } = "";
        public double AvgCpu { get; set; }
        public double AvgMem { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// The logger for the frontend, storing errors/crashes in local log files.
    /// </summary>
    public static class FrontendLogger
    {
        private static readonly object fileLock = new object();
        private const string ErrorsFile = "FrontendErrors.log";
        private const string CrashesFile = "FrontendCrashes.log";

        /// <summary>
        /// Logs an error message to FrontendErrors.log
        /// </summary>
        public static void LogError(string msg)
        {
            var line = $"[ERROR {DateTime.UtcNow:O}] {msg}";
            Console.WriteLine(line);
            lock (fileLock)
            {
                System.IO.File.AppendAllText(ErrorsFile, line + "\n");
            }
        }

        /// <summary>
        /// Logs a crash message to FrontendCrashes.log
        /// </summary>
        public static void LogCrash(string msg)
        {
            var line = $"[CRASH {DateTime.UtcNow:O}] {msg}";
            Console.WriteLine(line);
            lock (fileLock)
            {
                System.IO.File.AppendAllText(CrashesFile, line + "\n");
            }
        }

        /// <summary>
        /// Reads the last 100 lines from FrontendErrors.log
        /// </summary>
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

        /// <summary>
        /// Reads the last 100 lines from FrontendCrashes.log
        /// </summary>
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
