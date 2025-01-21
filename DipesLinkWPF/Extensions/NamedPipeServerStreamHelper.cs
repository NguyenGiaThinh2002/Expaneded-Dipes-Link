using DipesLink.ViewModels;
using SharedProgram.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;

namespace DipesLink.Extensions
{
    public static class NamedPipeServerStreamHelper
    {
        public static int CheckUSBDongleKeyProcessID = 0;
        public static int _numberLicense;

        private static void CheckDongleKeyForStation(object? sender, EventArgs e)
        {
            if (sender is int index)
            {
                _numberLicense = index;
            }
        }

        public static void StartDongleKeyProcess()
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

        public static void NamedPipeServerForUSBDongleCheck()
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
                            _numberLicense = int.Parse(message);
                            ViewModelSharedEvents.OnChangeDongleKeyHandler(int.Parse(message));
                            Console.WriteLine(_numberLicense.ToString());

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
