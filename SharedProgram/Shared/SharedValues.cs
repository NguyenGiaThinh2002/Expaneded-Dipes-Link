using SharedProgram.Models;
using System.IO;
using OperationStatus = SharedProgram.DataTypes.CommonDataType.OperationStatus;

namespace SharedProgram.Shared
{
    public class SharedValues
    {
        public static int Index {  get; set; }

        public static int SelectedPrinter { get; set; } = 0;

        public static int SelectedTemplate { get; set; } = 0;

        public static SettingsModel Settings = new();

        public static int NumberOfStation = 4;
        public static string AppName = "DipesLink";

        public static string DeviceTransferName = "DipesLinkDeviceTransfer";

        public static readonly string[] ColumnNames = new string[] { "Index", "ResultData", "Result", "ProcessingTime", "DateTime", "Device" };

        public static OperationStatus OperStatus = OperationStatus.Stopped;
        public static int TotalCode = 0;
        public static string[] DatabaseColunms = Array.Empty<string>();
        public static List<string[]> ListPrintedCodeObtainFromFile = new();

        public static List<List<string[]>> ListPrintedCodeObtainFromFileAllPrinter = new()
        {
            new List<string[]>(),
            new List<string[]>(),
            new List<string[]>(),
            new List<string[]>()
        };


        public static List<string[]> ListCheckedResultCode = new();
        public static JobModel? SelectedJob = new();
        public static string ConnectionString = "AccountDB.db";

        public static long SIZE_1MB = 1024 * 1024 * 1;
        public static long SIZE_100MB = 1024 * 1024 * 100;
        public static long SIZE_200MB = 1024 * 1024 * 200;
        public static long SIZE_50MB = 1024 * 1024 * 50;
    }
}
