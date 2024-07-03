using Cloudtoid;
using DipesLink.Models;
using DipesLink.Views.Converter;
using DipesLink.Views.Extension;
using SharedProgram.Shared;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

namespace DipesLink.Views.SubWindows
{
    /// <summary>
    /// Interaction logic for JobLogsWindow.xaml
    /// </summary>
    public partial class PrintedLogsWindow : Window
    {
        JobLogsDataTableHelper? _jobLogsDataTableHelper = new();
        public DataTable? PrintedDataTable { get; set; } = new();
        private string? _pageInfo;
        private JobOverview? _currentJob;

        public PrintedLogsWindow(JobOverview currentJob)
        {
            _currentJob = currentJob;
            InitializeComponent();
            Loaded += PrintedLogsWindow_Loaded;
            this.Closing += PrintedLogsWindow_Closing;
        }



        private async void PrintedLogsWindow_Closing(object? sender, CancelEventArgs e)
        {
            e.Cancel = true; // Cancel the default close operation
           await Task.Run(async () =>
            {
                await CleanupResourcesAsync();
                Dispatcher.Invoke(() =>  // Close the window on the UI thread after cleanup
                {
                    Closing -= PrintedLogsWindow_Closing; // Prevent re-entry
                    Close(); // Now close the window
                });
            });
        }

        private async Task CleanupResourcesAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (PrintedDataTable != null)
                    {
                        PrintedDataTable?.Clear();
                        PrintedDataTable?.Dispose();
                        PrintedDataTable = null;
                    }

                    if (_jobLogsDataTableHelper != null)
                    {
                        _jobLogsDataTableHelper.Dispose();
                        _jobLogsDataTableHelper = null;
                    }

                    // thinh comment lai
                    _currentJob = null;
                    GC.Collect();
                    //GC.WaitForPendingFinalizers();
                }
                catch (Exception)
                {

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

        public void CreateDataTemplate(DataGrid dataGrid)
        {
            if (PrintedDataTable is null || PrintedDataTable.Columns is null || PrintedDataTable.Rows is null) return;
            try
            {
                dataGrid.Columns.Clear();
                foreach (DataColumn column in PrintedDataTable.Columns)
                {
                    if (column.ColumnName == "Status") 
                    {
                        DataGridTemplateColumn templateColumn = new() { Header = column.ColumnName, Width = DataGridLength.Auto };
                        DataTemplate template = new();
                        FrameworkElementFactory factory = new(typeof(Image));

                        Binding binding = new(column.ColumnName)
                        {
                            Converter = new StatusToIconConverter(),
                            Mode = BindingMode.OneWay
                        };

                        factory.SetValue(Image.SourceProperty, binding);
                        factory.SetValue(Image.HeightProperty, 20.0);
                        factory.SetValue(Image.WidthProperty, 20.0);

                        template.VisualTree = factory;
                        templateColumn.CellTemplate = template;
                        dataGrid.Columns.Add(templateColumn);
                    }
                    else
                    {
                        DataGridTextColumn textColumn = new()
                        {
                            Header = column.ColumnName,
                            Binding = new Binding(column.ColumnName),
                            Width = DataGridLength.Auto
                        };
                        dataGrid.Columns.Add(textColumn);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static async Task<DataTable> InitDatabaseAsync(List<string[]> dbList)
        {
            var printedDataTable = new DataTable();
            if (dbList == null || dbList.IsEmpty())
            {
                return printedDataTable;
            }
            await Task.Run(() =>
            {
                foreach (var header in dbList[0]) // add column
                {
                    printedDataTable.Columns.Add(header);
                }
                for (int i = 1; i < dbList.Count; i++) // add Row data
                {
                    printedDataTable.Rows.Add(dbList[i]);
                }
                return printedDataTable;
            });
            return printedDataTable;
        }

        private void UpdateNumber()
        {
            try
            {
                TextBlockTotal.Text = PrintedDataTable?.Rows.Count.ToString();
                TextBlockPrinted.Text = _currentJob?.PrintedDataNumber;
                if(int.TryParse(TextBlockTotal.Text, out int total) && int.TryParse(TextBlockPrinted.Text, out int printed))
                {
                    TextBlockWait.Text = (total - printed).ToString();
                }
            }
            catch (Exception)
            {

            }
           
        }

        private async void PrintedLogsWindow_Loaded(object sender, RoutedEventArgs e)
        {
           

            // thinh
            ImageLoadingPrintedLog.Visibility = Visibility.Visible;
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Start both independent tasks
            var createTemplateTask = Task.Run(() => CreateDataTemplate(DataGridResult));
            Task<List<string[]>> printedStatusTask = InitDatabaseAndPrintedStatusAsync();

            // Wait for the printed status to be fetched before initializing the database with it
            List<string[]> printedStatus = await printedStatusTask;
            PrintedDataTable = await InitDatabaseAsync(printedStatus);

            // Wait for the template creation to finish if it hasn't already
            await createTemplateTask;

            // Proceed only if the DataTableHelper is initialized and the PrintedDataTable is not null
            if (_jobLogsDataTableHelper != null && PrintedDataTable != null)
            {
                // Initialize database asynchronously and update UI concurrently
                var dbInitTask = _jobLogsDataTableHelper.InitDatabaseAsync(PrintedDataTable, DataGridResult);
                await dbInitTask; // Ensure database initialization is completed before finishing
                ImageLoadingPrintedLog.Visibility = Visibility.Hidden;
            }
            UpdateNumber();
            UpdatePageInfo();
            ImageLoadingPrintedLog.Visibility = Visibility.Hidden;
            stopwatch.Stop();
            Debug.WriteLine($"Time loaded printed data: {stopwatch.ElapsedMilliseconds} ms");
        }

        private void UpdatePageInfo()
        {
            _pageInfo = string.Format("Page {0} of {1} ({2} items)", _jobLogsDataTableHelper?.Paginator?.CurrentPage + 1, _jobLogsDataTableHelper?.Paginator?.TotalPages, _jobLogsDataTableHelper?.NumberItemInCurPage);
            TextBlockPageInfo.Text = _pageInfo;
            TextBoxPage.Text = (_jobLogsDataTableHelper?.Paginator?.CurrentPage + 1).ToString(); // Get current page for Textbox Page
        }

        private async void PageAction_Click(object sender, RoutedEventArgs e)
        {
            if (_jobLogsDataTableHelper == null || _jobLogsDataTableHelper.Paginator == null) return;
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "ButtonFirst":
                    await _jobLogsDataTableHelper.UpdateDataGridAsync(DataGridResult, 1);
                    break;

                case "ButtonBack":
                    if (_jobLogsDataTableHelper.Paginator.PreviousPage())
                    {
                       await _jobLogsDataTableHelper.UpdateDataGridAsync(DataGridResult);
                    }
                    break;

                case "ButtonNext":
                    if (_jobLogsDataTableHelper.Paginator.NextPage())
                    {
                        await _jobLogsDataTableHelper.UpdateDataGridAsync(DataGridResult);
                    }
                    break;

                case "ButtonEnd":
                    await _jobLogsDataTableHelper.UpdateDataGridAsync(DataGridResult, _jobLogsDataTableHelper.Paginator.TotalPages);
                    break;

                case "ButtonGotoPage":
                    GotoPageAction();
                    break;
                default:
                    break;
            }
            ButtonPaginationVis();
            UpdatePageInfo();
        }

        private void ButtonPaginationVis()
        {
            if (_jobLogsDataTableHelper == null || _jobLogsDataTableHelper.Paginator == null) return;

            //Button Next - End
            if (_jobLogsDataTableHelper.Paginator.CurrentPage + 1 == _jobLogsDataTableHelper.Paginator.TotalPages)
            {
                ButtonNext.IsEnabled = false;
                ButtonEnd.IsEnabled = false;
            }
            else
            {
                ButtonNext.IsEnabled = true;
                ButtonEnd.IsEnabled = true;
            }

            // Button Back - First
            if (_jobLogsDataTableHelper.Paginator.CurrentPage + 1 == 1)
            {
                ButtonBack.IsEnabled = false;
                ButtonFirst.IsEnabled = false;
            }
            else
            {
                ButtonBack.IsEnabled = true;
                ButtonFirst.IsEnabled = true;
            }
        }

        private async void GotoPageAction()
        {
            if (_jobLogsDataTableHelper == null || _jobLogsDataTableHelper.Paginator == null) return;
            if (int.TryParse(TextBoxPage.Text, out int page))
            {
                if (page > 0 && page <= _jobLogsDataTableHelper.Paginator.TotalPages)
                {
                   await _jobLogsDataTableHelper.UpdateDataGridAsync(DataGridResult, page);
                }
                else
                {
                    MessageBox.Show("Page not found", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Page not found", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void ComboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_jobLogsDataTableHelper == null || _jobLogsDataTableHelper.Paginator == null) return;
            ComboBox cbb = (ComboBox)sender;
            JobLogsDataTableHelper.FilteredKeyword type = JobLogsDataTableHelper.FilteredKeyword.All;
            switch (cbb.SelectedIndex)
            {
                case 0: 
                    type = JobLogsDataTableHelper.FilteredKeyword.All;
                    break;
                case 1: 
                    type = JobLogsDataTableHelper.FilteredKeyword.Waiting;
                    break;
                case 2: 
                    type = JobLogsDataTableHelper.FilteredKeyword.Printed;
                    break;
                default: break;
            }

            await _jobLogsDataTableHelper.DatabaseFilteredForPrintStatusAsync(DataGridResult, type);
           
            UpdatePageInfo();
            ButtonPaginationVis();
        }

        private async void ButtonSearch_ClickAsync(object sender, RoutedEventArgs e)
        {
            await SearchActionAsync();
        }

        private async Task SearchActionAsync()
        {
            if (_jobLogsDataTableHelper == null || _jobLogsDataTableHelper.Paginator == null) return;
            await _jobLogsDataTableHelper.DatabaseSearchForPrintStatusAsync(DataGridResult, TextBoxSearch.Text);
            
            // Updates the user interface after completing a search
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdatePageInfo();
                ButtonPaginationVis();
            });
        }

        private async void ButtonRF_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Text = "";
            await SearchActionAsync();
        }

        #region Get Cell Value
       
        public static DataGridCell? GetCell(DataGrid dataGrid, DataGridRow row, int column)
        {
            if (row != null)
            {
                DataGridCellsPresenter presenter = FindVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null)
                {
                    dataGrid.ScrollIntoView(row, dataGrid.Columns[column]);
                    presenter = FindVisualChild<DataGridCellsPresenter>(row);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }
        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
        #endregion

        private void DataGridResult_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Close all detail
            foreach (var item in DataGridResult.Items)
            {
                DataGridRow row = (DataGridRow)DataGridResult.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null) row.DetailsVisibility = Visibility.Collapsed;
            }
        }

        private void ButtonRePrint_Click(object sender, RoutedEventArgs e)
        {
            var currentJob = CurrentViewModel<JobOverview>();
            currentJob?.RaiseReprint(currentJob.Index);
        }

        private async Task<List<string[]>> InitDatabaseAndPrintedStatusAsync()
        {
            var path = SharedPaths.PathSubJobsApp + $"{_currentJob.Index + 1}\\" + "printedPathString";
            var pathstr = SharedFunctions.ReadStringOfPrintedResponePath(path);

            // Get path db and printed list
            var pathDatabase = _currentJob?.DatabasePath;
            var pathBackupPrintedResponse = SharedPaths.PathPrintedResponse + $"Job{_currentJob?.Index + 1}\\" + pathstr;

            // Init Databse from file, add index column, status column, and string "Feild"
            List<string[]> tmp = await Task.Run(() => { return SharedFunctions.InitDatabaseWithStatus(pathDatabase); });

            // Update Printed status
            if (pathstr != "" && File.Exists(_currentJob?.DatabasePath) && tmp.Count > 1)
            {
                await Task.Run(() => { SharedFunctions.InitPrintedStatus(pathBackupPrintedResponse, tmp); });
            }
            return tmp;
        }

      

       

        private async void TextBoxSearch_KeyDownAsync(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
               await SearchActionAsync();
            }
        }
    }
}
