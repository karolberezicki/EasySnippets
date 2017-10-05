using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace EasySnippets
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            Console.WriteLine(@"An unexpected application exception occurred {0}", args.Exception);

            File.AppendAllLines(@".\es.log", new[] { $"{DateTime.UtcNow:yyyy-MM-dd HH\\:mm\\:ss.fff} {args.Exception.Message}", args.Exception.StackTrace });

            MessageBox.Show("An unexpected exception has occurred. Shutting down the application. Please check the log file for more details.");

            // Prevent default unhandled exception processing
            args.Handled = true;
        }
    }
}
