using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Windows;

namespace DipesLink.ViewModels
{
    public partial class MainViewModel
    {
        public static int _numberLicense;

        private void CheckDongleKeyForStation(object? sender, EventArgs e)
        {
          if(sender is int index)
            {
                _numberLicense = index;
            }
        }

        public void StartDongleKeyProcess()
        {
            try
            {
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string? applicationPath = Process.GetCurrentProcess().MainModule?.FileName;
                string local = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
                string helperPath = Path.Combine(local, "DongleKeyVerification.exe");

                if (!File.Exists(helperPath))
                {
                    return;
                }

                var process = Process.Start(new ProcessStartInfo  // Start the helper process to restart the application
                {
                    FileName = helperPath,
                    Arguments = $"{Process.GetCurrentProcess().Id} \"{applicationPath}\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false
                });

                if (process != null)
                {
                    CheckUSBDongleKeyProcessID = process.Id;
                }
                var serverThread = new Thread(NamedPipeServerForUSBDongleCheck)
                {
                    IsBackground = true
                };
                serverThread.Start();
            }
            catch (Exception)
            {
            }
        }

        public void NamedPipeServerForUSBDongleCheck()
        {
            while (true)
            {
                using (var pipeServer = new NamedPipeServerStream("NamedPipeUSBLicense", PipeDirection.In))
                {
                    pipeServer.WaitForConnection();
                    try
                    {
                        using var reader = new StreamReader(pipeServer);
                        string? message = reader.ReadLine();
                        if (message != null)
                        {
                            ViewModelSharedEvents.OnChangeDongleKeyHandler(int.Parse(message));
                            _numberLicense = int.Parse(message);
                            //MessageBox.Show(_numberLicense.ToString());

                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}
