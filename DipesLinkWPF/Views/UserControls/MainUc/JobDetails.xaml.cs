using Cloudtoid;
using DipesLink.Models;
using DipesLink.ViewModels;
using DipesLink.Views.Extension;
using DipesLink.Views.SubWindows;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DipesLink.Views.UserControls.MainUc
{
    /// <summary>
    /// Interaction logic for HomeUc.xaml
    /// </summary>
    public partial class JobDetails : UserControl
    {

        //  private PrintingDataTableHelper _printingDataTableHelper = new();
        private JobOverview? _currentJob;
        private readonly ConcurrentQueue<string[]> _queueCheckedCode = new();
        private PrintObserHelper? _PrintObserHelper;
        private CheckedObserHelper _checkedObserHelper = new();
        private readonly ConcurrentQueue<string[]> _queuePrintedCode = new();
        private readonly CancellationTokenSource _ctsGetPrintedCode = new();


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
            if (sender == null) return;
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
                // _printingDataTableHelper?.Dispose();
                //   _printingDataTableHelper = new();
                InitValues();

                DataGridDB.ItemsSource = null;
                DataGridDB.Columns.Clear();

                DataGridResult.ItemsSource = null;
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
            if (_currentJob == null) return;
            ViewModelSharedEvents.OnJobDetailChangeHandler(_currentJob.Index);
            if (!_currentJob.IsDBExist)
            {
                //  Debug.WriteLine("Event load database was called: " + _currentJob.Index);
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
                            var dataList = dbList.FirstOrDefault().Item1;
                            var currentPage = dbList.FirstOrDefault().Item2;
                            _PrintObserHelper = new(dataList, currentPage, DataGridDB);
                            _currentJob.PrintedDataNumber = _PrintObserHelper.PrintedNumber.ToString();
                            var vm = CurrentViewModel<JobOverview>();
                            if (vm != null)
                            {
                                vm.IsShowLoadingDB = Visibility.Collapsed;
                                ViewModelSharedEvents.OnEnableUIChangeHandler(vm.Index, true);
                            }
                            ViewModelSharedEvents.OnDataTableLoadingHandler();
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
                    UpdateCheckedNumber();
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
                                _PrintObserHelper?.CheckAndUpdateStatusAsync(data);
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
                            _PrintObserHelper?.CheckAndUpdateStatusAsync(data);
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

                    UpdateCheckedNumber();
                }

                await Task.Delay(batchDelay);
            }
        }

        private void FirtLoadForChartPercent()
        {
            if (_currentJob == null) return;
            _currentJob.TotalChecked = _checkedObserHelper.TotalChecked.ToString();
            _currentJob.TotalPassed = _checkedObserHelper.TotalPassed.ToString();
            _currentJob.TotalFailed = _checkedObserHelper.TotalFailed.ToString();
            _currentJob.RaisePercentageChange(_currentJob.Index);
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

        private void ViewChekedResult_PreMouDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
            //    _checkedObserHelper.CheckedList
                if (_currentJob == null) return;

                //CheckedLogsWindow jobLogsWindow = new(_checkedObserHelper)
                //{
                //    DataContext = DataContext as JobOverview,
                //    Num_TotalChecked = _currentJob.TotalRecDb
                //};

                //if (int.TryParse(_currentJob.PrintedDataNumber, out int printed))
                //{
                //    jobLogsWindow.Num_Printed = printed;
                //}

                //if (int.TryParse(TextBlockTotalChecked.Text, out int totalChecked))
                //{
                //    jobLogsWindow.Num_Verified = totalChecked;
                //    if (int.TryParse(TextBlockTotalFailed.Text, out int failed))
                //    {
                //        jobLogsWindow.Num_Failed = failed;
                //        jobLogsWindow.Num_Valid = totalChecked - failed;
                //    }
                //}
                //jobLogsWindow.ShowDialog();
            }
            catch (Exception)
            {
            }
        }

        private void PrintedData_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_currentJob == null) return;
                PrintingInfo printInfo = new()
                {
                    list = _PrintObserHelper?.PrintList,
                    columnNames = _PrintObserHelper.ColumnNames,
                };

                PrintedLogsWindow printedLogsWindow = new(printInfo);
                printedLogsWindow.ShowDialog();
            }
            catch (Exception)
            {
            }
        }

        private void ButtonSimulate_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
