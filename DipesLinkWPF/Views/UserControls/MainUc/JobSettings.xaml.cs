using DipesLink.Languages;
using DipesLink.ViewModels;
using DipesLink.Views.Extension;
using DipesLink.Views.SubWindows;
using DipesLink.Views.UserControls.CustomControl;
using SharedProgram.Models;
using SharedProgram.Shared;
using System.Diagnostics;
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
          
            //ViewModelSharedEvents.MainListBoxMenuChange += ListBoxMenu_SelectionChanged;
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
                //ComboBoxStationNum.SelectedIndex = vm.StationSelectedIndex;
                RadBasic.IsChecked = vm.ConnectParamsList[CurrentIndex()].VerifyAndPrintBasicSentMethod;
                InputArea.IsEnabled = vm.ConnectParamsList[CurrentIndex()].EnController;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
            var vm = CurrentViewModel<MainViewModel>();
            vm?.SelectionChangeSystemSettings(CurrentIndex());
            vm?.LockUI(CurrentIndex());// Lock UI when running
        }

        private int CurrentIndex() => ListBoxMenuStationSetting.SelectedIndex;


        private void TextBox_ParamsChanged(object sender, TextChangedEventArgs e)
        {
           
            if (IsInitializing) return;
            if(sender is IpAddressControl textBoxIp)
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
                        vm.ConnectParamsList[CurrentIndex()].PrinterPort = textBox.Text;
                        break;
                    case "TextBoxCamIP":
                        vm.ConnectParamsList[CurrentIndex()].CameraIP = textBox.Text;
                        break;
                    case "TextBoxControllerIP":
                        vm.ConnectParamsList[CurrentIndex()].ControllerIP = textBox.Text;
                        break;
                    case "TextBoxControllerPort":
                        vm.ConnectParamsList[CurrentIndex()].ControllerPort = textBox.Text;
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

        private void TextBoxNormalParamsHandler(TextBox textBox)
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
                        vm.ConnectParamsList[index].PrinterPort = textBox.Text;
                        break;
                    case "TextBoxCamIP":
                        vm.ConnectParamsList[index].CameraIP = textBox.Text;
                        break;
                    case "TextBoxControllerIP":
                        vm.ConnectParamsList[index].ControllerIP = textBox.Text;
                        break;
                    case "TextBoxControllerPort":
                        vm.ConnectParamsList[index].ControllerPort = textBox.Text;
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

        private void ModeSendCheckedChange(object sender, RoutedEventArgs e)
        {
            if (IsInitializing) return;
            var rad = sender as RadioButton;
            try
            {
                rad?.Dispatcher.Invoke(new Action(() =>
                {
                    var vm = CurrentViewModel<MainViewModel>();
                    if (vm == null) return;
                   
                    if (rad.Name == "RadBasic" && rad.IsChecked == true)
                    {
                       vm.ConnectParamsList[CurrentIndex()].VerifyAndPrintBasicSentMethod = true;
                    }
                    else
                    {
                        vm.ConnectParamsList[CurrentIndex()].VerifyAndPrintBasicSentMethod = false;
                    }
                    vm.AutoSaveConnectionSetting(CurrentIndex());
                }), DispatcherPriority.Background);
            }
            catch (Exception){}
        }
        

        private void SelectPrintField(object sender, RoutedEventArgs e) // Select fields for verify and print send
        {
            var vm = CurrentViewModel<MainViewModel>();
            if (vm is null) return;
            VerifyAndPrintPODFormat verifyAndPrintPODFormat = new()
            {
                Index = CurrentIndex(),
                DataContext = vm
            };

            var res = verifyAndPrintPODFormat.ShowDialog(); // Show diaglog select POD
            if(res == true)
            {
                List<PODModel> _PODFormat = verifyAndPrintPODFormat._PODFormat;
                vm.ConnectParamsList[CurrentIndex()].PrintFieldForVerifyAndPrint = _PODFormat;
                vm.ConnectParamsList[CurrentIndex()].FormatedPOD = verifyAndPrintPODFormat.FormatedPOD;
                vm.AutoSaveConnectionSetting(CurrentIndex());
            }
        }

        private void DefaultClick(object sender, RoutedEventArgs e)
        {
            var vm = CurrentViewModel<MainViewModel>();
            if(vm is null) return;
            vm.ConnectParamsList[CurrentIndex()].FailedDataSentToPrinter = "Failure";    

        }

        private void ComboBoxStationNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbb = sender as ComboBox;
            var vm = CurrentViewModel<MainViewModel>();
            if (vm is null) return;
            vm.StationSelectedIndex = cbb.SelectedIndex;
            vm.CheckStationChange();
            cbb.SelectedIndex = vm.StationSelectedIndex;
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
            var res = CusMsgBox.Show(LanguageModel.GetLanguage("RestartConfirmation"), "Restart Station", ButtonStyleMessageBox.OKCancel, ImageStyleMessageBox.Info);
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

    }
}
