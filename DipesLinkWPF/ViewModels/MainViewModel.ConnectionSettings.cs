using DipesLink.Views.UserControls.MainUc;
using IPCSharedMemory;
using SharedProgram.Shared;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using static SharedProgram.DataTypes.CommonDataType;

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

                #region Printer
                ViewModelSharedValues.Settings.SystemParamsList[i].PrinterIP = ConnectParamsList[i].PrinterIP;
                ViewModelSharedValues.Settings.SystemParamsList[i].PrinterPort = ConnectParamsList[i].PrinterPort;
                ViewModelSharedValues.Settings.SystemParamsList[i].IsCheckPrinterSettingsEnabled = ConnectParamsList[i].IsCheckPrinterSettingsEnabled;

                ViewModelSharedValues.Settings.SystemParamsList[i].PrinterIPs = ConnectParamsList[i].PrinterIPs;
                ViewModelSharedValues.Settings.SystemParamsList[i].PrinterPorts = ConnectParamsList[i].PrinterPorts;
                ViewModelSharedValues.Settings.SystemParamsList[i].IsCheckPrintersSettingsEnabled = ConnectParamsList[i].IsCheckPrintersSettingsEnabled;
                #endregion

                #region Camera
                ViewModelSharedValues.Settings.SystemParamsList[i].CameraIP = ConnectParamsList[i].CameraIP;
                ViewModelSharedValues.Settings.SystemParamsList[i].CameraSeries = ConnectParamsList[i].CameraSeries;
                ViewModelSharedValues.Settings.SystemParamsList[i].DatamanReadMode = ConnectParamsList[i].DatamanReadMode;
                #endregion

                #region Controller
                ViewModelSharedValues.Settings.SystemParamsList[i].EnController = ConnectParamsList[i].EnController;
                ViewModelSharedValues.Settings.SystemParamsList[i].ControllerIP = ConnectParamsList[i].ControllerIP;
                ViewModelSharedValues.Settings.SystemParamsList[i].ControllerPort = ConnectParamsList[i].ControllerPort;
                ViewModelSharedValues.Settings.SystemParamsList[i].DisableSensor = ConnectParamsList[i].DisableSensor;
                ViewModelSharedValues.Settings.SystemParamsList[i].DelaySensor = ConnectParamsList[i].DelaySensor;
                ViewModelSharedValues.Settings.SystemParamsList[i].PulseEncoder = ConnectParamsList[i].PulseEncoder;
                ViewModelSharedValues.Settings.SystemParamsList[i].EncoderDiameter = ConnectParamsList[i].EncoderDiameter;

                ViewModelSharedValues.Settings.SystemParamsList[i].DisableSensor2 = ConnectParamsList[i].DisableSensor2;
                ViewModelSharedValues.Settings.SystemParamsList[i].DelaySensor2 = ConnectParamsList[i].DelaySensor2;
                ViewModelSharedValues.Settings.SystemParamsList[i].PulseEncoder2 = ConnectParamsList[i].PulseEncoder2;
                ViewModelSharedValues.Settings.SystemParamsList[i].EncoderDiameter2 = ConnectParamsList[i].EncoderDiameter2;
                #endregion

                #region Barcode Scanner
                ViewModelSharedValues.Settings.SystemParamsList[i].ComName = ConnectParamsList[i].ComName;
                ViewModelSharedValues.Settings.SystemParamsList[i].BitPerSeconds = ConnectParamsList[i].BitPerSeconds;
                ViewModelSharedValues.Settings.SystemParamsList[i].Parity = ConnectParamsList[i].Parity;
                ViewModelSharedValues.Settings.SystemParamsList[i].DataBits = ConnectParamsList[i].DataBits;
                ViewModelSharedValues.Settings.SystemParamsList[i].StopBits = ConnectParamsList[i].StopBits;
                #endregion

                #region Verify and Print
                ViewModelSharedValues.Settings.SystemParamsList[i].PrintFieldForVerifyAndPrint = ConnectParamsList[i].PrintFieldForVerifyAndPrint;
                ViewModelSharedValues.Settings.SystemParamsList[i].FailedDataSentToPrinter = ConnectParamsList[i].FailedDataSentToPrinter;
                ViewModelSharedValues.Settings.SystemParamsList[i].VerifyAndPrintBasicSentMethod = ConnectParamsList[i].VerifyAndPrintBasicSentMethod;
                #endregion

            }

            ViewModelSharedValues.Settings.NumberOfStation = StationSelectedIndex + 1;
            ViewModelSharedFunctions.SaveSetting();
        }

        internal void AutoSaveConnectionSetting(int index, AutoSaveSettingsType autoSaveSettingsType) // Auto save Connection Setting according to Textbox change
        {
            if (JobSettings.IsInitializing) return;
            ConnectParamsList[index].Index = index;
            switch (autoSaveSettingsType)
            {
                case AutoSaveSettingsType.Printer:
                    #region Printer
                    ConnectParamsList[index].PrinterIP = CurrentConnectParams.PrinterIP;
                    ConnectParamsList[index].PrinterPort = CurrentConnectParams.PrinterPort;
                    ConnectParamsList[index].IsCheckPrinterSettingsEnabled = CurrentConnectParams.IsCheckPrinterSettingsEnabled;

                    ConnectParamsList[index].PrinterIPs = CurrentConnectParams.PrinterIPs;
                    ConnectParamsList[index].PrinterPorts = CurrentConnectParams.PrinterPorts;
                    ConnectParamsList[index].IsCheckPrintersSettingsEnabled = CurrentConnectParams.IsCheckPrintersSettingsEnabled;
                    #endregion
                    break;
                case AutoSaveSettingsType.Camera:
                    #region Camera
                    ConnectParamsList[index].CameraIP = CurrentConnectParams.CameraIP;
                    ConnectParamsList[index].CameraSeries = CurrentConnectParams.CameraSeries;
                    ConnectParamsList[index].DatamanReadMode = CurrentConnectParams.DatamanReadMode;
                    #endregion
                    break;
                case AutoSaveSettingsType.BarcodeScanner:
                    #region Barcode Scanner
                    ConnectParamsList[index].ComName = CurrentConnectParams.ComName;
                    ConnectParamsList[index].BitPerSeconds = CurrentConnectParams.BitPerSeconds;
                    ConnectParamsList[index].Parity = CurrentConnectParams.Parity;
                    ConnectParamsList[index].DataBits = CurrentConnectParams.DataBits;
                    ConnectParamsList[index].StopBits = CurrentConnectParams.StopBits;
                    #endregion
                    break;
                case AutoSaveSettingsType.Controller:
                    #region Controller
                    ConnectParamsList[index].EnController = CurrentConnectParams.EnController;
                    ConnectParamsList[index].ControllerIP = CurrentConnectParams.ControllerIP;
                    ConnectParamsList[index].ControllerPort = CurrentConnectParams.ControllerPort;
                    ConnectParamsList[index].DelaySensor = CurrentConnectParams.DelaySensor;
                    ConnectParamsList[index].DisableSensor = CurrentConnectParams.DisableSensor;
                    ConnectParamsList[index].PulseEncoder = CurrentConnectParams.PulseEncoder;
                    ConnectParamsList[index].EncoderDiameter = CurrentConnectParams.EncoderDiameter;
                    ConnectParamsList[index].DelaySensor2 = CurrentConnectParams.DelaySensor2;
                    ConnectParamsList[index].DisableSensor2 = CurrentConnectParams.DisableSensor2;
                    ConnectParamsList[index].PulseEncoder2 = CurrentConnectParams.PulseEncoder2;
                    ConnectParamsList[index].EncoderDiameter2 = CurrentConnectParams.EncoderDiameter2;
                    #endregion
                    break;
                case AutoSaveSettingsType.VerifyAndPrint:
                    #region Verify and Print Settings
                    ConnectParamsList[index].PrintFieldForVerifyAndPrint = CurrentConnectParams.PrintFieldForVerifyAndPrint;
                    ConnectParamsList[index].FailedDataSentToPrinter = CurrentConnectParams.FailedDataSentToPrinter;
                    ConnectParamsList[index].VerifyAndPrintBasicSentMethod = CurrentConnectParams.VerifyAndPrintBasicSentMethod;
                    #endregion
                    break;
                default:
                    break;
            }

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
