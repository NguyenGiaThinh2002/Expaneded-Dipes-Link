using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using static SharedProgram.DataTypes.CommonDataType;

namespace SharedProgram.Models
{
    public class JobModel : CommonSettingsModel
    {
        public int Index { get; set; }

        private string? _name;
        public string? Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        private JobType _JobType;
        public JobType JobType
        {
            get { return _JobType; }
            set { _JobType = value; OnPropertyChanged(); }
        }

        private CompareType _compareType = CompareType.None;
        public CompareType CompareType
        {
            get { return _compareType; }
            set { _compareType = value; OnPropertyChanged(); }
        }
        
        private string? _StaticText;
        public string? StaticText
        {
            get { return _StaticText; }
            set { _StaticText = value; OnPropertyChanged(); }
        }

        private string? _DatabasePath = "";
        public string? DatabasePath
        {
            get { return _DatabasePath; }
            set { _DatabasePath = value; OnPropertyChanged(); }
        }


        private string? _DataCompareFormat;
        public string? DataCompareFormat
        {
            get { return _DataCompareFormat; }
            set { _DataCompareFormat = value; OnPropertyChanged(); }
        }


        private int _TotalRecDb;

        public int TotalRecDb
        {
            get { return _TotalRecDb; }
            set
            {
                if (_TotalRecDb != value)
                {
                    _TotalRecDb = value;
                    OnPropertyChanged();
                }
            }
        }

        private PrinterSeries _PrinterSeries = PrinterSeries.None;
        public PrinterSeries PrinterSeries
        {
            get { return _PrinterSeries; }
            set { _PrinterSeries = value; OnPropertyChanged(); }
        }


        private string _PrinterTemplate ="";
        public string PrinterTemplate
        {
            get { return _PrinterTemplate; }
            set { _PrinterTemplate = value; OnPropertyChanged(); }
        }

        private CameraSeries _CameraSeries =  CameraSeries.Dataman;
        public CameraSeries CameraSeries
        {
            get { return _CameraSeries; }
            set { _CameraSeries = value; OnPropertyChanged(); }
        }

        private List<string>? _TemplateList;
        public List<string>? TemplateList
        {
            get { return _TemplateList; }
            set { _TemplateList = value; OnPropertyChanged(); }
        }
        private List<string>? _TemplateListFirstFound;
        public List<string>? TemplateListFirstFound
        {
            get { return _TemplateListFirstFound; }
            set { _TemplateListFirstFound = value; OnPropertyChanged(); }
        }

        private JobStatus _JobStatus;
        public JobStatus JobStatus
        {
            get { return _JobStatus; }
            set { _JobStatus = value; OnPropertyChanged(); }
        }

        private PrinterStatus _PrinterStatus;
        public PrinterStatus PrinterStatus
        {
            get { return _PrinterStatus; }
            set { _PrinterStatus = value; OnPropertyChanged(); }
        }

        private SelectJobModel? _SelectJob;
        public SelectJobModel? SelectJob
        {
            get { return _SelectJob; }
            set { _SelectJob = value; OnPropertyChanged(); }
        }

        private CompleteCondition _CompleteCondition = CompleteCondition.None;
        public CompleteCondition CompleteCondition
        {
            get { return _CompleteCondition; }
            set { _CompleteCondition = value; OnPropertyChanged(); }
        }

        private bool _OutputCamera = true;
        public bool OutputCamera
        {
            get { return _OutputCamera; }
            set { _OutputCamera = value; OnPropertyChanged(); }
        }

        private bool _IsImageExport;
        public bool IsImageExport
        {
            get { return _IsImageExport; }
            set 
            { 
                _IsImageExport = value; 
                OnPropertyChanged();
               
            }
        }

        private string? _ImageExportPath; 
        public string? ImageExportPath 
        {
            get { return _ImageExportPath; }
            set { _ImageExportPath = value; OnPropertyChanged(); }
        }

        public string? ExportNamePrefix { get; set; }
        public string ExportNamePrefixFormat { set; get; } = "yyyyMMdd_HHmmss";

        private string? _PrinterIP = "127.0.0.1";
        public string? PrinterIP
        {
            get { return _PrinterIP; }
            set { _PrinterIP = value; OnPropertyChanged(); }
        }

        private string? _PrinterPort = "2030";
        public string? PrinterPort
        {
            get { return _PrinterPort; }
            set { _PrinterPort = value; OnPropertyChanged(); }
        }

        private string? _PrinterWebPort = "80";
        public string? PrinterWebPort
        {
            get { return _PrinterWebPort; }
            set { _PrinterWebPort = value; OnPropertyChanged(); }
        }

        //[ObservableProperty]
        //public bool _PrinterCheckParams;

        private string? _CameraIP = "127.0.0.1";
        public string? CameraIP
        {
            get { return _CameraIP; }
            set { _CameraIP = value; OnPropertyChanged(); }
        }


        private CameraInfos? _cameraInfo;

        public CameraInfos? CameraInfo
        {
            get { return _cameraInfo; }
            set
            {
                if (_cameraInfo != value)
                {
                    _cameraInfo = value;
                    OnPropertyChanged();
                }
            }
        }


        private string? _ControllerIP = "127.0.0.1";
        public string? ControllerIP
        {
            get { return _ControllerIP; }
            set { _ControllerIP = value; OnPropertyChanged(); }
        }

        private string? _ControllerPort = "127.0.0.1";
        public string? ControllerPort
        {
            get { return _ControllerPort; }
            set { _ControllerPort = value; OnPropertyChanged(); }
        }

       
        private string _PrintedResponePath ="";
        public string PrintedResponePath
        {
            get { return _PrintedResponePath; }
            set { _PrintedResponePath = value; OnPropertyChanged(); }
        }


        private string _CheckedResultPath ="";
        public string CheckedResultPath
        {
            get { return _CheckedResultPath; }
            set
            {
                if (_CheckedResultPath != value)
                {
                    _CheckedResultPath = value;
                    OnPropertyChanged();
                }
            }
        }


        private List<PODModel> _PODFormat = new List<PODModel>();

        public List<PODModel> PODFormat
        {
            get { return _PODFormat; }
            set
            {
                if (_PODFormat != value)
                {
                    _PODFormat = value;
                    OnPropertyChanged();
                }
            }
        }


        private string? _SearchTemplateText;

        public string? SearchTemplateText
        {
            get { return _SearchTemplateText; }
            set
            {
                if (_SearchTemplateText != value)
                {
                    _SearchTemplateText = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _totalChecked = "0";
        public string TotalChecked
        {
            get { return _totalChecked; }
            set
            {
                if (_totalChecked != value)
                {
                    _totalChecked = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _totalPassed = "0";
        public string TotalPassed
        {
            get { if (!int.TryParse(_totalPassed, out _)) return "0"; return _totalPassed; }
            set
            {
                if (_totalPassed != value)
                {
                    _totalPassed = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _totalFailed = "0";
        public string TotalFailed
        {
            get { if (!int.TryParse(_totalFailed, out _)) return "0"; return _totalFailed; }
            set
            {
                if (_totalFailed != value)
                {
                    _totalFailed = value;
                    OnPropertyChanged();
                }
            }
        }

        [ObservableProperty]
        public string? _DisableSensor;

       

        

        public static JobModel? LoadFile(string fileName)
        {
            try
            {
                JobModel? info = null;
                var xs = new XmlSerializer(typeof(JobModel));
                using (var sr = new StreamReader(fileName))
                {
                    XmlReader xr = XmlReader.Create(sr);
                    if (xs != null && xr != null)
                        info = (JobModel?)xs.Deserialize(xr);
                }
                return info;
            }
            catch { return null; }

        }
    }
}
