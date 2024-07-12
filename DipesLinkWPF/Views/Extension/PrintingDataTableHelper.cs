using DipesLink.Models;
using DipesLink.ViewModels;
using DipesLink.Views.Converter;
using DipesLink.Views.Extension;
using DipesLink.Views;
using SharedProgram.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

public class PrintingDataTableHelper : ViewModelBase, IDisposable
{
    public Paginator? Paginator { get; set; }
    public JobOverview? CurrentViewModel { get; private set; }
    public MainWindow? CurrentWindow { get; set; }
    private List<string[]>? _orgDBList;
    public int PrintedNumber { get; set; }
    private int _rowIndex;
    public event EventHandler? OnDetectMissPrintedCode;
    public ObservableCollection<DynamicDataRowViewModel>? PrintedDataCollection { get => printedDataCollection;
        private set  { printedDataCollection = value; OnPropertyChanged(); } }
    private OptimizedSearch? _optimizedSearch;

    public async Task InitDatabaseAsync(List<string[]> dbList, DataGrid dataGrid, int currentPage, JobOverview? currentViewModel)
    {
        if (dbList is null || dbList.Count == 0) return;
        var headers = dbList[0];
        PrintedDataCollection = new ObservableCollection<DynamicDataRowViewModel>();
        await Task.Run(() =>
        {
            try
            {
                for (int i = 1; i < dbList.Count; i++)
                {
                    var row = new DynamicDataRowViewModel(headers);
                    for (int j = 0; j < headers.Length; j++)
                    {
                        row[headers[j]] = dbList[i][j];
                    }
                    PrintedDataCollection.Add(row);
                }
                _orgDBList = dbList;
                if (_orgDBList != null)
                {
                    InitializeOptimizedSearch(_orgDBList);
                }
                CounterPrintedFirstLoad(PrintedDataCollection);
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }
        });

        // Update UI after data collection is loaded
        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                if (currentViewModel == null) return;
                CurrentViewModel = currentViewModel;
                dataGrid.AutoGenerateColumns = false; // Disable auto-generation of columns
                dataGrid.Columns.Clear();
                SetupDataGridColumns(dataGrid, PrintedDataCollection);
               // Paginator = new Paginator(PrintedDataCollection, 500); // Set page size to 500
                dataGrid.ItemsSource = Paginator.GetPage(currentPage); // Set the ItemsSource to the current page
                currentViewModel.IsShowLoadingDB = Visibility.Collapsed;
                ViewModelSharedEvents.OnEnableUIChangeHandler(currentViewModel.Index, true);
                ViewModelSharedEvents.OnDataTableLoadingHandler();
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }
        });
    }

    private void CounterPrintedFirstLoad(ObservableCollection<DynamicDataRowViewModel> dataCollection)
    {
        PrintedNumber = dataCollection.Count(row => row["Status"]?.ToString() == "Printed");
    }

    public void InitializeOptimizedSearch(List<string[]> dbList)
    {
        _optimizedSearch = new OptimizedSearch(dbList);
    }

    private ConcurrentQueue<string[]> _updateQueue = new();
    private DateTime _lastUpdateTime = DateTime.MinValue;
    private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(50);

    public void ChangeStatusOnDataGrid(string[] printedCode, JobOverview? curViewModel, DataGrid dataGrid)
    {
        // Batches Update
        _updateQueue.Enqueue(printedCode);
        var now = DateTime.UtcNow;
        if (now - _lastUpdateTime < _updateInterval)
        {
            return;
        }
        _lastUpdateTime = now;

        // Start Batches Update
        ProcessQueue(dataGrid, curViewModel);
    }

    private void SetupDataGridColumns(DataGrid dataGrid, ObservableCollection<DynamicDataRowViewModel> dataCollection)
    {
        if (dataCollection.Count > 0)
        {
            foreach (var columnName in dataCollection[0].Keys)
            {
                if (columnName == "Status")
                {
                    DataGridTemplateColumn templateColumn = new() { Header = columnName, Width = DataGridLength.Auto };
                    DataTemplate template = new();
                    FrameworkElementFactory factory = new(typeof(Image)); // Create Image UI by Code behind instead XAML
                    Binding binding = new Binding($"[{columnName}]") { Converter = new StatusToIconConverter() };

                    factory.SetValue(Image.SourceProperty, binding); // Set binding for Image.
                    factory.SetValue(Image.HeightProperty, 20.0); // Image Height
                    factory.SetValue(Image.WidthProperty, 20.0);  // Image Width

                    template.VisualTree = factory; // add UI to VisualTree Template
                    templateColumn.CellTemplate = template; // CellTemplate = Template
                    dataGrid.Columns.Add(templateColumn); // Add DataGridTemplateColumn
                }
                else if (columnName != "Key") // Exclude "Key" column
                {
                    DataGridTextColumn textColumn = new()
                    {
                        Header = columnName,
                        Binding = new Binding($"[{columnName}]"),
                        Width = 100
                    };
                    dataGrid.Columns.Add(textColumn);
                }
            }
        }
    }

    private void ProcessQueue(DataGrid dataGrid, JobOverview? curViewModel)
    {
        if (!_updateQueue.IsEmpty)
        {
            while (_updateQueue.TryDequeue(out var printedCode))
            {
                UpdateDataGrid(printedCode, dataGrid, curViewModel);
                Thread.Sleep(1);
            }
        }
    }

    public async void UpdateDataGridAsync(DataGrid dataGrid)
    {
        if (Paginator != null)
        {
            if (CurrentViewModel == null) return;
            try
            {
                // Lấy trang hiện tại từ Paginator và cập nhật DataCollection
                var currentPageData = await Task.Run(() => Paginator.GetPage(Paginator.CurrentPage));
                Application.Current.Dispatcher.Invoke(() =>  // Update UI Flow
                {
                    CurrentViewModel.DataCollection.Clear();
                    foreach (var item in currentPageData)
                    {
                      //  CurrentViewModel.DataCollection.Add(item);
                    }
                    dataGrid.ItemsSource = CurrentViewModel.DataCollection;
                });
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }
        }
    }

    private void UpdateDataGrid(string[] printedCode, DataGrid dataGrid, JobOverview? curViewModel)
    {
        try
        {
            if (curViewModel == null)
            {
                return;
            }

            string rowIdentifier = printedCode[0];
            string newStatus = printedCode[^1];
            int _rowIndexTotal = 0;

            if (_optimizedSearch != null)
            {
                _rowIndexTotal = _optimizedSearch.FindIndexByData(printedCode);
            }

            if (Paginator == null)
            {
                return;
            }

            int currentPage = Paginator.GetCurrentPageNumber(_rowIndexTotal);

            if (Paginator != null && currentPage != Paginator.CurrentPage)
            {
                Paginator.CurrentPage = currentPage;
                UpdateDataGridAsync(dataGrid);  // Cập nhật DataGrid với trang mới
            }

            DynamicDataRowViewModel targetRow = null;
            _rowIndex = -1;
            for (int i = 0; i < curViewModel.DataCollection.Count; i++)
            {
                var row = curViewModel.DataCollection[i];
                if (row.ContainsKey("Index") && row["Index"]?.ToString() == rowIdentifier)
                {
                    row["Status"] = newStatus;
                    targetRow = row;
                    _rowIndex = i;
                    break;
                }
            }

            if (targetRow != null)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ScrollIntoView(_rowIndex, dataGrid);
                    // dataGrid.Items.Refresh(); // Refresh DataGrid to reflect changes
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }
        catch (Exception ex)
        {
            SharedFunctions.PrintConsoleMessage(ex.Message);
        }
    }

    public static void ScrollIntoView(int rowIndex, DataGrid dataGrid)
    {
        try
        {
            if (rowIndex >= 0 && rowIndex < dataGrid.Items.Count)
            {
                dataGrid.ScrollIntoView(dataGrid.Items[rowIndex]);
                dataGrid.SelectedIndex = rowIndex;

                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
                    if (row != null)
                    {
                        row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }
        catch (Exception ex)
        {
            SharedFunctions.PrintConsoleMessage(ex.Message);
        }
    }

    private bool disposedValue = false;
    private ObservableCollection<DynamicDataRowViewModel>? printedDataCollection;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                try
                {
                    Paginator?.Dispose();
                    _orgDBList?.Clear();
                    PrintedDataCollection?.Clear();
                    _optimizedSearch?.Dispose();
                    Paginator = null;
                    _orgDBList = null;
                }
                catch (Exception ex)
                {
                    SharedFunctions.PrintConsoleMessage(ex.Message);
                }
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
