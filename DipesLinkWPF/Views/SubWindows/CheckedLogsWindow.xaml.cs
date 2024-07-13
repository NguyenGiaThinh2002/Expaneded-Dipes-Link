using DipesLink.Models;
using DipesLink.Views.Converter;
using DipesLink.Views.Extension;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DipesLink.Views.SubWindows
{
    /// <summary>
    /// Interaction logic for JobLogsWindow.xaml
    /// </summary>
    public partial class CheckedLogsWindow : Window
    {
        private JobLogsDataTableHelper? _jobLogsDataTableHelper = new();
        private CheckedObserHelper? _checkedObserHelper = new();
        public List<string[]>? CheckedResultList { get; set; }
        public DataTable? CheckedDataTable { get; set; }
        private string? _pageInfo;

        public int Num_TotalChecked { get; set; }
        public int Num_Printed { get; set; }
        public int Num_Verified { get; set; }
        public int Num_Valid { get; set; }
        public int Num_Failed { get; set; }
        private string[] _columnNames = Array.Empty<string>();
        private List<string>? _imageNameList;
        private PrintingInfo? _printingInfo;
        private ObservableCollection<ExpandoObject>? filterList = new();
        private Paginator? _paginator;
        private int _MaxDatabaseLine = 500;




        public CheckedLogsWindow(PrintingInfo printingInfo)
        {
            _printingInfo = printingInfo;
            InitializeComponent();
            InitPrintData(); 
         //   Loaded += JobLogsWindow_LoadedAsync;
         //  Closing += JobLogsWindow_Closing;
        }

        private void InitPrintData()
        {
            if (_printingInfo == null || _printingInfo.columnNames == null) return;
            try
            {
                filterList = _printingInfo.list;
                _columnNames = _printingInfo.columnNames;
                CreateDataTemplate();
                GetOriginalList();
            }
            catch (Exception)
            {

            }
        }
        private void GetOriginalList()
        {
            if (_printingInfo == null || _printingInfo.list == null) return;
            try
            {
                _paginator = new Paginator(_printingInfo.list, _MaxDatabaseLine);
                _ = LoadPageAsync(0);
                UpdatePageInfo();
                UpdateNumber();


            }
            catch (Exception)
            {

            }
        }

        private void UpdateNumber()
        {
            try
            {
                if (_printingInfo == null || _printingInfo.list == null || _paginator == null) return;
                TextBlockTotal.Text = _printingInfo.list.Count.ToString();
                int countPrinted = _printingInfo.list.Count(item => ((IDictionary<string, object>)item)["Status"].ToString() == "Printed");
                int countWaiting = _printingInfo.list.Count(item => ((IDictionary<string, object>)item)["Status"].ToString() == "Waiting");
                TextBlockPrinted.Text = countPrinted.ToString();
              //  TextBlockWait.Text = countWaiting.ToString();
            }
            catch (Exception)
            {
            }
        }

        private int countDataPerPage;
        public async Task LoadPageAsync(int pageNumber)
        {
          //  ImageLoadingPrintedLog.Visibility = Visibility.Visible;
            if (_paginator == null) return;
            await Task.Run(() =>
            {
                try
                {
                    // Thread.Sleep(5000); test load symbol
                    if (pageNumber < 0 || pageNumber >= _paginator.TotalPages) return;
                    ObservableCollection<ExpandoObject> pageData = _paginator.GetPage(pageNumber);
                    countDataPerPage = pageData.Count;
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        DataGridCheckedLog.ItemsSource = pageData;
                        UpdatePageInfo();
                        ButtonPaginationVis();
                    });

                }
                catch (Exception)
                {

                }

            });
          //  ImageLoadingPrintedLog.Visibility = Visibility.Collapsed;
        }

        private void CreateDataTemplate()
        {
            DataGridCheckedLog.Columns.Clear();
            foreach (var columnName in _columnNames)
            {
                if (columnName == "Status")
                {
                    DataGridTemplateColumn templateColumn = new()
                    {
                        Header = columnName,
                        Width = DataGridLength.Auto
                    };
                    DataTemplate template = new();
                    FrameworkElementFactory factory = new(typeof(Image));
                    Binding binding = new(columnName)
                    {
                        Converter = new StatusToIconConverter(),
                        Mode = BindingMode.OneWay
                    };
                    factory.SetValue(Image.SourceProperty, binding);
                    factory.SetValue(Image.HeightProperty, 20.0);
                    factory.SetValue(Image.WidthProperty, 20.0);
                    template.VisualTree = factory;
                    templateColumn.CellTemplate = template;
                    DataGridCheckedLog.Columns.Add(templateColumn);
                }
                else
                {
                    DataGridTextColumn textColumn = new()
                    {
                        Header = columnName,
                        Binding = new Binding(columnName),
                        Width = DataGridLength.Auto
                    };
                    DataGridCheckedLog.Columns.Add(textColumn);
                }
            }
        }




        private async Task CleanupResourcesAsync()
        {
            await Task.Run(() =>
            {
                if (CheckedDataTable is not null)
                {
                    CheckedDataTable.Clear();
                    CheckedDataTable.Dispose();
                    CheckedDataTable = null;
                }

                if (CheckedResultList is not null)
                {
                    CheckedResultList.Clear();
                    CheckedResultList = null;
                }
                if (_jobLogsDataTableHelper != null)
                {
                    _jobLogsDataTableHelper.Dispose();
                    _jobLogsDataTableHelper = null;
                }

                if (_imageNameList is not null)
                {
                    _imageNameList.Clear();
                    _imageNameList = null;
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
        }


        private async void JobLogsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Cancel the default close operation
            await Task.Run(async () =>
            {
                await CleanupResourcesAsync();
                Dispatcher.Invoke(() =>  // Close the window on the UI thread after cleanup
                {
                    Closing -= JobLogsWindow_Closing; // Prevent re-entry
                    Close(); // Now close the window
                });
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
        private async void JobLogsWindow_LoadedAsync(object sender, RoutedEventArgs e)
        {
            //Stopwatch? stopwatch = Stopwatch.StartNew();
            //ImageLoadingJobLog.Visibility = Visibility.Visible;
            //if (_jobLogsDataTableHelper == null) return;
            //await Task.Run(()=> { CheckedDataTable = _checkedObserHelper?.GetDataTableDBAsync().Result.Copy(); });
            //if (CheckedDataTable != null)
            //{
            //    await _jobLogsDataTableHelper.InitDatabaseAsync(CheckedDataTable, DataGridResult);
            //}
            //UpdateParams();
            //UpdatePageInfo();
            //_imageNameList = GetImageNameList();
            //ImageLoadingJobLog.Visibility = Visibility.Hidden;
            //stopwatch.Stop();
            //Debug.Write($"Time loaded checked data: {stopwatch.ElapsedMilliseconds} ms\n");
            //stopwatch = null;
        }
       
        private void UpdateParams()
        {
            try
            {
                TextBlockTotal.Text = Num_TotalChecked.ToString("N0"); // Total 
                TextBlockPrinted.Text = Num_Printed.ToString("N0"); // Printed
                TextBlockVerified.Text = Num_Verified.ToString("N0"); // Verified
                TextBlockValid.Text = Num_Valid.ToString("N0"); // Valid
                TextBlockFailed.Text = Num_Failed.ToString("N0"); // Failed
            }
            catch (Exception)
            {
            }
           
        }

        private void UpdatePageInfo()
        {
            _pageInfo = string.Format("Page {0} of {1} ({2} items)", _jobLogsDataTableHelper?.Paginator?.CurrentPage + 1, _jobLogsDataTableHelper?.Paginator?.TotalPages, _jobLogsDataTableHelper?.NumberItemInCurPage);
            TextBlockPageInfo.Text = _pageInfo;
            TextBoxPage.Text = (_jobLogsDataTableHelper?.Paginator?.CurrentPage + 1).ToString(); // Get current page for Textbox Page
        }

        private async void PageAction_Click(object sender, RoutedEventArgs e)
        {
            //if (_jobLogsDataTableHelper == null || _jobLogsDataTableHelper.Paginator == null) return;
            //Button button = (Button)sender;
            //switch (button.Name)
            //{
            //    case "ButtonFirst":
            //        await _jobLogsDataTableHelper.UpdateDataGridAsync(DataGridResult, 1);
            //        break;

            //    case "ButtonBack":
            //        if (_jobLogsDataTableHelper.Paginator.PreviousPage())
            //        {
            //            await _jobLogsDataTableHelper.UpdateDataGridAsync(DataGridResult);
            //        }
            //        break;

            //    case "ButtonNext":
            //        if (_jobLogsDataTableHelper.Paginator.NextPage())
            //        {
            //           await  _jobLogsDataTableHelper.UpdateDataGridAsync(DataGridResult);
            //        }
            //        break;

            //    case "ButtonEnd":
            //       await _jobLogsDataTableHelper.UpdateDataGridAsync(DataGridResult, _jobLogsDataTableHelper.Paginator.TotalPages);
            //        break;

            //    case "ButtonGotoPage":
            //        GotoPageAction();
            //        break;
            //    default:
            //        break;
            //}
            //ButtonPaginationVis();
            //UpdatePageInfo();
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

        /// <summary>
        /// Goto Page Number Function
        /// </summary>
        private async void GotoPageAction()
        {
            //if (_jobLogsDataTableHelper == null || _jobLogsDataTableHelper.Paginator == null) return;
            //if (int.TryParse(TextBoxPage.Text, out int page))
            //{
            //    if (page > 0 && page <= _jobLogsDataTableHelper.Paginator.TotalPages)
            //    {
            //       await  _jobLogsDataTableHelper.UpdateDataGridAsync(DataGridResult, page);
            //    }
            //    else
            //    {
            //        MessageBox.Show("Page not found", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Page not found", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            //}
        }

        private void ComboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_jobLogsDataTableHelper == null || _jobLogsDataTableHelper.Paginator == null) return;
            ComboBox cbb = (ComboBox)sender;
            JobLogsDataTableHelper.FilteredKeyword type = JobLogsDataTableHelper.FilteredKeyword.All;
            switch (cbb.SelectedIndex)
            {
                case 0: // All
                    type = JobLogsDataTableHelper.FilteredKeyword.All;
                    break;
                case 1: // Valid
                    type = JobLogsDataTableHelper.FilteredKeyword.Valid;
                    break;
                case 2: //Invalid
                    type = JobLogsDataTableHelper.FilteredKeyword.Invalided;
                    break;
                case 3: //Duplicated
                    type = JobLogsDataTableHelper.FilteredKeyword.Duplicated;
                    break;
                case 4: // Null
                    type = JobLogsDataTableHelper.FilteredKeyword.Null;
                    break;
                case 5: //Missed
                    type = JobLogsDataTableHelper.FilteredKeyword.Missed;
                    break;
                case 6: //Failed
                    type = JobLogsDataTableHelper.FilteredKeyword.Failed;
                    break;
                default: break;
            }
           
          //  _jobLogsDataTableHelper.DatabaseFilteredAsync(DataGridResult, type);
            UpdatePageInfo();
            ButtonPaginationVis();
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchAction(TextBoxSearch.Text);
        }
      
        private void ButtonRF_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Text = "";
            SearchAction("");
        }

        private async void SearchAction(string keyword)
        {
            //if (_jobLogsDataTableHelper == null || _jobLogsDataTableHelper.Paginator == null) return;
            //await _jobLogsDataTableHelper.DatabaseSearchAsync(DataGridResult, keyword);
            //UpdatePageInfo();
            //ButtonPaginationVis();
        }

        private void DataGridResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //string imageId = GetValueCellInDataGrid(sender);
            //GetCurrentImage(imageId);

            //// Close all detail
            //foreach (var item in DataGridResult.Items)
            //{
            //    DataGridRow row = (DataGridRow)DataGridResult.ItemContainerGenerator.ContainerFromItem(item);
            //    if (row != null) row.DetailsVisibility = Visibility.Collapsed;
            //}

            //// Show Detail for Failed result in datagrid
            //var dataRowView = DataGridResult.SelectedItem as DataRowView;
            //if (dataRowView != null && dataRowView["Result"].ToString() != "Valid")
            //{
            //    DataGridRow selectedRow = (DataGridRow)DataGridResult.ItemContainerGenerator.ContainerFromItem(DataGridResult.SelectedItem);
            //    if (selectedRow != null) selectedRow.DetailsVisibility = Visibility.Visible;
            //}
               
        }

        private void GetCurrentImage(string imageId)
        {
            try
            {
                var firstLenght = imageId.Length;
                for (int i = 0; i < 7 - firstLenght; i++)
                {
                    imageId = "0" + imageId; // max 7 number
                }

                var curJob = CurrentViewModel<JobOverview>();
                if (curJob != null)
                {
                    string? imgFileName = _imageNameList?.Find(x => x.Contains(imageId));
                    if(imgFileName == null) { curJob.PathOfFailedImage = "pack://application:,,,/Images/Image_Not_Found.jpg"; return; }
                    string? imgPath =
                           curJob.ImageExportPath +
                           $"Job{curJob.Index + 1}\\" +
                           curJob.Name + "\\" +
                           imgFileName;
                    curJob.PathOfFailedImage = imgPath;
                    curJob.NameOfFailedImage = imgFileName;
                }
            }
            catch (Exception){}
        }

        private List<string> GetImageNameList()
        {
            try
            {
                var cur = CurrentViewModel<JobOverview>();
                string folderPath = 
                    CurrentViewModel<JobOverview>()?.ImageExportPath+
                    $"Job{CurrentViewModel<JobOverview>()?.Index + 1}\\" + 
                    CurrentViewModel<JobOverview>()?.Name;

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var dir = new DirectoryInfo(folderPath);
                string strFileNameExtension = string.Format("*{0}", "jpg");
                FileInfo[] files = dir.GetFiles(strFileNameExtension); //Getting Text files
                var result = new List<string>();
                foreach (FileInfo file in files)
                {
                    result.Add(file.Name);
                }
                result.Sort((a, b) => b.CompareTo(a));
                return result;
            }
            catch (Exception)
            {
                return new List<string>(0);
            }
        }

        #region Get Cell Value
        private string GetValueCellInDataGrid(object sender)
        {
            var dg = sender as DataGrid;
            if (dg?.SelectedItem == null) return "";
            // Assuming you want to access the first column's value
            if (dg.ItemContainerGenerator.ContainerFromItem(dg.SelectedItem) is DataGridRow row)
            {
                DataGridCell cell = GetCell(dg, row, 0); // 0 for the first column
                if (cell != null)
                {
                    if (cell.Content is TextBlock cellContent)
                    {
                        return cellContent.Text.Trim();
                    }
                }
            }
            return "";
        }

        public DataGridCell GetCell(DataGrid dataGrid, DataGridRow row, int column)
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
            //// Close all detail
            //foreach (var item in DataGridResult.Items)
            //{
            //    DataGridRow row = (DataGridRow)DataGridResult.ItemContainerGenerator.ContainerFromItem(item);
            //    if (row != null) row.DetailsVisibility = Visibility.Collapsed;
            //}
        }

        private void ButtonRePrint_Click(object sender, RoutedEventArgs e)
        {
            var currentJob = CurrentViewModel<JobOverview>();
            currentJob?.RaiseReprint(currentJob.Index);
        }

        private void Search_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchAction(TextBoxSearch.Text);
            }
           
        }

    }
}
