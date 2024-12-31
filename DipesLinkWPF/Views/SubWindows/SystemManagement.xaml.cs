using DipesLink.Extensions;
using DipesLink.Languages;
using DipesLink.ViewModels;
using DipesLink.Views.Extension;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static DipesLink.Views.Enums.ViewEnums;
using static SharedProgram.DataTypes.CommonDataType;


namespace DipesLink.Views.SubWindows
{

    public partial class SystemManagement : Window
    {
        public static bool IsInitializing = true;
        private MainViewModel _viewModel;
        public SystemManagement(MainViewModel viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
            InitControls();
            SetCurrentLanguage();
            IsInitializing = false;
            // thinh now
            InitializeComboBox(); 
        }

        private void InitializeComboBox()
        {
            ComboBoxStationNumber.Items.Clear();

            for (int i = 1; i <= NamedPipeServerStreamHelper._numberLicense; i++)
            {
                ComboBoxStationNumber.Items.Add(i);
            }

            //ComboBoxStationNumber.SelectedIndex = 0;
        }
        private void InitControls()
        {
            TextBoxTemplateName.Text = _viewModel.TemplateName;
            TextBoxErrImagePath.Text = _viewModel.ImageExpPath;
            ComboBoxStationNumber.IsEnabled = !_viewModel.JobList.Any(job => job.OperationStatus != OperationStatus.Stopped);
            ComboBoxPrinterNumber.IsEnabled = !_viewModel.JobList.Any(job => job.OperationStatus != OperationStatus.Stopped);
            ComboBoxStationNumber.SelectedIndex = _viewModel.StationSelectedIndex;
            ComboBoxDateTimeFormat.SelectedIndex = _viewModel.DateTimeFormatSelectedIndex;
            ComboBoxPrinterNumber.SelectedIndex = ViewModelSharedValues.Settings.NumberOfPrinter -1;
        }

        private void SetCurrentLanguage()
        {
            IsInitializing = true;
            try
            {
                string languageCode = ViewModelSharedValues.Settings.Language;

                if (languageCode == "en-US")
                {
                    ComboBoxLanguages.SelectedIndex = 0;
                }
                else
                { 
                    ComboBoxLanguages.SelectedIndex = 1;
                }
            }
            catch (Exception)
            {
            }
            IsInitializing = false;
        }

        private void ComboBoxLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsInitializing) return;
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;
            var selectedIndex = comboBox.SelectedIndex;

            // Count the number of running stations
            int numStationRun = _viewModel.JobList.Count(job => job.OperationStatus == SharedProgram.DataTypes.CommonDataType.OperationStatus.Running);
            if (numStationRun > 0)
            {
                CusAlert.Show(LanguageModel.GetLanguage("StopAllStations"), ImageStyleMessageBox.Warning);
                SetCurrentLanguage();
                return;
            }

            var languageModel = new LanguageModel();
            string selectedLanguage = selectedIndex == 0 ? "en-US" : "vi-VN";

            if (IsChangeLanguageAccepted())
            {
                languageModel.UpdateApplicationLanguage(selectedLanguage);
                ApplicationHelper.RestartApplication();
            }
            else
            {
                SetCurrentLanguage();
            }
        }    

        private bool IsChangeLanguageAccepted()
        {
            var res = CusMsgBox.Show(LanguageModel.GetLanguage("ChangeLanguageAndLogoutConfirmation"),
                LanguageModel.GetLanguage("WarningDialogCaption"),
                ButtonStyleMessageBox.OKCancel, 
                ImageStyleMessageBox.Warning);
            return res.Result;
        }

        private void ComboBoxLanguage_StationNumberChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbb = sender as ComboBox;
            if(_viewModel is null || cbb ==null) return;
            _viewModel.StationSelectedIndex = cbb.SelectedIndex;
            _viewModel.CheckStationChange();
            cbb.SelectedIndex = _viewModel.StationSelectedIndex;
        }

        private void ComboBoxDateTimeFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbb = sender as ComboBox;
            if (_viewModel is null || cbb == null) return;
            _viewModel.DateTimeFormatSelectedIndex = cbb.SelectedIndex;
            _viewModel.CheckDateTimeFormat();
        }

        private void TextBoxTemplateName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewModel is null || TextBoxTemplateName == null) return;
            _viewModel.TemplateName = TextBoxTemplateName.Text;
            _viewModel.CheckTemplateName();
        }

        private void ButtonChooseImagePath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.SelectImageExportPath(TextBoxErrImagePath.Text);
                if (Directory.Exists(_viewModel.ImageExpPath))
                {
                    TextBoxErrImagePath.Text = _viewModel.ImageExpPath;
                }
            }
            catch (Exception)
            {
            }
        }

        private void ComboBoxLanguage_PrinterNumberChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbb = sender as ComboBox;
            if (_viewModel is null || cbb == null || IsInitializing) return;
            _viewModel.CheckPrinterChange(ComboBoxPrinterNumber.SelectedIndex);
            ComboBoxPrinterNumber.SelectedIndex = ViewModelSharedValues.Settings.NumberOfPrinter -1 ;

        }
    }
}
