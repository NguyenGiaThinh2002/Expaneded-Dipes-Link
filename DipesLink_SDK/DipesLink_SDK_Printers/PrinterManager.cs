using IPCSharedMemory;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace DipesLink_SDK_Printers
{
    public class PrinterManager
    {
        public readonly List<RynanRPrinterTCPClient> _printers = new();

        /// <summary>
        /// Initializes the PrinterManager and creates the specified number of printers.
        /// </summary>
        /// <param name="printerCount">The number of printers to create.</param>
        /// <param name="ipc">The IPC shared helper.</param>
        public PrinterManager(int printerCount, IPCSharedHelper? ipc)
        {
            for (int i = 0; i < printerCount; i++)
            {
                AddPrinter(i, ipc);
            }
        }

        private void AddPrinter(int index, IPCSharedHelper? ipc)
        {
            // Check if the printer already exists by comparing its index.
            if (_printers.Any(p => p.GetIndex() == index))
            {
                Console.WriteLine($"Printer with index {index} already exists.");
                return;
            }

            var printer = new RynanRPrinterTCPClient(index, ipc);
            _printers.Add(printer);

            Console.WriteLine($"Printer {index} added.");
        }

        public void ConnectAllPrinters()
        {
            foreach (var printer in _printers)
            {
                if (printer.Connect())
                {
                    Console.WriteLine($"Printer {printer.GetIndex()} connected successfully.");
                }
                else
                {
                    Console.WriteLine($"Printer {printer.GetIndex()} failed to connect.");
                }
            }
        }

        public void SendDataToAllPrinters(string data)
        {
            foreach (var printer in _printers)
            {
                if (printer.IsConnected())
                {
                    printer.SendData(data);
                }
                else
                {
                    Console.WriteLine($"Printer {printer.GetIndex()} failed to connect.");
                }
            }
        }

        public void StartAllPrinter(string data)
        {
            foreach (var printer in _printers)
            {
                if (printer.IsConnected())
                {
                    printer.SendData(data);
                }
                else
                {
                    Console.WriteLine($"Printer {printer.GetIndex()} failed to connect.");
                }
            }
        }



        public void DisconnectAllPrinters()
        {
            foreach (var printer in _printers)
            {
                if (printer.Disconnect())
                {
                    Console.WriteLine($"Printer {printer.GetIndex()} disconnected successfully.");
                }
                else
                {
                    Console.WriteLine($"Printer {printer.GetIndex()} failed to disconnect.");
                }
            }
        }

        public void MonitorPrinters()
        {
            foreach (var printer in _printers)
            {
                bool isConnected = printer.IsConnected();
                Console.WriteLine($"Printer {printer.GetIndex()} is {(isConnected ? "connected" : "disconnected")}.");
            }
        }
    }
}
