using CommunityToolkit.Mvvm.ComponentModel;
using DipesLink.Models;
using DipesLink.Views.Models;
using FontAwesome.Sharp;
using IPCSharedMemory;
using SharedProgram.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLink.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        #region Fields
        protected int _NumberOfStation;
        protected int _MaxDatabaseLine = 500;
        protected static MainViewModel? _instance;
        public static int StationNumber = 4;
        protected bool _detectCamDisconnected;
        protected bool _detectPrinterDisconnected;
        protected List<IPCSharedHelper> listIPCUIToDevice1MB = new();
        public static int numberOfSelectedJobList;
        protected internal bool isSaveJob = false;
        protected readonly List<PODModel> _PODFormat = new();
        protected readonly List<PODModel> _TempPODFormat = new();
        #endregion

        #region Properties
        protected JobModel? _jobModel;

        protected JobModel _createNewJob = new()
        {
            PrinterSeries = PrinterSeries.RynanSeries,
            JobType = JobType.AfterProduction,
            CompareType = CompareType.Database,
            CompleteCondition = CompleteCondition.TotalChecked,
        };
        public JobModel CreateNewJob
        {
            get { return _createNewJob; }
            set
            {
                if (_createNewJob != value)
                {
                    _createNewJob = value;
                    OnPropertyChanged();
                }
            }
        }


        protected SelectJobModel? _SelectJob;
        public SelectJobModel? SelectJob
        {
            get { return _SelectJob; }
            set
            {
                if (_SelectJob != value)
                {
                    _SelectJob = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _TextFieldPodVis;
        public bool TextFieldPodVis
        {
            get { return _TextFieldPodVis; }
            set { _TextFieldPodVis = value; OnPropertyChanged(); }
        }

        private DataView? _dataView;
        public DataView? DataViewPODFormat
        {
            get { return _dataView; }
            set
            {
                if (_dataView != value)
                {
                    _dataView = value;
                    OnPropertyChanged();
                }
            }
        }

        private DataView? _CloneDataView;
        public DataView? CloneDataView
        {
            get { return _CloneDataView; }
            set { _CloneDataView = value; OnPropertyChanged(); }
        }

        private ObservableCollection<PODModel> _podList = new();
        public ObservableCollection<PODModel> PODList
        {
            get { return _podList; }
            set { _podList = value; OnPropertyChanged(); }
        }
        private PODModel? _SelectedColumnItem1;
        public PODModel SelectedColumnItem1
        {
            get { return _SelectedColumnItem1; }
            set
            {
                if (_SelectedColumnItem1 != value)
                {
                    _SelectedColumnItem1 = value;
                    OnPropertyChanged();
                }
            }
        }


        private PODModel? _SelectedColumnItem2;
        public PODModel SelectedColumnItem2
        {
            get { return _SelectedColumnItem2; }
            set
            {
                if (_SelectedColumnItem2 != value)
                {
                    _SelectedColumnItem2 = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _SelectedHeaderIndex;
        public int SelectedHeaderIndex
        {
            get { return _SelectedHeaderIndex; }
            set
            {
                if (_SelectedHeaderIndex != value)
                {
                    _SelectedHeaderIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<PODModel> _SelectedHeadersList = new();
        public ObservableCollection<PODModel> SelectedHeadersList
        {
            get { return _SelectedHeadersList; }
            set
            {
                if (_SelectedHeadersList != value)
                {
                    _SelectedHeadersList = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool isDebugMode;
        public bool IsDebugMode
        {
            get => isDebugMode;
            set
            {
                isDebugMode = value;
                OnPropertyChanged();
            }
        }

        private JobOverview? _currentJob;
        public JobOverview? CurrentJob
        {
            get { return _currentJob; }
            set { _currentJob = value; OnPropertyChanged(); }
        }

        private ObservableCollection<JobOverview> _jobList = new();
        public ObservableCollection<JobOverview> JobList
        {
            get => _jobList; set { _jobList = value; OnPropertyChanged(); }
        }


        private ObservableCollection<JobDeviceStatus> _jobDeviceStatusList = new();
        public ObservableCollection<JobDeviceStatus> JobDeviceStatusList
        {
            get { return _jobDeviceStatusList; }
            set { _jobDeviceStatusList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<PrinterState> _printerState = new();
        public ObservableCollection<PrinterState> PrinterStateList
        {
            get { return _printerState; }
            set
            {
                if (_printerState != value)
                {
                    _printerState = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _jobViewIndex;
        public int JobIndex
        {
            get { return _jobViewIndex; }
            set
            {
                if (_jobViewIndex != value)
                {
                    _jobViewIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private JobModel _currentJobSetting;
        public JobModel CurrentJobSetting
        {
            get { return _currentJobSetting; }
            set
            {
                if (_currentJobSetting != value)
                {
                    _currentJobSetting = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<JobModel> _jobSettingList;
        public ObservableCollection<JobModel> JobSettingList
        {
            get { return _jobSettingList; }
            set
            {
                if (_jobSettingList != value)
                {
                    _jobSettingList = value;
                    OnPropertyChanged();
                }
            }
        }

        private ConnectParamsModel _currentConnectParams;
        public ConnectParamsModel CurrentConnectParams
        {
            get { return _currentConnectParams; }
            set
            {
                if (_currentConnectParams != value)
                {
                    _currentConnectParams = value;
                    OnPropertyChanged();
                }
            }
        }


        private List<ConnectParamsModel> _connectParamsList = new();
        public List<ConnectParamsModel> ConnectParamsList
        {
            get { return _connectParamsList; }
            set
            {
                if (_connectParamsList != value)
                {
                    _connectParamsList = value;
                    OnPropertyChanged();
                }
            }
        }


        private int _stationSelectedIndex;
        public int StationSelectedIndex
        {
            get { return _stationSelectedIndex; }
            set
            {
                if (_stationSelectedIndex != value)
                {
                    _stationSelectedIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<TabItemModel> _tabStation = new();
        public ObservableCollection<TabItemModel> TabStation
        {
            get { return _tabStation; }
            set
            {
                if (_tabStation != value)
                {
                    _tabStation = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _selectedTabIndex = 0;
        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _tabControlEnable;
        public bool TabControlEnable
        {
            get { return _tabControlEnable; }
            set
            {
                if (_tabControlEnable != value)
                {
                    _tabControlEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TextPODResult { get; set; } = string.Empty;
        public List<string[]> RawDataList { get; set; } = new List<string[]>();
        public string TextFieldFeedback { get; set; } = string.Empty;

        [ObservableProperty]
        private List<LeftMenuItem>? _leftMenuItemSrc = new()
        {
            new LeftMenuItem{Icon=IconChar.Dollar,Name ="Home"}
        };


        [ObservableProperty]
        private Visibility _jobViewVisibility = Visibility.Visible;

        [ObservableProperty]
        private Visibility _jobViewVisibility1 = Visibility.Collapsed;

        [ObservableProperty]
        private double _actualWidthMainWindow;

        [ObservableProperty]
        private double _actualWidthGrid = 1000;
        #endregion

    }
}
