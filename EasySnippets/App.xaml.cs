using System;
using System.IO;
using Microsoft.UI.Xaml;
using EasySnippets.Views;

namespace EasySnippets;

public partial class App : Application
{
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "es.log");

    public App()
    {
        this.InitializeComponent();
        this.UnhandledException += App_UnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        m_window.Activate();
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        LogException(e.Exception);
        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            LogException(ex);
        }
    }

    private static void LogException(Exception ex)
    {
        try
        {
            File.AppendAllLines(LogPath, new[]
            {
                $"{DateTime.UtcNow:yyyy-MM-dd HH\\:mm\\:ss.fff} {ex.Message}",
                ex.StackTrace ?? string.Empty
            });
        }
        catch
        {
            // Prevent recursive failures in logging
        }
    }

    private Window m_window;
}