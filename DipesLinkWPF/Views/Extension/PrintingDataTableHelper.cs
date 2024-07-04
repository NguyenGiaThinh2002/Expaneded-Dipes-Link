using Cloudtoid;
using DipesLink.Models;
using DipesLink.ViewModels;
using DipesLink.Views.Converter;
using SharedProgram.Shared;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DipesLink.Views.Extension
{
    public class PrintingDataTableHelper : IDisposable
    {
        public Paginator? Paginator { get; set; }
        public JobOverview? CurrentViewModel { get; private set; }
        public MainWindow CurrentWindow { get; set; }
        private List<string[]>? _orgDBList;
        public int PrintedNumber { get; set; }
        private int _rowIndex;
        public event EventHandler? OnDetectMissPrintedCode;
        public DataTable? PrintedDataTable { get; private set; }
        private OptimizedSearch? _optimizedSearch;

        public async Task InitDatabaseAsync(List<string[]> dbList, DataGrid dataGrid, int currentPage, JobOverview? currentViewModel)
        {
            if (dbList is null || dbList.IsEmpty()) return;
            PrintedDataTable = new();
            await Task.Run(() =>
            {
                try
                {
                    foreach (var header in dbList[0]) // add column
                    {
                        PrintedDataTable.Columns.Add(header);
                    }
                    for (int i = 1; i < dbList.Count; i++) // add Row data
                    {
                        PrintedDataTable.Rows.Add(dbList[i]);
                    }
                    _orgDBList = dbList;
                    if (_orgDBList != null)
                    {
                        InitializeOptimizedSearch(_orgDBList);
                    }
                    CounterPrintedFirstLoad(PrintedDataTable);
                }
                catch (Exception ex)
                {
                    SharedFunctions.PrintDebugMessage(ex.Message);
                }
            });

            // Update UI after datatable loaded
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    if (currentViewModel == null) return;
                    CurrentViewModel = currentViewModel;
                    dataGrid.Columns.Clear();
                    ProcessMiniPageAsync(dataGrid, PrintedDataTable, currentPage);

                    currentViewModel.IsShowLoadingDB = Visibility.Collapsed;
                 //   currentViewModel.IsStartButtonEnable = true;
                    ViewModelSharedEvents.OnEnableUIChangeHandler(currentViewModel.Index, true);
                    //  currentViewModel.EnableUI = true;
                    ViewModelSharedEvents.OnDataTableLoadingHandler();

                }
                catch (Exception ex)
                {
                    SharedFunctions.PrintDebugMessage(ex.Message);
                }
            });
        }

        private void CounterPrintedFirstLoad(DataTable dataTable)
        {
            PrintedNumber = dataTable.Select("Status = 'Printed'").Length;
        }

        public void InitializeOptimizedSearch(List<string[]> dbList)
        {
            _optimizedSearch = new OptimizedSearch(dbList);
        }

        public async void ChangeStatusOnDataGrid(string[] printedCode, JobOverview? currentViewModel, DataGrid dataGrid)
        {
            try
            {
                if (currentViewModel == null) 
                {
                    return;
                }
                string rowIdentifier = printedCode[0];
                string newStatus = printedCode[^1];
                CurrentViewModel = currentViewModel;
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
                    await UpdateDataGridAsync(dataGrid);
                }

                foreach (DataRow row in CurrentViewModel.MiniDataTable.Rows)
                {
                    _rowIndex = CurrentViewModel.MiniDataTable.Rows.IndexOf(row);
                    if (row[0].ToString() == rowIdentifier)
                    {
                        row["Status"] = newStatus;
                        if (newStatus != "Printed")
                        {
                            //RaiseDetectMissPrintedCode();
                        }
                        break;
                    }
                }
                ScrollIntoView(_rowIndex, dataGrid);
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintDebugMessage(ex.Message);
            }
        }

        public async void ProcessMiniPageAsync(DataGrid dataGrid, DataTable dataTable, int currentPage)
        {
            Paginator = new Paginator(dataTable);
            if (Paginator == null) return;
            Paginator.CurrentPage = currentPage;
            DataTable? pageTable = null;
            try
            {
                pageTable = await Task.Run(() => Paginator.GetPage(Paginator.CurrentPage));
                foreach (DataColumn column in pageTable.Columns)
                {
                    if (column.ColumnName == "Status")
                    {
                        DataGridTemplateColumn templateColumn = new() { Header = column.ColumnName, Width = DataGridLength.Auto };
                        DataTemplate template = new();
                        FrameworkElementFactory factory = new(typeof(Image)); // Create Image UI by Code behind instead XAML
                        Binding binding = new(column.ColumnName) { Converter = new StatusToIconConverter() };

                        factory.SetValue(Image.SourceProperty, binding); // Set binding for Image.
                        factory.SetValue(Image.HeightProperty, 20.0); // Image Height
                        factory.SetValue(Image.WidthProperty, 20.0);  // Image Width

                        template.VisualTree = factory; // add UI to VisualTree Template
                        templateColumn.CellTemplate = template; // CellTemplate = Template
                        dataGrid.Columns.Add(templateColumn); // Add DataGridTemplateColumn
                    }
                    else
                    {
                        DataGridTextColumn textColumn = new()
                        {
                            Header = column.ColumnName,
                            Binding = new Binding(column.ColumnName),
                            Width = 100
                        };
                        dataGrid.Columns.Add(textColumn);
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                pageTable?.Dispose();
            }

            await UpdateDataGridAsync(dataGrid);
        }

        public async Task UpdateDataGridAsync(DataGrid dataGrid)
        {
            if (Paginator != null)
            {
                if (CurrentViewModel == null) return;
                try
                {
                    CurrentViewModel.MiniDataTable = await Task.Run(() => Paginator.GetPage(Paginator.CurrentPage));
                    Application.Current.Dispatcher.Invoke(() =>  //Update UI Flow
                    {
                        dataGrid.AutoGenerateColumns = false;
                        dataGrid.ItemsSource = CurrentViewModel.MiniDataTable.DefaultView;
                    });
                }
                catch (Exception ex)
                {
                    SharedFunctions.PrintDebugMessage(ex.Message);
                }
                finally
                {
                    CurrentViewModel.MiniDataTable?.Dispose();
                }
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

                    dataGrid.Dispatcher.InvokeAsync(() =>
                    {
                        DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex);
                        row?.Focus();
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintDebugMessage(ex.Message);
            }
        }


        private bool disposedValue = false;

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
                        PrintedDataTable?.Clear();
                        PrintedDataTable?.Dispose();
                        _optimizedSearch?.Dispose();
                        Paginator = null;
                        _orgDBList = null;
                        
                    }
                    catch (Exception ex)
                    {
                        SharedFunctions.PrintDebugMessage(ex.Message);
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // gọi nếu sử dụng dispose trực tiếp và nói với Finalizer đừng xếp việc thu gom rác vào hàng đợi cho đối tượng này
        }
    }
}
