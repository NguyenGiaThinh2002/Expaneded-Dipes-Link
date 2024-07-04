using CommunityToolkit.Mvvm.ComponentModel;
using DipesLink.Models;
using DipesLink.Views.Models;
using FontAwesome.Sharp;
using IPCSharedMemory;
using SharedProgram.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace DipesLink.ViewModels
{
    public partial class MainViewModel
    {
       

        [ObservableProperty]
        private List<LeftMenuItem>? _leftMenuItemSrc = new()
        {
            new LeftMenuItem{Icon=IconChar.Dollar,Name ="Home"}
        };

        private JobOverview _currentJob;
        public JobOverview CurrentJob
        {
            get { return _currentJob; }
            set { _currentJob = value; OnPropertyChanged(); }
        }

        private ObservableCollection<JobOverview> _JobList = new();
        /// <summary>
        /// Overview Job List
        /// </summary>
        public ObservableCollection<JobOverview> JobList
        {
            get => _JobList; set { _JobList = value; OnPropertyChanged(); }
        }

       
        private ObservableCollection<JobDeviceStatus> _jobDeviceStatusList = new();
        /// <summary>
        /// List icon for Camera and Printer Connection
        /// </summary>
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


        private int _JobViewIndex;

        public int JobIndex
        {
            get { return _JobViewIndex; }
            set
            {
                if (_JobViewIndex != value)
                {
                    _JobViewIndex = value;
                    OnPropertyChanged();
                }
            }
        }


        [ObservableProperty]
        private Visibility _jobViewVisibility = Visibility.Visible;

        [ObservableProperty]
        private Visibility _jobViewVisibility1 = Visibility.Collapsed;

        [ObservableProperty]
        private double _actualWidthMainWindow;

        [ObservableProperty]
        private double _actualWidthGrid = 1000;

      

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


        //[ObservableProperty]
        //private ObservableCollection<JobModel> _jobSettingList;

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
        public ConnectParamsModel CurrentConnectParams // Setting Object in ViewModel
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

        private List<ConnectParamsModel> _ConnectParamsList = new();
        public List<ConnectParamsModel> ConnectParamsList
        {
            get { return _ConnectParamsList; }
            set
            {
                if (_ConnectParamsList != value)
                {
                    _ConnectParamsList = value;
                    OnPropertyChanged();
                }
            }
        }


        private int _StationSelectedIndex;

        public int StationSelectedIndex
        {
            get { return _StationSelectedIndex; }
            set
            {
                if (_StationSelectedIndex != value)
                {
                    _StationSelectedIndex = value;
                   
                   
                    OnPropertyChanged();
                   
                }
            }
        }

      


        private ObservableCollection<TabItemModel> _TabStation  = new();
        public ObservableCollection<TabItemModel> TabStation
        {
            get { return _TabStation; }
            set
            {
                if (_TabStation != value)
                {
                    _TabStation = value;
                    OnPropertyChanged();
                }
            }
        }



        //private IPCSharedHelper? _ipcDeviceToUISharedMemory_DT;
        //private IPCSharedHelper? _ipcUIToDeviceSharedMemory_DT;
        //private IPCSharedHelper? _ipcUIToDeviceSharedMemory_DT1;
        //private IPCSharedHelper? _ipcDeviceToUISharedMemory_DB;
        //private IPCSharedHelper? _ipcDeviceToUISharedMemory_RD;

        private int _selectedTabIndex = 1;

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
    }

}
