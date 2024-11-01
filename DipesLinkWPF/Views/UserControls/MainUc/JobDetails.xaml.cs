using Cloudtoid;
using DipesLink.Models;
using DipesLink.ViewModels;
using DipesLink.Views.Extension;
using DipesLink.Views.SubWindows;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Runtime.CompilerServices;
using SharedProgram.Shared;

namespace DipesLink.Views.UserControls.MainUc
{
    /// <summary>
    /// Interaction logic for HomeUc.xaml
    /// </summary>
    public partial class JobDetails : UserControl
    {

        private JobOverview? _currentJob;
        private readonly ConcurrentQueue<string[]> _queueCheckedCode = new();
        private PrintObserHelper? _printObserHelper;
        private CheckedObserHelper _checkedObserHelper = new();
        private readonly ConcurrentQueue<string[]> _queuePrintedCode = new();
        private readonly CancellationTokenSource _ctsGetPrintedCode = new();
        private readonly TimeBaseExecution _timeBasedExecution = new(TimeSpan.FromMilliseconds(500));
        private List<string[]> _dataList = new();
        private int getJobIndexChanged;

        public JobDetails()
        {
            InitializeComponent();
            EventRegisterFirstTime();
            InitValues();
            Task.Run(TaskAddDataAsync);
            Task.Run(TaskChangePrintStatusAsync);
        }

        void EventRegisterFirstTime()
        {
            Loaded += StationDetailUc_Loaded;
            ViewModelSharedEvents.OnChangeJob += OnChangeJobHandler;
            ViewModelSharedEvents.OnListBoxMenuSelectionChange += ListBoxMenuSelectionChange;
            ViewModelSharedEvents.OnJobDetailChange += JobDetails_OnJobDetailChange;
        }

        private void JobDetails_OnJobDetailChange(object? sender, int e)
        {
            UpdateIPAddressInfo(e);
        }

        private void SimulateModeAction()
        {
            if (SharedFunctions.GetCurrentUsernameAndRole()?.FirstOrDefault()[0] == "System" && 
                SharedFunctions.GetCurrentUsernameAndRole()?.FirstOrDefault()[1] == "Administrator")
            {
                ButtonSimulate.Visibility = Visibility.Visible;
            }
            else
            {
                ButtonSimulate.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateIPAddressInfo(int e)
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
               // currentStation = index;
            }
            catch (Exception)
            {
            }
        }

        private async void ListBoxMenuSelectionChange(object? sender, EventArgs e)
        {
            if (sender == null) return;
            var selectedIndex = (int)sender;
            if (selectedIndex == 0 || selectedIndex == 1) // Conditions for loading the database when changing Jobs
            {
                await Task.Delay(100);
                await ActionChange(false);
            }
        }

        private void OnChangeJobHandler(object? sender, int jobIndex)
        {
            getJobIndexChanged = jobIndex;
            if (_currentJob is not null && _currentJob.Index == jobIndex)
            {
                _currentJob = null;
                InitValues();
                DataGridDB.ItemsSource = null;
                DataGridDB.Columns.Clear();
                DataGridResult.ItemsSource = null;
                DataGridResult.Columns.Clear();
                _checkedObserHelper.Dispose();
                _printObserHelper?.Dispose();
                _checkedObserHelper = new();
            }
            if (sender is not null && (string)sender == "ButtonAddJob")
            {
                ViewModelSharedEvents.OnMoveToJobDetailHandler(jobIndex);
            }
        }

        private void ChangeLayoutDataGrid()
        {
            if (_currentJob == null) return;
            if (_currentJob.CompareType != SharedProgram.DataTypes.CommonDataType.CompareType.Database)
            {
                _currentJob.RowHeightDatabase = new(0, GridUnitType.Pixel);
                _currentJob.RowHeightDatabaseTitle = new(0, GridUnitType.Pixel);
            }
            else
            {
                _currentJob.RowHeightDatabase = new(1, GridUnitType.Star);
                _currentJob.RowHeightDatabaseTitle = new(18, GridUnitType.Pixel);
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
            await ActionChange(true);
        }

        async Task ActionChange(bool isUserControlLoaded)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
             {
                 SimulateModeAction();
                 EventRegisterForJobEveryLoadUI();
             });

            if (_currentJob == null)
            {
                return;
            }

            ViewModelSharedEvents.OnJobDetailChanged(_currentJob.Index);

            if (!_currentJob.IsDBExist)
            {
                // Case 1: Only stations with Job changes can load the database,
                // Case 2: UserControl of this Station, Load will load database for this Station
                if (((getJobIndexChanged == _currentJob?.Index) && !isUserControlLoaded) || isUserControlLoaded)
                {
                    await PerformLoadDbAfterDelay();
                }
            }
        }

        private async Task PerformLoadDbAfterDelay()
        {
            await Task.Delay(100);
            _currentJob?.RaiseLoadDb(_currentJob.Index);
        }

        public void EventRegisterForJobEveryLoadUI()
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
                ChangeLayoutDataGrid();


            }
            catch (Exception) { }
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

        private void Shared_OnLoadCompleteDatabase(object? sender, EventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (sender is List<(List<string[]>, int)> dbList)
                    {
                        if (_currentJob != null)
                        {
                            _dataList = dbList.FirstOrDefault().Item1;
                            var currentPage = dbList.FirstOrDefault().Item2;
                            _printObserHelper = new(_dataList, currentPage, DataGridDB);
                            _currentJob.PrintedDataNumber = _printObserHelper.PrintedNumber.ToString();
                            var vm = CurrentViewModel<JobOverview>();
                            if (vm != null)
                            {
                                vm.IsShowLoadingDB = Visibility.Collapsed;
                                ViewModelSharedEvents.OnEnableUIChangeHandler(vm.Index, true);
                            }
                            ViewModelSharedEvents.OnDataTableLoadingHandler(vm.Index);
                        }
                    }
                });
            }
            catch (Exception) { }
        }

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
                    // UpdateCheckedNumber();
                    FirtLoadForChartPercent();
                }));
            }
        }

        private void Shared_OnChangePrintedCode(object? sender, EventArgs e)
        {
            if (sender is string[] printedCode)
            {
                _queuePrintedCode?.Enqueue(printedCode);
            }
        }

        private void Shared_OnChangeCheckedCode(object? sender, EventArgs e)
        {
            if (sender is string[] checkedCode)
            {
                _queueCheckedCode.Enqueue(checkedCode);
            }
        }

        private async Task TaskChangePrintStatusAsync()
        {
            var tempDataList = new List<string[]>();
            var batchSize = 50;
            var batchDelay = 50;

            while (true)
            {
                while (_queuePrintedCode.TryDequeue(out var result))
                {
                    tempDataList.Add(result);

                    if (tempDataList.Count >= batchSize)
                    {
                        var tempData = new List<string[]>(tempDataList);
                        tempDataList.Clear();

                        await Task.Run(() =>
                        {
                            foreach (var data in tempData)
                            {
                                _printObserHelper?.CheckAndUpdateStatusAsync(data);
                            }
                        });
                    }
                    await Task.Delay(1);
                }

                if (tempDataList.Count > 0)
                {
                    var tempData = new List<string[]>(tempDataList);
                    tempDataList.Clear();

                    await Task.Run(() =>
                    {
                        foreach (var data in tempData)
                        {
                            _printObserHelper?.CheckAndUpdateStatusAsync(data);
                        }
                    });
                }

                await Task.Delay(batchDelay);
            }

        }

        private async Task TaskAddDataAsync()
        {
            var tempDataList = new List<string[]>();
            var batchSize = 50;
            var batchDelay = 50;

            while (true)
            {
                while (_queueCheckedCode.TryDequeue(out var result))
                {
                    tempDataList.Add(result);

                    if (tempDataList.Count >= batchSize)
                    {
                        var tempData = new List<string[]>(tempDataList);
                        tempDataList.Clear();

                        await Task.Run(() =>
                        {
                            foreach (var data in tempData)
                            {
                                _checkedObserHelper?.AddNewData(data);
                            }
                        });

                    }

                    await Task.Delay(1);
                }

                if (tempDataList.Count > 0)
                {
                    var tempData = new List<string[]>(tempDataList);
                    tempDataList.Clear();

                    await Task.Run(() =>
                    {
                        foreach (var data in tempData)
                        {
                            _checkedObserHelper?.AddNewData(data);
                        }
                    });


                }
                _timeBasedExecution.ExecuteActionIfAllowed(() => UpdateCheckedNumber());
                await Task.Delay(batchDelay);
            }
        }

        private void FirtLoadForChartPercent()
        {
            try
            {
                if (_currentJob == null) return;
                _currentJob.TotalChecked = _checkedObserHelper.TotalChecked.ToString();
                _currentJob.TotalPassed = _checkedObserHelper.TotalPassed.ToString();
                _currentJob.TotalFailed = _checkedObserHelper.TotalFailed.ToString();
                _currentJob.RaisePercentageChange(_currentJob);
            }
            catch (Exception)
            {
            }

        }

        private void UpdateCheckedNumber()
        {
            Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                TextBlockTotalChecked.Text = _checkedObserHelper.TotalChecked.ToString("N0");
                TextBlockTotalPassed.Text = _checkedObserHelper.TotalPassed.ToString("N0");
                TextBlockTotalFailed.Text = _checkedObserHelper.TotalFailed.ToString("N0");
                FirtLoadForChartPercent();
            });
        }

        private void CheckedData_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_currentJob == null || _checkedObserHelper.CheckedList == null || !_checkedObserHelper.CheckedList.Any()) return;
                CheckedInfo printInfo = new()
                {
                    list = new(_checkedObserHelper.CheckedList),
                    columnNames = _checkedObserHelper?.ColumnNames,
                    RawList = new(_dataList),
                    PodFormat = _currentJob.PODFormat,
                    CurrentJob = _currentJob,
                };
                if (_currentJob.CompareType != SharedProgram.DataTypes.CommonDataType.CompareType.Database)
                {
                    printInfo.RawList.Clear();
                    printInfo.PodFormat.Clear();
                }
                CheckedLogsWindow checkedLogsWindow = new(printInfo);
                checkedLogsWindow.ShowDialog();
            }
            catch (Exception)
            {
            }
        }

        private void PrintedData_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_currentJob == null || _printObserHelper?.PrintList == null || !_printObserHelper.PrintList.Any()) return;
                PrintingInfo printInfo = new()
                {
                    list = new(_printObserHelper.PrintList),
                    columnNames = _printObserHelper.ColumnNames,
                };

                PrintedLogsWindow printedLogsWindow = new(printInfo);
                printedLogsWindow.ShowDialog();
            }
            catch (Exception)
            {
            }
        }


    }
}
