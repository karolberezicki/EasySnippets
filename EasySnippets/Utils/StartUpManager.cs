using System;
using System.Reflection;
using Microsoft.Win32;

namespace EasySnippets.Utils
{
    public class StartUpManager
    {
        public static void AddApplicationToCurrentUserStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key?.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }
        }

        public static void RemoveApplicationFromCurrentUserStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key?.DeleteValue(curAssembly.GetName().Name, false);
            }
        }

        public static bool IsApplicationAddedToCurrentUserStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                object currentValue = key?.GetValue(curAssembly.GetName().Name, null);
                return currentValue != null && currentValue.ToString().Equals(curAssembly.Location, StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}