using IPCSharedMemory;
using IPCSharedMemory.Controllers;
using SharedProgram.DeviceTransfer;
using SharedProgram.Models;
using SharedProgram.Shared;
using System.Diagnostics;
using System.IO.Ports;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLink_SDK_BarcodeScanner
{
    public class RS232BarcodeScanner : IBarcodeScanner, IDisposable
    {
        private static SerialPort _serialPort;
        private Thread? _threadMonitor;
        private readonly int _index;
        private readonly IPCSharedHelper? _ipc;

        public RS232BarcodeScanner(int index, IPCSharedHelper? ipc)
        {
            _index = index;
            _ipc = ipc;
            MonitorConnection();
            SharedEventsIpc.ScannerStatusChanged += SharedEventsIpc_ScannerStatusChanged;
            SharedEvents.OnScannerParametersChange += SharedEvents_OnScannerParametersChange;
        }

        private void SharedEvents_OnScannerParametersChange(object? sender, EventArgs e)
        {
            try
            {
                RestartConnection();
            }
            catch (Exception ex)
            {
                LogError("Error during parameter change: ", ex);
            }
        }

        public bool Connect()
        {
            InitializeSerialPort();

            try
            {
                _serialPort.Open();
                Debug.WriteLine("Barcode scanner is connected and ready.");
                return true;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException || ex is ArgumentOutOfRangeException || ex is ArgumentException)
            {
                LogError("Connection failed: ", ex);
            }
            catch (Exception ex)
            {
                LogError("An error occurred: ", ex);
            }
            return false;
        }

        public void Disconnect()
        {
            if (_serialPort?.IsOpen == true)
            {
                try
                {
                    _serialPort.DataReceived -= DataReceivedHandler;
                    _serialPort.Close();
                }
                catch (Exception ex)
                {
                    LogError("An error occurred while disconnecting: ", ex);
                }
            }
            else
            {
                Debug.WriteLine("Port is already closed or not initialized.");
            }
        }

        public bool IsConnected()
        {
            if (!_serialPort?.IsOpen == true)
            {
                return TryReconnect();
            }
            return true;
        }
          
        private bool TryReconnect()
        {
            try
            {
                _serialPort.Open();
                return true;
            }
            catch (Exception ex)
            {
                LogError("Reconnection failed: ", ex);
                return false;
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string data = _serialPort.ReadExisting().Trim();
            DetectModel detectModel = new()
            {
                Text = data,
                Device = Device.BarcodeScanner.ToString()
            };
            SharedEvents.RaiseOnScannerReadDataChangeEvent(detectModel);
        }

        private void MonitorConnection()
        {
            _threadMonitor = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        if (_serialPort == null || !_serialPort.IsOpen)
                        {
                            Disconnect();
                            Connect();
                        }

                        bool isConnected = IsConnected();
                        SharedEventsIpc.RaiseScannerStatusChanged(isConnected, EventArgs.Empty);
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("MonitorPrinterConnection error");
                    }
                    Thread.Sleep(2000);
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _threadMonitor.Start();
        }

        private void SharedEventsIpc_ScannerStatusChanged(object? sender, EventArgs e)
        {
            if (sender is not null && sender is bool scannerIsConnected)
            {
                ScannerStatus scannerStatus = scannerIsConnected ? ScannerStatus.Connected : ScannerStatus.Disconnected;
                MemoryTransfer.SendScannerStatusToUI(_ipc, _index, scannerStatus);
            }
        }

        private void InitializeSerialPort()
        {
            _serialPort = new SerialPort(DeviceSharedValues.ComName, DeviceSharedValues.BitPerSeconds, DeviceSharedValues.Parity, DeviceSharedValues.DataBits, DeviceSharedValues.StopBits);
            _serialPort.DataReceived += DataReceivedHandler;
        }

        private void RestartConnection()
        {
            Disconnect();
            Connect();
        }

        private void LogError(string message, Exception ex)
        {
            Debug.WriteLine($"{message}{ex.Message}");
        }

        public void Dispose()
        {
            Disconnect();
            _threadMonitor?.Abort(); // Ensure the thread is stopped
            _serialPort?.Dispose();
        }
    }

}
