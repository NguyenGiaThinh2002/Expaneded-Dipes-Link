using DipesLink.Languages;
using DipesLink.Models;
using DipesLink.Views.Converter;
using DipesLink.Views.Extension;
using SharedProgram.Models;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static DipesLink.Views.Enums.ViewEnums;
namespace DipesLink.ViewModels
{
    public partial class MainViewModel
    {

        internal void CheckStationChange()
        {
            if (StationSelectedIndex + 1 != ViewModelSharedValues.Settings.NumberOfStation)
            {

                var res = CusMsgBox.Show("Change number of station !", "Change Station", ButtonStyleMessageBox.OKCancel, ImageStyleMessageBox.Warning);
                if (res)
                {

                    ViewModelSharedValues.Settings.NumberOfStation = StationSelectedIndex + 1;
                    ViewModelSharedFunctions.SaveSetting();

                    string exePath = Process.GetCurrentProcess().MainModule.FileName;
                    Process.Start(exePath);
                    Application.Current.Shutdown();

                }
                else
                {
                    StationSelectedIndex = ViewModelSharedValues.Settings.NumberOfStation - 1;
                }
            }
        }

       internal void UIEnableControlByLoadingDb(int index , bool visible)
        {
            TabControlEnable = visible;
            JobList[index].IsStartButtonEnable = visible;
            
            
        }


        #region Button Control

        internal void UpdateJobInfo(int index)
        {
            GetCurrentJobDetail(index);
        }

        internal void ChangeJobByDeviceStatSymbol(int index)
        {
            JobIndex = index;
            GetCurrentJobDetail(index);
        }

        internal void MinusWidth()
        {
            if (ActualWidthMainWindow >= 1800)
            {
                ActualWidthGrid = 1800;
            }
        }

        private string _TitleApp = "Stations_Details";
        
        public string TitleApp
        {
            get {
                return LanguageModel.Language?[_TitleApp].ToString(); 
            }
            set
            {
                if (_TitleApp != value)
                {
                    
                    _TitleApp = value;
                    OnPropertyChanged();
                }
            }
        }
       


        internal void ChangeTitleMainWindow(TitleAppContext titleType)
        {
            switch (titleType)
            {
                case TitleAppContext.Overview:
                    TitleApp = "Stations_Overview"; 
                    break;
                case TitleAppContext.Home:
                    TitleApp = "Stations_Details";
                    break;
                case TitleAppContext.Jobs:
                    TitleApp = "Station_Jobs_Operations";
                    break;
                case TitleAppContext.Setting:
                    TitleApp = "Stations_Settings";
                    break;
                case TitleAppContext.Logs:
                    TitleApp = "Stations_Jobs_Logs";
                    break;
                default:
                    break;
            }
        }
        #endregion Button Control
    }
}
