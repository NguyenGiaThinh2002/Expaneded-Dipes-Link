using CommunityToolkit.Mvvm.Input;
using DipesLink.Views.Models;
using SharedProgram.Models;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLink.Models
{
    public partial class JobOverview : JobModel
    {
        #region ICommand 
        public ICommand StartCommandJob { get; set; }
        public ICommand PauseCommandJob { get; set; }
        public ICommand StopCommandJob { get; set; }
        public ICommand TriggerCommandJob { get; set; }
        public ICommand SimulateCommandJob { get; set; }
        #endregion End ICommand

        #region Properties

        private int _DeviceTransferID;
        public int DeviceTransferID
        {
            get { return _DeviceTransferID; }
            set
            {
                if (_DeviceTransferID != value)
                {
                    _DeviceTransferID = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _JobTitleName;
        public string? JobTitleName
        {
            get { return _JobTitleName; }
            set
            {
                if (_JobTitleName != value)
                {
                    _JobTitleName = value;
                    OnPropertyChanged();
                }
            }
        }

        private ImageSource? _ImageResult;
        public ImageSource? ImageResult
        {
            get { return _ImageResult; }
            set
            {
                if (_ImageResult != value)
                {
                    _ImageResult = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _sentDataNumber = "0";
        public string SentDataNumber
        {
            get { return _sentDataNumber; }
            set
            {
                if (_sentDataNumber != value)
                {
                    _sentDataNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CycleTimePOD_Store { get; set; }
        private string _cycleTimePOD;
        public string CycleTimePOD
        {
            get { return _cycleTimePOD; }
            set
            {
                if (_cycleTimePOD != value)
                {
                    _cycleTimePOD = value;
                    OnPropertyChanged();
                }
            }
        }

        public OperationStatus _operationStatus = OperationStatus.Stopped;
        public OperationStatus OperationStatus
        {
            get { return _operationStatus; }
            set
            {
                if (_operationStatus != value)
                {
                    _operationStatus = value;
                    OnPropertyChanged();
                }
            }
        }


       

        private string _receivedDataNumber = "0";
        public string ReceivedDataNumber
        {
            get { return _receivedDataNumber; }
            set
            {
                if (_receivedDataNumber != value)
                {
                    _receivedDataNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _printedDataNumber = "0";
        public string PrintedDataNumber
        {
            get { return _printedDataNumber; }
            set
            {
                if (_printedDataNumber != value)
                {
                    _printedDataNumber = value;
                    OnPropertyChanged();
                }
            }
        }


        private int _currentIndex;
        public int CurrentIndexDB
        {
            get { return _currentIndex; }
            set
            {
                if (_currentIndex != value)
                {
                    _currentIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _currentPage;
        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _currentCodeData;
        public string? CurrentCodeData
        {
            get { return _currentCodeData; }
            set
            {
                if (_currentCodeData != value)
                {
                    _currentCodeData = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _StationStatus;
        public string? StationStatus
        {
            get { return _StationStatus; }
            set
            {
                if (_StationStatus != value)
                {
                    _StationStatus = value;
                    OnPropertyChanged();
                }
            }
        }


        private List<PODModel>? _podModel;
        public List<PODModel>? ListPODFormat
        {
            get { return _podModel; }
            set
            {
                if (_podModel != value)
                {
                    _podModel = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _pathOfFailedImage;
        public string? PathOfFailedImage
        {
            get { return _pathOfFailedImage; }
            set
            {
                if (_pathOfFailedImage != value)
                {
                    _pathOfFailedImage = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? _nameOfFailedImage;
        public string? NameOfFailedImage
        {
            get { return _nameOfFailedImage; }
            set
            {
                if (_nameOfFailedImage != value)
                {
                    _nameOfFailedImage = value;
                    OnPropertyChanged();
                }
            }
        }


        private string? _eventsLogPath;
        public string? EventsLogPath
        {
            get { return _eventsLogPath; }
            set
            {
                if (_eventsLogPath != value)
                {
                    _eventsLogPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _processingTime;
        public int ProcessingTime
        {
            get { return _processingTime; }
            set
            {
                if (_processingTime != value)
                {
                    _processingTime = value;
                    OnPropertyChanged();
                }
            }
        }
        private Brush _compareResultColor = Brushes.Red;
        public Brush CompareResultColor
        {
            get { return _compareResultColor; }
            set
            {
                _compareResultColor = value;
                OnPropertyChanged();
            }
        }

        private ComparisonResult _compareResult = ComparisonResult.None;
        public ComparisonResult CompareResult
        {
            get
            {
                ChangeColorResult();
                return _compareResult;
            }
            set
            {
                ChangeColorResult();
                if (_compareResult != value)
                {
                    _compareResult = value;

                    OnPropertyChanged();
                }
            }
        }

        //private string _totalChecked = "0";
        //public string TotalChecked
        //{
        //    get { return _totalChecked; }
        //    set
        //    {
        //        if (_totalChecked != value)
        //        {
        //            _totalChecked = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}


        //private string _totalPassed = "0";
        //public string TotalPassed
        //{
        //    get { if (!int.TryParse(_totalPassed, out _)) return "0"; return _totalPassed; }
        //    set
        //    {
        //        if (_totalPassed != value)
        //        {
        //            _totalPassed = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        //private string _totalFailed = "0";
        //public string TotalFailed
        //{
        //    get { if (!int.TryParse(_totalFailed, out _)) return "0"; return _totalFailed; }
        //    set
        //    {
        //        if (_totalFailed != value)
        //        {
        //            _totalFailed = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}


        private DataTable _MiniDataTable = new();
        public DataTable MiniDataTable
        {
            get { return _MiniDataTable; }
            set
            {
                if (_MiniDataTable != value)
                {
                    _MiniDataTable = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<DynamicDataRowViewModel> _DataCollection = new();
        public ObservableCollection<DynamicDataRowViewModel> DataCollection
        {
            get { return _DataCollection; }
            set
            {
                if (_DataCollection != value)
                {
                    _DataCollection = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataTable _PrintedCodeDataTable = new();
        public DataTable PrintedCodeDataTable
        {
            get { return _PrintedCodeDataTable; }
            set
            {
                if (_PrintedCodeDataTable != value)
                {
                    _PrintedCodeDataTable = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataTable _CheckedCodeDataTable = new();
        public DataTable CheckedCodeDataTable
        {
            get { return _CheckedCodeDataTable; }
            set
            {
                if (_CheckedCodeDataTable != value)
                {
                    _CheckedCodeDataTable = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _statusStartButton = true;
        public bool StatusStartButton
        {
            get { return _statusStartButton; }
            set
            {
                if (_statusStartButton != value)
                {
                    _statusStartButton = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _statusStopButton = false;
        public bool StatusStopButton
        {
            get { return _statusStopButton; }
            set
            {
                if (_statusStopButton != value)
                {
                    _statusStopButton = value;
                    OnPropertyChanged();
                }
            }
        }


        private SolidColorBrush _statusColor = new(Colors.Red);
        public SolidColorBrush StatusColor
        {
            get { return _statusColor; }
            set
            {
                if (_statusColor != value)
                {
                    _statusColor = value;
                    OnPropertyChanged();
                }
            }
        }


        private string? _statusText = "STOP";
        public string? StatusText
        {
            get { return _statusText; }
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }


        private Visibility _IsShowLoadingDB = Visibility.Collapsed;

        public Visibility IsShowLoadingDB
        {
            get { return _IsShowLoadingDB; }
            set
            {
                if (_IsShowLoadingDB != value)
                {
                    _IsShowLoadingDB = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _IsStartButtonEnable = false;
        public bool IsStartButtonEnable
        {
            get { return _IsStartButtonEnable; }
            set
            {
                if (_IsStartButtonEnable != value)
                {
                    _IsStartButtonEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility _IsShowLoadingChecked = Visibility.Collapsed;

        public Visibility IsShowLoadingChecked
        {
            get { return _IsShowLoadingChecked; }
            set
            {
                if (_IsShowLoadingChecked != value)
                {
                    _IsShowLoadingChecked = value;
                    OnPropertyChanged();
                }
            }
        }


        private GridLength _rowHeightDatabaseTitle= new(0, GridUnitType.Pixel);
        public GridLength RowHeightDatabaseTitle
        {
            get { return _rowHeightDatabaseTitle; }
            set
            {
                if (_rowHeightDatabaseTitle != value)
                {
                    _rowHeightDatabaseTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        private GridLength _rowHeightDatabase = new(1, GridUnitType.Star);
        public GridLength RowHeightDatabase
        {
            get { return _rowHeightDatabase; }
            set
            {
                if (_rowHeightDatabase != value)
                {
                    _rowHeightDatabase = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDBExist { get; set; }
        public CircleChartModel CircleChart { get; set; } = new();
        public byte[]? CameraStsBytes { get; set; }
        public byte PrinterStsBytes { get; set; }
        public byte ControllerStsBytes { get; set; }

        public byte[]? CheckedStatisticNumberBytes { get; set; }
        // public byte[] CurrentPrintedCodeBytes { get; set; }
        // public byte[]? CurrentChekedCodeBytes { get; set; }

        public ConcurrentQueue<byte[]> QueueCurrentPrintedCode { get; set; } = new();
        public ConcurrentQueue<byte[]> QueueCurrentCheckedCode { get; set; } = new();
        public ConcurrentQueue<byte[]> QueueCameraDataDetect { get; set; } = new();
        public ConcurrentQueue<byte[]> QueueSentNumberBytes { get; set; } = new();
        public ConcurrentQueue<byte[]> QueueReceivedNumberBytes { get; set; } = new();
        public ConcurrentQueue<byte[]> QueuePrintedNumberBytes { get; set; } = new();
        public JobSystemSettings JobSystemSettings { get; set; }
        public bool IsLockUISetting { get; internal set; }


        #endregion

        #region Events Declaration

        public event EventHandler? TriggerButtonCommand;
        private void OnTriggerButtonCommandClick()
        {
            TriggerButtonCommand?.Invoke(Index, EventArgs.Empty);
        }

        public event EventHandler? PauseButtonCommand;
        private void OnPauseButtonCommandClick()
        {
            PauseButtonCommand?.Invoke(Index, EventArgs.Empty);
        }

        public event EventHandler? StopButtonCommand;
        public void OnStopButtonCommandClick()
        {
            StopButtonCommand?.Invoke(Index, EventArgs.Empty);
        }

        public event EventHandler? OnExportButtonCommand;
        public void OnExportButtonCommandHandler(int index)
        {
            OnExportButtonCommand?.Invoke(index, EventArgs.Empty);
        }

        public event EventHandler? StartButtonCommand;
        public void OnStartButtonCommandClick()
        {
            StartButtonCommand?.Invoke(Index, EventArgs.Empty);
        }


        public event EventHandler? OnLoadCompleteDatabase;
        public void RaiseLoadCompleteDatabase(object database)
        {
            OnLoadCompleteDatabase?.Invoke(database, EventArgs.Empty);
        }

        public event EventHandler? OnLoadCompleteCheckedDatabase;
        public void RaiseLoadCompleteCheckedDatabase(object database)
        {
            OnLoadCompleteCheckedDatabase?.Invoke(database, EventArgs.Empty);
        }


        public event EventHandler? OnChangePrintedCode;
        public void RaiseChangePrintedCode(object printedCode)
        {
            OnChangePrintedCode?.Invoke(printedCode, EventArgs.Empty);
        }


        public event EventHandler? OnChangeCheckedCode;
        public void RaiseChangeCheckedCode(object checkedCode)
        {
            OnChangeCheckedCode?.Invoke(checkedCode, EventArgs.Empty);
        }

        public event EventHandler? OnPercentageChange;
        public void RaisePercentageChange(object index)
        {
            OnPercentageChange?.Invoke(index, EventArgs.Empty);
        }

        public event EventHandler? OnReprint;
        public void RaiseReprint(object index)
        {
            OnReprint?.Invoke(index, EventArgs.Empty);
        }

        public event EventHandler? OnLoadDb;
        public void RaiseLoadDb(object index)
        {
            OnLoadDb?.Invoke(index, EventArgs.Empty);
        }

        public event EventHandler? SimulateButtonCommand;
        private void OnSimulateButtonCommandClick()
        {
            SimulateButtonCommand?.Invoke(Index, EventArgs.Empty);
        }

        //public event EventHandler? OnFindUnkList;
        //public void OnFindUnkListChanged()
        //{
        //    OnFindUnkList?.Invoke(Index, EventArgs.Empty);
        //}

        #endregion End Events Register

        #region Functions
        public JobOverview()
        {
            StartCommandJob = new RelayCommand(OnStartButtonCommandClick);
            PauseCommandJob = new RelayCommand(OnPauseButtonCommandClick);
            StopCommandJob = new RelayCommand(OnStopButtonCommandClick);
            TriggerCommandJob = new RelayCommand(OnTriggerButtonCommandClick);
            SimulateCommandJob = new RelayCommand(OnSimulateButtonCommandClick);
            JobSystemSettings = new JobSystemSettings();
        }

        private void ChangeColorResult()
        {
            switch (_compareResult)
            {
                case ComparisonResult.Valid:
                    CompareResultColor = Brushes.Green;
                    break;
                case ComparisonResult.Missed:
                case ComparisonResult.Invalided:
                    CompareResultColor = Brushes.Red;
                    break;
                case ComparisonResult.Duplicated:
                    CompareResultColor = Brushes.Orange;
                    break;
                case ComparisonResult.Null:
                    CompareResultColor = Brushes.Black;
                    break;
                default:
                    break;
            }
        }
        #endregion

        // public OperationStatus OperationStatus { get; set; } = OperationStatus.Stopped;
        //private DataView _PrintedCodeDataView;

        //public DataView PrintedCodeDataView
        //{
        //    get { return _PrintedCodeDataView; }
        //    set
        //    {
        //        if (_PrintedCodeDataView != value)
        //        {
        //            _PrintedCodeDataView = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        //private DataView _CheckedCodeDataView;
        //public DataView CheckedCodeDataView
        //{
        //    get { return _CheckedCodeDataView; }
        //    set
        //    {
        //        if (_CheckedCodeDataView != value)
        //        {
        //            _CheckedCodeDataView = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}
    }
}
