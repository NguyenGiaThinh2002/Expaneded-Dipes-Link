using Cloudtoid;
using Cognex.DataMan.SDK;
using Cognex.DataMan.SDK.Discovery;
using Cognex.DataMan.SDK.Utils;
using DipesLink_SDK_Cameras.Models;
using IPCSharedMemory;
using IPCSharedMemory.Controllers;
using SharedProgram.DeviceTransfer;
using SharedProgram.Models;
using SharedProgram.Shared;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static Cognex.DataMan.SDK.Discovery.EthSystemDiscoverer;

namespace DipesLink_SDK_Cameras
{
    public class DatamanCamera : Cameras
    {
        #region Properties and Fields

        private int _index;
        private Thread? _threadCameraStatusChecking;
        private bool _IsConnected;
        public bool IsConnected => _IsConnected;
        private EthSystemDiscoverer? _ethSystemDiscoverer = new();
        private List<EthSystemDiscoverer.SystemInfo> _cameraSystemInfoList = new();
        private DataManSystem? _dataManSystem;
        private ResultCollector? _results;
        private ISystemConnector? _connector;
        IPCSharedHelper? _ipc;
        private readonly object _CurrentResultInfoSyncLock = new();
        SystemInfo? _systemInfo;

        #endregion

        public DatamanCamera(int index, IPCSharedHelper? ipc)
        {

            _index = index;
            _ipc = ipc;
            SharedEventsIpc.CameraStatusChanged += SharedEvents_DeviceStatusChanged;
            SharedEvents.OnCameraOutputSignalChange += SharedEvents_OnCameraOutputSignalChange;
            SharedEvents.OnRaiseCameraIPAddress += SharedEvents_OnRaiseCameraIPAddress;
            _ethSystemDiscoverer.SystemDiscovered += new EthSystemDiscoverer.SystemDiscoveredHandler(OnEthSystemDiscovered);
            _ethSystemDiscoverer.Discover();

            _threadCameraStatusChecking = new Thread(CameraStatusChecking)
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            _threadCameraStatusChecking.Start();
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
                SharedFunctions.PrintDebugMessage(ex.Message);
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
                SharedFunctions.PrintDebugMessage(ex.Message);
            }
        }

        private void CameraStatusChecking(object? obj)
        {
            while (true)
            {
                try
                {
                    if (!IsConnected && PingIPCamera(DeviceSharedValues.CameraIP)) // Check if there is a new IP and try connecting
                    {
                        Connect();
                        GetSystemInfo(DeviceSharedValues.CameraIP);
                    }
                    else if (!PingIPCamera(DeviceSharedValues.CameraIP)) // Losing IP will result in loss of connection
                    {
                        _IsConnected = false;
                        Disconnect();
                        SharedEventsIpc.RaiseCameraStatusChanged(_IsConnected, EventArgs.Empty);
                        GetSystemInfo(DeviceSharedValues.CameraIP);
                    }
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    SharedFunctions.PrintDebugMessage(ex.Message);
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

        private void ShowCameraInfo(SystemInfo? systemInfo, bool isConnected)
        {
            try
            {
                CamInfo camInfo;
                CameraInfos camera;
                if (systemInfo is null)
                {
                    camInfo = new();
                    camera = new()
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

                    camera = new()
                    {
                        ConnectionStatus = isConnected,
                        Info = camInfo,
                    };
                }
#if DEBUG
                Console.WriteLine("Name: " + camera.Info.Name);
                Console.WriteLine("Serial Number: " + camera.Info.SerialNumber);
                Console.WriteLine("Type: " + camera.Info.Type);
#endif
                var arrInfo = DataConverter.ToByteArray(camera);
                MemoryTransfer.SendCameraStatusToUI(_ipc, _index, arrInfo);
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintDebugMessage(ex.Message);
            }
        }

        public void Connect()
        {
            try
            {
#if DEBUG
                if (DeviceSharedValues.CameraIP == "127.0.0.1") // Simulate allow local IP addess connected
                {
                    _IsConnected = true;
#if DEBUG
                    Console.WriteLine("Camera is connected !");
#endif
                    SharedEventsIpc.RaiseCameraStatusChanged(_IsConnected, EventArgs.Empty);
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

            var points = GetResultFromXmlString(imageGraphics.FirstOrDefault()); // Get polygon result
            DetectModel detectModel = new()
            {
                Text = Regex.Replace(strResult, @"\r\n", ""),  // Replace special characters in camera data by symbol ';'
                ImagePolyResult = points,
                ImageData = imageData,
                Image = GetImageWithFocusRectangle(imageResult, imageGraphics)
            };

            SharedEvents.RaiseOnCameraReadDataChangeEvent(detectModel); // Send data via Event
        }

        private static Bitmap GetImageWithFocusRectangle(Image? imageResult, List<string> imageGraphics)
        {
            Bitmap bitmap = new(100, 100);
            try
            {
                if (imageResult != null)
                {
                    bitmap = ((Bitmap)imageResult).Clone(new System.Drawing.Rectangle(0, 0, imageResult.Width, imageResult.Height), PixelFormat.Format24bppRgb);
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
                return bitmap.Clone(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
            }
            catch (Exception) { return bitmap; }
        }

        private string? GetResultFromXmlString(string? xmlString)
        {
            if (xmlString == null) return null;
            XDocument xmlDoc = XDocument.Parse(xmlString);
            XNamespace ns = "http://www.w3.org/2000/svg"; // For Sgv Format namespace
            IEnumerable<XElement> polygons = xmlDoc
                        .Descendants(ns + "polygon")
                        .Where(p => (string?)p
                        .Attribute("class") == "result");
            string? pointsValue = polygons.Select(x => x.Attribute("points"))?.FirstOrDefault()?.Value.ToString(); // Get Points value
            return pointsValue;
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
                    SharedEventsIpc.RaiseCameraStatusChanged(_IsConnected, EventArgs.Empty);
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
                    SharedEventsIpc.RaiseCameraStatusChanged(_IsConnected, EventArgs.Empty);
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

        #endregion

    }
}
