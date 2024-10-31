using DipesLink_SDK_BarcodeScanner;
using DipesLink_SDK_Cameras;
using DipesLink_SDK_PLC;
using DipesLink_SDK_Printers;
using IPCSharedMemory;
using SharedProgram.DeviceTransfer;
using SharedProgram.Models;
using SharedProgram.Shared;

namespace DipesLinkDeviceTransfer
{
    public partial class Program
    {
        public static DatamanCameraTCPRead? DatamanCameraTCPRead;
        public static DatamanCamera? DatamanCameraDeviceHandler;
        public static RynanRPrinterTCPClient? RynanRPrinterDeviceHandler;
        public static S7TCPIP? ControllerDeviceHandler;
        public static RS232BarcodeScanner? BarcodeScannerHandler;
        public static string? keyStep;
        private IPCSharedHelper? _ipcDeviceToUISharedMemory_DT;
        private IPCSharedHelper? _ipcUIToDeviceSharedMemory_DT;
        private IPCSharedHelper? _ipcDeviceToUISharedMemory_DB;
        private IPCSharedHelper? _ipcDeviceToUISharedMemory_RD;

        private void InitInstanceIPC()
        {
            _ipcDeviceToUISharedMemory_DT = new(JobIndex, "DeviceToUISharedMemory_DT", SharedValues.SIZE_1MB); // data
            _ipcUIToDeviceSharedMemory_DT = new(JobIndex, "UIToDeviceSharedMemory_DT", SharedValues.SIZE_1MB); // data
            _ipcDeviceToUISharedMemory_DB = new(JobIndex, "DeviceToUISharedMemory_DB", SharedValues.SIZE_200MB); // database
            _ipcDeviceToUISharedMemory_RD = new(JobIndex, "DeviceToUISharedMemory_RD", SharedValues.SIZE_50MB); // Realtime data
        }

        private static void GetArgumentList(string[] args)
        {
            try
            {
                DeviceSharedValues.Index = int.Parse(args[0]);
            }
            catch (Exception)  // for Device transfer only 
            {
#if DEBUG
                DeviceSharedValues.Index = 0;
                DeviceSharedValues.CameraIP = "192.168.15.146";
                DeviceSharedValues.PrinterIP = "192.168.15.51"; //192.168.3.52
                DeviceSharedValues.PrinterPort = 2030;
                DeviceSharedValues.ControllerIP = "127.0.0.1";
                DeviceSharedValues.ControllerPort = 2001;
                DeviceSharedValues.VPObject = new()
                {
                    FailedDataSentToPrinter = "Failure",
                    VerifyAndPrintBasicSentMethod = false,
                    PrintFieldForVerifyAndPrint = new()
                    {
                        new() { Index = 1,Type=SharedProgram.Models.PODModel.TypePOD.FIELD, PODName="", Value=""}
                    }
                };
#endif
            }

        }

        static CancellationTokenSource cts;
        static async Task Main(string[] args)
        {
            try
            {
                GetArgumentList(args);
                JobIndex = DeviceSharedValues.Index;
                new Program().NonStaticMainProgram();
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }

            cts = new CancellationTokenSource();
            await HoldConsoleAsync(cts.Token);

        }
        static async Task HoldConsoleAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    keyStep = Console.ReadLine();
                    await Task.Delay(10, token);
                }
            }
            catch (OperationCanceledException ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }
        }

        private void ProcessExitHandler(object? sender, EventArgs e)
        {
            cts?.Cancel();
            ReleaseResource();
        }

        public void NonStaticMainProgram()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExitHandler);
            InitInstanceIPC();
            ListenConnectionParam();
            AlwaySendPrinterOperationToUI();
          //  DatamanCameraDeviceHandler = new(JobIndex, _ipcDeviceToUISharedMemory_DT);
            RynanRPrinterDeviceHandler = new(JobIndex, _ipcDeviceToUISharedMemory_DT);
            ControllerDeviceHandler = new(JobIndex, _ipcDeviceToUISharedMemory_DT);
            BarcodeScannerHandler = new(JobIndex, _ipcDeviceToUISharedMemory_DT);
            CameraConnectionMonitor();
            InitEvents();


#if DEBUG

            Thread t = new(() =>
            {
                while (true)
                {
                    if (keyStep == "loaddb")
                    {
                        Console.WriteLine("loaddb");
                        DeviceSharedValues.ActionButtonType = SharedProgram.DataTypes.CommonDataType.ActionButtonType.LoadDB;
                        //ActionButtonFromUIProcessingAsync();
                        keyStep = "";
                    }
                    if (keyStep == "start")
                    {
                        Console.WriteLine("start");
                        DeviceSharedValues.ActionButtonType = SharedProgram.DataTypes.CommonDataType.ActionButtonType.Start;
                        // ActionButtonFromUIProcessingAsync();
                        keyStep = "";
                    }

                    if ((keyStep == "stop"))
                    {
                        _ = StopProcessAsync();
                        keyStep = "";
                    }
                    Thread.Sleep(1000);
                }
            });

            t.Start();
#endif
        }

        private void DiposeDatamanCamera()
        {

            try
            {
                if (DatamanCameraDeviceHandler == null) return;
                DatamanCameraDeviceHandler.Disconnect();
                DatamanCameraDeviceHandler.Dispose();
                DatamanCameraDeviceHandler = null;
                //if (DatamanCameraDeviceHandler == null)
                //{
                //  //  SharedEventsIpc.RaiseCameraStatusChanged(false, EventArgs.Empty);
                // //   Console.WriteLine("Dataman is diposed!");
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loi e: {ex}");
            }
        }

        private int _countTimeOutConnection = 0;
        private void CameraConnectionMonitor()
        {
            Task.Run(async () =>
            {
                try
                {
                   // SharedEventsIpc.CameraStatusChanged += SharedEvents_DeviceStatusChanged;
                    while (true)
                    {
                        switch (DeviceSharedValues.CameraSeries)
                        {
                            case SharedProgram.DataTypes.CommonDataType.CameraSeries.Unknown:
                                Console.WriteLine("No camera connection!");

                                break;
                            case SharedProgram.DataTypes.CommonDataType.CameraSeries.Dataman:
                                 Console.WriteLine("Dataman Camera Connected !");
                             //   Console.WriteLine("Vo day làm con mẹ ji !");
                                if (DatamanCameraDeviceHandler == null)
                                {
                                    DatamanCameraDeviceHandler = new(JobIndex, _ipcDeviceToUISharedMemory_DT);
                                }
                                else
                                {
                                    if (DatamanCameraDeviceHandler?.IsConnected == true)
                                    {
                                      //  DeviceSharedValues.IsCameraConnected = true;
                                       await Console.Out.WriteLineAsync($"Dataman connected");
                                        DeviceSharedValues.CameraInfos = DatamanCameraDeviceHandler.CameraInfo; // get status
                                    }
                                    else
                                    {
                                        _countTimeOutConnection++;
                                        if (_countTimeOutConnection >= 5)
                                        {
                                            DiposeDatamanCamera();
                                            _countTimeOutConnection = 0;
                                        }
                                        //    DeviceSharedValues.IsCameraConnected = false;
                                        //  SharedEventsIpc.RaiseCameraStatusChanged(false, EventArgs.Empty);
                                        await Console.Out.WriteLineAsync($"Dataman disconnected");
                                    }
                                }

                                break;
                            case SharedProgram.DataTypes.CommonDataType.CameraSeries.InsightVision:
                                // Console.WriteLine("Ket noi insight vision");
                               // DeviceSharedValues.IsCameraConnected = false;

                                // simulate camera info
                                DeviceSharedValues.CameraInfos = new CameraInfos
                                {
                                    ConnectionStatus = false,
                                    Index = 0,
                                    Info = new CamInfo()
                                    {
                                        IPAddress = "0.0.0.0",
                                        Name = "Comming soon !",
                                        Port = "80",
                                        SerialNumber = "",
                                        SubnetMask = "",
                                        Type = ""
                                    }
                                };
                                DiposeDatamanCamera();
                                break;
                            case SharedProgram.DataTypes.CommonDataType.CameraSeries.InsightVisionDual:
                                //Console.WriteLine("Ket noi insightvision dual");
                               // DeviceSharedValues.IsCameraConnected = false;


                                // simulate camera info
                                DeviceSharedValues.CameraInfos = new CameraInfos
                                {
                                    ConnectionStatus = false,
                                    Index = 0,
                                    Info = new CamInfo()
                                    {
                                        IPAddress = "0.0.0.0",
                                        Name = "Comming soon !",
                                        Port = "80",
                                        SerialNumber = "",
                                        SubnetMask = "",
                                        Type = ""
                                    }
                                };
                                DiposeDatamanCamera();
                                break;
                            default:
                                break;
                        }
                      
                        // SharedEventsIpc.RaiseCameraStatusChanged(DeviceSharedValues.IsCameraConnected, EventArgs.Empty);
                       // Console.WriteLine("dit con me may: " + DeviceSharedValues.CameraInfos.Info.IPAddress);
                        SendCameraInfoAndStatusToUi(JobIndex, _ipcDeviceToUISharedMemory_DT, DeviceSharedValues.CameraInfos);
                        // await Console.Out.WriteLineAsync($"Camera stst: {DeviceSharedValues.IsCameraConnected}");
                        await Task.Delay(1000);
                    }
                }
                catch (Exception)
                {
                }
            });
        }

        public Task<int> CheckDeviceConnectionAsync() // 0 OK, 1 Cam connect fail, 2 Printer connect fail
        {
            try
            {
                if (DatamanCameraDeviceHandler != null && !DatamanCameraDeviceHandler.IsConnected)
                {
                    return Task.FromResult(1);
                }

                if (RynanRPrinterDeviceHandler != null && !RynanRPrinterDeviceHandler.IsConnected() &&
                    SharedValues.SelectedJob?.CompareType == SharedProgram.DataTypes.CommonDataType.CompareType.Database &&
                    SharedValues.SelectedJob.PrinterSeries == SharedProgram.DataTypes.CommonDataType.PrinterSeries.RynanSeries) // Standalone will not check Printer Connection
                {
                    return Task.FromResult(2);
                }
                return Task.FromResult(0);
            }
            catch (Exception)
            {
                return Task.FromResult(2);
            }
        }

        public void InitEvents()
        {
            PrinterEventInit();
            CameraEventInit();
            ScannerEventInit();
        }
    }
}
