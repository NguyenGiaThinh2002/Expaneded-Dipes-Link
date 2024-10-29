using SharedProgram.Models;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharedProgram.DataTypes.CommonDataType;

namespace SharedProgram.DeviceTransfer
{
    public class DeviceSharedValues
    {
        // Connection parameters
        public static int Index = 0;

        public static bool EnController;

        public static string CameraIP = "";
        public static string PrinterIP = "";
        public static double PrinterPort = 0;
        public static string ControllerIP = "";
        public static double ControllerPort = 0;

        public static string ComName;
        public static int BitPerSeconds;
        public static Parity Parity;
        public static int DataBits;
        public static StopBits StopBits;

        public static bool IsCheckPrinterSettingsEnabled;
        public static string DelaySensor = "";
        public static string DisableSensor = "";
        public static string PulseEncoder = "";
        public static string EncoderDiameter = "";
        public static ActionButtonType ActionButtonType = ActionButtonType.Unknown;

        public static VerifyAndPrintModel VPObject = new();
    }
}
