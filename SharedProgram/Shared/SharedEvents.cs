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

        // Event Printer Data Changed
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
