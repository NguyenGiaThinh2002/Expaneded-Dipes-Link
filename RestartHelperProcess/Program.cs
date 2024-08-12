using System.Diagnostics;

namespace RestartProcessHelper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                return;
            }
            int processId = int.Parse(args[0]);
            string applicationPath = args[1];
            try
            {
                Process process = Process.GetProcessById(processId);
                process.WaitForExit();
                Thread.Sleep(2000); // Give it a little time to fully close
                Process.Start(applicationPath);
            }
            catch (Exception)
            {
            }
        }
    }
}
