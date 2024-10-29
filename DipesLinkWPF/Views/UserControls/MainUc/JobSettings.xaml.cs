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
namespace DipesLink.Views.UserControls.MainUc
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class JobSettings : UserControl
    {
        public static bool IsInitializing = true;
        private bool _firstLoad = false;
        public JobSettings()
        {
            InitializeComponent();
            EventRegister();  
        }

        private void EventRegister()
        {
            Loaded += SettingsUc_Loaded;
            TextBoxPrinterIP.TextChanged += TextBox_ParamsChanged;
            TextBoxCamIP.TextChanged += TextBox_ParamsChanged;
            TextBoxControllerIP.TextChanged += TextBox_ParamsChanged;
            ViewModelSharedEvents.OnMainListBoxMenu += MainListBoxMenuChange;
            ViewModelSharedEvents.OnChangeJobStatus += JobStatusChanged;

            comboBoxComName.SelectionChanged += AdjustData;
            comboBoxBitPerSeconds.SelectionChanged += AdjustData;
            comboBoxDataBits.SelectionChanged += AdjustData;
            comboBoxParity.SelectionChanged += AdjustData;
            comboBoxStopBits.SelectionChanged += AdjustData;
        }
        // UI NEW CODE 
        private void AdjustData(object sender, EventArgs e)
        {
            if (IsInitializing) return;
            if (sender is not ComboBox comboBox) return;

                var vm = CurrentViewModel<MainViewModel>();
                if (vm == null) return;
                if (vm.ConnectParamsList == null) return;
                // vm.AutoSaveConnectionSetting(CurrentIndex());
                if (sender == comboBoxComName)
                {
                    switch (comboBoxComName.SelectedIndex)
                    {
                        case 0:
                            if (!(vm.ConnectParamsList[CurrentIndex()].ComName == "COM3"))
                            {
                                vm.ConnectParamsList[CurrentIndex()].ComName = "COM3";
                            }
                            break;
                        case 1:
                            if (!(vm.ConnectParamsList[CurrentIndex()].ComName == "COM4"))
                            {
                                vm.ConnectParamsList[CurrentIndex()].ComName = "COM4";
                            }
                            break;
                        case 2:
                            if (!(vm.ConnectParamsList[CurrentIndex()].ComName == "COM5"))
                            {
                                vm.ConnectParamsList[CurrentIndex()].ComName = "COM5";
                            }
                            break;
                        case 3:
                            if (!(vm.ConnectParamsList[CurrentIndex()].ComName == "COM6"))
                            {
                                vm.ConnectParamsList[CurrentIndex()].ComName = "COM6";
                            }
                            break;
                        case 4:
                            if (!(vm.ConnectParamsList[CurrentIndex()].ComName == "COM7"))
                            {
                                vm.ConnectParamsList[CurrentIndex()].ComName = "COM7";
                            }
                            break;
                        default:
                            break;
                    }

                    InitSerialDivComName(vm);
                }
                if (sender == comboBoxBitPerSeconds)
                {
                    switch (comboBoxBitPerSeconds.SelectedIndex)
                    {
                        case 0:
                            if (!(vm.ConnectParamsList[CurrentIndex()].BitPerSeconds == 9600))
                            {
                                vm.ConnectParamsList[CurrentIndex()].BitPerSeconds = 9600;
                                //Shared.SerialDevController.
                            }
                            break;
                        case 1:
                            if (!(vm.ConnectParamsList[CurrentIndex()].BitPerSeconds == 19200))
                            {
                                vm.ConnectParamsList[CurrentIndex()].BitPerSeconds = 19200;
                            }
                            break;
                        case 2:
                            if (!(vm.ConnectParamsList[CurrentIndex()].BitPerSeconds == 38400))
                            {
                                vm.ConnectParamsList[CurrentIndex()].BitPerSeconds = 38400;
                            }
                            break;
                        case 3:
                            if (!(vm.ConnectParamsList[CurrentIndex()].BitPerSeconds == 57600))
                            {
                                vm.ConnectParamsList[CurrentIndex()].BitPerSeconds = 57600;
                            }
                            break;
                        case 4:
                            if (!(vm.ConnectParamsList[CurrentIndex()].BitPerSeconds == 115200))
                            {
                                vm.ConnectParamsList[CurrentIndex()].BitPerSeconds = 115200;
                            }
                            break;
                        default:
                            break;
                    }

                    InitSerialDivBitPerSecond(vm);
                }

                if (sender == comboBoxDataBits)
                {
                    switch (comboBoxDataBits.SelectedIndex)
                    {
                        case 0:
                            if (!(vm.ConnectParamsList[CurrentIndex()].DataBits == 7))
                            {
                                vm.ConnectParamsList[CurrentIndex()].DataBits = 7;
                            }
                            break;
                        case 1:
                            if (!(vm.ConnectParamsList[CurrentIndex()].DataBits == 8))
                            {
                                vm.ConnectParamsList[CurrentIndex()].DataBits = 8;
                            }
                            break;
                        default:
                            break;
                    }

                    InitSerialDivDataBits(vm);
                }

                if (sender == comboBoxParity)
                {
                    switch (comboBoxParity.SelectedIndex)
                    {
                        case 0:
                            if (!(vm.ConnectParamsList[CurrentIndex()].Parity == Parity.None))
                            {
                                vm.ConnectParamsList[CurrentIndex()].Parity = Parity.None;
                            }
                            break;
                        case 1:
                            if (!(vm.ConnectParamsList[CurrentIndex()].Parity == Parity.Odd))
                            {
                                vm.ConnectParamsList[CurrentIndex()].Parity = Parity.Odd;
                            }
                            break;
                        case 2:
                            if (!(vm.ConnectParamsList[CurrentIndex()].Parity == Parity.Even))
                            {
                                vm.ConnectParamsList[CurrentIndex()].Parity = Parity.Even;
                            }
                            break;
                        default:
                            break;
                    }

                    InitSerialDivParity(vm);
                }

                if (sender == comboBoxStopBits)
                {
                    switch (comboBoxStopBits.SelectedIndex)
                    {
                        case 0:
                            if (!(vm.ConnectParamsList[CurrentIndex()].StopBits == StopBits.One))
                            {
                                vm.ConnectParamsList[CurrentIndex()].StopBits = StopBits.One;
                            }
                            break;
                        case 1:
                            if (!(vm.ConnectParamsList[CurrentIndex()].StopBits == StopBits.Two))
                            {
                                vm.ConnectParamsList[CurrentIndex()].StopBits = StopBits.Two;
                            }
                            break;
                        default:
                            break;
                    }

                    InitSerialDivStopBits(vm);
                }
                vm.AutoSaveConnectionSetting(CurrentIndex());
        }

        private void UpdateCheckAllPrinterSettings(MainViewModel vm)
        {
            CheckAllPrinterSettings.IsOn = vm.ConnectParamsList[CurrentIndex()].IsCheckPrinterSettingsEnabled;
        }

        private void UpdateSerialDevComboBoxValues(MainViewModel vm)
        {
            InitSerialDivComName(vm);
            InitSerialDivBitPerSecond(vm);
            InitSerialDivDataBits(vm);
            InitSerialDivParity(vm);
            InitSerialDivStopBits(vm);
        }

        private void InitSerialDivComName(MainViewModel vm)
        {
            if (vm == null) return;
            if (vm.ConnectParamsList == null) return;
            switch (vm.ConnectParamsList[CurrentIndex()].ComName)
            {
                case "COM3":
                    comboBoxComName.SelectedIndex = 0;
                    break;
                case "COM4":
                    comboBoxComName.SelectedIndex = 1;
                    break;
                case "COM5":
                    comboBoxComName.SelectedIndex = 2;
                    break;
                case "COM6":
                    comboBoxComName.SelectedIndex = 3;
                    break;
                case "COM7":
                    comboBoxComName.SelectedIndex = 4;
                    break;
                default:
                    break;
            }
        }

        private void InitSerialDivBitPerSecond(MainViewModel vm)
        {
            if (vm == null) return;
            if (vm.ConnectParamsList == null) return;
            switch (vm.ConnectParamsList[CurrentIndex()].BitPerSeconds)
            {
                case 9600:
                    comboBoxBitPerSeconds.SelectedIndex = 0;
                    break;
                case 19200:
                    comboBoxBitPerSeconds.SelectedIndex = 1;
                    break;
                case 38400:
                    comboBoxBitPerSeconds.SelectedIndex = 2;
                    break;
                case 57600:
                    comboBoxBitPerSeconds.SelectedIndex = 3;
                    break;
                case 115200:
                    comboBoxBitPerSeconds.SelectedIndex = 4;
                    break;
                default:
                    break;
            }
        }

        private void InitSerialDivDataBits(MainViewModel vm)
        {
            if (vm == null) return;
            if (vm.ConnectParamsList == null) return;
            switch (vm.ConnectParamsList[CurrentIndex()].DataBits)
            {
                case 7:
                    comboBoxDataBits.SelectedIndex = 0;
                    break;
                case 8:
                    comboBoxDataBits.SelectedIndex = 1;
                    break;
                default:
                    break;
            }
        }

        private void InitSerialDivParity(MainViewModel vm)
        {
            if (vm == null) return;
            if (vm.ConnectParamsList == null) return;
            switch (vm.ConnectParamsList[CurrentIndex()].Parity)
            {
                case Parity.None:
                    comboBoxParity.SelectedIndex = 0;
                    break;
                case Parity.Odd:
                    comboBoxParity.SelectedIndex = 1;
                    break;
                case Parity.Even:
                    comboBoxParity.SelectedIndex = 2;
                    break;
                default:
                    break;
            }
        }

        private void InitSerialDivStopBits(MainViewModel vm)
        {
            if (vm == null) return;
            if (vm.ConnectParamsList == null) return;
            switch (vm.ConnectParamsList[CurrentIndex()].StopBits)
            {
                case StopBits.One:
                    comboBoxStopBits.SelectedIndex = 0;
                    break;
                case StopBits.Two:
                    comboBoxStopBits.SelectedIndex = 1;
                    break;
                default:
                    break;
            }
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
                UpdateSendModeVerifyAndPrint(vm);
                UpdateSerialDevComboBoxValues(vm);
                UpdateCheckAllPrinterSettings(vm);
            }
            catch (Exception)
            {
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
                UpdateSendModeVerifyAndPrint(vm);
                UpdateSerialDevComboBoxValues(vm);
                UpdateCheckAllPrinterSettings(vm);
            }
            catch (Exception)
            {
            }
        }

        void UpdateSendModeVerifyAndPrint(MainViewModel vm)
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
                            break;
                        case "TextBoxPrinterPort":
                           // vm.ConnectParamsList[CurrentIndex()].PrinterPort = textBox.Text;
                            break;
                        case "TextBoxCamIP":
                            vm.ConnectParamsList[CurrentIndex()].CameraIP = textBox.Text;
                            break;
                        case "TextBoxControllerIP":
                            vm.ConnectParamsList[CurrentIndex()].ControllerIP = textBox.Text;
                            break;
                        case "TextBoxControllerPort":
                          //  vm.ConnectParamsList[CurrentIndex()].ControllerPort = textBox.Text;
                            break;
                        case "TextBoxErrorField":
                            vm.ConnectParamsList[CurrentIndex()].FailedDataSentToPrinter = textBox.Text;
                            break;
                        case "NumDelaySensor":
                            vm.ConnectParamsList[CurrentIndex()].DelaySensor = int.Parse(textBox.Text);
                            break;
                        case "NumDisSensor":
                            vm.ConnectParamsList[CurrentIndex()].DisableSensor = int.Parse(textBox.Text);
                            break;
                        case "NumPulseEncoder":
                            vm.ConnectParamsList[CurrentIndex()].PulseEncoder = int.Parse(textBox.Text);
                            break;
                        case "NumEncoderDia":
                            vm.ConnectParamsList[CurrentIndex()].EncoderDiameter = double.Parse(textBox.Text);
                            break;

                        default:
                            break;
                    }
                    vm.AutoSaveConnectionSetting(CurrentIndex());

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
                            break;
                        case "TextBoxPrinterPort":
                           // vm.ConnectParamsList[index].PrinterPort = textBox.Text;
                            break;
                        case "TextBoxCamIP":
                            vm.ConnectParamsList[index].CameraIP = textBox.Text;
                            break;
                        case "TextBoxControllerIP":
                            vm.ConnectParamsList[index].ControllerIP = textBox.Text;
                            break;
                        case "TextBoxControllerPort":
                          //  vm.ConnectParamsList[index].ControllerPort = textBox.Text;
                            break;
                        case "NumDelaySensor":
                            vm.ConnectParamsList[index].DelaySensor = int.Parse(textBox.Text);
                            break;
                        case "NumDisSensor":
                            vm.ConnectParamsList[index].DisableSensor = int.Parse(textBox.Text);
                            break;
                        case "NumPulseEncoder":
                            vm.ConnectParamsList[index].PulseEncoder = int.Parse(textBox.Text);
                            break;
                        case "NumEncoderDia":
                            vm.ConnectParamsList[index].EncoderDiameter = double.Parse(textBox.Text);
                            break;
                        case "TextBoxErrorField":
                            vm.ConnectParamsList[index].FailedDataSentToPrinter = textBox.Text;
                            break;
                        default:
                            break;
                    }
                    vm.AutoSaveConnectionSetting(index);

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
                    vm?.AutoSaveConnectionSetting(CurrentIndex());

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
                    vm.AutoSaveConnectionSetting(CurrentIndex());
                }), DispatcherPriority.Background);
            }
            catch (Exception) { }
        }

        private void ToggleButtonState(object sender, RoutedEventArgs e)
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
                    Debug.WriteLine("Vao day 3");
                    vm.AutoSaveConnectionSetting(CurrentIndex());
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
                    vm.AutoSaveConnectionSetting(CurrentIndex());
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

        private void NumDelaySensor_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

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
                    vm?.AutoSaveConnectionSetting(CurrentIndex());
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
                vm.AutoSaveConnectionSetting(CurrentIndex());
            }
            catch (Exception) { }
        }

      


        private void NumUpdownParamsHandler(HandyControl.Controls.NumericUpDown num)
        {
            try
            {
                num?.Dispatcher.BeginInvoke(new Action(() => // Use BeginInvoke to Update Input Last Value 
                {
                    var vm = CurrentViewModel<MainViewModel>();
                    if (vm == null) return;
                    if (vm.ConnectParamsList == null) return;
                 //   num.Text ??= string.Empty;
                    var index = CurrentIndex();
                    switch (num.Name)
                    {
                        case "NumPort":
                            vm.ConnectParamsList[index].PrinterPort = num.Value;
                            break;
                        case "NumPLCPort":
                            vm.ConnectParamsList[index].ControllerPort = num.Value;
                            break;
                        case "NumDelaySensor":
                            vm.ConnectParamsList[index].DelaySensor = (int)num.Value;
                            break;
                        case "NumDisSensor":
                            vm.ConnectParamsList[index].DisableSensor = (int)num.Value;
                            break;
                        case "NumPulseEncoder":
                            vm.ConnectParamsList[index].PulseEncoder = (int)num.Value;
                            break;
                        case "NumEncoderDia":
                            vm.ConnectParamsList[index].EncoderDiameter = num.Value;
                            break;
                        default:
                            break;
                    }
                    vm.AutoSaveConnectionSetting(index);

                }), DispatcherPriority.Background);
            }
            catch (Exception)
            {
            }
        }

        private void NumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (IsInitializing) return;
            if (sender is HandyControl.Controls.NumericUpDown numericUpdown)
            {
                NumUpdownParamsHandler(numericUpdown);
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
                vm.AutoSaveConnectionSetting(CurrentIndex());
            }
        }
    }
}
