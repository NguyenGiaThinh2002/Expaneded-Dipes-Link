using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace SharedProgram.Models
{
    public class ConnectParamsModel : SettingsModel
    {
        public int Index { get; set; }
       

        public string CameraIP { get; set; } = "127.0.0.1";

        private string cameraModel;
        public string CameraModel { get => cameraModel; set { cameraModel = value; OnPropertyChanged(); }  }
        public string PrinterIP { get; set; } = "127.0.0.1";
        public double PrinterPort { get; set; } = 0;
        public string ControllerIP { get; set; } = "127.0.0.1";
        public double ControllerPort { get; set; } = 0;

        public double DelaySensor { get; set; } = 0;
        public double DisableSensor { get; set; } = 0;
        public double PulseEncoder { get; set; } = 0;
        public double EncoderDiameter { get; set; } = 0.00;

        public bool EnController { get; set; } = false;

        public CameraInfos? CameraInfors { get; set; }
        //private int _delaySensor;

        //public int DelaySensor
        //{
        //    get { return _delaySensor; }
        //    set { _delaySensor = value; OnPropertyChanged(); }
        //}



        private bool _IsLockUISetting;
        public bool IsLockUISetting
        {
            get { return _IsLockUISetting; }
            set { _IsLockUISetting = value; OnPropertyChanged(); }
        }

      

        private ObservableCollection<string> _responseMessList = new();

        [XmlIgnore]
        public ObservableCollection<string> ResponseMessList
        {
            get { return _responseMessList; }
            set
            {
                if (_responseMessList != value)
                {
                    _responseMessList = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _VerifyAndPrintBasicSentMethod_UI;
        public bool VerifyAndPrintBasicSentMethod_UI
        {
            get { return _VerifyAndPrintBasicSentMethod_UI; }
            set
            {
                if (_VerifyAndPrintBasicSentMethod_UI != value)
                {
                    _VerifyAndPrintBasicSentMethod_UI = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _VerifyAndPrintBasicSentMethod;
        public bool VerifyAndPrintBasicSentMethod
        {
            get { return _VerifyAndPrintBasicSentMethod; }
            set
            {
                if (_VerifyAndPrintBasicSentMethod != value)
                {
                    _VerifyAndPrintBasicSentMethod = value;
                    VerifyAndPrintBasicSentMethod_UI = value;
                }
            }
        }


        private string? _FormatedPOD;

        public string? FormatedPOD
        {
            get { return _FormatedPOD; }
            set
            {
                if (_FormatedPOD != value)
                {
                    _FormatedPOD = value;
                    OnPropertyChanged();
                }
            }
        }
        //public List<PODModel> PrintFieldForVerifyAndPrint { get; set; } = new();

        private List<PODModel> _PrintFieldForVerifyAndPrint;

        public List<PODModel> PrintFieldForVerifyAndPrint
        {
            get { return _PrintFieldForVerifyAndPrint; }
            set
            {
                if (_PrintFieldForVerifyAndPrint != value)
                {
                    _PrintFieldForVerifyAndPrint = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _FailedDataSentToPrinter = "Failure";
        

        public string FailedDataSentToPrinter
        {
            get { return _FailedDataSentToPrinter; }
            set
            {
                if (_FailedDataSentToPrinter != value)
                {
                    _FailedDataSentToPrinter = value;
                    OnPropertyChanged();
                }
            }
        }

        private string? serialNumber;
        public string? SerialNumber { get => serialNumber; set { serialNumber = value; OnPropertyChanged(); } }
    }
}
