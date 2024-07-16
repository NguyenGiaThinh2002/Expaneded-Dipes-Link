using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedProgram.Models
{
    public class SelectJobModel : JobModel
    {
        //[ObservableProperty]
        //private ObservableCollection<string>? _JobFileList;

        private ObservableCollection<string>? _JobFileList = new();

        public ObservableCollection<string>? JobFileList
        {
            get { return _JobFileList; }
            set { _JobFileList = value; OnPropertyChanged(); }
        }

        private bool _isButtonOperationJobEnable = true;
        public bool IsButtonOperationJobEnable
        {
            get { return _isButtonOperationJobEnable; }
            set { _isButtonOperationJobEnable = value; OnPropertyChanged(); }
        }


        private string? _SelectedJob;

        public string? SelectedJob
        {
            get { return _SelectedJob; }
            set { _SelectedJob = value; OnPropertyChanged(); }
        }


      
        private string? _SelectedWorkJob;

        public string? SelectedWorkJob
        {
            get { return _SelectedWorkJob; }
            set { _SelectedWorkJob = value; OnPropertyChanged(); }
        }


      

        private ObservableCollection<string>? _SelectedJobFileList;

        public ObservableCollection<string>? SelectedJobFileList
        {
            get { return _SelectedJobFileList; }
            set { _SelectedJobFileList = value; OnPropertyChanged(); }
        }

    }
}
