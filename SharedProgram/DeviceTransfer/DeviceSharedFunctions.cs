using SharedProgram.Models;
using SharedProgram.Shared;
using System.Text;
using static SharedProgram.DataTypes.CommonDataType;

namespace SharedProgram.DeviceTransfer
{
    public class DeviceSharedFunctions
    {
        public static void GetConnectionParamsSetting(ConnectParamsModel connectParams)
        {
            if (connectParams == null) return;
            try
            {
                DeviceSharedValues.EnController = connectParams.EnController;
                DeviceSharedValues.PrinterIP = connectParams.PrinterIP.Trim();
                DeviceSharedValues.PrinterPort = connectParams.PrinterPort;
                DeviceSharedValues.CameraIP = connectParams.CameraIP.Trim();
                DeviceSharedValues.ControllerIP = connectParams.ControllerIP.Trim();
                DeviceSharedValues.ControllerPort = connectParams.ControllerPort;

                DeviceSharedValues.DelaySensor = connectParams.DelaySensor.ToString().Trim();
                DeviceSharedValues.DisableSensor = connectParams.DisableSensor.ToString().Trim();
                DeviceSharedValues.PulseEncoder = connectParams.PulseEncoder.ToString().Trim();
                DeviceSharedValues.EncoderDiameter = connectParams.EncoderDiameter.ToString().Trim();

                DeviceSharedValues.VPObject.PrintFieldForVerifyAndPrint = connectParams.PrintFieldForVerifyAndPrint; // list pod verify and print
                DeviceSharedValues.VPObject.VerifyAndPrintBasicSentMethod = connectParams.VerifyAndPrintBasicSentMethod;
                DeviceSharedValues.VPObject.FailedDataSentToPrinter = connectParams.FailedDataSentToPrinter;
                SharedEvents.RaiseOnVerifyAndPrindSendDataMethod();
                SharedEvents.OnRaiseCameraIPAddressHandler(DeviceSharedValues.CameraIP);

#if DEBUG
                Console.WriteLine("Camera IP : " + DeviceSharedValues.CameraIP);
                Console.WriteLine("Printer IP : " + DeviceSharedValues.PrinterIP);
                Console.WriteLine("Controller IP : " + DeviceSharedValues.ControllerIP);
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
