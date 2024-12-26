using SharedProgram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedProgram.Shared
{
    public class SharedEvents
    {

        public class PrinterDataHandler
        {
            // Event declaration with index as an additional parameter
            public static event EventHandler<PrinterDataEventArgs>? OnPrinterDataChange;

            // Method to raise the event with index and data
            public static void RaiseOnPrinterDataChangeEvent(PODDataModel data, int index)
            {
                // Raise the event if there are any subscribers, passing the data and index
                OnPrinterDataChange?.Invoke(null, new PrinterDataEventArgs(data, index));
            }
        }

        public class PrinterDataEventArgs : EventArgs
        {
            public PODDataModel Data { get; }
            public int Index { get; }

            public PrinterDataEventArgs(PODDataModel data, int index)
            {
                Data = data;
                Index = index;
            }
        }

        //Event Printer Data Changed
        public static event EventHandler? OnPrinterDataChange;
        public static void RaiseOnPrinterDataChangeEvent(PODDataModel data)
        {
            OnPrinterDataChange?.Invoke(data, EventArgs.Empty);
        }

        public static event EventHandler? OnControllerDataChange;
        public static void RaiseOnControllerDataChangeEvent(string data)
        {
            OnControllerDataChange?.Invoke(data, EventArgs.Empty);
        }

        public static event EventHandler? OnScannerReadDataChange;
        public static void RaiseOnScannerReadDataChangeEvent(DetectModel detectModel)
        {
            OnScannerReadDataChange?.Invoke(detectModel, EventArgs.Empty);
        }

        public static event EventHandler? OnScannerParametersChange;
        public static void RaiseOnScannerParametersChangeEvent()
        {
            OnScannerParametersChange?.Invoke(null, EventArgs.Empty);
        }

        public static event EventHandler? OnCameraReadDataChange;
        public static void RaiseOnCameraReadDataChangeEvent(DetectModel detectModel)
        {
            OnCameraReadDataChange?.Invoke(detectModel, EventArgs.Empty);
           
        }

        public static event EventHandler? OnCameraOutputSignalChange;
        public static void RaiseOnCameraOutputSignalChangeEvent()
        {
            OnCameraOutputSignalChange?.Invoke(null, EventArgs.Empty);
        }

        public static event EventHandler? OnVerifyAndPrindSendDataMethod;
        public static void RaiseOnVerifyAndPrindSendDataMethod()
        {
            OnVerifyAndPrindSendDataMethod?.Invoke(true, EventArgs.Empty);
        }

        public static event EventHandler? OnRaiseCameraIPAddress;
        public static void OnRaiseCameraIPAddressHandler(string ipAddress)
        {
            OnRaiseCameraIPAddress?.Invoke(ipAddress, EventArgs.Empty);
        }
    }
}
