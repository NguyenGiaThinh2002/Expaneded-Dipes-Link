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
        public static DatamanCamera? DatamanCameraDeviceHandler;
        public static RynanRPrinterTCPClient? RynanRPrinterDeviceHandler;
        public static S7TCPIP? ControllerDeviceHandler;
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
                DeviceSharedValues.Index = 0;
                DeviceSharedValues.CameraIP = "192.168.15.93";
                DeviceSharedValues.PrinterIP = "192.168.15.49"; //192.168.3.52
                DeviceSharedValues.PrinterPort = "2030";
                DeviceSharedValues.ControllerIP = "127.0.0.1";
                DeviceSharedValues.ControllerPort = "2001";
                DeviceSharedValues.VPObject = new()
                {
                    FailedDataSentToPrinter = "Failure",
                    VerifyAndPrintBasicSentMethod = false,
                    PrintFieldForVerifyAndPrint = new()
                    {
                        new() { Index = 1,Type=PODModel.TypePOD.FIELD, PODName="", Value=""}
                    }
                };
            }
        }


        static void Main(string[] args)
        {
            try
            {
                GetArgumentList(args);
                JobIndex = DeviceSharedValues.Index;
                new Program().NonStaticMainProgram();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            #region Hold control Readkey
            Thread consoleReadThread = new(() =>
            {
                while (true)
                {
                    var key = Console.ReadLine();
                    if (key == "start")
                    {
                        keyStep = "start";
                    }
                    if (key == "v")
                    {
                        keyStep = "v";
                    }
                    if (key == "f")
                    {
                        keyStep = "f";
                    }
                    if (key == "d")
                    {
                        keyStep = "d";
                    }
                    if (key == "n")
                    {
                        keyStep = "n";
                    }
                    if (key == "e")
                    {
                        keyStep = "e";
                    }
                    if (key == "loaddb")
                    {
                        keyStep = "loaddb";
                    }
                    Thread.Sleep(10);
                }
            });
            consoleReadThread.Start();
            #endregion
        }

        private void ProcessExitHandler(object? sender, EventArgs e)
        {
            ReleaseResource();
        }

        public async void NonStaticMainProgram()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExitHandler);
            InitInstanceIPC();
            ListenConnectionParam();
            AlwaySendPrinterOperationToUI();
            DatamanCameraDeviceHandler = new(JobIndex, _ipcDeviceToUISharedMemory_DT);
            RynanRPrinterDeviceHandler = new(JobIndex, _ipcDeviceToUISharedMemory_DT);
            ControllerDeviceHandler = new(JobIndex, _ipcDeviceToUISharedMemory_DT);
            InitEvents();

            Thread t = new(() =>
            {
                while (true)
                {
                    if (keyStep == "loaddb")
                    {
                        DeviceSharedValues.ActionButtonType = SharedProgram.DataTypes.CommonDataType.ActionButtonType.LoadDB;
                        ActionButtonFromUIProcessingAsync();
                        keyStep = "";
                    }
                    if (keyStep == "start")
                    {
                        DeviceSharedValues.ActionButtonType = SharedProgram.DataTypes.CommonDataType.ActionButtonType.Start;
                        ActionButtonFromUIProcessingAsync();
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
        }

        public Task<int> CheckDeviceConnectionAsync() // 0 OK, 1 Cam connect fail, 2 Printer connect fail
        {
            if (DatamanCameraDeviceHandler != null && !DatamanCameraDeviceHandler.IsConnected)
                return Task.FromResult(1);

            if (RynanRPrinterDeviceHandler != null && !RynanRPrinterDeviceHandler.IsConnected() && SharedValues.SelectedJob.CompareType == SharedProgram.DataTypes.CommonDataType.CompareType.Database)
                return Task.FromResult(2);

            return Task.FromResult(0);
        }

        public void InitEvents()
        {
            PrinterEventInit();
            CameraEventInit();
        }


        public void SimulateJobInformation(JobModel jobIn, out JobModel jobOut)
        {
            jobIn.Name = "MyJob";
            jobIn.DatabasePath = "D:\\Program\\DPLink\\SampleDB\\OneMillion.csv";
            jobIn.CheckedResultPath = "240313163302_DPLINK_AfterProduction_Template.csv";
            jobIn.PrintedResponePath = "240313163303_Printed_DPLINK_AfterProduction_Template.csv";
            jobIn.PrinterTemplate = "podtest";
            jobIn.PODFormat = new List<PODModel>
            {
               new() { Index = 1, PODName="Data" ,Type = PODModel.TypePOD.FIELD,Value ="" }
            };
            jobOut = jobIn;
        }

    }
}
