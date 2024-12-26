using SharedProgram.Models;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLinkDeviceTransfer
{
    public partial class Program
    {
        public static int JobIndex { get; private set; }
        public readonly object _PrintLocker = new();
        public bool _IsPrintedWait = false;

        public readonly static object _SyncObjCodeList = new();
        public ComparisonResult _PrintedResult = ComparisonResult.Valid;
        public readonly object _StopLocker = new();
        public bool _IsStopOK = false;
        public PrinterStatus _PrinterStatus = PrinterStatus.Null;

        public bool _IsAfterProductionMode = false;
        public bool _IsOnProductionMode = false;
        public bool _IsVerifyAndPrintMode = false;
        public int NumberOfSentPrinter { get; set; }

        public int ReceivedCode { get; set; }
        public int NumberPrinted { get; set; }
        public int TotalChecked { get; set; }
        public int NumberOfCheckPassed { get; set; }
        public int NumberOfCheckFailed { get; set; }
        public int CountFeedback { get; set; }



        // private string[] DatabaseColunms = Array.Empty<string>();
        //  private JobModel? _SelectedJob = new();
        public object _SyncObjCheckedResultList = new();

        /// <summary>
        /// List of data in POD format with key is data and value is the status checked or not
        /// </summary>
        public readonly ConcurrentDictionary<string, SharedProgram.Controller.CompareStatus> _CodeListPODFormat = new();
        public int _NumberOfDuplicate = 0;
        //private int TotalCode = 0;
        public int _CurrentPage = 0;
        public readonly int _MaxDatabaseLine = 500;

        //public static string ExportNamePrefixFormat = "yyyyMMdd_HHmmss";
        //public string ExportNamePrefixFormat { get; set; } = "yyyyMMdd_HHmmss";
        //public string? ExportNamePrefix { get; set; }

        #region Cancellation Token
        public CancellationTokenSource _CTS_SendWorkingDataToPrinter = new();
        public CancellationTokenSource _CTS_ReceiveDataFromPrinter = new();
        public Dictionary<int, CancellationTokenSource> _CTS_ReceiveDataFromPrinterDictionary = new();
        public CancellationTokenSource _CTS_ReceiveDataFromAllPrinter = new();
        public CancellationTokenSource _CTS_CompareAction = new();
        public CancellationTokenSource _CTS_BackupCheckedResult = new();
        public CancellationTokenSource _CTS_BackupPrintedResponse = new();
        //public List<CancellationTokenSource> _CTS_BackupPrintedResponseList = new List<CancellationTokenSource>();
        public Dictionary<int, CancellationTokenSource> _CTS_BackupPrintedResponseDictionary = new();

        public CancellationTokenSource _CTS_UIUpdatePrintedResponse = new();

        public CancellationTokenSource _CTS_UIUpdateCheckedResult = new();
        public CancellationTokenSource _CTS_SendCompleteDataToUI = new();
        public CancellationTokenSource _CTS_BackupFailedImage = new();
        #endregion


        #region LIST
        /// <summary>
        /// List data to print contains printed status
        /// </summary>
       // private List<string[]> _ListPrintedCodeObtainFromFile = new();

        /// <summary>
        /// List checked Result 
        /// </summary>
       // private List<string[]> ListCheckedResultCode = new();
        public static string[] _PrintProductTemplateList = Array.Empty<string>();

        public static List<string[]> _PrintProductTemplateLists = new List<string[]>()
            {
                new string[] {},  // First empty array
                new string[] {},  // Second empty array
                new string[] {},  // Third empty array
                new string[] {}   // Fourth empty array
            };


        #endregion

        public int _countReceivedCode;
        public int _countSentCode;
        public int _countPrintedCode;

        public int _countTotalCheked;
        public int _countTotalPassed;
        public int _countTotalFailed;
        public int _countFb;
        public double _cycleTimePODDataTransfer;
       

        #region QUEUE
        /// <summary>
        /// Queue for printer raw data
        /// </summary>
        private ConcurrentQueue<object> _QueueBufferPrinterReceivedData = new();

        public readonly List<ConcurrentQueue<object>> _QueueBufferPrinterReceivedDataList = new()
        {
            new ConcurrentQueue<object>(),
            new ConcurrentQueue<object>(),
            new ConcurrentQueue<object>(),
            new ConcurrentQueue<object>()
        };

        /// <summary>
        /// Buffer for camera raw data
        /// </summary>
        public readonly ConcurrentQueue<DetectModel?> _QueueBufferCameraReceivedData = new();

        /// <summary>
        /// Queue Buffer for Camera Data Compared (use for UI update)
        /// </summary>
        public readonly ConcurrentQueue<DetectModel?> _QueueBufferCameraDataCompared = new();

        /// Buffer for scanner data
        /// </summary>
        public readonly ConcurrentQueue<DetectModel?> _QueueBufferScannerReceivedData = new();

        /// <summary>
        /// Queue Buffer for Scanner Data Compared (use for UI update)
        /// </summary>
        public readonly ConcurrentQueue<DetectModel?> _QueueBufferScannerDataCompared = new();


        /// <summary>
        /// Queue contain only string of Data by POD Data Compared (use for UI update)
        /// </summary>
        public readonly ConcurrentQueue<string?> _QueueBufferPODDataCompared = new();

        /// <summary>
        /// Queue for Backup printed code and status
        /// </summary>
        public ConcurrentQueue<List<string[]>> _QueueBufferBackupPrintedCode = new();

        public readonly List<ConcurrentQueue<string>> _QueueBufferBackupPrintedCodeTemp = new()
        {
            new ConcurrentQueue<string>(),
            new ConcurrentQueue<string>(),
            new ConcurrentQueue<string>(),
            new ConcurrentQueue<string>()
        };

        /// <summary>
        /// Queue for backup checked result 
        /// </summary>
        public ConcurrentQueue<List<string[]>?> _QueueBufferBackupCheckedResult = new();

        /// <summary>
        /// Queue send real time DetectModel from Camera to UI
        /// </summary>
        public readonly ConcurrentQueue<DetectModel> _QueueCheckedResultForUpdateUI = new();

        /// <summary>
        /// Queue for Update DataTable printed code on UI
        /// </summary>
        public ConcurrentQueue<string[]> _QueuePrintedCode = new();

        public ConcurrentQueue<ExportImagesModel> _QueueBackupFailedImage = new();

        /// <summary>
        /// Queue for Update DataTable checked result on UI
        /// </summary>
        /// 

        // camera
        public ConcurrentQueue<string[]> _QueueCheckedResult = new();

        public ConcurrentQueue<int> _QueueTotalChekedNumber = new();

        public ConcurrentQueue<int> _QueueTotalPassedNumber = new();

        public ConcurrentQueue<int> _QueueTotalFailedNumber = new();

        // printer

        public ConcurrentQueue<int> _QueueReceivedCodeNumber = new();

        public ConcurrentQueue<int> _QueueSentCodeNumber = new();

        public ConcurrentQueue<int> _QueuePrintedCodeNumber = new();

        public ConcurrentQueue<int> _QueueCountFeedback = new();

        public ConcurrentQueue<byte[]> _QueueCurrentPositionInDatabase = new();

        #endregion


    }
}
