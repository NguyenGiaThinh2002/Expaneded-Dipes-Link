using CommunityToolkit.Mvvm.ComponentModel;
using DipesLink.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DipesLink.Models
{
    public class JobDeviceStatus : ObservableObject
    {

        private int _index;

        public int Index
        {
            get { return _index; }
            set
            {
                if (_index != value)
                {
                    _index = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }


        private bool _IsCamConnected;

        public bool IsCamConnected
        {
            get { return _IsCamConnected; }
            set
            {
                if (_IsCamConnected != value)
                {
                    _IsCamConnected = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool _IsPrinterConnected;

        public bool IsPrinterConnected
        {
            get { return _IsPrinterConnected; }
            set
            {
                if (_IsPrinterConnected != value)
                {
                    _IsPrinterConnected = value;
                    OnPropertyChanged();
                }
            }
        }

        private SolidColorBrush _CameraStatusColor = new(Colors.Red);
        public SolidColorBrush CameraStatusColor
        {
            get { return _CameraStatusColor; }
            set { _CameraStatusColor = value; OnPropertyChanged(); }
        }

        private SolidColorBrush _PrinterStatusColor = new(Colors.Red);
        public SolidColorBrush PrinterStatusColor
        {
            get { return _PrinterStatusColor; }
            set { _PrinterStatusColor = value; OnPropertyChanged(); }
        }


        private SolidColorBrush _ControllerStatusColor = new(Colors.Red);
        public SolidColorBrush ControllerStatusColor
        {
            get { return _ControllerStatusColor; }
            set
            {
                if (_ControllerStatusColor != value)
                {
                    _ControllerStatusColor = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
