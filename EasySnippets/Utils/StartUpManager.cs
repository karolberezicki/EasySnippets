using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Win32;

namespace EasySnippets.Utils
{
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Windows only app")]
    public class StartUpManager
    {
        public static void AddApplicationToCurrentUserStartup()
        {
            using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            var curAssembly = Assembly.GetExecutingAssembly();
            key?.SetValue(curAssembly.GetName().Name, AppContext.BaseDirectory);
        }

        public static void RemoveApplicationFromCurrentUserStartup()
        {
            using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            var curAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            if (!string.IsNullOrWhiteSpace(curAssemblyName))
            {
                key?.DeleteValue(curAssemblyName, false);
            }

        }

        public static bool IsApplicationAddedToCurrentUserStartup()
        {
            using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            var curAssembly = Assembly.GetExecutingAssembly();
            var currentValue = key?.GetValue(curAssembly.GetName().Name, null)?.ToString();
            return currentValue?.Equals(AppContext.BaseDirectory, StringComparison.InvariantCultureIgnoreCase) ?? false;
        }
    }
}