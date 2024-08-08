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
            string applicationPath = Process.GetCurrentProcess().MainModule.FileName;
            string helperPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "RestartHelperProcess.exe");

            if (!File.Exists(helperPath))
            {
                MessageBox.Show("RestartHelper.exe not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Start the helper process to restart the application
            Process.Start(new ProcessStartInfo
            {
                FileName = helperPath,
                Arguments = $"{Process.GetCurrentProcess().Id} \"{applicationPath}\"",
                UseShellExecute = false
            });

            // Close the current application
            Application.Current.Shutdown();
        }
    }
}
