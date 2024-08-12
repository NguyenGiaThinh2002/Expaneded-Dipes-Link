using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace DipesLink.Extensions
{
    public static class ApplicationHelper
    {
        public static void RestartApplication()
        {
            try
            {
                string? applicationPath = Process.GetCurrentProcess().MainModule?.FileName;
                string local = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                string helperPath = Path.Combine(local, "RestartProcessHelper.exe");

                if (!File.Exists(helperPath))
                {
                    return;
                }

                Process.Start(new ProcessStartInfo  // Start the helper process to restart the application
                {
                    FileName = helperPath,
                    Arguments = $"{Process.GetCurrentProcess().Id} \"{applicationPath}\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false
                });

            }
            catch (Exception)
            {
            }

            Application.Current.Shutdown();  
        }
    }
}
