using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using System.Text.RegularExpressions;
using SharedProgram.Shared;
using SharedProgram.Models;
using IPCSharedMemory;
using IPCSharedMemory.Controllers;
using System.Reflection;
using SharedProgram.DeviceTransfer;
using static SharedProgram.DataTypes.CommonDataType;
using System.Windows;
using System.Data;

namespace DipesLink_SDK_BarcodeScanner
{
    public class RS232BarcodeScanner : IBarcodeScanner
    {
        private static SerialPort _serialPort;
        private Thread? _ThreadMonitor;
        //private static string _comName = "COM3";
        //private static int _bitPerSecond = 9600;
        //private static Parity _parity = Parity.None;
        //private static int _dataBits = 8;
        //private static StopBits _stopBits = StopBits.One;

        private readonly int _Index;
        IPCSharedHelper? _ipc;

        public RS232BarcodeScanner(int index, IPCSharedHelper? ipc)
        {
            _Index = index;
            _ipc = ipc;
            MonitorConnection();
            SharedEventsIpc.ScannerStatusChanged += SharedEventsIpc_ScannerStatusChanged;
            SharedEvents.OnScannerParametersChange += SharedEvents_OnScannerParametersChange;
        }

        private void SharedEvents_OnScannerParametersChange(object? sender, EventArgs e)
        {
            try
            {              
                Disconnect();
                Connect();
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }
        }
        public bool Connect()
        {

            _serialPort = new SerialPort(DeviceSharedValues.ComName, DeviceSharedValues.BitPerSeconds, DeviceSharedValues.Parity, DeviceSharedValues.DataBits, DeviceSharedValues.StopBits);
            _serialPort.DataReceived += DataReceivedHandler;

            try
            {
                _serialPort.Open();
                //MessageBox.Show("Barcode scanner is connected and ready.");
                Debug.WriteLine("Barcode scanner is connected and ready.");
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine("Access to the port is denied: " + ex.Message);
            }
            catch (IOException ex)
            {
                Debug.WriteLine("I/O error occurred: " + ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine("Invalid port parameters: " + ex.Message);
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine("Invalid port name: " + ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occurred: " + ex.Message);
            }
            return false;
        }

        public void Disconnect()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    _serialPort.DataReceived -= DataReceivedHandler; // Hủy đăng ký sự kiện
                    _serialPort.Close(); // Đóng cổng serial
                    _serialPort.Dispose(); // Giải phóng tài nguyên
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("An error occurred while disconnecting: " + ex.Message);
                }
            }
            else
            {
                Debug.WriteLine("Port is already closed or not initialized.");
            }
        }

        public bool IsConnected()
        {
            if (!_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Open();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Reconnection failed: " + ex.Message);
                }
                return false;
            }
            else
            {
                return true;
            }
        }
        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            string data = _serialPort.ReadExisting();
            string text = data.Trim();
            // Process the received data here (for now, we'll just log it)
            //Debug.WriteLine($"Data received: {text}");
            //MessageBox.Show("Data Received: " + text);

            DetectModel detectModel = new()
            {
                //Text = Regex.Replace(text, @"\r\n", ""),  // Replace special characters in camera data by symbol ';'
                Text = text,
                Device = "Barcode Scanner"
                //ImageBytes = GetImageWithFocusRectangle(imageResult, imageGraphics),
                //Image = new(SharedFunctions.GetImageFromImageByte(imageData)),
            };
            SharedEvents.RaiseOnScannerReadDataChangeEvent(detectModel); // Send data via Event
        }

        private void MonitorConnection()
        {
            _ThreadMonitor = new(() =>
            {
                bool firstState = true;
                while (true)
                {
                    try
                    {
                        if (_serialPort == null || !_serialPort.IsOpen)
                        {
                            Disconnect();
                            Connect();
                        }
                        firstState = IsConnected();                  
                        SharedEventsIpc.RaiseScannerStatusChanged(firstState, EventArgs.Empty);
                    }
                    catch (Exception) { Debug.WriteLine("MonitorPrinterConnection error"); }
                    Thread.Sleep(2000);
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _ThreadMonitor.Start();
        }

        private void SharedEventsIpc_ScannerStatusChanged(object? sender, EventArgs e)
        {

            if (sender == null) return;
            try
            {

                bool scannerIsConnected = (bool)sender;
                ScannerStatus scannerSts;
                if (scannerIsConnected)
                {
                    scannerSts = ScannerStatus.Connected;
                }
                else
                {
                    scannerSts = ScannerStatus.Disconnected;
                }

                MemoryTransfer.SendScannerStatusToUI(_ipc, _Index, scannerSts);
            }
            catch (Exception)   
            {
            }
        }
    }
}
