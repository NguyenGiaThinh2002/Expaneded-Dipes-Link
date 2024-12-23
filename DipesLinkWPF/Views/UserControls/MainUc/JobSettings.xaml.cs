using DipesLink.Languages;
using DipesLink.ViewModels;
using DipesLink.Views.Extension;
using DipesLink.Views.SubWindows;
using DipesLink.Views.UserControls.CustomControl;
using MahApps.Metro.Controls;
using SharedProgram.Models;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using static DipesLink.Views.Enums.ViewEnums;
using TextBox = System.Windows.Controls.TextBox;
using SharedProgram.DataTypes;
using System.Windows.Media;
using DipesLink_SDK_Cameras;
using System.Windows.Data;
using System.Collections.ObjectModel;
using SharedProgram.Shared;
namespace DipesLink.Views.UserControls.MainUc
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class JobSettings : UserControl
    {
        #region Variables
        public static bool IsInitializing = true;
        private bool _firstLoad = false;
        static string[] comNames = new[] { "COM3", "COM4", "COM5", "COM6", "COM7" };
        static int[] bitRates = new[] { 9600, 19200, 38400, 57600, 115200 };
        static int[] dataBits = new[] { 7, 8 };
        static Parity[] parities = new[] { Parity.None, Parity.Odd, Parity.Even };
        static StopBits[] stopBits = new[] { StopBits.One, StopBits.Two };
        static CommonDataType.CameraSeries[] cameraSeries = new[] { CommonDataType.CameraSeries.Dataman, CommonDataType.CameraSeries.InsightVision, CommonDataType.CameraSeries.InsightVisionDual };
        static CommonDataType.DatamanReadMode[] datamanReadModes = new[] { CommonDataType.DatamanReadMode.Basic, CommonDataType.DatamanReadMode.MultiRead };
        static CommonDataType.AutoSaveSettingsType settingType;
        static int currentSelectedPrinter = 0;

        #endregion

        public JobSettings()
        {
            InitializeComponent();
            EventRegister();
        }

        private void EventRegister()
        {
            Loaded += SettingsUc_Loaded;

            ViewModelSharedEvents.OnMainListBoxMenu += MainListBoxMenuChange;
            ViewModelSharedEvents.OnChangeJobStatus += JobStatusChanged;

            #region Printer
            TextBoxPrinterIP.TextChanged += TextBox_ParamsChanged;
            #endregion

            #region Camera
            ComboBoxCameraType.SelectionChanged += AdjustData;
            TextBoxCamIP.TextChanged += TextBox_ParamsChanged;
            ComboBoxDatamanReadMode.SelectionChanged += AdjustData;
            #endregion

            #region Controller
            TextBoxControllerIP.TextChanged += TextBox_ParamsChanged;
            #endregion

            #region Barcode Scanner
            comboBoxComName.SelectionChanged += AdjustData;
            comboBoxBitPerSeconds.SelectionChanged += AdjustData;
            comboBoxDataBits.SelectionChanged += AdjustData;
            comboBoxParity.SelectionChanged += AdjustData;
            comboBoxStopBits.SelectionChanged += AdjustData;
            #endregion
        }

        private void LoadUIPrinter()
        {
            if (ViewModelSharedValues.Settings.NumberOfPrinter > 1)
            {
                ButtonItemsControl.ItemsSource = Enumerable.Range(1, ViewModelSharedValues.Settings.NumberOfPrinter).Select(n => n.ToString()).ToList();
                PrinterSelection.Visibility = Visibility.Visible;

                //ButtonItemsControl.LayoutUpdated += (sender, e) =>
                //{
                //    bool isFirst = true;
                //    foreach (var item in ButtonItemsControl.Items)
                //    {
                //        var container = ButtonItemsControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                //        if (container != null)
                //        {
                //            var button = FindVisualChild<Button>(container);
                //            if (isFirst)
                //            {
                //                //button.Tag = "Selected";  // Mark the first button as selected
                //                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#003C5D"));
                //                isFirst = false;  // Set flag to false after first item
                //            }
                //            else
                //            {
                //                button.Tag = null;  // Reset the tag for other buttons
                //            }
                //        }
                //    }
                //};



            }
            else
            {
                PrinterSelection.Visibility = Visibility.Collapsed;
            }
        }

        // UI NEW CODE 
        private void AdjustData(object sender, EventArgs e)
        {
            if (IsInitializing) return;
            if (sender is not ComboBox comboBox) return;

            var vm = CurrentViewModel<MainViewModel>();
            if (vm == null) return;
            if (vm.ConnectParamsList == null) return;
            int currentIndex = CurrentIndex();
            switch (((ComboBox)sender).Name)
            {
                case "comboBoxComName":
                    var newComName = comNames[comboBox.SelectedIndex];
                    vm.ConnectParamsList[currentIndex].ComName = newComName;
                    settingType = CommonDataType.AutoSaveSettingsType.BarcodeScanner;
                    break;
                case "comboBoxBitPerSeconds":
                    var newBitRate = bitRates[comboBox.SelectedIndex];
                    vm.ConnectParamsList[currentIndex].BitPerSeconds = newBitRate;
                    settingType = CommonDataType.AutoSaveSettingsType.BarcodeScanner;
                    break;
                case "comboBoxDataBits":
                    var newDataBit = dataBits[comboBox.SelectedIndex];
                    vm.ConnectParamsList[currentIndex].DataBits = newDataBit;
                    settingType = CommonDataType.AutoSaveSettingsType.BarcodeScanner;
                    break;
                case "comboBoxParity":
                    var newParity = parities[comboBox.SelectedIndex];
                    vm.ConnectParamsList[currentIndex].Parity = newParity;
                    settingType = CommonDataType.AutoSaveSettingsType.BarcodeScanner;
                    break;
                case "comboBoxStopBits":
                    var newStopBit = stopBits[comboBox.SelectedIndex];
                    vm.ConnectParamsList[currentIndex].StopBits = newStopBit;
                    settingType = CommonDataType.AutoSaveSettingsType.BarcodeScanner;
                    break;
                case "ComboBoxCameraType":
                    var newCameraType = cameraSeries[comboBox.SelectedIndex];
                    vm.ConnectParamsList[currentIndex].CameraSeries = newCameraType;
                    settingType = CommonDataType.AutoSaveSettingsType.Camera;
                    break;
                case "ComboBoxDatamanReadMode":
                    var newDatamanReadMode = datamanReadModes[comboBox.SelectedIndex];
                    vm.ConnectParamsList[currentIndex].DatamanReadMode = newDatamanReadMode;
                    settingType = CommonDataType.AutoSaveSettingsType.Camera;
                    break;
            }
            StackPanelDatamanReadMode.Visibility = vm.ConnectParamsList[CurrentIndex()].CameraSeries == CommonDataType.CameraSeries.Dataman ? Visibility.Visible : Visibility.Collapsed;

            vm.AutoSaveConnectionSetting(CurrentIndex(), settingType);
        }

        private void JobStatusChanged(object? sender, EventArgs e)
        {
            LockUIPreventChangeJobWhenRun();
        }

        private void LockUIPreventChangeJobWhenRun()
        {
            CurrentViewModel<MainViewModel>()?.LockUI(ListBoxMenuStationSetting.SelectedIndex);
        }

        private void MainListBoxMenuChange(object? sender, EventArgs e)
        {
            ListBoxMenu_SelectionChanged(sender, null);
        }

        private void SettingsUc_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                IsInitializing = false;
                var vm = CurrentViewModel<MainViewModel>();
                if (vm is null) return;
                vm.SaveConnectionSetting();
                InputArea.IsEnabled = vm.ConnectParamsList[CurrentIndex()].EnController;
                UpdateUIValues(vm);
                LoadUIPrinter();
            }
            catch (Exception)
            {
            }

        }

        private void UpdateUIValues(MainViewModel vm)
        {
            #region Update UI values
            UpdateSendModeVerifyAndPrintState(vm);
            UpdateScannerComboBoxValues(vm);
            UpdateCameraTypeValues(vm);
            UpdateCheckAllPrinterSettingsState(vm);
            UpdateDatamanReadModeValues(vm);
            #endregion
        }

        private void UpdateDatamanReadModeValues(MainViewModel vm)
        {
            if (vm?.ConnectParamsList == null) return;
            var currentDatamanReadMode = vm.ConnectParamsList[CurrentIndex()].DatamanReadMode;

            int currentIndex = Array.IndexOf(datamanReadModes, currentDatamanReadMode);
            if (currentIndex >= 0)
            {
                ComboBoxDatamanReadMode.SelectedIndex = currentIndex;
            }
            else
            {
                ComboBoxDatamanReadMode.SelectedIndex = -1;
            }
        }

        private T? CurrentViewModel<T>() where T : class
        {
            if (DataContext is T viewModel)
            {
                return viewModel;
            }
            else
            {
                return null;
            }
        }

        private void ListBoxMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var vm = CurrentViewModel<MainViewModel>();
                vm?.SelectionChangeSystemSettings(CurrentIndex());
                vm?.LockUI(CurrentIndex());// Lock UI when running
                UpdateUIValues(vm);
            }
            catch (Exception)
            {
            }
        }

        #region Update UI Values
        void UpdateSendModeVerifyAndPrintState(MainViewModel vm)
        {
            try
            {
                var isBasicSendMode = vm.ConnectParamsList[CurrentIndex()].VerifyAndPrintBasicSentMethod;
                RadBasic.IsChecked = isBasicSendMode == true;
                RadCompare.IsChecked = isBasicSendMode == false;
            }
            catch (Exception)
            {
            }
        }

        private void UpdateScannerComboBoxValues(MainViewModel vm)
        {
            UpdateScannerComName(vm);
            UpdateScannerBitPerSecond(vm);
            UpdateScannerDataBits(vm);
            UpdateScannerParity(vm);
            UpdateScannerStopBits(vm);
        }

        private void UpdateScannerComName(MainViewModel vm)
        {
            if (vm?.ConnectParamsList == null) return;
            var currentComName = vm.ConnectParamsList[CurrentIndex()].ComName;
            // Find the index of the current COM name in the array
            int selectedIndex = Array.IndexOf(comNames, currentComName);
            if (selectedIndex >= 0)
            {
                comboBoxComName.SelectedIndex = selectedIndex;
            }
        }

        private void UpdateScannerBitPerSecond(MainViewModel vm)
        {
            if (vm?.ConnectParamsList == null) return;
            var currentBitRate = vm.ConnectParamsList[CurrentIndex()].BitPerSeconds;

            int selectedIndex = Array.IndexOf(bitRates, currentBitRate);
            if (selectedIndex >= 0)
            {
                comboBoxBitPerSeconds.SelectedIndex = selectedIndex;
            }
        }

        private void UpdateScannerDataBits(MainViewModel vm)
        {
            if (vm?.ConnectParamsList == null) return;
            var currentDataBits = vm.ConnectParamsList[CurrentIndex()].DataBits;

            int selectedIndex = Array.IndexOf(dataBits, currentDataBits);
            if (selectedIndex >= 0)
            {
                comboBoxDataBits.SelectedIndex = selectedIndex;
            }
        }

        private void UpdateScannerParity(MainViewModel vm)
        {
            if (vm?.ConnectParamsList == null) return;
            var currentParity = vm.ConnectParamsList[CurrentIndex()].Parity;

            int selectedIndex = Array.IndexOf(parities, currentParity);
            if (selectedIndex >= 0)
            {
                comboBoxParity.SelectedIndex = selectedIndex;
            }
        }

        private void UpdateScannerStopBits(MainViewModel vm)
        {
            if (vm?.ConnectParamsList == null) return;
            var currentStopBits = vm.ConnectParamsList[CurrentIndex()].StopBits;

            int selectedIndex = Array.IndexOf(stopBits, currentStopBits);
            if (selectedIndex >= 0)
            {
                comboBoxStopBits.SelectedIndex = selectedIndex;
            }
        }

        private void UpdateCameraTypeValues(MainViewModel vm)
        {
            if (vm?.ConnectParamsList == null) return;
            var currentCameraSeries = vm.ConnectParamsList[CurrentIndex()].CameraSeries;

            int selectedIndex = Array.IndexOf(cameraSeries, currentCameraSeries);
            if (selectedIndex >= 0)
            {
                ComboBoxCameraType.SelectedIndex = selectedIndex;
            }
            else
            {
                ComboBoxCameraType.SelectedIndex = -1;
            }
        }

        private void UpdateCheckAllPrinterSettingsState(MainViewModel vm)
        {
            CheckAllPrinterSettings.IsOn = vm.ConnectParamsList[CurrentIndex()].IsCheckPrinterSettingsEnabled;
        }
        #endregion

        private int CurrentIndex() => ListBoxMenuStationSetting.SelectedIndex;

        private void TextBox_ParamsChanged(object sender, TextChangedEventArgs e)
        {
            if (IsInitializing) return;
            if (sender is IpAddressControl textBoxIp)
            {
                TextBoxIpParamsHandler(textBoxIp);
            }
            if (sender is TextBox textBox)
            {
                TextBoxNormalParamsHandler(textBox);
            }
        }

        private void TextBoxIpParamsHandler(IpAddressControl textBox)
        {

            try
            {
                textBox?.Dispatcher.BeginInvoke(new Action(() => // Use BeginInvoke to Update Input Last Value 
                { 
                    var vm = CurrentViewModel<MainViewModel>();
                    if (vm == null) return;
                    if (vm.ConnectParamsList == null) return;
                    textBox.Text ??= string.Empty;

                    switch (textBox.Name)
                    {
                        case "TextBoxPrinterIP":
                            vm.ConnectParamsList[CurrentIndex()].PrinterIP = textBox.Text;
                            vm.ConnectParamsList[CurrentIndex()].PrinterIPs[currentSelectedPrinter] = textBox.Text;                        
                            vm.AutoSaveConnectionSetting(CurrentIndex(), CommonDataType.AutoSaveSettingsType.Printer);
                            break;

                        case "TextBoxCamIP":
                            vm.ConnectParamsList[CurrentIndex()].CameraIP = textBox.Text;
                            vm.AutoSaveConnectionSetting(CurrentIndex(), CommonDataType.AutoSaveSettingsType.Camera);
                            break;
                        case "TextBoxControllerIP":
                            vm.ConnectParamsList[CurrentIndex()].ControllerIP = textBox.Text;
                            vm.AutoSaveConnectionSetting(CurrentIndex(), CommonDataType.AutoSaveSettingsType.Controller);
                            break;

                        case "TextBoxErrorField":
                            vm.ConnectParamsList[CurrentIndex()].FailedDataSentToPrinter = textBox.Text;
                            vm.AutoSaveConnectionSetting(CurrentIndex(), CommonDataType.AutoSaveSettingsType.VerifyAndPrint);
                            break;

                        default:
                            break;
                    }


                }), DispatcherPriority.Background);
            }
            catch (Exception)
            {
            }
        }

        private void TextBoxNormalParamsHandler(TextBox textBox)
        {
            try
            {
                textBox?.Dispatcher.BeginInvoke(new Action(() => // Use BeginInvoke to Update Input Last Value 
                {
                    var vm = CurrentViewModel<MainViewModel>();
                    if (vm == null) return;
                    if (vm.ConnectParamsList == null) return;
                    textBox.Text ??= string.Empty;
                    var index = CurrentIndex();
                    switch (textBox.Name)
                    {
                        case "TextBoxPrinterIP":
                            vm.ConnectParamsList[index].PrinterIP = textBox.Text;
                            vm.AutoSaveConnectionSetting(index, CommonDataType.AutoSaveSettingsType.Printer);
                            break;
                        case "TextBoxCamIP":
                            vm.ConnectParamsList[index].CameraIP = textBox.Text;
                            vm.AutoSaveConnectionSetting(index, CommonDataType.AutoSaveSettingsType.Camera);
                            break;
                        case "TextBoxControllerIP":
                            vm.ConnectParamsList[index].ControllerIP = textBox.Text;
                            vm.AutoSaveConnectionSetting(index, CommonDataType.AutoSaveSettingsType.Controller);
                            break;
                        case "TextBoxErrorField":
                            vm.ConnectParamsList[index].FailedDataSentToPrinter = textBox.Text;
                            vm.AutoSaveConnectionSetting(index, CommonDataType.AutoSaveSettingsType.VerifyAndPrint);
                            break;
                        default:
                            break;
                    }


                }), DispatcherPriority.Background);
            }
            catch (Exception)
            {
            }
        }

        private void ButtonWebView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = ListBoxMenuStationSetting.SelectedIndex;
                var domain = CurrentViewModel<MainViewModel>()?.ConnectParamsList[index].PrinterIP;
                PrinterWebView pww = new()
                {
                    TitleContext = $"Printer {index + 1} Web Remote - Address: {domain}",
                    Address = domain
                };
                pww.Show();
            }
            catch (Exception) { }
        }

        private void Integer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            if (IsInitializing) return;
            try
            {
                var integerUpDown = sender as TAlex.WPF.Controls.NumericUpDown;
                if (integerUpDown?.ContentText == null) return;
                integerUpDown?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var vm = CurrentViewModel<MainViewModel>();
                    if (vm == null) return;

                    switch (integerUpDown.Name)
                    {
                        case "NumDelaySensor":
                            ViewModelSharedValues.Settings.SystemParamsList[CurrentIndex()].DelaySensor = int.Parse(integerUpDown.ContentText);
                            vm.ConnectParamsList[CurrentIndex()].DelaySensor = int.Parse(integerUpDown.ContentText);
                            break;
                        case "NumDisSensor":
                            vm.ConnectParamsList[CurrentIndex()].DisableSensor = int.Parse(integerUpDown.ContentText);
                            break;
                        case "NumPulseEncoder":
                            vm.ConnectParamsList[CurrentIndex()].PulseEncoder = int.Parse(integerUpDown.ContentText);
                            break;
                        default:
                            break;
                    }
                    Debug.WriteLine("Vao day 1");
                    vm?.AutoSaveConnectionSetting(CurrentIndex(), CommonDataType.AutoSaveSettingsType.Controller);

                }), DispatcherPriority.Background);
            }
            catch (Exception)
            {

            }
        }

        private void Double_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal> e)
        {
            if (IsInitializing) return;
            try
            {
                var doubleUpDown = sender as TAlex.WPF.Controls.NumericUpDown;
                if (doubleUpDown?.ContentText == null) return;
                doubleUpDown?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var vm = CurrentViewModel<MainViewModel>();
                    if (vm == null) return;

                    switch (doubleUpDown.Name)
                    {
                        case "NumEncoderDia":
                            vm.ConnectParamsList[CurrentIndex()].EncoderDiameter = double.Parse(doubleUpDown.ContentText);
                            break;

                        default:
                            break;
                    }
                    vm.AutoSaveConnectionSetting(CurrentIndex(), CommonDataType.AutoSaveSettingsType.Controller);
                }), DispatcherPriority.Background);
            }
            catch (Exception) { }
        }

        private void ToggleButtonEnableControllerState(object sender, RoutedEventArgs e)
        {
            if (IsInitializing) return;
            try
            {
                var toggleButton = sender as ToggleButton;
                var vm = CurrentViewModel<MainViewModel>();
                toggleButton?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (vm == null) return;
                    vm.ConnectParamsList[CurrentIndex()].EnController = (bool)toggleButton.IsChecked;
                    vm.AutoSaveConnectionSetting(CurrentIndex(), SharedProgram.DataTypes.CommonDataType.AutoSaveSettingsType.Controller);
                }), DispatcherPriority.Background);

                if (vm.ConnectParamsList[CurrentIndex()].EnController)
                {
                    InputArea.IsEnabled = true;
                }
                else
                {
                    InputArea.IsEnabled = false;
                }

            }
            catch (Exception) { }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            CurrentViewModel<MainViewModel>()?.ConnectParamsList[CurrentIndex()].ResponseMessList.Clear();
        }

        private void SelectPrintField(object sender, RoutedEventArgs e) // Select fields for verify and print send
        {
            try
            {
                var vm = CurrentViewModel<MainViewModel>();
                if (vm is null) return;
                VerifyAndPrintPODFormat verifyAndPrintPODFormat = new()
                {
                    Index = CurrentIndex(),
                    DataContext = vm
                };

                var res = verifyAndPrintPODFormat.ShowDialog(); // Show diaglog select POD
                if (res == true)
                {
                    List<PODModel> _PODFormat = verifyAndPrintPODFormat._PODFormat;
                    vm.ConnectParamsList[CurrentIndex()].PrintFieldForVerifyAndPrint = _PODFormat;
                    vm.ConnectParamsList[CurrentIndex()].FormatedPOD = verifyAndPrintPODFormat.FormatedPOD;
                    vm.AutoSaveConnectionSetting(CurrentIndex(), CommonDataType.AutoSaveSettingsType.VerifyAndPrint);
                }
            }
            catch (Exception)
            {
            }
        }

        private void DefaultClick(object sender, RoutedEventArgs e)
        {
            var vm = CurrentViewModel<MainViewModel>();
            if (vm is null) return;
            vm.ConnectParamsList[CurrentIndex()].FailedDataSentToPrinter = "Failure";

        }

        private void ComboBoxStationNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var cbb = sender as ComboBox;
                var vm = CurrentViewModel<MainViewModel>();
                if (vm is null) return;
                vm.StationSelectedIndex = cbb.SelectedIndex;
                vm.CheckStationChange();
                cbb.SelectedIndex = vm.StationSelectedIndex;
            }
            catch (Exception)
            {
            }
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Check if the input is a number, if not, handle the event
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private async void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var res = CusMsgBox.Show(LanguageModel.GetLanguage("RestartConfirmation"),
                    LanguageModel.GetLanguage("WarningDialogCaption"),
                    ButtonStyleMessageBox.OKCancel,
                    ImageStyleMessageBox.Info);

                if (res.Result)
                {
                    var vm = CurrentViewModel<MainViewModel>();
                    var job = vm?.JobList[CurrentIndex()];
                    await ViewModelSharedFunctions.RestartDeviceTransfer(job);

                    if (job?.DeviceTransferID == null || job?.DeviceTransferID == 0)
                    {
                        CusAlert.Show(LanguageModel.GetLanguage("RestartFailed", job?.Index), ImageStyleMessageBox.Error, true);
                    }
                    else
                    {
                        vm?.DeleteSeletedJob(CurrentIndex());
                        vm?.UpdateJobInfo(CurrentIndex());
                        ViewModelSharedEvents.OnChangeJobHandler(((Button)sender).Name, CurrentIndex()); // event trigger for clear data job detail
                        CusAlert.Show(LanguageModel.GetLanguage("RestartSuccessfully", job?.Index), ImageStyleMessageBox.Info, true);
                    }
                    //  vm?.AutoSaveConnectionSetting(CurrentIndex());
                }
            }
            catch (Exception)
            {
            }
        }

        private void RadBasic_Click(object sender, RoutedEventArgs e)
        {
            ChangeVNPCompareMode();
        }

        private void RadCompare_Click(object sender, RoutedEventArgs e)
        {
            ChangeVNPCompareMode();
        }

        private void ChangeVNPCompareMode()
        {
            try
            {
                var vm = CurrentViewModel<MainViewModel>();
                if (vm == null) return;
                if (RadBasic.IsChecked == true)
                {
                    vm.ConnectParamsList[CurrentIndex()].VerifyAndPrintBasicSentMethod = true;
                }
                if (RadCompare.IsChecked == true)
                {
                    vm.ConnectParamsList[CurrentIndex()].VerifyAndPrintBasicSentMethod = false;
                }
                vm.AutoSaveConnectionSetting(CurrentIndex(), CommonDataType.AutoSaveSettingsType.VerifyAndPrint);
            }
            catch (Exception) { }
        }

        private void NumUpdownParamsHandler(NumericUpDown numericUpDown, double value)
        {
            try
            {
                var vm = CurrentViewModel<MainViewModel>();
                if (vm == null) return;
                if (vm.ConnectParamsList == null) return;
                var index = CurrentIndex();

                switch (numericUpDown.Name)
                {
                    case "NumPort":
                        vm.ConnectParamsList[index].PrinterPort = value;
                        vm.ConnectParamsList[index].PrinterPort = value;
                        vm.ConnectParamsList[index].PrinterPorts[currentSelectedPrinter] = value;
                        vm.AutoSaveConnectionSetting(index, CommonDataType.AutoSaveSettingsType.Printer);
                        break;
                    case "NumPLCPort":
                        vm.ConnectParamsList[index].ControllerPort = value;
                        vm.AutoSaveConnectionSetting(index, CommonDataType.AutoSaveSettingsType.Controller);
                        break;
                    case "NumDelaySensor":
                        vm.ConnectParamsList[index].DelaySensor = (int)value;
                        vm.AutoSaveConnectionSetting(index, CommonDataType.AutoSaveSettingsType.Controller);
                        break;
                    case "NumDisSensor":
                        vm.ConnectParamsList[index].DisableSensor = (int)value;
                        vm.AutoSaveConnectionSetting(index, CommonDataType.AutoSaveSettingsType.Controller);
                        break;
                    case "NumPulseEncoder":
                        vm.ConnectParamsList[index].PulseEncoder = (int)value;
                        vm.AutoSaveConnectionSetting(index, CommonDataType.AutoSaveSettingsType.Controller);
                        break;
                    case "NumEncoderDia":
                        vm.ConnectParamsList[index].EncoderDiameter = value;
                        vm.AutoSaveConnectionSetting(index, CommonDataType.AutoSaveSettingsType.Controller);
                        break;
                    case "NumBuffer":
                        vm.ConnectParamsList[index].NumberOfBuffer = (int)value;
                        vm.AutoSaveConnectionSetting(index, CommonDataType.AutoSaveSettingsType.Printer);
                        break;
                    //case "NumPort1":
                    //    SharedValues.SelectedPrinter = (int)value;
                    //    MessageBox.Show(SharedValues.SelectedPrinter.ToString());
                    //    break;
                    default:               
                        break;
                }
            }
            catch (Exception)
            {
            }
        }


        private void NumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (IsInitializing) return;
            if (sender is MahApps.Metro.Controls.NumericUpDown numericUpDown && e.NewValue != null)
            {
                double newValue = (double)e.NewValue; // Ensure it matches the NumericUpDown's value type
                NumUpdownParamsHandler(numericUpDown, newValue);
            }
        }

        private void CheckAllPrinterSettings_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            var vm = CurrentViewModel<MainViewModel>();
            if (vm == null) return;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    vm.ConnectParamsList[CurrentIndex()].IsCheckPrinterSettingsEnabled = true;
                }
                else
                {
                    vm.ConnectParamsList[CurrentIndex()].IsCheckPrinterSettingsEnabled = false;
                }
                vm.AutoSaveConnectionSetting(CurrentIndex(), CommonDataType.AutoSaveSettingsType.Printer);
            }
        }

        //private void TextBoxIP_ParamsChanged(object sender, TextChangedEventArgs e)
        //{
        //    var ipAddressControl = sender as IpAddressControl;
        //    if (ipAddressControl != null)
        //    {
        //        // thinh sua PrinterIPs
        //        string updatedPrinterIP = ipAddressControl.Text;
        //        var container = FindParent<ContentPresenter>(ipAddressControl);
        //        var index = ItemControl_PrinterIPs.ItemContainerGenerator.IndexFromContainer(container);
        //        var vm = CurrentViewModel<MainViewModel>();
        //        vm.ConnectParamsList[CurrentIndex()].PrinterIPs[index] = updatedPrinterIP;
        //        vm.AutoSaveConnectionSetting(CurrentIndex(), CommonDataType.AutoSaveSettingsType.Printer);

        //        // MessageBox.Show($"Item changed at index: {index}" + updatedPrinterIP);
        //    }
        //}

        //private void NumericUpDownPorts_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        //{
        //    var numericUpDown = sender as MahApps.Metro.Controls.NumericUpDown;
        //    if (numericUpDown != null && e.NewValue != null)
        //    {
        //        var container = FindParent<ContentPresenter>(numericUpDown);
        //        var index = ItemControl_PrinterPorts.ItemContainerGenerator.IndexFromContainer(container);
        //        double newValue = (double)e.NewValue;
        //        var vm = CurrentViewModel<MainViewModel>();
        //        vm.ConnectParamsList[CurrentIndex()].PrinterPorts[index] = newValue;
        //        vm.AutoSaveConnectionSetting(CurrentIndex(), CommonDataType.AutoSaveSettingsType.Printer);

        //        //MessageBox.Show($"NumericUpDown value changed at index: {index}");
        //    }
        //}

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        private void SelectPrinter_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ButtonItemsControl.Items)
            {
                var container = ButtonItemsControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                if (container != null)
                {
                    var button = FindVisualChild<Button>(container);
                    if (button != null)
                    {
                        button.Tag = null; // Clear selection
                    }
                }
            }

            // Cast the sender to a Button
            if (sender is Button clickedButton && int.TryParse(clickedButton.Content.ToString(), out int templateNumber))
            {
                if (clickedButton.Tag == null || clickedButton.Tag.ToString() != "Selected")
                {
                    clickedButton.Tag = "Selected";  // Set as selected
                }
                else
                {
                    clickedButton.Tag = null;  // Deselect
                }
                HandlePrinterSelection(templateNumber);
            }
            else
            {
                MessageBox.Show("Unknown template selected.");
            }
        }


        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild)
                {
                    return tChild;
                }

                var result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private void HandlePrinterSelection(int templateNumber)
        {
            currentSelectedPrinter = templateNumber - 1;
            if (CurrentViewModel<MainViewModel>() is MainViewModel vm)
            {
                vm.CurrentConnectParams.PrinterIP = vm.CurrentConnectParams.PrinterIPs[templateNumber - 1];
                vm.CurrentConnectParams.PrinterPort = vm.CurrentConnectParams.PrinterPorts[templateNumber - 1];
                TextBoxPrinterIP.Text = vm.CurrentConnectParams.PrinterIP;
                NumPort.Value = vm.CurrentConnectParams.PrinterPort;
                //vm.AutoSaveConnectionSetting(CurrentIndex(), CommonDataType.AutoSaveSettingsType.Printer);
            }

        }
    }
}
