using CommunityToolkit.Mvvm.DependencyInjection;
using DipesLink_SDK_Printers;
using IPCSharedMemory;
using Newtonsoft.Json.Linq;
using SharedProgram.Controller;
using SharedProgram.DeviceTransfer;
using SharedProgram.Models;
using SharedProgram.Shared;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Shapes;
using static SharedProgram.DataTypes.CommonDataType;
using static SharedProgram.Shared.SharedEvents;
using PrinterStatus = SharedProgram.DataTypes.CommonDataType.PrinterStatus;
namespace DipesLinkDeviceTransfer
{

    public partial class Program
    {
        // dang lam
        //private readonly int _Index;
        //public PrinterProcessing(int index)
        //{
        //    _Index = index;
        //    PrinterEventInit();
        //}

        //public int GetIndex()
        //{
        //    return _Index;
        //}

        #region Declarations
        private CancellationTokenSource _CTS_SendStsPrint = new();
        private CancellationTokenSource _CTS_SendStsCheck = new();
        private CancellationTokenSource _CTS_SendData = new();
        private ConcurrentDictionary<string, int> _Emergency = new();
        private readonly object _PrintedResponseLocker = new();
        private bool _IsPrintedResponse = false;
        private bool _IsRechecked = false;
        private readonly object _CheckLocker = new();
        private ComparisonResult _CheckedResult = ComparisonResult.Valid;
        private bool _IsCheckedWait = true;
        public event EventHandler? OnReceiveVerifyDataEvent;
        private CancellationTokenSource? _VirtualCTS;
        private PrinterSettingsModel _PrinterSettingsModel;
        #endregion Declarations
        public void PrinterEventInit()
        {
            OnReceiveVerifyDataEvent -= SendVerifiedDataToPrinter;
            OnReceiveVerifyDataEvent += SendVerifiedDataToPrinter;
            SharedEvents.OnPrinterDataChange += SharedEvents_OnPrinterDataChange; // Printer Data change event
            PrinterDataHandler.OnPrinterDataChange += PrinterDataHandler_OnPrinterDataChange;
            ReceiveDataFromPrinterHandlerAsync();
            for (int i = 1; i < DeviceSharedValues.numberOfPrinter; i++)
            {
                ReceiveTemplateFromAllPrinter(i);
            }
        }

        private void SharedEvents_OnPrinterDataChange(object? sender, EventArgs e)
        {
            // thinh sua now
            if (sender is PODDataModel podData) // && podData.Index == SharedValues.SelectedPrinter
            {
                //Console.WriteLine(podData.Text);
                _QueueBufferPrinterReceivedData.Enqueue(sender);
            }

        }

        private void PrinterDataHandler_OnPrinterDataChange(object? sender, PrinterDataEventArgs e)
        {
            if (e is PrinterDataEventArgs eventArgs)
            {
                var podData = eventArgs.Data;
                var index = eventArgs.Index;
                Console.WriteLine(podData.Text);
                _QueueBufferPrinterReceivedDataList[index].Enqueue(podData);

                // Add the PODDataModel to the queue based on the index
                //if(index == 0)
                //{
                //    Console.WriteLine(podData.Text);
                //    _QueueBufferPrinterReceivedData.Enqueue(podData);
                //}

            }
        }

        private async void SendWorkingDataToPrinterHandlerAsync()
        {
            _CTS_SendWorkingDataToPrinter = new();
            var token = _CTS_SendWorkingDataToPrinter.Token;
            Stopwatch stopwSend = new();
            await Task.Run(async () =>
            {
                await Task.Delay(500, token); // Wait printer ready
                int counterCodeSent = 0;
                List<string[]> codeList;
                lock (_SyncObjCodeList)
                {
                    codeList = new List<string[]>(SharedValues.ListPrintedCodeObtainFromFile); //Clone list code
                }
                lock (_PrintLocker)
                {
                    _IsPrintedWait = false; // Reset waiting mode
                    _PrintedResult = ComparisonResult.Valid;
                }
                try
                {
                    var spinWait = new SpinWait();
                    int startIndex = codeList.FindIndex(x => x.Last() != "Printed"); // Get the first not-printed code in database (first waiting)
                    if (startIndex == -1) return;
                    _IsPrintedWait = true;  // Init waiting send data


                    for (int codeIndex = startIndex; codeIndex < codeList.Count; codeIndex++)
                    {
                        token.ThrowIfCancellationRequested();
                        string[] codeModel = codeList[codeIndex];
                        if (codeModel[^1] != "Printed" && codeModel[^1] != "Duplicate")
                        {
                            string data = "";
                            token.ThrowIfCancellationRequested();
                            data = string.Join(";", codeModel.Take(codeModel.Length - 1).Skip(1));// Trim data (exclude Index, Status column)
                            string command = string.Format("DATA;{0}", data); // Init send command
                            if (_printerManager._printers[SharedValues.SelectedPrinter] != null )
                            {
                                _printerManager.SendDataToAllPrinters(command);

                                NumberOfSentPrinter++;
                                Interlocked.Exchange(ref _countSentCode, NumberOfSentPrinter);
                                _QueueSentCodeNumber.Enqueue(_countSentCode);
                            }

                            counterCodeSent++;

                            if (SharedValues.OperStatus == OperationStatus.Processing)
                            {
                                if (counterCodeSent >= DeviceSharedValues.NumberOfBuffer && _IsAfterProductionMode)
                                {
                                    SharedValues.OperStatus = OperationStatus.Running;
                                }
                                else if (counterCodeSent >= 1 && _IsOnProductionMode)
                                {
                                    SharedValues.OperStatus = OperationStatus.Running;
                                }
                            }

                            if (_IsOnProductionMode)
                            {
                                lock (_PrintLocker)
                                {
                                    _IsPrintedWait = true;
                                    while (_IsPrintedWait) Monitor.Wait(_PrintLocker);
                                    if (_PrintedResult != ComparisonResult.Valid && _PrintedResult != ComparisonResult.Duplicated)
                                    {
                                        codeIndex--;
                                    }

                                }
                            }
                            stopwSend?.Stop();
                            //   await Console.Out.WriteLineAsync("Time sent POD: " + stopwSend?.ElapsedMilliseconds.ToString());
                            Interlocked.Exchange(ref _cycleTimePODDataTransfer, double.Parse(stopwSend?.ElapsedMilliseconds.ToString()));
                            // After production mode
                            if (_IsAfterProductionMode)
                            {
                                if (counterCodeSent < DeviceSharedValues.NumberOfBuffer) // Send 100 buffer data
                                {
                                    stopwSend = Stopwatch.StartNew();
                                    await Task.Delay(20, token);
                                }
                                else
                                {
                                    bool detectStopSend = true;
                                    while (detectStopSend)
                                    {
                                        if (_QueueCountFeedback.TryDequeue(out int res))
                                        {
                                            stopwSend = Stopwatch.StartNew();
                                            detectStopSend = false;
                                        }
                                        //await Task.Delay(1, token);
                                        spinWait.SpinOnce();
                                    }
                                }
                            }
                        }
                    }

                    if (SharedValues.OperStatus == OperationStatus.Processing)
                    {
                        SharedValues.OperStatus = OperationStatus.Running;
                    }

                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Thread send data to printer was canceled !");
                }
                catch (Exception)
                {
                    Console.WriteLine("Thread send data to printer was error!");
                    _ = StopProcessAsync();
                }
            }, _CTS_SendWorkingDataToPrinter.Token);
        }

        private async void ReceiveDataFromPrinterHandlerAsync()
        {
            _CTS_ReceiveDataFromPrinter = new();
            var token = _CTS_ReceiveDataFromPrinter.Token;
            await Task.Run(async () =>
            {
                try
                {
                    int countTemp = 0;
                    while (true)
                    {
                                              
                        token.ThrowIfCancellationRequested();
                        var isDoneDequeue = _QueueBufferPrinterReceivedData.TryDequeue(out object? result);

                        if (!isDoneDequeue || result == null)
                        {
                            await Task.Delay(1, token);
                            continue;
                        }
                        try
                        {
                            if (result is PODDataModel podDataModel)
                            {
                                // MessageBox.Show(SharedValues.SelectedPrinter.ToString());

                                string[] pODcommand = podDataModel.Text.Split(';');
                                var PODResponseModel = new PODResponseModel { Command = pODcommand[0] };
                                if (PODResponseModel.Command != null)
                                {

                                    switch (PODResponseModel.Command)
                                    {
                                        case "RSLI": // Feedback  Printer Template  
                                            pODcommand = pODcommand.Skip(1).ToArray();
                                            pODcommand = pODcommand.Take(pODcommand.Length - 1).ToArray();
                                            PODResponseModel.Template = pODcommand;
                                            _PrintProductTemplateList = PODResponseModel.Template;
                                            _PrintProductTemplateLists[0] = PODResponseModel.Template;
                                            foreach (var item in _PrintProductTemplateList)
                                            {
                                                SharedFunctions.PrintConsoleMessage(item);
                                            }
                                            break;

                                        case "DATA": //Feedback data sent
                                            if (SharedValues.OperStatus != OperationStatus.Stopped)
                                            {
                                                ReceivedCode++;
                                                Interlocked.Exchange(ref _countReceivedCode, ReceivedCode);
                                                _QueueReceivedCodeNumber.Enqueue(_countReceivedCode);
                                            }
                                            break;

                                        case "RSFP": //Feedback data printed

                                            if (_IsOnProductionMode)
                                            {
                                                lock (_PrintedResponseLocker)
                                                {
                                                    _IsPrintedResponse = true; // Notify that have a printed response
                                                }
                                            }

                                            if (_IsAfterProductionMode)
                                            {
                                                // Condition for wait send
                                                CountFeedback++;
                                                Interlocked.Exchange(ref _countFb, CountFeedback);
                                                _QueueCountFeedback.Enqueue(_countFb);
                                            }
                                            //Example Receive data: RSFP;1/101;DATA; data1;data2;data3
                                            pODcommand = pODcommand.Skip(3).ToArray();  // get  [data1;data2;data3]
                                            ResultFinishPrint(pODcommand);
                                            await Task.Delay(1, token);
                                            break;

                                        case "STAR": //Feedback start print
                                            // Wait for all printer to start
                                            await Task.Delay(200);
                                            StartCommandHandler(pODcommand, PODResponseModel, podDataModel);
                                            break;

                                        case "STOP": //Feedback stop print
                                            StopCommandHandler(pODcommand, PODResponseModel);
                                            break;

                                        case "MON":  // Monitor 
                                            MonitorCommandHandler(pODcommand, PODResponseModel);
                                            break;

                                        default:
                                            break;
                                    }
                                }
                            }
                            
                        }
                        catch (Exception)
                        {
#if DEBUG
                            Console.WriteLine("ReceiveDataFromPrinterHandlerAsync logic Fail");
#endif
                        }                     
                    }

                }
                catch (OperationCanceledException)
                {
#if DEBUG
                    Console.WriteLine("Receive Data From Printer Thread was Canceled !");
#endif
                }
                catch (Exception)
                {
#if DEBUG
                    Console.WriteLine("Receive Data From Printer Thread was failed !");
#endif
                    KillAllProccessThread();
                    _ = StopProcessAsync();
                }
            });
        }

        private async void ReceiveTemplateFromAllPrinter(int printerIndex)
        {
            try
            {
                _CTS_ReceiveDataFromPrinterDictionary[printerIndex] = new CancellationTokenSource();
                var token = _CTS_ReceiveDataFromPrinterDictionary[printerIndex].Token;
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    // chay tu printer thu 2 tro di
                    
                        try
                        {
                            var isThisDoneDequeue = _QueueBufferPrinterReceivedDataList[printerIndex].TryDequeue(out object? thisResult);
                            if (!isThisDoneDequeue || thisResult == null)
                            {
                                await Task.Delay(1);
                                continue;
                            }
                            try
                            {
                                if (thisResult is PODDataModel podDataModel1)
                                {
                                    string[] pODcommand1 = podDataModel1.Text.Split(';');
                                    var PODResponseModel1 = new PODResponseModel { Command = pODcommand1[0] };
                                    if (PODResponseModel1.Command != null)
                                    {
                                        switch (PODResponseModel1.Command)
                                        {
                                            case "RSLI": // Feedback  Printer Template  
                                                pODcommand1 = pODcommand1.Skip(1).ToArray();
                                                pODcommand1 = pODcommand1.Take(pODcommand1.Length - 1).ToArray();
                                                PODResponseModel1.Template = pODcommand1;
                                                _PrintProductTemplateLists[printerIndex] = PODResponseModel1.Template;
                                                break;

                                            case "RSFP": //Feedback data printed

                                                if (_IsOnProductionMode)
                                                {
                                                    lock (_PrintedResponseLocker)
                                                    {
                                                        _IsPrintedResponse = true; // Notify that have a printed response
                                                    }
                                                }
                                                pODcommand1 = pODcommand1.Skip(3).ToArray();  // get  [data1;data2;data3]
                                                string resultText = string.Join(",", pODcommand1) + ",Printed";
                                                _QueueBufferBackupPrintedCodeTemp[printerIndex].Enqueue(resultText);
                                                await Task.Delay(1, token);
                                                break;
                                        }
                                        await Task.Delay(1);
                                    }
                                }

                            }
                            catch (OperationCanceledException) { }
                        }
                        catch (OperationCanceledException)
                        { }

                    }
                

            }
            catch (Exception) { }
        }

        //private async void ReceiveTemplateFromAllPrinter()
        //{
        //    try
        //    {
        //        for (int i = 0; i < DeviceSharedValues.numberOfPrinter; i++)
        //        {
        //            var isThisDoneDequeue = _QueueBufferPrinterReceivedData[i].TryDequeue(out object? thisResult);
        //            if (!isThisDoneDequeue || thisResult == null)
        //            {
        //                await Task.Delay(10);
        //                // continue;
        //            }

        //            if (thisResult is PODDataModel podDataModel1)
        //            {
        //                string[] pODcommand1 = podDataModel1.Text.Split(';');
        //                var PODResponseModel1 = new PODResponseModel { Command = pODcommand1[0] };

        //                if (PODResponseModel1.Command != null)
        //                {
        //                    switch (PODResponseModel1.Command)
        //                    {
        //                        case "RSLI":
        //                            pODcommand1 = pODcommand1.Skip(1).Take(pODcommand1.Length - 2).ToArray();
        //                            PODResponseModel1.Template = pODcommand1;
        //                            _PrintProductTemplateLists[i] = PODResponseModel1.Template;
        //                            break;
        //                    }
        //                    await Task.Delay(300);
        //                }
        //            }
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        // Handle exceptions
        //    }
        //}


        public void GetPrinterTemplateList()
        {
            Task requestTemplateTask = Task.Run(async () =>
            {
                for (int i = 0; i < DeviceSharedValues.numberOfPrinter; i++)
                {
                    if (_printerManager._printers[i] != null && _printerManager._printers[i].IsConnected())
                    {
                        _printerManager._printers[i].SendData("RQLI"); //send request template list command to printer
                        //ReceiveTemplateFromAllPrinter();
                        await Task.Delay(100);
                        SendTemplateListToUI(DeviceSharedValues.Index, _PrintProductTemplateLists[i], i); //_PrintProductTemplateList
                    }
                }

            });
        }

        private void SendTemplateListToUI(int index, string[] printerTemplate, int PrinterIndex) //
        {
            try
            {
                byte[] byteRes = DataConverter.ToByteArray(printerTemplate);
                MemoryTransfer.SendPrinterTemplateListToUI(_ipcDeviceToUISharedMemory_DT, index, byteRes, PrinterIndex);//
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }

        }

    

        public void SendCompleteDataToUIAsync()
        {
            _CTS_SendStsPrint = new();
            var tokenSts = _CTS_SendStsPrint.Token;
            Task.Run(async () =>
            {
                byte[]? sentDataNumberBytes = new byte[7];
                byte[]? receivedDataNumberBytes = new byte[7];
                byte[]? printedDataNumberBytes = new byte[7];

                try
                {
                    while (true)
                    {
                        if (tokenSts.IsCancellationRequested)
                        {
                            if (_QueueSentCodeNumber.IsEmpty &&
                            _QueueReceivedCodeNumber.IsEmpty &&
                            _QueuePrintedCodeNumber.IsEmpty &&
                            _QueueCurrentPositionInDatabase.IsEmpty)
                            {
                                tokenSts.ThrowIfCancellationRequested();
                            }
                        }

                        // Sent POD Number
                        if (_QueueSentCodeNumber.TryDequeue(out int sentNumber))
                        {
                            sentDataNumberBytes = SharedFunctions.StringToFixedLengthByteArray(_countSentCode.ToString(), 7); // max 2000000
                            MemoryTransfer.SendCounterSentToUI(_ipcDeviceToUISharedMemory_DT, JobIndex, sentDataNumberBytes);
                        }

                        // Received POD Number
                        if (_QueueReceivedCodeNumber.TryDequeue(out int receivedNumber))
                        {
                            receivedDataNumberBytes = SharedFunctions.StringToFixedLengthByteArray(_countReceivedCode.ToString(), 7); // max 2000000
                            MemoryTransfer.SendCounterReceivedToUI(_ipcDeviceToUISharedMemory_DT, JobIndex, receivedDataNumberBytes);
                        }

                        // Printed Number
                        if (_QueuePrintedCodeNumber.TryDequeue(out int printedNumber))
                        {
                            printedDataNumberBytes = SharedFunctions.StringToFixedLengthByteArray(printedNumber.ToString(), 7);
                            MemoryTransfer.SendCounterPrintedToUI(_ipcDeviceToUISharedMemory_DT, JobIndex, printedDataNumberBytes);
                        }

                        // Current position (index and page)
                        if (_QueueCurrentPositionInDatabase.TryDequeue(out byte[]? currentPos)) // Current Pos in Databse (index and page)
                        {
                            MemoryTransfer.SendCurrentPosDatabaseToUI(_ipcDeviceToUISharedMemory_DT, JobIndex, currentPos);
                        }

                        // Cycle time POD transfer 
                        MemoryTransfer.SendCycleTimePODTransferToUI(_ipcDeviceToUISharedMemory_DT, JobIndex, DataConverter.ToByteArray(_cycleTimePODDataTransfer));

                        await Task.Delay(1);

                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Thread sent sts print canceled !");
                }
                catch (Exception)
                {
                    Console.WriteLine("Thread sent sts print fail !");
                }
            });

            // Total Checked, Total Passed, Total Failed
            _CTS_SendStsCheck = new();
            var tokenResult = _CTS_SendStsCheck.Token;
            Task.Run(async () =>
            {
                byte[]? totalCheckedNumberBytes = new byte[7];
                byte[]? totalPassedNumberBytes = new byte[7];
                byte[]? totalFailedNumberBytes = new byte[7];

                try
                {
                    while (true)
                    {
                        if (tokenResult.IsCancellationRequested)
                        {
                            if (_QueueTotalChekedNumber.IsEmpty &&
                            _QueueTotalPassedNumber.IsEmpty &&
                            _QueueTotalFailedNumber.IsEmpty)
                            {
                                tokenResult.ThrowIfCancellationRequested();
                            }
                        }

                        bool triggerData = false;

                        //Total Checked
                        if (_QueueTotalChekedNumber.TryDequeue(out int totalCheckedNum))
                        {
                            totalCheckedNumberBytes = SharedFunctions.StringToFixedLengthByteArray(totalCheckedNum.ToString(), 7);
                            triggerData = true;
                        }

                        //Total Passed
                        if (_QueueTotalPassedNumber.TryDequeue(out int totalPassedNum))
                        {
                            totalPassedNumberBytes = SharedFunctions.StringToFixedLengthByteArray(totalPassedNum.ToString(), 7);
                            triggerData = true;
                        }

                        //Total Failed
                        if (_QueueTotalFailedNumber.TryDequeue(out int totalFailedNum))
                        {
                            totalFailedNumberBytes = SharedFunctions.StringToFixedLengthByteArray(totalFailedNum.ToString(), 7);
                            triggerData = true;
                        }

                        if (triggerData)
                        {
                            byte[] combineBytes = SharedFunctions.CombineArrays(totalCheckedNumberBytes, totalPassedNumberBytes, totalFailedNumberBytes);
                            MemoryTransfer.SendCheckedStatisticsToUI(_ipcDeviceToUISharedMemory_RD, JobIndex, combineBytes);
                        }

                        await Task.Delay(1);
                    }

                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Thread sent sts checked canceled !");
                }
                catch (Exception)
                {
                    Console.WriteLine("Thread sent sts checked fail !");
                }
            });

            _CTS_SendData = new();
            var tokenData = _CTS_SendData.Token;
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        try
                        {
                            if (tokenData.IsCancellationRequested)
                            {
                                if (_QueuePrintedCode.IsEmpty &&
                                _QueueCheckedResultForUpdateUI.IsEmpty &&
                                _QueueCheckedResult.IsEmpty)
                                {
                                    tokenData.ThrowIfCancellationRequested();
                                }
                            }

                            //Printed Code
                            if (_QueuePrintedCode.TryDequeue(out string[]? data))
                            {
                                MemoryTransfer.SendPrintedCodeRawToUI(_ipcDeviceToUISharedMemory_DT, JobIndex, DataConverter.ToByteArray(data));
                            }

                            //Detect Model Data
                            if (_QueueCheckedResultForUpdateUI.TryDequeue(out DetectModel? detectModel)) // Detect model heavy size, So we will use Task
                            {
                                var detectModelBytes = DataConverter.ToByteArray(detectModel);
                                MemoryTransfer.SendDetectModelToUI(_ipcDeviceToUISharedMemory_RD, JobIndex, detectModelBytes);
                            }

                            if (_QueueCheckedResult.TryDequeue(out string[]? checkedResult))
                            {

                                var checkedResultBytes = DataConverter.ToByteArray(checkedResult);
                                MemoryTransfer.SendCheckedResultRawToUI(_ipcDeviceToUISharedMemory_RD, JobIndex, checkedResultBytes);

                            }
                        }
                        catch (Exception) { }
                        await Task.Delay(1);
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Thread sent realtime data canceled !");
                }
                catch (Exception)
                {
                    Console.WriteLine("Thread sent realtime data fail !");
                }
            });
        }

        private CheckCondition CheckAllTheConditions()
        {
            // Check Camera Connection
#if !DEBUG
            if (_datamanCameraDeviceHandler != null && !_datamanCameraDeviceHandler.IsConnected)
            {
                return CheckCondition.NotConnectCamera;
            }
#endif
            if (SharedValues.SelectedJob != null && SharedValues.SelectedJob.CompareType == CompareType.Database && SharedValues.SelectedJob.PrinterSeries == PrinterSeries.RynanSeries)
            {
                SharedFunctions.PrintConsoleMessage(SharedValues.SelectedJob.PrinterSeries.ToString());
                //Check Printer Connection
                if (_printerManager._printers[SharedValues.SelectedPrinter] != null && !_printerManager._printers[SharedValues.SelectedPrinter].IsConnected())
                {
                    return CheckCondition.NotConnectPrinter;
                }

                // Check Printer Template
                if (SharedValues.SelectedJob.CompareType == CompareType.Database && (SharedValues.SelectedJob == null || SharedValues.SelectedJob.PrinterTemplate == ""))
                {
                    return CheckCondition.MissingParameterActivation;
                }

                // Check list POD code for Print and Check
                if (_CodeListPODFormat == null || _CodeListPODFormat.IsEmpty)
                {
                    return CheckCondition.MissingParameterActivation;
                }
            }

            return CheckCondition.Success;
        }

        private CheckPrinterSettings CheckAllSettingsPrinter(string printerIP)
        {
            _PrinterSettingsModel = SharedFunctions.GetSettingsPrinter(printerIP);

            if (!_PrinterSettingsModel.IsSupportHttpRequest)
            {
                return CheckPrinterSettings.NotSupported;
            }

            if (!_PrinterSettingsModel.EnablePOD)
            {
                return CheckPrinterSettings.PODNotEnabled;
            }

            if (!_PrinterSettingsModel.ResponsePODCommand)
            {
                return CheckPrinterSettings.ResponsePODCommandNotEnable;
            }

            if (!_PrinterSettingsModel.ResponsePODData)
            {
                return CheckPrinterSettings.ResponsePODDataNotEnable;
            }
            // 0: json, 1: Raw data, 2: Customise
            if (_PrinterSettingsModel.PodDataType != 1)
            {
                return CheckPrinterSettings.NotRawData;
            }
            // 0:print all, 1:print last, 2 print last and repeat
            var podMode = SharedValues.SelectedJob.JobType == JobType.AfterProduction ? 0 : 1;
            if (_PrinterSettingsModel.PodMode != podMode)
            {
                return CheckPrinterSettings.PODMode;
            }

            if (!_PrinterSettingsModel.EnableMonitor)
            {
                return CheckPrinterSettings.MonitorNotEnable;
            }

            return CheckPrinterSettings.Success;
        }

        public void StartProcess()
        {
            try
            {
                if (SharedValues.SelectedJob == null) return;
                if (SharedValues.OperStatus == OperationStatus.Running || SharedValues.OperStatus == OperationStatus.Processing) // Only Start 1 times
                {
                    SharedFunctions.PrintConsoleMessage("Notify Error: The system has started !");
                    _IsRechecked = false;
                    NotificationProcess(NotifyType.StartSync);
                    return;
                }

                var checkInitDataNotifyType = CheckInitDataErrorAndGenerateMessage();
                if (checkInitDataNotifyType != NotifyType.Unk && SharedValues.SelectedJob.CompareType == CompareType.Database)
                {
                    NotificationProcess(checkInitDataNotifyType);
                    SharedFunctions.PrintConsoleMessage("Check Init Data Error: " + checkInitDataNotifyType.ToString());
                    return;
                }

                bool isDatabaseDeny = SharedValues.SelectedJob.CompareType == CompareType.Database && SharedValues.TotalCode == 0;
                if (isDatabaseDeny)
                {
                    SharedFunctions.PrintConsoleMessage("Database doesn't exist !");
                    NotificationProcess(NotifyType.DatabaseDoNotExist);
                    return;
                }

                CheckCondition checkCondition = CheckAllTheConditions();

                if (checkCondition != CheckCondition.Success)
                {
                    switch (checkCondition)
                    {
                        case CheckCondition.NotLoadDatabase:
                            Console.WriteLine("Notify Error: Please check database connection");
                            NotificationProcess(NotifyType.NotLoadDatabase);
                            break;
                        case CheckCondition.NotConnectCamera:
                            Console.WriteLine("Notify Error: Please check camera connection");
                            NotificationProcess(NotifyType.NotConnectCamera);
                            break;
                        case CheckCondition.NotLoadTemplate:
                            Console.WriteLine("Notify Error: Please check the printer template");
                            NotificationProcess(NotifyType.NotLoadTemplate);
                            break;
                        case CheckCondition.MissingParameter:
                            Console.WriteLine("Notify Error: Some parameter is missing ! Please check again");
                            NotificationProcess(NotifyType.MissingParameter);
                            break;
                        case CheckCondition.NotConnectPrinter:
                            Console.WriteLine("Notify Error: Please check printer connection");
                            NotificationProcess(NotifyType.NotConnectPrinter);
                            break;
                        case CheckCondition.NotConnectServer:
                            NotificationProcess(NotifyType.NotConnectServer);
                            break;
                        case CheckCondition.LeastOneAction:
                            NotificationProcess(NotifyType.LeastOneAction);
                            break;
                        case CheckCondition.MissingParameterActivation:
                            Console.WriteLine("Notify Error: Some activation params are missing! Please check again");
                            NotificationProcess(NotifyType.MissingParameterActivation);
                            break;
                        case CheckCondition.MissingParameterPrinting:
                            NotificationProcess(NotifyType.MissingParameterPrinting);
                            break;
                        case CheckCondition.MissingParameterWarehouseInput:
                            NotificationProcess(NotifyType.MissingParameterWarehouseInput);
                            break;
                        case CheckCondition.MissingParameterWarehouseOutput:
                            NotificationProcess(NotifyType.MissingParameterWarehouseOutput);
                            break;
                        case CheckCondition.CreatingWarehouseInputReceipt:
                            NotificationProcess(NotifyType.CreatingWarehouseInputReceipt);
                            break;
                        case CheckCondition.NoJobsSelected:
                            Console.WriteLine("Notify Error: Please select a jon for system");
                            NotificationProcess(NotifyType.NoJobsSelected);
                            break;
                        default:
                            break;
                    }
                    return;
                }

                bool isNeedToCheckPrinter = SharedValues.SelectedJob.PrinterSeries == PrinterSeries.RynanSeries && SharedValues.SelectedJob.CompareType == CompareType.Database;

                CheckPrinterSettings checkPrinterSettings = CheckAllSettingsPrinter(DeviceSharedValues.PrinterIPs[SharedValues.SelectedPrinter]);
                //&& SharedValues.SelectedJob.IsCheckPrinterSettingsEnabled
                if (checkPrinterSettings != CheckPrinterSettings.Success && isNeedToCheckPrinter && DeviceSharedValues.IsCheckPrinterSettingsEnabled && !_IsRechecked)
                {
                    switch (checkPrinterSettings)
                    {
                        case CheckPrinterSettings.NotSupported:
                            NotificationProcess(NotifyType.NotSupported);
                            break;
                        case CheckPrinterSettings.NotRawData:
                            NotificationProcess(NotifyType.NotRawData);
                            break;
                        case CheckPrinterSettings.PODNotEnabled:
                            NotificationProcess(NotifyType.PODNotEnabled);
                            break;
                        case CheckPrinterSettings.ResponsePODDataNotEnable:
                            NotificationProcess(NotifyType.ResponsePODDataNotEnable);
                            break;
                        case CheckPrinterSettings.ResponsePODCommandNotEnable:
                            NotificationProcess(NotifyType.ResponsePODCommandNotEnable);
                            break;
                        case CheckPrinterSettings.MonitorNotEnable:
                            NotificationProcess(NotifyType.MonitorNotEnable);
                            break;
                        case CheckPrinterSettings.PODMode:
                            if (SharedValues.SelectedJob.JobType == JobType.AfterProduction)
                                NotificationProcess(NotifyType.PODModeMustBePrintAll);
                            else
                                NotificationProcess(NotifyType.PODModeMustBePrintLast);
                            break;
                        default:
                            break;
                    }
                    return;
                }

                SharedValues.SelectedJob.ExportNamePrefix = DateTime.Now.ToString(SharedValues.SelectedJob.ExportNamePrefixFormat);
                // thinh dang lam
                for(int i = 0; i < DeviceSharedValues.numberOfPrinter; i++)
                {
                    _QueueBufferPrinterReceivedDataList[i].Clear();
                }
                _QueueBufferPrinterReceivedData.Clear();
                DisposeAndClearListQueue(_QueueBufferPrinterReceivedDataList);


                if (SharedValues.SelectedJob.CompareType == CompareType.Database &&
                    SharedValues.SelectedJob.PrinterSeries == PrinterSeries.RynanSeries && !_IsRechecked)
                {
                    SharedValues.OperStatus = OperationStatus.Processing;
                    if (_printerManager._printers[SharedValues.SelectedPrinter] != null)
                    {
                        _printerManager.StopAllPrinter("STOP");
                        Thread.Sleep(50);
                        //string templateNameWithoutExt = SharedValues.SelectedJob.PrinterTemplate.Replace(".dsj", "");
                        //string startPrintCommand = string.Format("STAR;{0};1;1;true", templateNameWithoutExt);

                        for (int i = 0; i < DeviceSharedValues.numberOfPrinter; i++)
                        {
                            
                            if (_printerManager._printers[i].IsConnected()){
                                //MessageBox.Show("IP: " + i + "___" + DeviceSharedValues.PrinterIPs[i]);
                                string templateNameWithoutExt = SharedValues.SelectedJob.TemplateManager.PrinterTemplateList[i].Replace(".dsj", "");
                                string startPrintCommand = string.Format("STAR;{0};1;1;true", templateNameWithoutExt);
                                _printerManager._printers[i].SendData(startPrintCommand); // Send Start command to printer
                                //MessageBox.Show("PrinterTemplateList " + i + templateNameWithoutExt.ToString());
                            }

                        }


                    }
                }
                else
                {
                    SharedValues.OperStatus = OperationStatus.Running;
                }
                bool isNonePrinted = SharedValues.SelectedJob.CompareType == CompareType.CanRead || SharedValues.SelectedJob.CompareType == CompareType.StaticText;
                SharedValues.SelectedJob.JobStatus = JobStatus.Unfinished;

                SaveJobChangedSettings(SharedValues.SelectedJob);

                SendCompleteDataToUIAsync();

                CompareAsync();

                ExportImagesToFileAsync();

                UpdateUICheckedResultAsync();

                ExportCheckedResultToFileAsync();

                if (SharedValues.SelectedJob.CompareType == CompareType.Database)
                {
                    try
                    {
                        if (DeviceSharedValues.numberOfPrinter > 1)
                        {
                            for (int i = 1; i < DeviceSharedValues.numberOfPrinter; i++)
                            {
                                ExportAllPrinterPrintedResponseToFileAsyncTemp(i);
                            }
                        }
                    }
                    catch (Exception ex) { }

                    UpdateUIPrintedResponseAsync();
                    ExportPrintedResponseToFileAsync();

                

                }
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }
        }

        public async void RePrintAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (!_IsAfterProductionMode) return; // Only use for After Production

                    lock (_SyncObjCodeList)
                    {
                        if (SharedValues.ListPrintedCodeObtainFromFile.Count > 0 && _CodeListPODFormat.Count > 0)
                        {
                            _TotalMissed = 0;
                            int statusColIndex = SharedValues.ListPrintedCodeObtainFromFile[0].Length - 1;
                            foreach (var item in _CodeListPODFormat)
                            {
                                if (!item.Value.Status) // Not yet compare or failed compare
                                {
                                    if (SharedValues.ListPrintedCodeObtainFromFile[item.Value.Index][statusColIndex] == "Printed") // Is Printed
                                        SharedValues.ListPrintedCodeObtainFromFile[item.Value.Index][statusColIndex] = "Reprint"; // change to Re-Print state
#if DEBUG
                                    Console.WriteLine("Status Reprint: " + SharedValues.ListPrintedCodeObtainFromFile[item.Value.Index][statusColIndex]);
#endif
                                }
                                else // Valid compare
                                {
                                    if (SharedValues.ListPrintedCodeObtainFromFile[item.Value.Index][statusColIndex] != "Printed") // Not Printed
                                        SharedValues.ListPrintedCodeObtainFromFile[item.Value.Index][statusColIndex] = "Printed";
#if DEBUG
                                    Console.WriteLine("Status Printed: " + SharedValues.ListPrintedCodeObtainFromFile[item.Value.Index][statusColIndex]);
#endif
                                }
                            }
                            _TotalMissed = SharedValues.TotalCode - NumberOfCheckPassed;
                        }
                    }
                    if (NumberOfCheckPassed < SharedValues.TotalCode)
                    {
                        StartProcess();
                    }
                }
                catch (Exception ex)
                {
                    SharedFunctions.PrintConsoleMessage(ex.Message);
                }
            });
        }

        public async Task StopProcessAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    KillTThreadSendWorkingDataToPrinter();
                    DisposeAndClearListQueue(_QueueBufferPrinterReceivedDataList);
                    _QueueBufferPrinterReceivedData.Clear();
                    _QueueBufferCameraReceivedData.Clear();
                    _QueueBufferScannerReceivedData.Clear();
                    ClearBufferUpdateUI();

                    if (SharedValues.SelectedJob != null && SharedValues.SelectedJob.PrinterSeries == PrinterSeries.RynanSeries)
                    {
                        if (_printerManager._printers[SharedValues.SelectedPrinter] != null)
                        {
                            _printerManager.StopAllPrinter("STOP");

                            lock (_StopLocker)
                            {
                                _IsStopOK = false;
                                int countTimeout = 0;
                                while (!_IsStopOK && countTimeout < 1)
                                {
                                    Monitor.Wait(_StopLocker, 3000); //Wait until there is a stop notify from the printer
                                    countTimeout++;
                                }
                            }
                        }
                    }

                    _TotalMissed = 0;
     

                    foreach (var kvp in _CTS_BackupPrintedResponseDictionary)
                    {
                        kvp.Value?.Cancel(); // Dispose each CancellationTokenSource
                    }



                    _CTS_UIUpdatePrintedResponse.Cancel();
                    _CTS_CompareAction.Cancel();
                    _QueueBufferCameraReceivedData.Enqueue(null);
                    _QueueBufferPODDataCompared.Enqueue(null);
                    _QueueBufferScannerReceivedData.Enqueue(null);
                    _QueueBufferScannerDataCompared.Enqueue(null);
                    _VirtualCTS?.Cancel();

                    SharedValues.OperStatus = OperationStatus.Stopped;

                    if (SharedValues.SelectedJob != null && SharedValues.SelectedJob.CompareType == CompareType.Database)
                    {
                        int currentCheckNumber = SharedValues.SelectedJob.CompleteCondition == CompleteCondition.TotalChecked ? TotalChecked : NumberOfCheckPassed;

                        if (currentCheckNumber >= SharedValues.TotalCode - _NumberOfDuplicate)
                        {
                            SharedValues.SelectedJob.JobStatus = JobStatus.Accomplished;
                            SaveJobChangedSettings(SharedValues.SelectedJob);
                        }
                    }

                }
                catch (Exception ex)
                {

                }
            });
            _IsRechecked = false;

        }

        public bool GetPrinterStatus()
        {
            if (_printerManager._printers[SharedValues.SelectedPrinter] != null && !_printerManager._printers[SharedValues.SelectedPrinter].IsConnected())
            {
                return false;
            }
            return true;
        }


        private void SendVerifiedDataToPrinter(object? sender, EventArgs e)
        {
            try
            {
                string command = "DATA;";
                string[]? arr = sender as string[];

                if (DeviceSharedValues.VPObject.VerifyAndPrintBasicSentMethod) // is basic
                {
                    command += arr[1] == null ? DeviceSharedValues.VPObject.FailedDataSentToPrinter : arr[1]; // If null => send custom string, else send detectmodel.text (read = send)
                }
                else // is compare
                {
                    if (DeviceSharedValues.VPObject.PrintFieldForVerifyAndPrint.Count == 0) // If no select any field => send all field or send n failure field
                    {
                        command += string.Join(";", arr.Take(arr.Length - 1).Skip(1)
                            .Select(x => x ?? DeviceSharedValues.VPObject.FailedDataSentToPrinter));
                    }
                    else // if select field => send selected field or send n value failure
                    {
                        command += string.Join(";", DeviceSharedValues.VPObject.PrintFieldForVerifyAndPrint
                           .Where(x => x.Index < arr.Length - 1)
                           .Select(x => arr[x.Index] == null ? DeviceSharedValues.VPObject.FailedDataSentToPrinter : arr[x.Index]));
                    }
                }
                if (_printerManager._printers[SharedValues.SelectedPrinter] != null)
                {
                    _printerManager._printers[SharedValues.SelectedPrinter].SendData(command);
                    NumberOfSentPrinter++;
                    Interlocked.Exchange(ref _countSentCode, NumberOfSentPrinter);
                    _QueueSentCodeNumber.Enqueue(_countSentCode);

                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }
        }

        // end printer

        private NotifyType CheckInitDataErrorAndGenerateMessage()
        {

            if (_InitDataErrorList.Count > 0)
            {
                foreach (var value in _InitDataErrorList)
                {
                    NotifyType tmp = value switch
                    {
                        NotifyType.DatabaseUnknownError => value,
                        NotifyType.PrintedStatusUnknownError => value,
                        NotifyType.CheckedResultUnknownError => value,
                        NotifyType.CannotAccessDatabase => value,
                        NotifyType.CannotAccessCheckedResult => value,
                        NotifyType.CannotAccessPrintedResponse => value,
                        NotifyType.DatabaseDoNotExist => value,
                        NotifyType.CheckedResultDoNotExist => value,
                        NotifyType.PrintedResponseDoNotExist => value,
                        NotifyType.CannotCreatePodDataList => value,
                        NotifyType.Unknown => value,
                        _ => NotifyType.Unk,
                    };

                    return tmp;
                }
            }
            return NotifyType.Unk;
        }

        public void Recheck()
        {
            if (_barcodeScannerHandler == null || !_barcodeScannerHandler.IsConnected())
            {
                NotificationProcess(NotifyType.NotConnectScanner);
            }
            else
            {
                _IsRechecked = true;
                StartProcess();
            }

        }
        public static void SaveJobChangedSettings(JobModel selectedJob)
        {

            try
            {
                string jobFilePath = SharedPaths.PathSubJobsApp + $"{JobIndex + 1}\\" + selectedJob.Name + SharedValues.Settings.JobFileExtension;
                string jobFileSelectedPath = SharedPaths.PathSelectedJobApp + $"Job{JobIndex + 1}\\" + selectedJob.Name + SharedValues.Settings.JobFileExtension;

                selectedJob.SaveJobFile(jobFilePath);
                if (File.Exists(jobFileSelectedPath))
                {
                    selectedJob.SaveJobFile(jobFileSelectedPath);
                }
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }
        }

        private void ClearBufferUpdateUI()
        {
            _QueueSentCodeNumber?.Clear();
            _QueueReceivedCodeNumber?.Clear();
        }

        private void ResultFinishPrint(string[] podCommand)
        {
            if (SharedValues.SelectedJob == null) return;

            string printedResult = "";

            if (SharedValues.SelectedJob.JobType == JobType.VerifyAndPrint) // Verify and Print
            {
                if (DeviceSharedValues.VPObject.VerifyAndPrintBasicSentMethod)
                {
                    printedResult = podCommand[0];
                }
                else // Compares mode
                {
                    if (DeviceSharedValues.VPObject.PrintFieldForVerifyAndPrint.Count > 0)
                    {
                        if (_Emergency.TryGetValue(string.Join("", podCommand), out int codeIndex))
                        {
                            lock (_SyncObjCodeList)
                            {
                                // thinh dang lam now
                                printedResult = SharedFunctions.GetCompareDataByPODFormat(SharedValues.ListPrintedCodeObtainFromFile[codeIndex], SharedValues.SelectedJob.PODFormat);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in SharedValues.SelectedJob.PODFormat)
                        {
                            if (item.Type == PODModel.TypePOD.FIELD)
                            {
                                int indexItem = item.Index - 1;
                                if (indexItem < podCommand.Length)
                                    printedResult += podCommand[item.Index - 1];
                            }
                            else if (item.Type == PODModel.TypePOD.TEXT)
                            {
                                printedResult += item.Value;
                            }
                        }
                    }
                }
            }
            else // After production
            {
                foreach (PODModel podData in SharedValues.SelectedJob.PODFormat)
                {
                    if (podData.Type == PODModel.TypePOD.FIELD)
                    {
                        int indexItem = podData.Index - 1;
                        if (indexItem < podCommand.Length)
                            printedResult += podCommand[indexItem]; // Get printed Result by index POD format
                    }
                    else if (podData.Type == PODModel.TypePOD.TEXT)
                    {
                        printedResult += podData.Value; //Custom Text String concatenation
                    }
                }
            }

            _QueueBufferPODDataCompared.Enqueue(printedResult); // Add to UI update queue 
#if DEBUG
            // Debug.WriteLine($"UI Update Queue: {printedResult}");
#endif
        }

        private void StartCommandHandler(string[] podCommand, PODResponseModel podResponse, PODDataModel podData)
        {
            try
            {
                for (int i = 0; i < podCommand.Length; i++)
                {
                    if (i == 0) podResponse.Command = podCommand[i]; // Command (STAR, STOP,..)
                    if (i == 1) podResponse.Status = podCommand[i]; // OK . Ready
                    if (i == 2) podResponse.Error = podCommand[i]; // Error code
                }

                if (podResponse.Status != null && (podResponse.Status == "OK" || podResponse.Status == "READY"))
                {
                    // If not Verify and Print : Send POD First
                    if (!_IsVerifyAndPrintMode)
                    {
                        SendWorkingDataToPrinterHandlerAsync(); // Send POD data to printer when printer ready receive data
                    }
                    else
                    {
                        if (SharedValues.OperStatus == OperationStatus.Processing)
                        {
                            SharedValues.OperStatus = OperationStatus.Running;
                        }
                    }
                }
                else // Error case
                {
                    string message = "Unknown";
                    NotifyType notifyType = NotifyType.Unk;
                    switch (podResponse.Error)
                    {
                        case "001":
                            message = "Open templates failed (dose not exist, others templates being opening,...)";
                            notifyType = NotifyType.NotLoadTemplate;
                            break;
                        case "002":
                            message = "Start pages, End pages is invalid";
                            notifyType = NotifyType.StartEndPageInvalid;
                            break;
                        case "003":
                            message = "No printhead is selected";
                            notifyType = NotifyType.NoPrintheadSelected;
                            break;
                        case "004":
                            message = "Speed limit";
                            notifyType = NotifyType.PrinterSpeedLimit;
                            break;
                        case "005":
                            message = "Printhead disconnected";
                            notifyType = NotifyType.PrintheadDisconnected;
                            break;
                        case "006":
                            message = "Unknown printhead";
                            notifyType = NotifyType.UnknownPrinthead;
                            break;
                        case "007":
                            message = "No cartridges";
                            notifyType = NotifyType.NoCartridges;
                            break;
                        case "008":
                            message = "Invalid cartridges";
                            notifyType = NotifyType.InvalidCartridges;
                            break;

                        case "009":
                            message = "Out of ink";
                            notifyType = NotifyType.OutOfInk;
                            break;
                        case "010":
                            message = "Cartridges is locked";
                            notifyType = NotifyType.CartridgesLocked;
                            break;
                        case "011":
                            message = "Invalid version";
                            notifyType = NotifyType.InvalidVersion;
                            break;
                        case "012":
                            message = "Incorrect printhead";
                            notifyType = NotifyType.IncorrectPrinthead;
                            break;
                        default:
                            break;
                    }
                    SharedFunctions.PrintConsoleMessage(message);

                    NotificationProcess(notifyType);
                    _ = StopProcessAsync();
                }
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage("StartCommandHandler" + ex.Message);
            }
        }

        private void StopCommandHandler(string[] podCommand, PODResponseModel podResponse)
        {
            try
            {
                podResponse.Status = podCommand[1];

                // Is STOP
                if (podResponse.Status != null && podResponse.Status == "OK")
                {
                    lock (_StopLocker)
                    {
                        _IsStopOK = true;
                        Monitor.PulseAll(_StopLocker); //Notification has stopped completed
                    }
                }
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }
        }

        bool flagStoppedMonitor = false;
        private async void MonitorCommandHandler(string[] podCommand, PODResponseModel podResponse)
        {
            if (SharedValues.SelectedJob == null) return;
            try
            {
                podResponse.Status = podCommand[3];

                // Detect printer suddenly stop 
                if (SharedValues.OperStatus == OperationStatus.Stopped)
                {
                    flagStoppedMonitor = false;
                }

                if (podResponse.Status == "Stop" &&
                    !flagStoppedMonitor &&
                    (SharedValues.OperStatus == OperationStatus.Running) &&
                    SharedValues.SelectedJob.CompareType == CompareType.Database &&
                    SharedValues.SelectedJob.JobType != JobType.StandAlone && !_IsRechecked)
                {
                    flagStoppedMonitor = true;
                    await Task.Run(async () =>
                    {
                        await Task.Delay(2000); // waiting 2s for transfer states
                        if (SharedValues.OperStatus == OperationStatus.Running)
                        {
                            NotificationProcess(NotifyType.PrinterSuddenlyStop);
                        }
                    });

                    await StopProcessAsync();

                }

                switch (podResponse.Status)
                {
                    case "Stop":
                        _PrinterStatus = PrinterStatus.Stop;
                        break;
                    case "Processing":
                        _PrinterStatus = PrinterStatus.Processing;
                        break;
                    case "Ready":
                    case "Start":
                        _PrinterStatus = PrinterStatus.Ready;
                        _PrinterStatus = PrinterStatus.Start;
                        break;
                    case "WaitingData":
                        _PrinterStatus = PrinterStatus.WaitingData;
                        break;
                    case "Printing":
                        _PrinterStatus = PrinterStatus.Printing;
                        break;
                    case "Connected":
                        _PrinterStatus = PrinterStatus.Connected;
                        break;
                    case "Disconnected":
                        _PrinterStatus = PrinterStatus.Disconnected;
                        break;
                    case "Error":
                        _PrinterStatus = PrinterStatus.Error;
                        break;
                    case "Disable":
                        _PrinterStatus = PrinterStatus.Disable;
                        break;
                    case "":
                        _PrinterStatus = PrinterStatus.Null;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Monitor response handle fail !");
            }
        }

        #region COMPARE 

        private async void CompareAsync()
        {
            if (SharedValues.SelectedJob == null) return;
            _CTS_CompareAction = new();
            var token = _CTS_CompareAction.Token;
            var measureTimeStopw = new Stopwatch();
            await Task.Run(async () =>
            {
                int currentCheckedIndex = -1;
                string staticText = SharedValues.SelectedJob.StaticText ?? string.Empty;
                bool isAutoComplete = SharedValues.SelectedJob.CompareType == CompareType.Database;

                bool isReprint =
                    SharedValues.SelectedJob.CompareType == CompareType.Database &&
                    SharedValues.SelectedJob.JobType == JobType.AfterProduction &&
                    _TotalMissed > 0 &&
                    SharedValues.SelectedJob.CompleteCondition == CompleteCondition.TotalChecked; // Condition for RePrint

                bool isDBStandalone = SharedValues.SelectedJob.CompareType == CompareType.Database && SharedValues.SelectedJob.JobType == JobType.StandAlone;
                int reprintStopCond = TotalChecked + _TotalMissed - _NumberOfDuplicate;
                int stopCond = SharedValues.TotalCode - _NumberOfDuplicate;
                bool isOneMore = false;
                bool isComplete = false;
                try
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            if (_QueueBufferCameraReceivedData.IsEmpty && _QueueBufferScannerReceivedData.IsEmpty)
                                token.ThrowIfCancellationRequested();
                        }
                        else
                        {
                            if (isComplete)
                            {
                                continue;  // Complete Job
                            }
                            if (isAutoComplete)
                            {
                                bool completeCondition = false;
                                int stopNumber = SharedValues.SelectedJob.CompleteCondition == CompleteCondition.TotalChecked ? TotalChecked : NumberOfCheckPassed;

                                // get stop number for verify and print mode
                                if (SharedValues.SelectedJob.JobType == JobType.VerifyAndPrint)
                                {
                                    // is total pass check
                                    if (!(SharedValues.SelectedJob.CompleteCondition == CompleteCondition.TotalChecked))
                                    {
                                        if (isOneMore)
                                        {
                                            stopNumber++;
                                        }

                                        if (NumberOfCheckPassed >= SharedValues.TotalCode - _NumberOfDuplicate)
                                        {
                                            isOneMore = true;
                                        }
                                    }
                                    stopNumber--;
                                }
                                if (!isReprint)
                                {
                                    completeCondition = SharedValues.OperStatus != OperationStatus.Stopped && stopNumber >= stopCond;
                                }
                                else
                                {
                                    completeCondition = SharedValues.OperStatus != OperationStatus.Stopped && stopNumber >= reprintStopCond;
                                }

                                if (completeCondition && !_IsRechecked)
                                {
#if DEBUG
                                    Console.WriteLine("Complete the barcode verification process !");
#endif
                                    await StopProcessAsync();
                                    NotificationProcess(NotifyType.ProcessCompleted);
                                    isComplete = true;
                                    continue;
                                }
                            }
                        }

                        DetectModel? detectModel = null;

                        if (!_QueueBufferScannerReceivedData.IsEmpty && _IsRechecked)
                        {
                            _ = _QueueBufferScannerReceivedData.TryDequeue(out detectModel);
                        }
                        else if (!_IsRechecked)
                        {
                            _ = _QueueBufferCameraReceivedData.TryDequeue(out detectModel);
                        }

                        int compareIndex = _StartIndex + TotalChecked;
                        if (detectModel != null)
                        {
                            measureTimeStopw = Stopwatch.StartNew();

                            switch (SharedValues.SelectedJob.CompareType)
                            {
                                case CompareType.None:
                                    break;
                                case CompareType.CanRead:
                                    detectModel.CompareResult = CanreadCompare(detectModel.Text);
                                    break;
                                case CompareType.StaticText:
                                    detectModel.CompareResult = StaticTextCompare(detectModel.Text, staticText);
                                    break;
                                case CompareType.Database:
                                    bool isNeedToCheckPrintedResponse = true; 
                                    if (_IsOnProductionMode) 
                                    {
                                        lock (_PrintedResponseLocker) 
                                        {
                                            isNeedToCheckPrintedResponse = _IsPrintedResponse;
                                            _IsPrintedResponse = false;
                                        }
                                    }

                                    if (!isNeedToCheckPrintedResponse || _CodeListPODFormat == null)
                                    {
                                        detectModel.CompareResult = ComparisonResult.Invalided;
                                    }
                                    else 
                                    {
                                        detectModel.CompareResult = DatabaseCompare(detectModel.Text, ref currentCheckedIndex); // Conclude Compare Result

                                        if (_IsOnProductionMode)
                                        {
                                            lock (_CheckLocker)
                                            {
                                                _CheckedResult = detectModel.CompareResult;
                                                _IsCheckedWait = false;
                                                Monitor.PulseAll(_CheckLocker); // Mở khoá các lock ở phần cập nhật UI
                                            }
                                        }
                                    }

                                    if (_IsVerifyAndPrintMode)
                                    {
                                        VerifyAndPrintProcessing(detectModel, ref currentCheckedIndex);
                                    }
                                    break;
                                default:
                                    break;
                            }

                            // Output trigger for Camera
                            if (SharedValues.SelectedJob.OutputCamera)
                            {
                                bool outputCondition = detectModel.CompareResult != ComparisonResult.Valid;
                                if (outputCondition)
                                {
                                    SharedEvents.RaiseOnCameraOutputSignalChangeEvent();
                                }
                            }

                            measureTimeStopw.Stop();
                            TotalChecked++;
                            Interlocked.Exchange(ref _countTotalCheked, TotalChecked);
                            _QueueTotalChekedNumber.Enqueue(_countTotalCheked);

                            if (detectModel.CompareResult == ComparisonResult.Valid)
                            {
                                NumberOfCheckPassed++;
                                Interlocked.Exchange(ref _countTotalPassed, NumberOfCheckPassed);
                                _QueueTotalPassedNumber.Enqueue(_countTotalPassed);
                            }
                            else
                            {
                                NumberOfCheckFailed++;
                                Interlocked.Exchange(ref _countTotalFailed, NumberOfCheckFailed);
                                _QueueTotalFailedNumber.Enqueue(_countTotalFailed);
                            }

                            // Modify Detect Model
                            detectModel.Index = compareIndex;
                            detectModel.CompareTime = measureTimeStopw.ElapsedMilliseconds;
                            detectModel.ProcessingDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");

                            // For Standalone Mode:  instead Enqueue in RSFP feedback
                            if (isDBStandalone)
                            {
                                if (detectModel.CompareResult == ComparisonResult.Valid)
                                {
                                    _QueueBufferPODDataCompared.Enqueue(detectModel.Text);
                                }
                            }
                            // Add result compare to buffer
                            if (_QueueBufferCameraReceivedData.IsEmpty)
                            {
                                _QueueBufferScannerDataCompared.Enqueue(detectModel);
                            }
                            else
                            {
                                _QueueBufferCameraDataCompared.Enqueue(detectModel);
                            }
                            // _QueueBufferCameraDataCompared.Enqueue(detectModel);
                        }
                        await Task.Delay(1, token);
                    }
                }
                catch (OperationCanceledException)
                {
#if DEBUG
                    Console.WriteLine("Thread Compare was Canceled !");
#endif
                    _CTS_UIUpdateCheckedResult?.Cancel();
                    _QueueBufferCameraDataCompared.Enqueue(null);
                    _QueueBufferScannerDataCompared.Enqueue(null);
                    NumberOfSentPrinter = 0;
                    ReceivedCode = 0;

                }
                catch (Exception)
                {
#if DEBUG
                    Console.WriteLine("Thread Compare Fail");
#endif
                    _ = StopProcessAsync();
                }
            });
        }

        #region VerifyAndPrint

        public async Task InitVerifyAndPrindSendDataMethod()
        {
            if (DeviceSharedValues.VPObject.VerifyAndPrintBasicSentMethod) return;
            _Emergency?.Clear();
            _Emergency = await InitVNPUpdatePrintedStatusConditionBuffer();
        }

        private async Task<ConcurrentDictionary<string, int>> InitVNPUpdatePrintedStatusConditionBuffer()
        {

            var result = new ConcurrentDictionary<string, int>();
            var _CheckedResultCodeSet = new HashSet<string>();
            string validCond = ComparisonResult.Valid.ToString();
            int columnCount = SharedValues.ColumnNames.Length;
            foreach (string[] array in SharedValues.ListCheckedResultCode)
            {
                if (columnCount == array.Length && array[2] == validCond)
                {
                    _CheckedResultCodeSet.Add(array[1]);
                }
            }

            if (SharedValues.ListPrintedCodeObtainFromFile.Count > 0)
            {
                int codeLenght = SharedValues.ListPrintedCodeObtainFromFile[0].Count() - 1;
                for (int index = 0; index < SharedValues.ListPrintedCodeObtainFromFile.Count; index++)
                {
                    string[] row = SharedValues.ListPrintedCodeObtainFromFile[index].ToArray();  // Get row data
                    string data = "";
                    foreach (PODModel item in SharedValues.SelectedJob.PODFormat) // Get data by POD compare
                    {
                        if (item.Type == PODModel.TypePOD.DATETIME)
                        {
                            data += DateTime.Now;
                        }
                        else if (item.Type == PODModel.TypePOD.FIELD)
                        {
                            data += row[item.Index];
                        }
                        else
                        {
                            data += item.Value;
                        }
                    }

                    if (!_CheckedResultCodeSet.Contains(data)) // Data unchecked
                    {
                        if (_IsVerifyAndPrintMode)
                        {
                            string tmp = "";
                            for (int i = 1; i < row.Length - 1; i++)
                            {
                                PODModel tmpPOD = DeviceSharedValues.VPObject.PrintFieldForVerifyAndPrint.Find(x => x.Index == i);  // Find data by POD Send 
                                if (tmpPOD != null)
                                {
                                    tmp += row[tmpPOD.Index];
                                }
                            }

                            result.TryAdd(tmp, index); 
                        }
                    }
                }
            }
            await Task.Delay(10);
            _CheckedResultCodeSet.Clear();
            return result;
        }

        private void VerifyAndPrintProcessing(DetectModel detectModel, ref int currentCheckedIndex)
        {
            try
            {


                if (GetPrinterStatus())
                {
                    string[] arr = Array.Empty<string>();

                    //Valid
                    if (detectModel.CompareResult == ComparisonResult.Valid)
                    {
                        if (DeviceSharedValues.VPObject.VerifyAndPrintBasicSentMethod) // Verify and Print Basic Sent : Send exactly camera data to printer
                        {
                            arr = new string[3];
                            arr[1] = detectModel.Text;
                        }
                        else
                        {
                            lock (_SyncObjCodeList)
                            {
                                arr = SharedValues.ListPrintedCodeObtainFromFile[currentCheckedIndex]; //Verify and Print Compare send method
                            }
                        }
                    }
                    // Invalid
                    else
                    {
                        if (DeviceSharedValues.VPObject.VerifyAndPrintBasicSentMethod)
                        {
                            arr = new string[3];
                        }
                        else
                        {
                            lock (_SyncObjCodeList)
                            {
                                arr = new string[SharedValues.ListPrintedCodeObtainFromFile[0].Length];
                            }
                        }
                    }
                    RaiseOnReceiveVerifyDataEvent(arr);
                    currentCheckedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }

        }

        public void RaiseOnReceiveVerifyDataEvent(object sender)
        {
            OnReceiveVerifyDataEvent?.Invoke(sender, EventArgs.Empty);
        }

        #endregion End VerifyAndPrint

        private static ComparisonResult CanreadCompare(string text) => (text.Equals(string.Empty)) ? ComparisonResult.Invalided : ComparisonResult.Valid;

        private ComparisonResult StaticTextCompare(string text, string staticText) => (!text.Equals(string.Empty) && text.Equals(staticText)) ? ComparisonResult.Valid : ComparisonResult.Invalided;

        private ComparisonResult DatabaseCompare(string data, ref int currentValidIndex)
        {
            if (_CodeListPODFormat == null)
            {
                return ComparisonResult.Invalided;
            }
            else
            {
                bool checkNull = (data == "");
                if (!checkNull)
                {
                    if (_CodeListPODFormat.TryGetValue(data, out CompareStatus? compareStatus))
                    {
                        if (compareStatus.Index == -1)
                        {
                            compareStatus.Index = SharedValues.ListPrintedCodeObtainFromFile.FindIndex(x => SharedFunctions.GetCompareDataByPODFormat(x, SharedValues.SelectedJob.PODFormat) == data);
                        }
                        if (!compareStatus.Status) // Check duplicate
                        {
                            _CodeListPODFormat[data].Status = true;
                            currentValidIndex = compareStatus.Index;
                            return ComparisonResult.Valid;
                        }
                        else
                        {
                            return ComparisonResult.Duplicated;
                        }
                    }
                    else
                    {
                        return ComparisonResult.Invalided;
                    }
                }
                // Null code
                else
                {
                    return ComparisonResult.Null;
                }
            }
        }
        #endregion

        // UPDATE UI 
        private async void UpdateUIPrintedResponseAsync()
        {
            if (SharedValues.SelectedJob == null) return;
            _CTS_UIUpdatePrintedResponse = new();
            var token = _CTS_UIUpdatePrintedResponse.Token;

            await Task.Run(async () =>
            {
                List<string[]> strPrintedResponseList = new();
                var isAutoComplete = SharedValues.SelectedJob.CompareType == CompareType.Database; // Check if need to auto stop procces when compare type is verify and print
                int numOfResponse = NumberPrinted;
                int currentIndex = 0;
                int currentPage = 0;
                try
                {
                    while (true)
                    {
                        // Only stop if handled all data
                        if (token.IsCancellationRequested)
                            if (_QueueBufferPODDataCompared.IsEmpty)
                                token.ThrowIfCancellationRequested(); // Stop thread

                        // Waiting until have data
                        _QueueBufferPODDataCompared.TryDequeue(out string? podCommand);
                        if (podCommand != null)
                        {
                            //Console.WriteLine(podCommand);
                            // Count Printed Number
                            NumberPrinted++;
                            Interlocked.Exchange(ref _countPrintedCode, NumberPrinted);
                            _QueuePrintedCodeNumber.Enqueue(_countPrintedCode);

                            // For On Production Mode
                            if (_IsOnProductionMode)
                            {
                                var checkedResult = ComparisonResult.None;

                                // Wait for the camera to check
                                lock (_CheckLocker)
                                {
                                    while (_IsCheckedWait) Monitor.Wait(_CheckLocker); // Waiting until detect data was verify
                                    checkedResult = _CheckedResult;
                                    _IsCheckedWait = true;
                                }

                                //Cam Check completed Release key to send POD
                                lock (_PrintLocker)
                                {
                                    _IsPrintedWait = false;
                                    _PrintedResult = checkedResult;
                                    Monitor.PulseAll(_PrintLocker);
                                }

                                if (checkedResult != ComparisonResult.Valid) continue;
                            }

                            // Update printed status
                            if (_CodeListPODFormat.TryGetValue(podCommand, out CompareStatus? compareStatus)) // Get status from done compare list
                            {
                                currentIndex = compareStatus.Index % _MaxDatabaseLine;
                                currentPage = compareStatus.Index / _MaxDatabaseLine;

                                var arrCurId = SharedFunctions.StringToFixedLengthByteArray(currentIndex.ToString(), 7);
                                var arrCurPage = SharedFunctions.StringToFixedLengthByteArray(currentPage.ToString(), 7);
                                var bytesPos = SharedFunctions.CombineArrays(arrCurId, arrCurPage);
                                _QueueCurrentPositionInDatabase.Enqueue(bytesPos);

                                // thinh dang lam
                                // Printed response backup data
                                if (SharedValues.ListPrintedCodeObtainFromFile[compareStatus.Index].Last() == "Waiting")
                                {
                                    lock (_SyncObjCodeList)// Update status column by index compare
                                    {
                                        (SharedValues.ListPrintedCodeObtainFromFile[compareStatus.Index])[^1] = "Printed";
                                        strPrintedResponseList.Add(SharedValues.ListPrintedCodeObtainFromFile[compareStatus.Index]); // Add new printed data to list
                                        _QueuePrintedCode.Enqueue(SharedValues.ListPrintedCodeObtainFromFile[compareStatus.Index]); // Queue raw code for UI
                                    }
                                }

                                // Add data to queue of Backup printed 
                                var printedCode = new List<string[]>(strPrintedResponseList);
                                _QueueBufferBackupPrintedCode.Enqueue(printedCode);

                                // thinh them dieu kiem dung cho may in
                                if (NumberPrinted == SharedValues.SelectedJob.TotalRecDb)
                                {
                                    _ = StopProcessAsync();
                                    //SharedValues.SelectedJob.JobStatus = JobStatus.Accomplished;
                                }
                                // Clear list
                                strPrintedResponseList.Clear();
                            }
                        }

                        //Time delay avoid frezee user interface
                        await Task.Delay(1, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    SharedFunctions.PrintConsoleMessage("Thread update printed status was stoppped!");
                    ReleaseIPCTask();
                    _CTS_BackupPrintedResponse?.Cancel(); // Stop thread export respone data to file
                    _QueueBufferBackupPrintedCode.Enqueue(null); // Queue for backup printed code and status
                }
                catch (Exception)
                {
                    SharedFunctions.PrintConsoleMessage("Thread update printed status was error!");
                    KillAllProccessThread();
                    _ = StopProcessAsync();
                }
            });
        }

        private async void UpdateUICheckedResultAsync()
        {
            _CTS_UIUpdateCheckedResult = new();
            var token = _CTS_UIUpdateCheckedResult.Token;

            await Task.Run(async () =>
            {
                List<string[]> strResultCheckList = new();
                string[] strResult = Array.Empty<string>();
                try
                {
                    while (true)
                    {
                        if (token.IsCancellationRequested)
                            if (_QueueBufferCameraDataCompared.IsEmpty && _QueueBufferScannerDataCompared.IsEmpty)
                                token.ThrowIfCancellationRequested();

                        DetectModel? detectModel = null;
                        if (_QueueBufferCameraDataCompared.IsEmpty)
                        {
                            _ = _QueueBufferScannerDataCompared.TryDequeue(out detectModel);
                        }
                        else
                        {
                            _ = _QueueBufferCameraDataCompared.TryDequeue(out detectModel);
                        }
                        if (detectModel == null) { await Task.Delay(1); continue; };

                        var image = detectModel.Image ?? new Bitmap(100, 100);
                        if (SharedValues.SelectedJob != null && SharedValues.SelectedJob.IsImageExport)
                        {
                            if (detectModel.CompareResult != ComparisonResult.Valid)
                            {
                                var bitmapImage = new ExportImagesModel(image, detectModel.Index);
                                if (bitmapImage != null)
                                    _QueueBackupFailedImage.Enqueue(bitmapImage);
                            }
                        }
                        // Get result 
                        strResult = new string[] { detectModel.Index + "", detectModel.Text, detectModel.CompareResult.ToString(), (detectModel.CompareTime) + " ms", detectModel.ProcessingDateTime, detectModel.Device };
                        strResultCheckList.Add(strResult);

                        lock (_SyncObjCheckedResultList)
                        {
                            SharedValues.ListCheckedResultCode.Add(strResult);
                            _QueueCheckedResult.Enqueue(strResult);

                        }

                        _QueueBufferBackupCheckedResult.Enqueue(new List<string[]>(strResultCheckList)); 
                        _QueueCheckedResultForUpdateUI.Enqueue(detectModel); 
                                                                            
                        _ = strResult.DefaultIfEmpty();
                        strResultCheckList.Clear();
                    }
                }
                catch (OperationCanceledException)
                {
#if DEBUG
                    Console.WriteLine("Thread update checked result was stopped!");
#endif
                    ReleaseBackupTask();
                }
                catch (Exception)
                {
#if DEBUG
                    Console.WriteLine("Thread update checked result was error!");
#endif
                    KillAllProccessThread();
                    _ = StopProcessAsync();
                }

            });
        }

        private void ReleaseBackupTask()
        {
            _CTS_BackupFailedImage?.Cancel(); //Cancel backup failed image task
            _CTS_BackupCheckedResult?.Cancel(); //Cancel backup checked result task
        }

        private void KillTThreadSendWorkingDataToPrinter()
        {
            _CTS_SendWorkingDataToPrinter?.Cancel();
        }

        private void KillAllProccessThread()
        {
            _CTS_UIUpdateCheckedResult?.Cancel();
            _CTS_UIUpdatePrintedResponse?.Cancel();
            _CTS_BackupCheckedResult?.Cancel();
            _CTS_BackupPrintedResponse?.Cancel();
            _CTS_BackupFailedImage?.Cancel();

            DisposeAndClearCTS(_CTS_BackupPrintedResponseDictionary);

            _CTS_SendStsPrint.Cancel();
            _CTS_SendStsPrint.Cancel();
            _CTS_SendData.Cancel();
        }

        public void DisposeAndClearCTS(Dictionary<int, CancellationTokenSource> resourceDictionary)
        {
            foreach (var kvp in resourceDictionary)
            {
                kvp.Value?.Dispose(); // Dispose each CancellationTokenSource
            }
            resourceDictionary.Clear(); // Clear the dictionary
        }

        public void DisposeAndClearListQueue(List<ConcurrentQueue<object>> listQueue)
        {
            foreach (var item in listQueue)
            {
                item.Clear(); // Dispose each CancellationTokenSource
            }
            //resourceDictionary.Clear(); // Clear the dictionary
        }
       

        public void ReleaseResource()
        {
            try
            {
                // Release Cancel Token Source
                _CTS_SendWorkingDataToPrinter?.Cancel();
                _CTS_ReceiveDataFromPrinter?.Cancel();
                _CTS_ReceiveDataFromAllPrinter.Cancel();
                _CTS_CompareAction?.Cancel();
                _CTS_UIUpdatePrintedResponse?.Cancel();
                _CTS_UIUpdateCheckedResult?.Cancel();
                _CTS_BackupPrintedResponse?.Cancel();
                _CTS_BackupCheckedResult?.Cancel();
                _CTS_BackupFailedImage?.Cancel();

                DisposeAndClearCTS(_CTS_BackupPrintedResponseDictionary);
                
                _CTS_SendWorkingDataToPrinter?.Dispose();
                _CTS_ReceiveDataFromAllPrinter?.Dispose();
                _CTS_ReceiveDataFromPrinter?.Dispose();
                _CTS_CompareAction?.Dispose();
                _CTS_UIUpdatePrintedResponse?.Dispose();
                _CTS_UIUpdateCheckedResult?.Dispose();
                _CTS_BackupPrintedResponse?.Dispose();
                _CTS_BackupCheckedResult?.Dispose();
                _CTS_BackupFailedImage?.Dispose();

                SharedValues.ListCheckedResultCode.Clear();
                _CodeListPODFormat.Clear();
                SharedValues.ListPrintedCodeObtainFromFile.Clear();

                _QueueBufferCameraReceivedData.Clear();

                _QueueBufferPrinterReceivedData.Clear();
                DisposeAndClearListQueue(_QueueBufferPrinterReceivedDataList);

                //_QueueBufferPrinterReceivedDataList[SharedValues.SelectedPrinter].Clear();
                _QueueBufferBackupPrintedCode.Clear();
                _QueueBufferBackupCheckedResult.Clear();
                _QueueBufferPODDataCompared.Clear();
                _QueueCheckedResult.Clear();
                _QueuePrintedCode.Clear();
                _QueueTotalChekedNumber.Clear();
                _QueueTotalPassedNumber.Clear();
                _QueueTotalFailedNumber.Clear();
                _QueueCountFeedback.Clear();
                _QueueSentCodeNumber.Clear();
                _QueueReceivedCodeNumber.Clear();
                _QueuePrintedCodeNumber.Clear();
                _QueueBackupFailedImage.Clear();

                OnReceiveVerifyDataEvent -= SendVerifiedDataToPrinter;

            }
            catch (Exception) { }
        }

        private void ReleaseIPCTask()
        {
            _CTS_SendStsPrint?.Cancel();
            _CTS_SendStsCheck?.Cancel();
            _CTS_SendData?.Cancel();
            SharedFunctions.PrintConsoleMessage("Thread send IPC was stoppped!");

        }

        public void StartAllThreadForTesting()
        {
            //try
            //{
            //    if (_printerManager._printers[SharedValues.SelectedPrinter].IsConnected())
            //    {
            //        if (SharedValues.SelectedJob is null) { SharedFunctions.PrintConsoleMessage("Job Not Found !"); return; }
            //        SharedFunctions.PrintConsoleMessage("Please Disconnect Printer before start Simulate !"); return;
            //    }

            //    SharedValues.OperStatus = OperationStatus.Running;
            //    SimulationPerformanceTestAsync();
            //    SaveJobChangedSettings(SharedValues.SelectedJob);
            //    SendCompleteDataToUIAsync();
            //    CompareAsync();
            //    ExportImagesToFileAsync();
            //    UpdateUICheckedResultAsync();
            //    ExportCheckedResultToFileAsync();

            //    if (SharedValues.SelectedJob.CompareType == CompareType.Database)
            //    {
            //        UpdateUIPrintedResponseAsync();
            //        ExportPrintedResponseToFileAsync();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    SharedFunctions.PrintConsoleMessage(ex.Message);
            //}
        }

        public async void SimulationPerformanceTestAsync()
        {

            //_VirtualCTS = new CancellationTokenSource();
            //var token = _VirtualCTS.Token;
            //await Task.Run(async () =>
            //{
            //    var codes = new List<string[]>();

            //    lock (_SyncObjCodeList)
            //    {
            //        codes = SharedValues.ListPrintedCodeObtainFromFile.Where(x => x.Last() == "Waiting").ToList();
            //    }

            //    if (SharedValues.SelectedJob?.JobType == JobType.VerifyAndPrint)
            //    {
            //    }

            //    try
            //    {
            //        for (int i = 0; i < codes.Count; i++)
            //        {
            //            token.ThrowIfCancellationRequested();
            //            string[] codeModel = codes[i];
            //            if (codeModel.Last() == "Printed") continue;

            //            PODDataModel podDataModel = new()
            //            {
            //                Text = "RSFP;1/101;DATA;"
            //            };
            //            podDataModel.Text += string.Join(";", codeModel.Take(codeModel.Length - 1).Skip(1));
            //            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //            string imageBytespath = System.IO.Path.Combine(currentDirectory, "Resources", "ImageBytes.bin");
            //            var imageBytes = SharedFunctions.ReadByteArrayFromFile(imageBytespath);

            //            DetectModel detectModel = new()
            //            {
            //                ImageBytes = imageBytes,
            //                Image = new(SharedFunctions.GetImageFromImageByte(imageBytes)),
            //                Text = SharedFunctions.GetCompareDataByPODFormat(codeModel, SharedValues.SelectedJob.PODFormat)
            //            };
            //            _QueueBufferPrinterReceivedData[SharedValues.SelectedPrinter].Enqueue(podDataModel);
            //            _QueueBufferCameraReceivedData.Enqueue(detectModel);

            //            await Task.Delay(20);
            //        }
            //    }
            //    catch (OperationCanceledException)
            //    {
            //        Console.WriteLine("Thread virual data was stopped !");
            //    }
            //    catch
            //    {
            //        Console.WriteLine("Thread virual data was stopped !");
            //        _VirtualCTS?.Cancel();
            //    }
            //});
        }

    }
}
