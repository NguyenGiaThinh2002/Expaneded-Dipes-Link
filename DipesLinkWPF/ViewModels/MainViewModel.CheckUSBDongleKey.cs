using SharedProgram.Shared;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;

namespace DipesLink.ViewModels
{
    public partial class MainViewModel
    {

        private void CheckDongleKeyForStation(object? sender, EventArgs e)
        {
            try
            {
                var numberLicense = (int)sender;
                for (int i = 0; i < SharedValues.NumberOfStation; i++)
                {
                    JobList[i].IsHaveLicense = numberLicense > i;
                }
            }
            catch (Exception)
            {

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
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal,
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
