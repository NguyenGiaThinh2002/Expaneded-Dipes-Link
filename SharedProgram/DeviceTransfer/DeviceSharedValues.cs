using SharedProgram.Models;
using System.IO.Ports;
using static SharedProgram.DataTypes.CommonDataType;

namespace SharedProgram.DeviceTransfer
{
    public class DeviceSharedValues
    {
        public static int Index = 0;

        #region Printer
        public static string PrinterIP = "";
        public static double PrinterPort = 0;
        public static bool IsCheckPrinterSettingsEnabled;
        #endregion

        #region Camera
        public static string CameraIP = "";
        public static CameraSeries CameraSeries = CameraSeries.Unknown; 
        public static DatamanReadMode DatamanReadMode = DatamanReadMode.Basic;
        public static bool IsCameraConnected = false;
        public static CameraInfos? CameraInfos = new CameraInfos { Index = 0, ConnectionStatus = false, Info = new CamInfo() };
        #endregion

        #region Barcode Scanner
        public static string ComName;
        public static int BitPerSeconds;
        public static Parity Parity;
        public static int DataBits;
        public static StopBits StopBits;
        #endregion

        #region Controller
        public static bool EnController;
        public static string ControllerIP = "";
        public static double ControllerPort = 0;
        public static string DelaySensor = "";
        public static string DisableSensor = "";
        public static string PulseEncoder = "";
        public static string EncoderDiameter = "";
        public static string DelaySensor2 = "";
        public static string DisableSensor2 = "";
        public static string PulseEncoder2 = "";
        public static string EncoderDiameter2 = "";
        #endregion

        #region Verify and Print
        public static VerifyAndPrintModel VPObject = new();
        #endregion

        public static ActionButtonType ActionButtonType = ActionButtonType.Unknown;

        public static int WidthImage { get; set; }
        public static int HeigthImage { get; set; }
    }
}
