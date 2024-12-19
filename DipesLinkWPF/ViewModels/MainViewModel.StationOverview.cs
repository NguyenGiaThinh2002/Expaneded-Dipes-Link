using DipesLink.Languages;
using DipesLink.Models;
using DipesLink.Views.Converter;
using DipesLink.Views.Extension;
using SharedProgram.Models;
using SharedProgram.Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
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
                //ChangeNumberOfStation
                var res = CusMsgBox.Show(
                    LanguageModel.GetLanguage("ChangeNumberOfStation"), 
                    LanguageModel.GetLanguage("WarningDialogCaption"), 
                    ButtonStyleMessageBox.OKCancel,
                    ImageStyleMessageBox.Warning);
                if (res.Result)
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

        internal void CheckPrinterChange(int newNumber)
        {
            if (newNumber+1 != ViewModelSharedValues.Settings.NumberOfPrinter)
            {
                var res = CusMsgBox.Show(
                   LanguageModel.GetLanguage("ChangeNumberOfStation"),
                   LanguageModel.GetLanguage("WarningDialogCaption"),
                   ButtonStyleMessageBox.OKCancel,
                   ImageStyleMessageBox.Warning);
                if (res.Result)
                {

                    ViewModelSharedValues.Settings.NumberOfPrinter = newNumber + 1;
                    ViewModelSharedFunctions.SaveSetting();

                    string exePath = Process.GetCurrentProcess().MainModule.FileName;
                    Process.Start(exePath);
                    Application.Current.Shutdown();

                }
                else
                {

                    //PrinterSelectedIndex = ViewModelSharedValues.Settings.NumberOfPrinter - 1;
                }
            }

        }

        internal void CheckDateTimeFormat()
        {
            switch (DateTimeFormatSelectedIndex)
            {
                case 0:
                    DateTimeFormat = "yyyyMMdd_HHmmss";
                    break;
                case 1:
                    DateTimeFormat = "ddMMyyyy_HHmmss";
                    break;
                case 2:
                    DateTimeFormat = "MMddyyyy_HHmmss";
                    break;
                case 3:
                    DateTimeFormat = "MMyyyydd_HHmmss";
                    break;
                case 4:
                    DateTimeFormat = "ddyyyyMM_HHmmss";
                    break;
                default:
                    break;
            }
            ViewModelSharedValues.Settings.DateTimeFormat = DateTimeFormat;
            ViewModelSharedValues.Settings.DateTimeFormatSelectedIndex = DateTimeFormatSelectedIndex;
            ViewModelSharedFunctions.SaveSetting();
        }
        internal void CheckTemplateName()
        {
            ViewModelSharedValues.Settings.TemplateName = TemplateName;
            ViewModelSharedFunctions.SaveSetting();
        }

        internal void UIEnableControlByLoadingDb(int index, bool visible)
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
            get
            {
                return LanguageModel.LangResource[_TitleApp].ToString();
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
