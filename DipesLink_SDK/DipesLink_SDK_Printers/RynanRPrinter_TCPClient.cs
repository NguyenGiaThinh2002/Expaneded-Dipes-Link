using IPCSharedMemory;
using IPCSharedMemory.Controllers;
using SharedProgram.DeviceTransfer;
using SharedProgram.Models;
using SharedProgram.Shared;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using static SharedProgram.DataTypes.CommonDataType;
using static SharedProgram.Shared.SharedEvents;

namespace DipesLink_SDK_Printers
{
    public class RynanRPrinterTCPClient : IPrinter
    {
        private TcpClient? _TcpClient;
        private NetworkStream? _NetworkStream;
        private StreamReader? _StreamReader;
        private StreamWriter? _StreamWriter;

        private Thread? _ThreadReceiveData;
        private Thread? _ThreadMonitorPrinter;
        private readonly int _Index;
        private readonly byte _StartPackage = 0x02;
        private readonly byte _EndPackage = 0x03;
        private readonly ConcurrentQueue<int> _PackagesQueue = new();
        private CancellationTokenSource _ThreadReceiveDataCts = new();
        IPCSharedHelper? _ipc;
        private readonly ReceivedType _ReceivedType = ReceivedType.R20;
        public enum ReceivedType
        {
            R20,
            Unknown
        }
        string oldIP = "";
        double oldPort = 0;

        public RynanRPrinterTCPClient(int index, IPCSharedHelper? ipc)
        {
            _Index = index;
            _ipc = ipc;
            MonitorPrinterConnection();
            SharedEventsIpc.PrinterStatusChanged += SharedEvents_PrinterStatusChanged;
        }

        public int GetIndex()
        {
            return _Index;
        }

        private static bool PingIPPrinter(string ipAddress)
        {
            try
            {
                Ping pingSender = new();
                PingReply reply = pingSender.Send(ipAddress);

                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsConnected()
        {
            try
            {
                if (_TcpClient?.Client != null)
                {
                    if (_TcpClient.Client.Connected && PingIPPrinter(DeviceSharedValues.PrinterIPs[_Index]) && !IsChangeParams())
                    {
                        return _TcpClient.Client.Connected;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private void MonitorPrinterConnection()
        {

            _ThreadMonitorPrinter = new(() =>
            {
                bool firstState = false;
                while (true)
                {
                    try
                    {
                        bool isConnected = IsConnected();
                        if (!isConnected ) // && firstState == false
                        {
                           
                            Disconnect();
                            Connect();
                        }                        
                        //if (firstState == false)
                        //{                          
                        //}
                        
                        if (firstState != isConnected)
                        {
                            firstState = isConnected;
                            SharedEventsIpc.RaisePrinterStatusChanged(firstState, EventArgs.Empty);

                            if (firstState)
                                Console.WriteLine("Printer is Connected !");
                            else
                                Console.WriteLine("Printer is Disconnected !");
                        }
                    }
                    catch (Exception) { Debug.WriteLine("MonitorPrinterConnection error"); }
                    Thread.Sleep(2000);
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _ThreadMonitorPrinter.Start();

        }

        private bool IsChangeParams()
        {
            if (DeviceSharedValues.PrinterIPs[_Index] == oldIP && DeviceSharedValues.PrinterPorts[_Index] == oldPort)
            {
                return false;
            }
            else
            {
                oldIP = DeviceSharedValues.PrinterIPs[_Index];
                oldPort = DeviceSharedValues.PrinterPorts[_Index];
                return true;
            }
        }

        public bool Connect()
        {
            try
            {
                _TcpClient = new TcpClient();
                Task connectTask = _TcpClient.ConnectAsync(DeviceSharedValues.PrinterIPs[_Index], int.Parse(DeviceSharedValues.PrinterPorts[_Index].ToString()));
                oldIP = DeviceSharedValues.PrinterIPs[_Index];
                oldPort = DeviceSharedValues.PrinterPorts[_Index];

                SharedFunctions.PrintConsoleMessage($"printerIP + {_Index} + {DeviceSharedValues.PrinterIPs[_Index]}");

                SharedFunctions.PrintConsoleMessage($"Port + {_Index} + {DeviceSharedValues.PrinterPorts[_Index].ToString()}");

                connectTask.Wait(3000);
                if (!connectTask.IsCompleted)
                {
                    Disconnect();
                    return false;
                }
                _TcpClient.SendTimeout = 1000;
                _NetworkStream = _TcpClient.GetStream();
                _StreamReader = new StreamReader(_NetworkStream);
                _StreamWriter = new StreamWriter(_NetworkStream) { AutoFlush = true }; // AutoFlush use for realtime write

                /* The code checks whether the connection is virtual or not, For example, when the connection is lost but there is no notification of lost connection  
                 * by sending periodic keep-alive packets */
                uint dummy = 0;
                byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
                BitConverter.GetBytes((uint)1000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
                _TcpClient.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
                //if(_Index == 0) { }
                _ThreadReceiveData = new Thread(ReceiveData)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                _ThreadReceiveData.Start();

                SharedFunctions.PrintConsoleMessage($"printer {_Index} is connected");
                //MessageBox.Show($"printer {_Index} is connected");
                return true;
            }
            catch (Exception)
            {
                SharedFunctions.PrintConsoleMessage($"printer {_Index} is disconnected");
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                KillThreadReceiveData();

                _StreamReader?.Close();
                _StreamWriter?.Close();
                _NetworkStream?.Close();
                _TcpClient?.Close();

                _TcpClient?.Dispose();
                _StreamReader?.Dispose();
                _StreamWriter?.Dispose();
                _NetworkStream?.Dispose();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void SplitCommands(string data)
        {
            // STX and ETX in ASCII
            char STX = '\u0002';
            char ETX = '\u0003';

            int start = data.IndexOf(STX);
            while (start != -1)
            {
                int end = data.IndexOf(ETX, start + 1);
                if (end != -1)
                {
                    string command = data.Substring(start + 1, end - start - 1);
                    RaiseOnPODReceiveDataEventEvent(new PODDataModel { Text = command });
                    start = data.IndexOf(STX, end + 1);
                }
                else
                {
                    break;
                }
            }
        }

        private void ReceiveData()
        {
            try
            {
                if (_ReceivedType == ReceivedType.R20)
                {

                    var buffer = new byte[1024];
                    int bytesRead;
                    StringBuilder commandBuilder = new();
                    while ((bytesRead = _NetworkStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        SplitCommands(data);
                        Thread.Sleep(1);
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReceiveData via tcp error: " + ex);
            }
        }

        public void SendData(string data)
        {
            try
            {
                _StreamWriter?.Write((char)_StartPackage + data + (char)_EndPackage);
            }
            catch (Exception) { }
        }

        private void KillThreadReceiveData()
        {
            try
            {
                if (_ThreadReceiveData != null && _ThreadReceiveData.IsAlive)
                {
                    _ThreadReceiveDataCts.Cancel();
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("KillThreadReceiveData fail");
            }

        }

        public void RaiseOnPODReceiveDataEventEvent(PODDataModel data)
        {
            // Them _Index
            //data.Index = _Index;     
            //SharedEvents.RaiseOnPrinterDataChangeEvent(data);
            if(_Index == 0)
            {
                RaiseOnPrinterDataChangeEvent(data);
            }
            else
            {
                PrinterDataHandler.RaiseOnPrinterDataChangeEvent(data, _Index);
            }
        }

        private void SharedEvents_PrinterStatusChanged(object? sender, EventArgs e)
        {
            if (sender == null) return;
            try
            {
                bool printerIsConnected = (bool)sender;
                // thinh sua 
                bool isConnected = IsConnected();
                printerIsConnected = isConnected;
                PrinterStatus printerSts;
                if (printerIsConnected)
                {
                    printerSts = PrinterStatus.Connected;
                }
                else
                {
                    printerSts = PrinterStatus.Disconnected;
                }
                MemoryTransfer.SendPrinterStatusToUI(_ipc, _Index, printerSts);
            }
            catch (Exception)
            {
            }
        }

    }
}
