using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Discovery;
using Cognex.DataMan.SDK.Utils;
using IPCSharedMemory;
using IPCSharedMemory.Controllers;
using SharedProgram.DeviceTransfer;
using SharedProgram.Models;
using SharedProgram.Shared;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Services.Description;
using System.Windows.Shapes;
using static Cognex.DataMan.SDK.Discovery.EthSystemDiscoverer;
using static SharedProgram.DataTypes.CommonDataType;
using Encoder = System.Drawing.Imaging.Encoder;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using OperationCanceledException = System.OperationCanceledException;
using Rectangle = System.Drawing.Rectangle;

namespace DipesLink_SDK_Cameras
{
    public class DatamanCamera : Cameras, IDisposable
    {
        #region Properties and Fields

        private int _index;
       // private Thread? _threadCameraStatusChecking;
        private bool _IsConnected;
        
        public bool IsConnected =>_IsConnected;

        private EthSystemDiscoverer? _ethSystemDiscoverer = new();
        private List<EthSystemDiscoverer.SystemInfo> _cameraSystemInfoList = new();
        private DataManSystem? _dataManSystem;
        private ResultCollector? _results;
        private ISystemConnector? _connector;
        private IPCSharedHelper? _ipc;
        private readonly object _CurrentResultInfoSyncLock = new();
        private SystemInfo? _systemInfo;
        private DatamanCameraTCPRead? _datamanCameraTCPRead;
       // CameraInfos? _cameraInfos;
       // public CameraInfos? CamInfo => _cameraInfos;
        private bool _disposed = false;

        #endregion

        public DatamanCamera(int index, IPCSharedHelper? ipc)
        {
           // Console.WriteLine("Init dataman cam");
            _index = index;
            _ipc = ipc;
           
            SharedEvents.OnCameraOutputSignalChange += SharedEvents_OnCameraOutputSignalChange;
            SharedEvents.OnRaiseCameraIPAddress += SharedEvents_OnRaiseCameraIPAddress;
            _ethSystemDiscoverer.SystemDiscovered += new EthSystemDiscoverer.SystemDiscoveredHandler(OnEthSystemDiscovered);
            _ethSystemDiscoverer.Discover();

            //_threadCameraStatusChecking = new Thread(CameraStatusChecking)
            //{
            //    IsBackground = true,
            //    Priority = ThreadPriority.Normal
            //};
            //  _threadCameraStatusChecking.Start();
            Task.Run(() => { CameraStatusChecking(); });
           
        }

        private void DatamanCameraTCPRead_OnCamReceiveMessageEvent(object? sender, EventArgs e)
        {
            try
            {
                if(sender == null) return;
                string getStringVar = ((string)sender).Replace("\r\n", "");
                if (getStringVar.Equals("#"))
                {
                    getStringVar = "";
                }
                else
                {
                    getStringVar = getStringVar.Replace("#", "");
                }
                var bitmap = new Bitmap(100, 100);
                var detectModel = new DetectModel
                {
                    Text = getStringVar,
                    Device = "Camera"
                };
                if (DeviceSharedValues.DatamanReadMode == SharedProgram.DataTypes.CommonDataType.DatamanReadMode.MultiRead) // MultiRead Mode
                {
                    SharedEvents.RaiseOnCameraReadDataChangeEvent(detectModel);
                   
                    SharedFunctions.PrintConsoleMessage($"MutiRead Data: {detectModel.Text}");
                }
            }
            catch (Exception)
            {
            }
        }

        private void SharedEvents_OnRaiseCameraIPAddress(object? sender, EventArgs e)
        {
            try
            {
                string? ipFromAppSetting = sender as string;
                Console.WriteLine("ipFromAppSetting" + ipFromAppSetting);
                //Console.WriteLine("ipFromDiscover" + _systemInfo.IPAddress.ToString());
                GetSystemInfo(ipFromAppSetting);
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }
        }

        private void GetSystemInfo(string? ipFromAppSetting)
        {
            try
            {
                _systemInfo = _cameraSystemInfoList.FirstOrDefault(x => x.IPAddress.ToString().Equals(ipFromAppSetting));
                ShowCameraInfo(_systemInfo, IsConnected);
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }
        }

        CancellationTokenSource? _ctsCamChecking;
        private void CameraStatusChecking()
        {
            _ctsCamChecking = new CancellationTokenSource();
            var token = _ctsCamChecking.Token;
            while (true)
            {
                try
                {
                   
                        token.ThrowIfCancellationRequested();
                  
                   // if (_dataManSystem == null) continue;
                    if (!IsConnected && PingIPCamera(DeviceSharedValues.CameraIP)) // Check if there is a new IP and try connecting
                    {

                        #region Connect to Read with tcp
                        if (_datamanCameraTCPRead==null)
                        {
                            _datamanCameraTCPRead = new DatamanCameraTCPRead(DeviceSharedValues.CameraIP, 23, 1000, 1000);
                            _datamanCameraTCPRead?.Connect();
                            DatamanCameraTCPRead.OnCamReceiveMessageEvent -= DatamanCameraTCPRead_OnCamReceiveMessageEvent;
                            DatamanCameraTCPRead.OnCamReceiveMessageEvent += DatamanCameraTCPRead_OnCamReceiveMessageEvent;
                        }
                        #endregion

                        Connect();
                        GetSystemInfo(DeviceSharedValues.CameraIP);
                    }
                    else if (!PingIPCamera(DeviceSharedValues.CameraIP)) // Losing IP will result in loss of connection
                    {
                        _IsConnected = false;
                        DeviceSharedValues.IsCameraConnected = false; // Save to global variable
                        #region Disconnect Dataman tcp read
                        if (_datamanCameraTCPRead != null)
                        {
                            _datamanCameraTCPRead?.Disconnect();
                            DatamanCameraTCPRead.OnCamReceiveMessageEvent -= DatamanCameraTCPRead_OnCamReceiveMessageEvent;
                            _datamanCameraTCPRead?.Dispose();
                           _datamanCameraTCPRead = null;
                        }
                        #endregion

                        Disconnect();
                       // SharedEventsIpc.RaiseCameraStatusChanged(_IsConnected, EventArgs.Empty);
                        GetSystemInfo(DeviceSharedValues.CameraIP);
                    }
                    Thread.Sleep(1000);
                }
                catch (OperationCanceledException)
                {
                    // Task was canceled
                    break;
                }
                catch (Exception ex)
                {
                    SharedFunctions.PrintConsoleMessage(ex.Message);
                }
            }
        }

        private void SharedEvents_OnCameraOutputSignalChange(object? sender, EventArgs e)
        {
            OutputAction();
        }

        private void SharedEvents_DeviceStatusChanged(object? sender, EventArgs e)
        {
            try
            {
                if (sender == null) return;
                bool camIsConnected = (bool)sender;
            }
            catch (Exception) { }
        }


        public CameraInfos CameraInfo { get; set; }
        public  void ShowCameraInfo(SystemInfo? systemInfo, bool isConnected)
        {
            try
            {
                CamInfo camInfo;
              //  CameraInfos camera;
                if (systemInfo is null)
                {
                    camInfo = new();
                    CameraInfo = new()
                    {
                        ConnectionStatus = isConnected,
                        Info = camInfo,
                    };
                }
                else
                {
                    camInfo = new()
                    {
                        SerialNumber = systemInfo.SerialNumber,
                        Name = systemInfo.Name,
                        Type = systemInfo.Type,
                    };

                    CameraInfo = new()
                    {
                        ConnectionStatus = isConnected,
                        Info = camInfo,
                    };
                }
#if DEBUG
                //Console.WriteLine("Name: " + camera.Info.Name);
                //Console.WriteLine("Serial Number: " + camera.Info.SerialNumber);
                //Console.WriteLine("Type: " + camera.Info.Type);
#endif
                byte[] arrInfo = DataConverter.ToByteArray(CameraInfo);
             //   Console.WriteLine("ádfffffffadsgaddddd");
              
               
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
              
            }
        }
       

        public void Connect()
        {
            try
            {
#if DEBUG
                if (DeviceSharedValues.CameraIP == "1.1.1.1") // Simulate allow local IP addess connected
                {
                    _IsConnected = true;
#if DEBUG
                 //   Console.WriteLine("Camera is connected !");
#endif
                  //  SharedEventsIpc.RaiseCameraStatusChanged(_IsConnected, EventArgs.Empty);
                    return;
                }
#endif

                // Console.WriteLine("Camera name------------------------------------: "+ _cameraSystemInfoList[0].Name);

                if (_cameraSystemInfoList.Count < 1) return;

                EthSystemDiscoverer.SystemInfo? currentCamera = _cameraSystemInfoList
                    .Where(x => x.IPAddress.ToString() == DeviceSharedValues.CameraIP)
                    .ToList()
                    .FirstOrDefault();

                EthSystemConnector? conn = new(currentCamera?.IPAddress);

                if (conn.Address != null)
                {
                    _connector = conn;
                    _dataManSystem = new(_connector) { DefaultTimeout = 1000 };

                    _dataManSystem.SystemConnected += new SystemConnectedHandler(OnSystemConnected);
                    _dataManSystem.SystemDisconnected += new SystemDisconnectedHandler(OnSystemDisconnected);

                    ResultTypes resultTypes = ResultTypes.ReadXml | ResultTypes.Image | ResultTypes.ImageGraphics;

                    _results = new(_dataManSystem, resultTypes);
                    _results.ComplexResultCompleted += ResultCollector_ComplexResultCompleted;
                    _dataManSystem.Connect();
                    _dataManSystem.SetResultTypes(resultTypes);
                }
                if (conn.Address == null)
                {
                    _dataManSystem?.Dispose();
                    _dataManSystem = null;
                }
            }
            catch (Exception)
            {
#if DEBUG
                Console.WriteLine("Camera connection error !");
#endif
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_dataManSystem == null || _dataManSystem.State != ConnectionState.Connected)
                    return;

                _dataManSystem.Disconnect();
                CleanupConnection();

                _results?.ClearCachedResults();
                _results = null;
            }
            catch { }
        }

        private void CleanupConnection()
        {
            if (null != _dataManSystem)
            {
                _dataManSystem.SystemConnected -= OnSystemConnected;
                _dataManSystem.SystemDisconnected -= OnSystemDisconnected;
            }

            _connector = null;
            _dataManSystem = null;
        }

        #region Event

        private void OnEthSystemDiscovered(EthSystemDiscoverer.SystemInfo systemInfo)
        {

            Console.WriteLine("OnEthSystemDiscovered Found: " + systemInfo.IPAddress.ToString());
            //  IPFoundByEthDiscovered = systemInfo.IPAddress.ToString();
            //Console.WriteLine("Ip Set: " + DeviceSharedValues.CameraIP);

            bool hasExist = CheckCameraInfoHasExist(systemInfo, _cameraSystemInfoList);
            if (!hasExist)
            {
                _cameraSystemInfoList.Add(systemInfo);
            }
            GetSystemInfo(DeviceSharedValues.CameraIP);
        }

        private void ResultCollector_ComplexResultCompleted(object sender, ComplexResult complexResult)
        {
            ResultCollector resultCollector = (ResultCollector)sender;
            FieldInfo? field = resultCollector.GetType().GetField("_dmSystem", BindingFlags.NonPublic | BindingFlags.Instance);

            System.Drawing.Image? imageResult = null;
            string strResult = "";
            List<string> imageGraphics = new();

            byte[] imageData = Array.Empty<byte>(); ;
            byte[] resultData;

            lock (_CurrentResultInfoSyncLock)
            {
                foreach (SimpleResult simpleResult in complexResult.SimpleResults)
                {
                    switch (simpleResult.Id.Type)
                    {
                        case ResultTypes.Image: //Image taken from camera
                            imageData = simpleResult.Data;
                            imageResult = SharedFunctions.GetImageFromImageByte(simpleResult.Data);
                            break;

                        case ResultTypes.ImageGraphics: //The polygon image identifies the camera's object, in addition to the information the code reads (this is almost enough).
                            resultData = simpleResult.Data;
                            imageGraphics.Add(simpleResult.GetDataAsString());
                            break;

                        case ResultTypes.ReadString: //Data read from code
                            strResult = Encoding.UTF8.GetString(simpleResult.Data);
                            break;

                        case ResultTypes.ReadXml: // Data read from code (taken from xml)
                            strResult = SharedFunctions.GetReadStringFromResultXml(simpleResult.GetDataAsString());
                            break;

                        default:
                            break;
                    }
                }
            }
            string getStringVar = strResult.Replace("\r\n", "");
            if (getStringVar.Equals("#"))
            {
                getStringVar = "";
            }
            else
            {
                getStringVar = getStringVar.Replace("#", "");
            }
            DetectModel detectModel = new()
            {
                Text = getStringVar,  // Replace special characters in camera data by symbol ';'
                ImageBytes = GetImageWithFocusRectangle(imageResult, imageGraphics),
                Image = new(SharedFunctions.GetImageFromImageByte(imageData)),
                Device = Device.Camera.ToString(),
            };
            //SharedFunctions.PrintConsoleMessage("Image size: " + detectModel.ImageBytes.Length.ToString());
            if(DeviceSharedValues.DatamanReadMode == SharedProgram.DataTypes.CommonDataType.DatamanReadMode.Basic)
            {
                SharedEvents.RaiseOnCameraReadDataChangeEvent(detectModel); // Send data via Event
                Console.WriteLine($"Basic Read Data: {detectModel.Text}");
            }
        }

        private static byte[] GetImageWithFocusRectangle(Image? imageResult, List<string> imageGraphics)
        {
            Bitmap bitmap = new(100, 100);
            try
            {
                if (imageResult != null)
                {
                    bitmap = ((Bitmap)imageResult).Clone(new Rectangle(0, 0, imageResult.Width, imageResult.Height), PixelFormat.Format24bppRgb);
                }
                else
                {
                    using var g = Graphics.FromImage(bitmap);
                    g.Clear(Color.White);
                }
                if (imageGraphics.Count > 0)
                {
                    using var graphicsImage = Graphics.FromImage(bitmap);
                    foreach (string graphics in imageGraphics)
                    {
                        ResultGraphics resultGraphics = GraphicsResultParser.Parse(graphics, new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
                        ResultGraphicsRenderer.PaintResults(graphicsImage, resultGraphics);
                    }
                }
                // Compress and save the image to a MemoryStream
                using (var memoryStream = new MemoryStream())
                {
                    ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);
                    EncoderParameters encoderParams = new(1);
                    encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 50L); // Adjust quality as needed
                    bitmap.Save(memoryStream, jpegCodec, encoderParams);
                   // Bitmap compressedBitmap = new(memoryStream);
                    return memoryStream.ToArray();//compressedBitmap.Clone(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
                }
              //  return bitmap.Clone(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
            }
            catch (Exception) { return Array.Empty<byte>(); }
        }

        private static ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private void OnSystemDisconnected(object sender, EventArgs args)
        {
#if DEBUG
            Console.WriteLine("Camera is disconnected !");
#endif
            if (sender is DataManSystem)
            {
                DataManSystem? dataManSystem = sender as DataManSystem;
                if (dataManSystem?.Connector is EthSystemConnector ethSystemConnector)
                {
                    _IsConnected = false;
                  //  SharedEventsIpc.RaiseCameraStatusChanged(_IsConnected, EventArgs.Empty);
                }
            }
        }

        private void OnSystemConnected(object sender, EventArgs args)
        {

#if DEBUG
            Console.WriteLine("Camera is connected !");
#endif

            if (sender is DataManSystem)
            {
                DataManSystem? dataManSystem = sender as DataManSystem;
                if (dataManSystem?.Connector is EthSystemConnector ethSystemConnector)
                {
                    _IsConnected = true;
                  //  SharedEventsIpc.RaiseCameraStatusChanged(_IsConnected, EventArgs.Empty);
                }
            }
        }

        #endregion

        #region Functions

        private static bool PingIPCamera(string ipAddress) // Function to check whether the IP address exists or not
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

        private static bool CheckCameraInfoHasExist(EthSystemDiscoverer.SystemInfo cameraInfoNeedCheck, List<EthSystemDiscoverer.SystemInfo> cameraSystemInfoList)
        {
            foreach (object systemInfo in cameraSystemInfoList)
            {

                if (systemInfo is EthSystemDiscoverer.SystemInfo)
                {
                    EthSystemDiscoverer.SystemInfo? ethSystemInfo = systemInfo as EthSystemDiscoverer.SystemInfo;
                    if (ethSystemInfo?.SerialNumber == cameraInfoNeedCheck.SerialNumber)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Operation

        public bool ManualInputTrigger()
        {
            try
            {
                DmccResponse? response = _dataManSystem?.SendCommand("TRIGGER ON");
                return true;
            }
            catch (Exception) { return false; }
        }

        public bool OutputAction()
        {
            try
            {
                DmccResponse? response = _dataManSystem?.SendCommand("OUTPUT.USER1");
                return true;
            }
            catch (Exception) { return false; }
        }


        #region Dispose

        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception)
            {
            }
            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                try
                {
                    // Dispose managed resources
                    
                    //_threadCameraStatusChecking?.Interrupt();
                    //_threadCameraStatusChecking = null;

                    if (_ethSystemDiscoverer != null)
                    {
                        _ethSystemDiscoverer.SystemDiscovered -= OnEthSystemDiscovered;
                        _ethSystemDiscoverer = null;
                    }

                    if (_datamanCameraTCPRead != null)
                    {
                        _datamanCameraTCPRead.Disconnect();
                        DatamanCameraTCPRead.OnCamReceiveMessageEvent -= DatamanCameraTCPRead_OnCamReceiveMessageEvent;
                        _datamanCameraTCPRead.Dispose();
                        _datamanCameraTCPRead = null;
                    }
                    if(_results != null)
                        _results.ComplexResultCompleted -= ResultCollector_ComplexResultCompleted;
                    SharedEventsIpc.CameraStatusChanged -= SharedEvents_DeviceStatusChanged;
                    SharedEvents.OnCameraOutputSignalChange -= SharedEvents_OnCameraOutputSignalChange;
                    SharedEvents.OnRaiseCameraIPAddress -= SharedEvents_OnRaiseCameraIPAddress;

                    if(_ctsCamChecking != null)
                    {
                        _ctsCamChecking.Cancel();
                        _ctsCamChecking.Dispose();
                        _ctsCamChecking = null;
                    }

                    //if (_ipc != null)
                    //{
                    //    try
                    //    {
                    //        _ipc.Dispose();
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        Console.WriteLine($"Error disposing IPCSharedHelper: {ex.Message}");
                    //    }
                    //    _ipc = null;
                    //}

                    Disconnect();
                   // ShowCameraInfo(null,false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error Dispose camera dataman: {ex}");
                }
               
            }

            // Dispose unmanaged resources

            _disposed = true;
        }

        ~DatamanCamera()
        {
            Dispose(false);
        }

        #endregion
        #endregion

    }
}
