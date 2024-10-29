using SharedProgram.Models;
using System.Collections.Concurrent;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLinkDeviceTransfer
{
    public partial class Program
    {
        public static int JobIndex { get; private set; }
        private readonly object _PrintLocker = new();
        private bool _IsPrintedWait = false;

        readonly static object _SyncObjCodeList = new();
        private ComparisonResult _PrintedResult = ComparisonResult.Valid;
        private readonly object _StopLocker = new();
        private bool _IsStopOK = false;
        private PrinterStatus _PrinterStatus = PrinterStatus.Null;

        private bool _IsAfterProductionMode = false;
        private bool _IsOnProductionMode = false;
        private bool _IsVerifyAndPrintMode = false;
        public int NumberOfSentPrinter { get; set; }

        public int ReceivedCode { get; set; }
        public int NumberPrinted { get; set; }
        public int TotalChecked { get; set; }
        public int NumberOfCheckPassed { get; set; }
        public int NumberOfCheckFailed { get; set; }
        public int CountFeedback { get; set; }

        

       // private string[] DatabaseColunms = Array.Empty<string>();
      //  private JobModel? _SelectedJob = new();
        private object _SyncObjCheckedResultList = new();

        /// <summary>
        /// List of data in POD format with key is data and value is the status checked or not
        /// </summary>
        private readonly ConcurrentDictionary<string, SharedProgram.Controller.CompareStatus> _CodeListPODFormat = new();
        private int _NumberOfDuplicate = 0;
        //private int TotalCode = 0;
        private int _CurrentPage = 0;
        private readonly int _MaxDatabaseLine = 500;

        //public static string ExportNamePrefixFormat = "yyyyMMdd_HHmmss";
        //public string ExportNamePrefixFormat { get; set; } = "yyyyMMdd_HHmmss";
        //public string? ExportNamePrefix { get; set; }

        #region Cancellation Token
        private CancellationTokenSource _CTS_SendWorkingDataToPrinter = new();
        private CancellationTokenSource _CTS_ReceiveDataFromPrinter = new();
        private CancellationTokenSource _CTS_CompareAction = new();
        private CancellationTokenSource _CTS_BackupCheckedResult = new();
        private CancellationTokenSource _CTS_BackupPrintedResponse = new();
        private CancellationTokenSource _CTS_UIUpdatePrintedResponse = new();
        private CancellationTokenSource _CTS_UIUpdateCheckedResult = new();
        private CancellationTokenSource _CTS_SendCompleteDataToUI = new();
        private CancellationTokenSource _CTS_BackupFailedImage = new();
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
        private static string[] _PrintProductTemplateList = Array.Empty<string>();


        #endregion

        private int _countReceivedCode;
        private int _countSentCode;
        private int _countPrintedCode;
        private int _countTotalCheked;
        private int _countTotalPassed;
        private int _countTotalFailed;
        private int _countFb;
        private double _cycleTimePODDataTransfer;
       

        #region QUEUE
        /// <summary>
        /// Queue for printer raw data
        /// </summary>
        private ConcurrentQueue<object> _QueueBufferPrinterReceivedData = new();
        
        /// <summary>
        /// Buffer for camera raw data
        /// </summary>
        private readonly ConcurrentQueue<DetectModel?> _QueueBufferCameraReceivedData = new();

        /// <summary>
        /// Queue Buffer for Camera Data Compared (use for UI update)
        /// </summary>
        private readonly ConcurrentQueue<DetectModel?> _QueueBufferCameraDataCompared = new();

        /// Buffer for scanner data
        /// </summary>
        private readonly ConcurrentQueue<DetectModel?> _QueueBufferScannerReceivedData = new();

        /// <summary>
        /// Queue Buffer for Scanner Data Compared (use for UI update)
        /// </summary>
        private readonly ConcurrentQueue<DetectModel?> _QueueBufferScannerDataCompared = new();


        /// <summary>
        /// Queue contain only string of Data by POD Data Compared (use for UI update)
        /// </summary>
        private readonly ConcurrentQueue<string?> _QueueBufferPODDataCompared = new();

        /// <summary>
        /// Queue for Backup printed code and status
        /// </summary>
        private ConcurrentQueue<List<string[]>?> _QueueBufferBackupPrintedCode = new();

        /// <summary>
        /// Queue for backup checked result 
        /// </summary>
        private ConcurrentQueue<List<string[]>?> _QueueBufferBackupCheckedResult = new();

        /// <summary>
        /// Queue send real time DetectModel from Camera to UI
        /// </summary>
        private readonly ConcurrentQueue<DetectModel> _QueueCheckedResultForUpdateUI = new();

        /// <summary>
        /// Queue for Update DataTable printed code on UI
        /// </summary>
        private ConcurrentQueue<string[]> _QueuePrintedCode = new();

        private ConcurrentQueue<ExportImagesModel> _QueueBackupFailedImage = new();

        /// <summary>
        /// Queue for Update DataTable checked result on UI
        /// </summary>
        private ConcurrentQueue<string[]> _QueueCheckedResult = new();

        private ConcurrentQueue<int> _QueueReceivedCodeNumber = new();

        private ConcurrentQueue<int> _QueueSentCodeNumber = new();

        private ConcurrentQueue<int> _QueuePrintedCodeNumber = new();

        private ConcurrentQueue<int> _QueueTotalChekedNumber = new();

        private ConcurrentQueue<int> _QueueTotalPassedNumber = new();

        private ConcurrentQueue<int> _QueueTotalFailedNumber = new();

        private ConcurrentQueue<int> _QueueCountFeedback = new();

        private ConcurrentQueue<byte[]> _QueueCurrentPositionInDatabase = new();

        #endregion


    }
}
