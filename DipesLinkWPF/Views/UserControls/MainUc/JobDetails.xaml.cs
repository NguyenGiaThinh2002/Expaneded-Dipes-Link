using DipesLink.Models;
using DipesLink.ViewModels;
using DipesLink.Views.Extension;
using DipesLink.Views.SubWindows;
using SharedProgram.Shared;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DipesLink.Views.UserControls.MainUc
{
    /// <summary>
    /// Interaction logic for HomeUc.xaml
    /// </summary>
    public partial class JobDetails : UserControl
    {

        PrintingDataTableHelper _printingDataTableHelper = new();
        JobOverview? _currentJob;
        ConcurrentQueue<string[]> _queueCheckedCode = new();
        private CheckedObserHelper? _checkedObserHelper = new();
        private ConcurrentQueue<string[]>? _queuePrintedCode = new();
        private CancellationTokenSource ctsGetPrintedCode = new();
        PrintObserHelper? _PrintObserHelper;

        public JobDetails()
        {
            InitializeComponent();
            Loaded += StationDetailUc_Loaded;
            ViewModelSharedEvents.OnChangeJob += OnChangeJobHandler;
            ViewModelSharedEvents.OnListBoxMenuSelectionChange += ViewModelSharedEvents_OnListBoxMenuSelectionChange;
            InitValues();
            Task.Run(TaskAddDataAsync);
            Task.Run(TaskChangePrintStatusAsync);
        }

        private void ViewModelSharedEvents_OnListBoxMenuSelectionChange(object? sender, EventArgs e)
        {
            var selectedIndex = (int)sender;
            if (selectedIndex == 0 || selectedIndex == 1)
            {
                EventRegister();
            }
        }

        private void OnChangeJobHandler(object? sender, int jobIndex)
        {
            if (_currentJob is not null && _currentJob.Index == jobIndex)
            {
                Debug.WriteLine($"Clear data of Job: {jobIndex}");
                _printingDataTableHelper?.Dispose(); // Release DataTable Helper for Printing
                _printingDataTableHelper = new();  // Create new DataTable Helper for Printing

                InitValues();

                DataGridDB.ItemsSource = null; // Clear Datagrid printed
                DataGridDB.Columns.Clear();

                DataGridResult.ItemsSource = null; // Clear Datagrid checked
                DataGridResult.Columns.Clear();

                if (sender is not null && (string)sender == "ButtonAddJob")
                {
                    ViewModelSharedEvents.OnMoveToJobDetailHandler(jobIndex);
                }
            }
        }

        public void InitValues()
        {
            TextBlockTotalChecked.Text = "0";
            TextBlockTotalPassed.Text = "0";
            TextBlockTotalFailed.Text = "0";
        }

        public async void StationDetailUc_Loaded(object sender, RoutedEventArgs e)
        {
            EventRegister();
            ViewModelSharedEvents.OnJobDetailChangeHandler(_currentJob.Index);
            if (!_currentJob.IsDBExist)
            {
                Debug.WriteLine("Event load database was called: " + _currentJob.Index);
                await PerformLoadDbAfterDelay();
            }
        }

        private async Task PerformLoadDbAfterDelay()
        {
            await Task.Delay(100);
            _currentJob?.RaiseLoadDb(_currentJob.Index);

        }

        public void EventRegister()
        {
            try
            {

                if (_currentJob == null)
                {
                    _currentJob = CurrentViewModel<JobOverview>();
                    if (_currentJob == null) return;
                    _currentJob.OnLoadCompleteDatabase -= Shared_OnLoadCompleteDatabase;
                    _currentJob.OnChangePrintedCode -= Shared_OnChangePrintedCode;
                    _currentJob.OnLoadCompleteCheckedDatabase -= Shared_OnLoadCompleteCheckedDatabase;
                    _currentJob.OnChangeCheckedCode -= Shared_OnChangeCheckedCode;

                    _currentJob.OnLoadCompleteDatabase += Shared_OnLoadCompleteDatabase;
                    _currentJob.OnChangePrintedCode += Shared_OnChangePrintedCode;
                    _currentJob.OnLoadCompleteCheckedDatabase += Shared_OnLoadCompleteCheckedDatabase;
                    _currentJob.OnChangeCheckedCode += Shared_OnChangeCheckedCode;

                    if (_currentJob.Name == null)
                    {
                        if (_currentJob.IsShowLoadingDB == Visibility.Collapsed)
                        {
                            ViewModelSharedEvents.OnEnableUIChangeHandler(_currentJob.Index, true);
                        }
                    }
                }
                else
                {

                }

            }
            catch (Exception) { }
        }


        #region VIEWMODEL HANDLER

        public void CallbackCommand(Action<MainViewModel> execute)
        {
            try
            {
                if (DataContext is MainViewModel model)
                {
                    execute?.Invoke(model);
                }
                else
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
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


        #endregion VIEWMODEL HANDLER

        #region DATAGRID FOR DATABASE

        
        private void Shared_OnLoadCompleteDatabase(object? sender, EventArgs e) // sự kiện báo load xong database ở UI được gửi từ device transfer
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (sender is List<(List<string[]>, int)> dbList)  // Item 1: db, item 2: current page
                    {
                        var dataList = dbList.FirstOrDefault().Item1;
                        var currentPage = dbList.FirstOrDefault().Item2;
                        _PrintObserHelper = new(dataList, currentPage, DataGridDB);
                        var vm = CurrentViewModel<JobOverview>();
                        if (vm != null)
                        {
                            vm.IsShowLoadingDB = Visibility.Collapsed;
                            ViewModelSharedEvents.OnEnableUIChangeHandler(vm.Index, true);
                        }
                        ViewModelSharedEvents.OnDataTableLoadingHandler();
                    }
                });
            }
            catch (Exception)
            {
            }
        }

        private void Shared_OnChangePrintedCode(object? sender, EventArgs e)
        {
            if (sender is string[] printedCode)
            {
                _queuePrintedCode.Enqueue(printedCode);
            }
        }


        private async Task TaskChangePrintStatusAsync()
        {
            try
            {
                while (true)
                {
                    if (ctsGetPrintedCode.IsCancellationRequested && _queuePrintedCode.IsEmpty)
                    {
                        ctsGetPrintedCode.Token.ThrowIfCancellationRequested();
                    }
                    while (_queuePrintedCode.TryDequeue(out var code))
                    {
                        _PrintObserHelper.CheckAndUpdateStatusAsync(code);
                        await Task.Delay(1);
                    }
                    await Task.Delay(1);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        #endregion DATAGRID FOR AFTER PRODUCTION

        #region DATAGRID FOR CHECKED CODE

        private void Shared_OnLoadCompleteCheckedDatabase(object? sender, EventArgs e)
        {
            var listChecked = sender as List<string[]>;
            if (listChecked != null)
            {
                Application.Current?.Dispatcher.Invoke(new Action(() =>
                {
                    DataGridResult.AutoGenerateColumns = false;
                    _checkedObserHelper.ConvertListToObservableCol(listChecked);
                    CheckedObserHelper.CreateDataTemplate(DataGridResult);
                    _checkedObserHelper.TakeFirtloadCollection();
                    DataGridResult.ItemsSource = _checkedObserHelper.DisplayList;
                    UpdateCheckedNumber();
                    FirtLoadForChartPercent();
                }));
            }
        }

        private void FirtLoadForChartPercent()
        {
            if (_currentJob != null)
            {
                _currentJob.TotalChecked = _checkedObserHelper.TotalChecked.ToString();
                _currentJob.TotalPassed = _checkedObserHelper.TotalPassed.ToString();
                _currentJob.TotalFailed = _checkedObserHelper.TotalFailed.ToString();
                _currentJob.RaisePercentageChange(_currentJob.Index);
            }
        }

        private void Shared_OnChangeCheckedCode(object? sender, EventArgs e)
        {
            if (sender is string[] checkedCode)
            {
                _queueCheckedCode.Enqueue(checkedCode);
            }
        }

        private async Task TaskAddDataAsync()
        {
            while (true)
            {
                while (_queueCheckedCode.TryDequeue(out var result))
                {
                    _checkedObserHelper.AddNewData(result);
                    UpdateCheckedNumber();
                    await Task.Delay(1);
                }
                await Task.Delay(1);
            }
        }

        private async void UpdateCheckedNumber()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
             {
                 TextBlockTotalChecked.Text = _checkedObserHelper.TotalChecked.ToString();
                 TextBlockTotalPassed.Text = _checkedObserHelper.TotalPassed.ToString();
                 TextBlockTotalFailed.Text = _checkedObserHelper.TotalFailed.ToString();
             });
        }


        #endregion DATAGRID FOR CHECKED CODE

        private void ViewChekedResult_PreMouDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (_currentJob == null) return;

                JobLogsWindow jobLogsWindow = new(_checkedObserHelper)
                {
                    DataContext = DataContext as JobOverview,
                    Num_TotalChecked = _currentJob.TotalRecDb
                };

                if (int.TryParse(_currentJob.PrintedDataNumber, out int printed))
                {
                    jobLogsWindow.Num_Printed = printed;
                }

                if (int.TryParse(TextBlockTotalChecked.Text, out int totalChecked))
                {
                    jobLogsWindow.Num_Verified = totalChecked;
                    if (int.TryParse(TextBlockTotalFailed.Text, out int failed))
                    {
                        jobLogsWindow.Num_Failed = failed;
                        jobLogsWindow.Num_Valid = totalChecked - failed;
                    }
                }
                jobLogsWindow.ShowDialog();
            }
            catch (Exception)
            {
            }
        }

        private void PrintedData_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (_currentJob == null) return;
                PrintedLogsWindow printedLogsWindow = new(_currentJob);
                printedLogsWindow.ShowDialog();
            }
            catch (Exception)
            {
            }
        }

    }
}
