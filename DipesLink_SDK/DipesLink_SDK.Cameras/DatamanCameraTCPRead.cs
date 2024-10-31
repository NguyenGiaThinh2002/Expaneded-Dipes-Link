using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLink_SDK_Cameras
{
    public class DatamanCameraTCPRead:IDisposable
    {
        #region Variables
        private string _serverIP = "127.0.0.1";
        private int _port = 1997;
        private readonly int _TimeOutOfConnection = 1000;
        private readonly int _SendTimeout = 1000;
        private readonly byte _StartPackage = 0x02;
        protected byte _EndPackage = 0x03;
        private readonly bool _IsVersion = false;
        private TcpClient? _TcpClient;
        private NetworkStream? _NetworkStream;
        private StreamReader? _StreamReader;
        private StreamWriter? _StreamWriter;
        private Thread? _ThreadReceiveData;
        public static event EventHandler? OnCamReceiveMessageEvent;
        #endregion

        public DatamanCameraTCPRead(string serverIP, int port, int timeOutOfConnection, int sendTimeout)
        {
            _serverIP = serverIP;
            _port = port;
            _TimeOutOfConnection = timeOutOfConnection;
            _SendTimeout = sendTimeout;
        }
        //public DatamanCameraTCPRead()
        //{

        //}

        public bool Connect()
        {
            try
            {
                _TcpClient = new TcpClient();
                var task = _TcpClient.ConnectAsync(_serverIP, _port);
                task.Wait(_TimeOutOfConnection);
                if (!task.IsCompleted)
                {
                    _ = Disconnect();
                    return false;
                }
                _TcpClient.SendTimeout = _SendTimeout;
                _NetworkStream = _TcpClient.GetStream();
                _StreamReader = new StreamReader(_NetworkStream);
                _StreamWriter = new StreamWriter(_NetworkStream)
                {
                    AutoFlush = true
                };

                uint dummy = 0;
                byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes((uint)5000).CopyTo(inOptionValues, Marshal.SizeOf(dummy));
                BitConverter.GetBytes((uint)1000).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2);
                _TcpClient.Client.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
                _ThreadReceiveData = new Thread(ReceiveData)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                };
                _ThreadReceiveData.Start();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ReceiveData()
        {
            var spinWait = new SpinWait();
            if (_IsVersion)
            {
                Byte[] bytes;
                int counter;
                while (true)
                {
                    try
                    {
                        if (_TcpClient.ReceiveBufferSize > 0)
                        {
                            bytes = new byte[_TcpClient.ReceiveBufferSize];
                            _NetworkStream.Read(bytes, 0, bytes.Length);
                            counter = 0;
                            for (int index = 0; index < bytes.Length; index++)
                            {
                                if (bytes[index] > 0x00)
                                {
                                    counter++;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            string dataRead = Encoding.ASCII.GetString(bytes, 0, counter);
                            RaiseOnCamReceiveMessageEvent(dataRead);
                            if (counter == 0)
                            {
                                OnDisconnect();
                            }
                        }
                        else
                        {
                            spinWait.SpinOnce();
                        }
                    }
                    catch (Exception) { }
                    spinWait.SpinOnce();
                }
            }
            else
            {
                while (true)
                {
                    try
                    {
                        var dataRead = _StreamReader.ReadLine();
                        if (dataRead != null && dataRead.Length > 0)
                        {
                            RaiseOnCamReceiveMessageEvent(dataRead);
                        }
                    }
                    catch (Exception) { }
                    spinWait.SpinOnce();
                }
            }
        }

        public static void RaiseOnCamReceiveMessageEvent(object data)
        {
            OnCamReceiveMessageEvent?.Invoke(data, EventArgs.Empty);
        }

        #region Disconnection
        private async void OnDisconnect()
        {
            await Task.Run(() =>
            {
                Disconnect();
            });
        }
        public bool Disconnect()
        {
            try
            {
                KillThreadReceiveData();

                if (_StreamReader != null)
                {
                    _StreamReader.Close();
                    _StreamReader = null;
                }

                if (_StreamWriter != null)
                {
                    _StreamWriter.Close();
                    _StreamWriter = null;
                }

                if (_NetworkStream != null)
                {
                    _NetworkStream.Close();
                    _NetworkStream = null;
                }

                _TcpClient?.Dispose();
                _TcpClient = null;
            }
            catch (Exception)
            { }
            return true;
        }
        private void KillThreadReceiveData()
        {
            if (_ThreadReceiveData != null && _ThreadReceiveData.IsAlive)
            {
                _ThreadReceiveData.Abort();
                _ThreadReceiveData = null;
            }
        }
        #endregion

        #region IDisposable Support
        private bool _disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Disconnect();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
