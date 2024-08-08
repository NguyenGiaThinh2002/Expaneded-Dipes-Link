using DipesLink.Extensions;
using DipesLink.Languages;
using DipesLink.Models;
using DipesLink.ViewModels;
using DipesLink.Views.Extension;
using DipesLink.Views.SubWindows;
using DipesLink.Views.UserControls.MainUc;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static DipesLink.Views.Enums.ViewEnums;
using Application = System.Windows.Application;

namespace DipesLink.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static event EventHandler<EventArgs>? MainWindowSizeChangeCustomEvent;
        public static SplashScreenLoading? SplashScreenLoading = new();
        private static int currentStation = 0;
        public MainWindow()
        {
            InitializeComponent();
            SetWindowTitle();
            DataContext = MainViewModel.GetIntance();
            EventRegister();
            new LanguageModel().UpdateApplicationLanguage("");
        }

        private void SetWindowTitle()
        {
            var softwareName = Properties.Settings.Default.SoftwareName;
            var version = Properties.Settings.Default.SoftwareVersion;
            this.Title = $"{softwareName} - {version}";
        }
        private void EventRegister()
        {
            ViewModelSharedEvents.OnChangeJob += OnChangeJobHandler;
            ViewModelSharedEvents.OnJobDetailChange += JobDetails_OnJobDetailChange;
            ViewModelSharedEvents.OnMoveToJobDetail += MoveToJobDetail;
            SizeChanged += MainWindow_SizeChanged;
            Closing += MainWindow_Closing;
            Closed += MainWindow_Closed;
            AllStationUc.DoneLoadUIEvent += AllStationUc_DoneLoadUIEvent;
            Shared.OnActionLoadingSplashScreen += Shared_OnActionLoadingSplashScreen;
            ListBoxMenu.SelectionChanged += ListBoxMenu_SelectionChanged;
        }

        private void OnChangeJobHandler(object? sender, int e)
        {
            var vm = CurrentViewModel<MainViewModel>();
            if (vm is null) return;
            vm.JobList[e].IsDBExist = false;
            vm.GetCurrentJobDetail(e);
        }

        private void MoveToJobDetail(object? sender, EventArgs e)
        {
            try
            {
                var vm = CurrentViewModel<MainViewModel>();
                if (vm is null || sender == null) return;
                int index = (int)sender;
                ListBoxMenu.SelectedIndex = 0;
                vm.SelectedTabIndex = index;
            }
            catch (Exception)
            {
            }
        }

        private void JobDetails_OnJobDetailChange(object? sender, int e)
        {
            try
            {
                int index = e;
                var camIP = ViewModelSharedValues.Settings.SystemParamsList[index].CameraIP;
                var printerIP = ViewModelSharedValues.Settings.SystemParamsList[index].PrinterIP;
                var controllerIP = ViewModelSharedValues.Settings.SystemParamsList[index].ControllerIP;

                TextBlockControllerIP.Text = controllerIP.ToString();
                TextBlockPrinterIP.Text = printerIP.ToString();
                TextBlockCamIP.Text = camIP.ToString();
                currentStation = index;
            }
            catch (Exception)
            {
            }

        }

        private bool CanApplicationClose()
        {
            try
            {
                var viewModel = CurrentViewModel<MainViewModel>();
                if (viewModel == null) return false;  // Ensure viewModel is not null
                int runningPrintersCount = viewModel.PrinterStateList.Count(printerState => printerState.State != "Stopped" && printerState.State != "Dừng");
                if (runningPrintersCount > 0)
                {
                    CusAlert.Show(LanguageModel.GetLanguage("StopAllStations"), ImageStyleMessageBox.Warning);
                    return false; 
                }
                
                var isExit = CusMsgBox.Show(
                    LanguageModel.GetLanguage("ExitConfirmation"), 
                    LanguageModel.GetLanguage("WarningDialogCaption"), 
                    ButtonStyleMessageBox.YesNo, 
                    ImageStyleMessageBox.Warning);
                return isExit.Result;  
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !CanApplicationClose();
        }

        private void ButtonExitApp_Click(object sender, RoutedEventArgs e)
        {
            if (CanApplicationClose())
            {
                Application.Current.Shutdown();  // Or this.Close() if it's from a window class
            }
        }

        private void ListBoxMenu_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                ViewModelSharedEvents.OnMainListBoxMenuChanged(ListBoxMenu.SelectedIndex);
                ViewModelSharedEvents.OnListBoxMenuSelectionChangeHandler(ListBoxMenu.SelectedIndex);
                if (ListBoxMenu.SelectedIndex != -1)
                    ComboBoxSelectView.SelectedIndex = 0;
                if (sender is ListBox lb)
                {
                    var vm = CurrentViewModel<MainViewModel>();
                    switch (lb.SelectedIndex)
                    {
                        case -1:
                            vm?.ChangeTitleMainWindow(Enums.ViewEnums.TitleAppContext.Overview);
                            break;
                        case 0:
                            vm?.ChangeTitleMainWindow(Enums.ViewEnums.TitleAppContext.Home);
                            break;
                        case 1:
                            vm?.ChangeTitleMainWindow(Enums.ViewEnums.TitleAppContext.Jobs); break;
                        case 2:
                            vm?.ChangeTitleMainWindow(Enums.ViewEnums.TitleAppContext.Setting);
                            break;
                        case 3:
                            vm?.ChangeTitleMainWindow(Enums.ViewEnums.TitleAppContext.Logs);
                            break;
                    }
                }

            }
            catch (Exception)
            {
            }
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
          
            CurrentViewModel<MainViewModel>()?.KillProcessByID();
            CurrentViewModel<MainViewModel>()?.ReleaseResource();
            Application.Current?.Shutdown();
        }

        private void AllStationUc_DoneLoadUIEvent(object? sender, EventArgs e)
        {
            MainWindowSizeChangeCustomEvent?.Invoke(ActualWidth, EventArgs.Empty);
        }

        private void Shared_OnActionLoadingSplashScreen(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (sender == null) return;
                var actionType = (DataTypes.ActionSplashScreen)sender;

                if (actionType == DataTypes.ActionSplashScreen.Show)
                {
                    SplashScreenLoading ??= new SplashScreenLoading();
                    SplashScreenLoading.ShowDialog();
                }
                else
                {
                    if (SplashScreenLoading != null)
                    {
                        SplashScreenLoading.Close();
                        SplashScreenLoading = null;
                    }
                }
            });
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

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double actualWidth = ActualWidth;
            MainWindowSizeChangeCustomEvent?.Invoke(ActualWidth, EventArgs.Empty);
        }

        private void DeviceStatListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var index = DeviceStatListBox.SelectedIndex;
            CurrentViewModel<MainViewModel>()?.ChangeJobByDeviceStatSymbol(index);
        }

        private void ComboBoxSelectView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var cbb = (System.Windows.Controls.ComboBox)sender;
            if (cbb.SelectedIndex == 1)
            {
                ListBoxMenu.SelectedIndex = -1;
            }
            else
            {
                ListBoxMenu.SelectedIndex = 0;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var viewModel = CurrentViewModel<MainViewModel>(); 
                var menuItem = sender as MenuItem;
                switch (menuItem.Name)
                {
                    case "Account_Management":
                        UsersManagement um = new();
                        um.Show();
                        break;
                    case "About_DP_Link":
                        var aboutPopup = new AboutPopup();
                        aboutPopup.ShowDialog();
                        break;
                    case "System_Management":
                        var systemManagement = new SystemManagement(viewModel);
                        systemManagement.ShowDialog();
                        break;
                    case "Logout": //Restart
                        Logout(viewModel.JobList.Any(job => job.OperationStatus != SharedProgram.DataTypes.CommonDataType.OperationStatus.Stopped));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        private void Logout(bool isNotRunning)
        {
            try
            {
                if (!isNotRunning)
                {
                    var res = CusMsgBox.Show(LanguageModel.GetLanguage("LogoutConfirmation"), 
                        LanguageModel.GetLanguage("WarningDialogCaption"), 
                        Enums.ViewEnums.ButtonStyleMessageBox.OKCancel, 
                        Enums.ViewEnums.ImageStyleMessageBox.Warning);
                    if (res.Result)
                    {
                        ApplicationHelper.RestartApplication();
                    }
                }
                else
                {
                    CusAlert.Show(LanguageModel.GetLanguage("StopAllStations"), ImageStyleMessageBox.Warning);
                }
            }
            catch (Exception)
            {
            }
           
        }
        //public static void RestartApplication()
        //{
        //    try
        //    {
        //        Process.Start(Process.GetCurrentProcess().MainModule.FileName);
        //        Application.Current.Shutdown();
        //    }
        //    catch (Exception)
        //    {

        //    }
           
        //}

        private void BorderUser_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {

                // Find the button that was clicked
                var userUI = sender as Border;
                if (userUI != null)
                {
                    // Find the ContextMenu resource
                    ContextMenu contextMenu = this.FindResource("MyContextMenu") as ContextMenu;
                    if (contextMenu != null)
                    {
                        foreach (var item in contextMenu.Items)
                        {
                            if (item is MenuItem menuItem)
                            {
                                menuItem.Click -= MenuItem_Click;
                                menuItem.Click += MenuItem_Click;
                            }
                        }


                        // Set the placement target and open the context menu
                        contextMenu.PlacementTarget = userUI;
                        contextMenu.IsOpen = true;

                        TextBlock adminTextBlock = contextMenu.Template.FindName("UsernameTextBlock", contextMenu) as TextBlock;
                        if (adminTextBlock != null)
                        {
                            adminTextBlock.Text = (string)Application.Current.Properties["Username"]; // Thay đổi nội dung
                        }
                    }
                }

            }
            catch (Exception)
            {

            }
        }

        private void ChangeView()
        {
            try
            {


                ListBoxMenu.SelectedIndex = ListBoxMenu.SelectedIndex != -1 ? -1 : 0;
                if (ListBoxMenu.SelectedIndex != -1)
                {
                    StackPanelIPDisplay.Visibility = Visibility.Visible;
                    ToggleButtonChangeView.IsChecked = false; // This will trigger the setter for True

                }
                else
                {
                    // or
                    StackPanelIPDisplay.Visibility = Visibility.Hidden;
                    ToggleButtonChangeView.IsChecked = true; // This will trigger the setter for False

                }
            }
            catch (Exception)
            {

            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeView();
        }

        private void ListBoxItem_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ToggleButtonChangeView.IsChecked = false;
            StackPanelIPDisplay.Visibility = Visibility.Visible;
            JobDetails_OnJobDetailChange(sender, currentStation);
        }

        private void Eng_Button_Click(object sender, RoutedEventArgs e)
        {
            new LanguageModel().UpdateApplicationLanguage("en-US");
        }

        private void Vi_Button_Click(object sender, RoutedEventArgs e)
        {
            new LanguageModel().UpdateApplicationLanguage("vi-VN");
        }

    }
}