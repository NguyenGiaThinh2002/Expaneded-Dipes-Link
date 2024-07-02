using IPCSharedMemory;
using SharedProgram.DeviceTransfer;
using SharedProgram.Models;
using SharedProgram.Shared;
using System.Diagnostics;
using static IPCSharedMemory.Datatypes.Enums;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLinkDeviceTransfer
{
    /// <summary>
    /// IPC Memory Name distinguished by indexes
    /// </summary>
    /// 
    public partial class Program
    {
        private void ListenConnectionParam()
        {
            Task.Run(() => ActionButtonFromUIProcessingAsync());

            Task.Run(async () =>
            {
                using IPCSharedHelper ipc = new(JobIndex, "UIToDeviceSharedMemory_DT", SharedValues.SIZE_1MB, isReceiver: true);
                MemoryTransfer.SendRestartStatusToUI(_ipcDeviceToUISharedMemory_DT, JobIndex, DataConverter.ToByteArray(RestartStatus.Successful));
                while (true)
                {
                    bool isCompleteDequeue = ipc.MessageQueue.TryDequeue(out byte[]? result);
                    if (isCompleteDequeue && result != null)
                    {
                        switch (result[0])
                        {
                            case (byte)SharedMemoryCommandType.UICommand:
                                switch (result[2])
                                {
                                    // Get Params Setting for Connection (IP, Port) device 
                                    case (byte)SharedMemoryType.ParamsSetting:
                                        GetConenctionParams(result);
                                        break;

                                    // Get Action Button Type (Start , Stop, Trigger) signal 
                                    case (byte)SharedMemoryType.ActionButton:
                                        GetActionButtons(result);
                                        break;
                                }
                                break;
                        }
                    }
                    await Task.Delay(10);
                }
            });
            
        }

        private void GetActionButtons(byte[] result)
        {
            var resultActionButton = new byte[result.Length - 3];
            Array.Copy(result, 3, resultActionButton, 0, resultActionButton.Length);
            DeviceSharedFunctions.GetActionButton(resultActionButton);
        }

        private void GetConenctionParams(byte[] result)
        {
            try
            {
                var resultParams = new byte[result.Length - 3]; // reject header
                Array.Copy(result, 3, resultParams, 0, resultParams.Length);
                var systemParams = DataConverter.FromByteArray<ConnectParamsModel>(resultParams);
                if (systemParams is null) return;
                DeviceSharedFunctions.GetConnectionParamsSetting(systemParams);
                SendSettingToSensorController();
            }
            catch (Exception) { }
        }

        private void SendSettingToSensorController()
        {
            try
            {
                SharedEvents.OnControllerDataChange -= SharedEvents_OnControllerDataChange;
                SharedEvents.OnControllerDataChange += SharedEvents_OnControllerDataChange;
                if (ControllerDeviceHandler != null && ControllerDeviceHandler.IsConnected() && DeviceSharedValues.EnController)
                {
                    string strDelaySensor = int.Parse(DeviceSharedValues.DelaySensor).ToString("D5");
                    string strDisableSensor = int.Parse(DeviceSharedValues.DisableSensor).ToString("D5");
                    string strPulseEncoder = int.Parse(DeviceSharedValues.PulseEncoder).ToString("D5");
                    float encoderDia = float.Parse(DeviceSharedValues.EncoderDiameter) * 100.0f;
                    string strEncoderDia = ((int)encoderDia).ToString("D5");
                    string strCommand = string.Format("(P{0}D{1}L{2}H{3})",
                           strPulseEncoder,
                           strEncoderDia,
                           strDelaySensor,
                           strDisableSensor);
                    //Console.WriteLine("Command: " + strCommand);
                    ControllerDeviceHandler.SendData(strCommand);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error send data to controller {ex.Message}");
            }
        }

        private void SharedEvents_OnControllerDataChange(object? sender, EventArgs e)
        {
            try
            {
                if (sender is string)
                {
                    var message = sender as string;
                    if (message != null && DeviceSharedValues.EnController)
                    {
                        var fullMess = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ": " + message + "\n";
                        //   Console.WriteLine("Full mess: " + fullMess);
                        var data = DataConverter.ToByteArray(fullMess);
                        //  Console.WriteLine(data.Length);
                        MemoryTransfer.SendControllerResponseMessageToUI(_ipcDeviceToUISharedMemory_DT, JobIndex, data);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AlwaySendPrinterOperationToUI()
        {
            Task.Run(async () =>
            {

                while (true)
                {
                    try
                    {
                     //   await Console.Out.WriteLineAsync("ConnecttionPrinter State : " + SharedValues.OperStatus);
                        MemoryTransfer.SendOperationStatusToUI(_ipcDeviceToUISharedMemory_DT, JobIndex, DataConverter.ToByteArray(SharedValues.OperStatus));
                        
                    }
                    catch (Exception) { }

                    await Task.Delay(2000);
                }
            });
        }

        private bool _isPauseAction;
      
        private bool _isStopOrPauseAction;
        private async void ActionButtonFromUIProcessingAsync()
        {
            while (true)
            {
                switch (DeviceSharedValues.ActionButtonType)
                {
                    case ActionButtonType.LoadDB: // Lệnh load dữ liệu khi UI yêu cầu lần đầu
                       // await Console.Out.WriteLineAsync("Vao day mot cach tu nhien");
                        await LoadSelectedJob();
                        NotificationProcess(NotifyType.DeviceDBLoaded);
                        break;
                    case ActionButtonType.Start:

                        _isStopOrPauseAction = false;
                        StartProcessAction(false); // Start without DB Load  
                        break;
                    case ActionButtonType.Pause: //Pause 
                        _isStopOrPauseAction = true;
                        _isPauseAction = true;
                        NotificationProcess(NotifyType.PauseSystem);
                        _ = StopProcessAsync();
                        break;
                    case ActionButtonType.Stop: //Stop 
                        _isPauseAction = false;
                        _isStopOrPauseAction = true;
                        NotificationProcess(NotifyType.StopSystem);
                        _ = StopProcessAsync();
                        break;
                    case ActionButtonType.Trigger: // Trigger
                        TriggerCamera();
                        break;
                    case ActionButtonType.ReloadTemplate: // Reload template
                        ObtainPrintProductTemplateList();
                        break;
                    case ActionButtonType.Reprint: // RePrint
#if DEBUG
                        Console.WriteLine("Start Reprint !");
#endif
                        RePrintAsync();
                        break;
                    default:
                        break;
                }
                DeviceSharedValues.ActionButtonType = ActionButtonType.Unknown; //Prevent state retention
                await Task.Delay(10);
            }
        }

        private async void StartProcessAction(bool startWithDB)
        {
            Task<int> connectionCode = CheckDeviceConnectionAsync();
            if (connectionCode == null) return;
            int code = connectionCode.Result;
            if (code == 0)
            {
                if (startWithDB)
                {
                    await LoadSelectedJob();
                }
                StartProcess();
            }
            else if (code == 1)
            {
#if DEBUG
                await Console.Out.WriteLineAsync("Please check camera connection !");
#endif
                NotificationProcess(NotifyType.NotConnectCamera);
            }
            else if (code == 2)
            {
#if DEBUG
                await Console.Out.WriteLineAsync("Please check printer connection !");
#endif
                NotificationProcess(NotifyType.NotConnectPrinter);
            }
        }

        private void TriggerCamera()
        {
            if (DatamanCameraDeviceHandler != null && DatamanCameraDeviceHandler.IsConnected)
            {
                DatamanCameraDeviceHandler.ManualInputTrigger();
            }
        }

        private static bool _isLoading = false;
        private static readonly object lockObject = new();
        public async Task LoadSelectedJob()
        {
            lock (lockObject)
            {
                if (_isLoading)
                {
                    SharedFunctions.PrintDebugMessage("Task loading DB is already running.");
                    return;
                }
                _isLoading = true;
                SharedFunctions.PrintDebugMessage("Loading DB");
            }
            try
            {
                string? selectedJobName = SharedFunctions.GetSelectedJobNameList(JobIndex).FirstOrDefault();
                _SelectedJob = SharedFunctions.GetJobSelected(selectedJobName, JobIndex);
                await Console.Out.WriteLineAsync(_SelectedJob?.Index.ToString());
                if (_SelectedJob != null)
                {
                    _IsAfterProductionMode = _SelectedJob.JobType == JobType.AfterProduction;
                    _IsOnProductionMode = _SelectedJob.JobType == JobType.OnProduction;
                    _IsVerifyAndPrintMode = _SelectedJob.JobType == JobType.VerifyAndPrint;

                    _TotalCode = 0;
                    NumberOfSentPrinter = 0;
                    ReceivedCode = 0;
                    NumberPrinted = 0;

                    TotalChecked = 0;
                    NumberOfCheckPassed = 0;
                    NumberOfCheckFailed = 0;

                    _ListCheckedResultCode.Clear();
                    _ListPrintedCodeObtainFromFile.Clear();
                    _CodeListPODFormat.Clear();

                    SharedFunctions.SaveStringOfPrintedResponePath(SharedPaths.PathSubJobsApp + $"{JobIndex + 1}\\","printedPathString", _SelectedJob.PrintedResponePath);
                    SharedFunctions.SaveStringOfCheckedPath(SharedPaths.PathCheckedResult + $"Job{JobIndex + 1}\\", "checkedPathString", _SelectedJob.CheckedResultPath);
                    
                    await InitDataAsync(_SelectedJob);
                }
            }
            catch (Exception ex) 
            { 
                SharedFunctions.PrintDebugMessage(ex.Message); 
            }
            finally
            {
                lock (lockObject)
                {
                    _isLoading = false; // done loading db flag
                    SharedFunctions.PrintDebugMessage("Loading DB Completed !");
                }
            }
        }
        private void NotificationProcess(NotifyType notifyType)
        {
            MemoryTransfer.SendMessageJobStatusToUI(_ipcDeviceToUISharedMemory_DT, JobIndex, DataConverter.ToByteArray(notifyType));
        }

    }
}
