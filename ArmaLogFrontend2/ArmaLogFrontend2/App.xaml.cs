using System;
using System.Windows;
using System.Windows.Threading;

namespace ArmaLogFrontend
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static bool hasShownCrash = false;

        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (!hasShownCrash)
            {
                hasShownCrash = true;
                FrontendLogger.LogCrash($"Unhandled exception => {e.Exception.Message}\n{e.Exception.StackTrace}");
                MessageBox.Show($"CRASH => {e.Exception.Message}\nCheck FrontendCrashes.log for details.",
                                "App Crash",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            e.Handled = true;
        }
    }
}
