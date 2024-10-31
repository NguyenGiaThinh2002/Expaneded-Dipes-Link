using SharedProgram.Models;
using SharedProgram.Shared;
using System.IO.Ports;
using System.Text;
using static SharedProgram.DataTypes.CommonDataType;

namespace SharedProgram.DeviceTransfer
{
    public class DeviceSharedFunctions
    {
        /// <summary>
        /// Nhận dữ liệu cài đặt từ software cho process
        /// </summary>
        /// <param name="connectParams"></param>
        public static void GetConnectionParamsSetting(ConnectParamsModel connectParams)
        {
            if (connectParams == null) return;
            try
            {

                #region Printer
                DeviceSharedValues.PrinterIP = connectParams.PrinterIP.Trim();
                DeviceSharedValues.PrinterPort = connectParams.PrinterPort;
                DeviceSharedValues.IsCheckPrinterSettingsEnabled = connectParams.IsCheckPrinterSettingsEnabled;
                #endregion

                #region Camera
                DeviceSharedValues.CameraIP = connectParams.CameraIP.Trim();
                DeviceSharedValues.CameraSeries = connectParams.CameraSeries;
                DeviceSharedValues.DatamanReadMode = connectParams.DatamanReadMode;
                #endregion

                #region Barcode Scanner
                DeviceSharedValues.ComName = connectParams.ComName.Trim();
                DeviceSharedValues.BitPerSeconds = connectParams.BitPerSeconds;
                DeviceSharedValues.Parity = connectParams.Parity;
                DeviceSharedValues.DataBits = connectParams.DataBits;
                DeviceSharedValues.StopBits = connectParams.StopBits;
                #endregion

                #region Controller
                DeviceSharedValues.EnController = connectParams.EnController;
                DeviceSharedValues.ControllerIP = connectParams.ControllerIP.Trim();
                DeviceSharedValues.ControllerPort = connectParams.ControllerPort;
                DeviceSharedValues.DelaySensor = connectParams.DelaySensor.ToString().Trim();
                DeviceSharedValues.DisableSensor = connectParams.DisableSensor.ToString().Trim();
                DeviceSharedValues.PulseEncoder = connectParams.PulseEncoder.ToString().Trim();
                DeviceSharedValues.EncoderDiameter = connectParams.EncoderDiameter.ToString().Trim();
                DeviceSharedValues.DelaySensor2 = connectParams.DelaySensor2.ToString().Trim();
                DeviceSharedValues.DisableSensor2 = connectParams.DisableSensor2.ToString().Trim();
                DeviceSharedValues.PulseEncoder2 = connectParams.PulseEncoder2.ToString().Trim();
                DeviceSharedValues.EncoderDiameter2 = connectParams.EncoderDiameter2.ToString().Trim();
                #endregion

                #region Verify and Print    
                DeviceSharedValues.VPObject.PrintFieldForVerifyAndPrint = connectParams.PrintFieldForVerifyAndPrint; // list pod verify and print
                DeviceSharedValues.VPObject.VerifyAndPrintBasicSentMethod = connectParams.VerifyAndPrintBasicSentMethod;
                DeviceSharedValues.VPObject.FailedDataSentToPrinter = connectParams.FailedDataSentToPrinter;
                #endregion
  
                SharedEvents.RaiseOnVerifyAndPrindSendDataMethod();
                SharedEvents.OnRaiseCameraIPAddressHandler(DeviceSharedValues.CameraIP);

                SharedEvents.RaiseOnScannerParametersChangeEvent();
#if DEBUG
                Console.WriteLine("Camera IP : " + DeviceSharedValues.CameraIP);
                Console.WriteLine("Printer IP : " + DeviceSharedValues.PrinterIP);
                Console.WriteLine("Controller IP : " + DeviceSharedValues.ControllerIP);
                Console.WriteLine("Controller IP : " + DeviceSharedValues.IsCheckPrinterSettingsEnabled);
                Console.WriteLine($"Camera Series: {DeviceSharedValues.CameraSeries}");
                Console.WriteLine($"Camera Dataman Readmode: {DeviceSharedValues.DatamanReadMode}");

                //Console.WriteLine("Basic mode ? : " + DeviceSharedValues.VPObject.VerifyAndPrintBasicSentMethod);
                //Console.WriteLine("Fail Data: " + DeviceSharedValues.VPObject.FailedDataSentToPrinter);
                //Console.WriteLine("Print POD List: ");
                //foreach (var item in DeviceSharedValues.VPObject.PrintFieldForVerifyAndPrint)
                //{
                //    Console.WriteLine(item.ToString());
                //}
#endif
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Get string data of Connection Setting from byte Result
        /// </summary>
        /// <param name="result"></param>
        public static void GetConnectionParamsSetting(byte[] result)
        {
            // Index
            byte[] indexBytes = new byte[1];
            Array.Copy(result, 0, indexBytes, 0, 1);
            DeviceSharedValues.Index = int.Parse(Encoding.ASCII.GetString(indexBytes).Trim());
#if DEBUG
            Console.WriteLine("Index: " + Encoding.ASCII.GetString(indexBytes).Trim());
#endif

            // Cam IP
            byte[] cameraIPBytes = new byte[15];
            Array.Copy(result, indexBytes.Length, cameraIPBytes, 0, 15);
            DeviceSharedValues.CameraIP = Encoding.ASCII.GetString(cameraIPBytes).Trim();
#if DEBUG
            Console.WriteLine("Cam IP: " + Encoding.ASCII.GetString(cameraIPBytes).Trim());
#endif

            // Printer IP
            byte[] printerIPBytes = new byte[15];
            Array.Copy(result,
                indexBytes.Length +
                cameraIPBytes.Length, printerIPBytes, 0, 15);
            DeviceSharedValues.PrinterIP = Encoding.ASCII.GetString(printerIPBytes).Trim();
#if DEBUG
            Console.WriteLine("Printer IP: " + Encoding.ASCII.GetString(printerIPBytes).Trim());
#endif

            // Printer Port
            byte[] printerPortBytes = new byte[5];
            Array.Copy(result,
                indexBytes.Length +
                cameraIPBytes.Length +
                printerIPBytes.Length, printerPortBytes, 0, 5);
             var port = double.Parse(Encoding.ASCII.GetString(printerPortBytes).Trim());
            DeviceSharedValues.PrinterPort = port;
#if DEBUG
            Console.WriteLine("Printer Port: " + Encoding.ASCII.GetString(printerPortBytes).Trim());
#endif

            // Controller IP 
            byte[] controllerIPBytes = new byte[15];
            Array.Copy(result,
                indexBytes.Length +
                cameraIPBytes.Length +
                printerIPBytes.Length +
                printerPortBytes.Length, controllerIPBytes, 0, 15);
            DeviceSharedValues.ControllerIP = Encoding.ASCII.GetString(controllerIPBytes).Trim();
#if DEBUG
            Console.WriteLine("Controller IP : " + Encoding.ASCII.GetString(controllerIPBytes).Trim());
#endif

            // Controller Port
            byte[] controllerPortBytes = new byte[5];
            Array.Copy(result,
                indexBytes.Length +
                cameraIPBytes.Length +
                printerIPBytes.Length +
                printerPortBytes.Length +
                controllerIPBytes.Length, controllerPortBytes, 0, 5);
            var plcPort = Encoding.ASCII.GetString(controllerPortBytes);
            DeviceSharedValues.ControllerPort = double.Parse(plcPort);
#if DEBUG
            Console.WriteLine("Controller Port: " + Encoding.ASCII.GetString(controllerPortBytes).Trim());
#endif
            // Scanner Parameters
            byte[] scannerComNameBytes = new byte[5];
            Array.Copy(result,
                indexBytes.Length +
                cameraIPBytes.Length +
                printerIPBytes.Length +
                printerPortBytes.Length +
                controllerIPBytes.Length, scannerComNameBytes, 0, 5);
            DeviceSharedValues.ComName = Encoding.ASCII.GetString(scannerComNameBytes).Trim();

            byte[] scannerBitPerSecondBytes = new byte[5];
            Array.Copy(result,
                indexBytes.Length +
                cameraIPBytes.Length +
                printerIPBytes.Length +
                printerPortBytes.Length +
                controllerIPBytes.Length, scannerBitPerSecondBytes, 0, 5);
            DeviceSharedValues.BitPerSeconds = int.Parse(Encoding.ASCII.GetString(scannerBitPerSecondBytes));

            byte[] scannerParityBytes = new byte[5];
            Array.Copy(result,
                indexBytes.Length +
                cameraIPBytes.Length +
                printerIPBytes.Length +
                printerPortBytes.Length +
                controllerIPBytes.Length, scannerParityBytes, 0, 5);
            DeviceSharedValues.Parity = (Parity)Enum.Parse(typeof(Parity), Encoding.ASCII.GetString(scannerParityBytes));

            byte[] scannerDataBitsSecondBytes = new byte[5];
            Array.Copy(result,
                indexBytes.Length +
                cameraIPBytes.Length +
                printerIPBytes.Length +
                printerPortBytes.Length +
                controllerIPBytes.Length, scannerDataBitsSecondBytes, 0, 5);
            DeviceSharedValues.DataBits = int.Parse(Encoding.ASCII.GetString(scannerDataBitsSecondBytes));

            byte[] scannerStopBitsBytes = new byte[5];
            Array.Copy(result,
                indexBytes.Length +
                cameraIPBytes.Length +
                printerIPBytes.Length +
                printerPortBytes.Length +
                controllerIPBytes.Length, scannerStopBitsBytes, 0, 5);
            DeviceSharedValues.StopBits = (StopBits)Enum.Parse(typeof(StopBits), Encoding.ASCII.GetString(scannerStopBitsBytes));
        }

        public static void GetActionButton(byte[] result)
        {
            // Index
            byte[] indexBytes = new byte[1];
            Array.Copy(result, 0, indexBytes, 0, 1);

            // Button Type
            byte[] buttonTypeBytes = new byte[1];
            Array.Copy(result, indexBytes.Length, buttonTypeBytes, 0, 1);
            DeviceSharedValues.ActionButtonType = DataConverter.FromByteArray<ActionButtonType>(buttonTypeBytes);  //Encoding.ASCII.GetString(buttonTypeBytes).Trim();
#if DEBUG
            Console.WriteLine("Action button: " + DeviceSharedValues.ActionButtonType.ToString().Trim()); // Test show data to console
#endif
        }
    }
}
