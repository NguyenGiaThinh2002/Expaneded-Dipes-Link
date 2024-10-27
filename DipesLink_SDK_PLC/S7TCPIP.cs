using SharedProgram.DeviceTransfer;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using IPCSharedMemory.Controllers;
using IPCSharedMemory;
using static SharedProgram.DataTypes.CommonDataType;
using SharedProgram.Models;
using SharedProgram.Shared;

namespace DipesLink_SDK_PLC
{
    public class S7TCPIP : IPLC_TCPIP
    {
        private TcpClient? _TcpClient;
        private NetworkStream? _NetworkStream;
        private StreamReader? _StreamReader;
        private StreamWriter? _StreamWriter;

        private Thread? _ThreadReceiveData;
        private Thread? _ThreadMonitor;
        private readonly int _Index;
        private CancellationTokenSource _ThreadReceiveDataCts = new();
       
        IPCSharedHelper? _ipc;
        string oldIP = "";
        double oldPort = 0;


        public S7TCPIP(int index, IPCSharedHelper? ipc)
        {
            _Index = index;
            _ipc = ipc;
            MonitorConnection();
            SharedEventsIpc.ControllerStatusChanged += SharedEventsIpc_ControllerStatusChanged;
         
        }

        public static void RaiseOnPODReceiveDataEventEvent(string data)
        {
            SharedEvents.RaiseOnControllerDataChangeEvent(data);
        }

        private void SharedEventsIpc_ControllerStatusChanged(object? sender, EventArgs e)
        {
           
            if (sender == null) return;
            try
            {
                
                bool controllerIsConnected = (bool)sender;
                ControllerStatus controllerSts;
                if (controllerIsConnected)
                {
                    controllerSts = ControllerStatus.Connected;
                }
                else
                {
                    controllerSts = ControllerStatus.Disconnected;
                }
               
                MemoryTransfer.SendControllerStatusToUI(_ipc,_Index, controllerSts);
            }
            catch (Exception)
            {
            }
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
                        if (firstState == false)
                        {
                            Disconnect();
                            Connect();
                        }
                        bool isConnected = IsConnected();
                        if (firstState != isConnected)
                        {
                            firstState = isConnected;
                           
                            //if (firstState)
                            //    Console.WriteLine("Controller is Connected !");
                            //else
                            //    Console.WriteLine("Controller is Disconnected !");
                        }
                        SharedEventsIpc.RaiseControllerStatusChanged(firstState, EventArgs.Empty);
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

        public bool Connect()
        {
            try
            {
                _TcpClient = new TcpClient();
                Task connectTask = _TcpClient.ConnectAsync(DeviceSharedValues.ControllerIP, int.Parse(DeviceSharedValues.ControllerPort.ToString()));
                oldIP = DeviceSharedValues.ControllerIP;
                oldPort = DeviceSharedValues.ControllerPort;
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

                _ThreadReceiveData = new Thread(ReceiveData)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                _ThreadReceiveData.Start();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ReceiveData()
        {
            try
            {
                var buffer = new byte[1024];
                int bytesRead;
                StringBuilder commandBuilder = new();
                if (_NetworkStream is null) return;
                while ((bytesRead = _NetworkStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("data: "+ data);
                    RaiseOnPODReceiveDataEventEvent(data);
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReceiveData via tcp error: " + ex);
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

        public void SendData(string data)
        {
            try
            {
                _StreamWriter?.Write(data);
            }
            catch (Exception) { }
        }

        public bool IsChangeParams()
        {
            if (DeviceSharedValues.ControllerIP == oldIP && DeviceSharedValues.ControllerPort == oldPort)
            {
                return false;
            }
            else
            {
                oldIP = DeviceSharedValues.ControllerIP;
                oldPort = DeviceSharedValues.ControllerPort;
                return true;
            }
        }

        public bool IsConnected()
        {
            try
            {
                if (_TcpClient?.Client != null)
                {
                    if (_TcpClient.Client.Connected && PingIP(DeviceSharedValues.ControllerIP, int.Parse(DeviceSharedValues.ControllerPort.ToString())) && !IsChangeParams())
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

        public bool PingIP(string ipAddress, int port)
        {
            try
            {
                using (var tcpClient = new TcpClient())
                {
                    var result = tcpClient.BeginConnect(ipAddress, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(1000));
                    if (!success)
                    {
                        return false;
                    }

                    tcpClient.EndConnect(result);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
