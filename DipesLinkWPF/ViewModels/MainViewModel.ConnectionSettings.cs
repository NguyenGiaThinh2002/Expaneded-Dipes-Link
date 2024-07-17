using DipesLink.Views.UserControls.MainUc;
using IPCSharedMemory;
using SharedProgram.Shared;
using System.Diagnostics;

namespace DipesLink.ViewModels
{
    partial class MainViewModel
    {
        private void InitJobConnectionSettings() //Get saved params of Connection setting to List
        {
            ConnectParamsList = ViewModelSharedValues.Settings.SystemParamsList;
        }

        internal void SaveConnectionSetting()
        {
           // ConnectParamsList = ViewModelSharedValues.Settings.SystemParamsList;
            for (int i = 0; i < _numberOfStation; i++)
            {
                SendConnectionParamsToDeviceTransfer(i);
   
                ViewModelSharedValues.Settings.SystemParamsList[i].Index = ConnectParamsList[i].Index;
                ViewModelSharedValues.Settings.SystemParamsList[i].EnController = ConnectParamsList[i].EnController;
                ViewModelSharedValues.Settings.SystemParamsList[i].CameraIP = ConnectParamsList[i].CameraIP;
                ViewModelSharedValues.Settings.SystemParamsList[i].PrinterIP = ConnectParamsList[i].PrinterIP;
                ViewModelSharedValues.Settings.SystemParamsList[i].PrinterPort = ConnectParamsList[i].PrinterPort;
                ViewModelSharedValues.Settings.SystemParamsList[i].ControllerIP = ConnectParamsList[i].ControllerIP;
                ViewModelSharedValues.Settings.SystemParamsList[i].ControllerPort = ConnectParamsList[i].ControllerPort;

                ViewModelSharedValues.Settings.SystemParamsList[i].DisableSensor = ConnectParamsList[i].DisableSensor;
                ViewModelSharedValues.Settings.SystemParamsList[i].DelaySensor = ConnectParamsList[i].DelaySensor;
                ViewModelSharedValues.Settings.SystemParamsList[i].PulseEncoder = ConnectParamsList[i].PulseEncoder;
                ViewModelSharedValues.Settings.SystemParamsList[i].EncoderDiameter = ConnectParamsList[i].EncoderDiameter;

                ViewModelSharedValues.Settings.SystemParamsList[i].PrintFieldForVerifyAndPrint = ConnectParamsList[i].PrintFieldForVerifyAndPrint;
                ViewModelSharedValues.Settings.SystemParamsList[i].FailedDataSentToPrinter = ConnectParamsList[i].FailedDataSentToPrinter;
                ViewModelSharedValues.Settings.SystemParamsList[i].VerifyAndPrintBasicSentMethod = ConnectParamsList[i].VerifyAndPrintBasicSentMethod;
            }

            ViewModelSharedValues.Settings.NumberOfStation = StationSelectedIndex + 1;
            

            ViewModelSharedFunctions.SaveSetting();
        }

        internal void AutoSaveConnectionSetting(int index) // Auto save Connection Setting according to Textbox change
        {
            if (JobSettings.IsInitializing) return;           
            ConnectParamsList[index].Index = index;
            ConnectParamsList[index].EnController = CurrentConnectParams.EnController;
            ConnectParamsList[index].CameraIP = CurrentConnectParams.CameraIP;
            ConnectParamsList[index].PrinterIP = CurrentConnectParams.PrinterIP;
            ConnectParamsList[index].PrinterPort = CurrentConnectParams.PrinterPort;
            ConnectParamsList[index].ControllerIP = CurrentConnectParams.ControllerIP;
            ConnectParamsList[index].ControllerPort = CurrentConnectParams.ControllerPort;
            ConnectParamsList[index].DelaySensor = CurrentConnectParams.DelaySensor;
            ConnectParamsList[index].DisableSensor = CurrentConnectParams.DisableSensor;
            ConnectParamsList[index].PulseEncoder = CurrentConnectParams.PulseEncoder;
            ConnectParamsList[index].EncoderDiameter = CurrentConnectParams.EncoderDiameter;

            ConnectParamsList[index].PrintFieldForVerifyAndPrint = CurrentConnectParams.PrintFieldForVerifyAndPrint;
            ConnectParamsList[index].FailedDataSentToPrinter = CurrentConnectParams.FailedDataSentToPrinter;
            ConnectParamsList[index].VerifyAndPrintBasicSentMethod = CurrentConnectParams.VerifyAndPrintBasicSentMethod;
            ConnectParamsList[index].Index = index;
            ConnectParamsList[index].EnController = CurrentConnectParams.EnController;
            ConnectParamsList[index].CameraIP = CurrentConnectParams.CameraIP;
            ConnectParamsList[index].PrinterIP = CurrentConnectParams.PrinterIP;
            ConnectParamsList[index].PrinterPort = CurrentConnectParams.PrinterPort;
            ConnectParamsList[index].ControllerIP = CurrentConnectParams.ControllerIP;
            ConnectParamsList[index].ControllerPort = CurrentConnectParams.ControllerPort;
            ConnectParamsList[index].DelaySensor = CurrentConnectParams.DelaySensor;
            ConnectParamsList[index].DisableSensor = CurrentConnectParams.DisableSensor;
            ConnectParamsList[index].PulseEncoder = CurrentConnectParams.PulseEncoder;
            ConnectParamsList[index].EncoderDiameter = CurrentConnectParams.EncoderDiameter;

            CurrentConnectParams = ConnectParamsList[index];
            SaveConnectionSetting();
        }

        /// <summary>
        /// Update camera infor (Type and Model) to UI
        /// </summary>
        internal void UpdateCameraInfo(int index) 
        {
            try
            {
                ConnectParamsList[index].CameraModel = JobList[index].CameraInfo.Info.Name;
                ConnectParamsList[index].SerialNumber = JobList[index].CameraInfo.Info.SerialNumber;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        internal void SelectionChangeSystemSettings(int index)
        {
            CurrentConnectParams = ConnectParamsList[index];
        }

        internal void SendConnectionParamsToDeviceTransfer(int stationIndex)
        {
            try
            {
                int i = stationIndex;
                var sysParamsBytes =  DataConverter.ToByteArray(ViewModelSharedValues.Settings.SystemParamsList[i]);
                MemoryTransfer.SendConnectionParamsToDevice(listIPCUIToDevice1MB[stationIndex], stationIndex, sysParamsBytes);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message);  }
        }
    }
}
