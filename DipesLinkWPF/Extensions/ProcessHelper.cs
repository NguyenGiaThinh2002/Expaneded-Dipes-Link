using System.Diagnostics;

namespace DipesLink.Extensions
{
    public static class ProcessHelper
    {
        public static void StartProcess(string processPath, string arguments = "")
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = processPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using Process process = new() { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();
        }
    }
}
