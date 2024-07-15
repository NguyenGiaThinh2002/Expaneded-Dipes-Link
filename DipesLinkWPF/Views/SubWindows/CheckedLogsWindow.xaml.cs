using DipesLink.Models;
using DipesLink.Views.Converter;
using DipesLink.Views.Extension;
using SharedProgram.Shared;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Security.Cryptography.Pkcs;
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
        //  private JobLogsDataTableHelper? _jobLogsDataTableHelper = new();
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
        private CheckedInfo? _printingInfo;
        private ObservableCollection<CheckedResultModel>? filterList = new();
        private Paginator<CheckedResultModel>? _paginator;
        private int _MaxDatabaseLine = 500;
        private JobOverview? _currentJob;



        public CheckedLogsWindow(CheckedInfo printingInfo)
        {
            _printingInfo = printingInfo;
            InitializeComponent();
            DataContext = _printingInfo.CurrentJob;
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
                _currentJob = _printingInfo.CurrentJob;
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
                _paginator = new Paginator<CheckedResultModel>(_printingInfo.list, _MaxDatabaseLine);
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
                if (_printingInfo == null || _printingInfo.RawList == null || _printingInfo.list == null || _paginator == null) return;

                int countTotal = _printingInfo.RawList.Skip(1).ToList().Count;
                int countValid = _printingInfo.list.Count(item => item.Result == "Valid");
                int countInvalided = _printingInfo.list.Count(item => item.Result == "Invalided");
                int countDuplicated = _printingInfo.list.Count(item => item.Result == "Duplicated");
                int countNull = _printingInfo.list.Count(item => item.Result == "Null");
                int countVerified = _printingInfo.list.Count(item => item.Result != "Missed");
                int countFailed = _printingInfo.list.Count(item => item.Result != "Valid");
                int countMissed = countTotal - countVerified;

                TextBlockTotal.Text = countTotal.ToString();
                TextBlockValid.Text = countValid.ToString();
                TextBlockVerified.Text = countVerified.ToString();
                TextBlockFailed.Text = countFailed.ToString();
                TextBlockUnk.Text = countMissed.ToString();

            }
            catch (Exception)
            {
            }
        }

        private void CreateDataTemplate()
        {
            DataGridCheckedLog.AutoGenerateColumns = false;
            DataGridCheckedLog.Columns.Clear();
            var properties = typeof(CheckedResultModel).GetProperties();
            foreach (var property in properties)
            {
                if (property.Name == "Result")
                {
                    DataGridTemplateColumn templateColumn = new() { Header = property.Name, Width = DataGridLength.Auto };
                    DataTemplate template = new();
                    FrameworkElementFactory factory = new(typeof(Image));

                    Binding binding = new(property.Name)
                    {
                        Converter = new ResultCheckedImgConverter(),
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
                        Header = property.Name,
                        Binding = new Binding(property.Name),
                        Width = DataGridLength.Auto
                    };
                    DataGridCheckedLog.Columns.Add(textColumn);
                }
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
                    ObservableCollection<CheckedResultModel> pageData = _paginator.GetPage(pageNumber);
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

        private async Task CleanupResourcesAsync()
        {
            //await Task.Run(() =>
            //{
            //    if (CheckedDataTable is not null)
            //    {
            //        CheckedDataTable.Clear();
            //        CheckedDataTable.Dispose();
            //        CheckedDataTable = null;
            //    }

            //    if (CheckedResultList is not null)
            //    {
            //        CheckedResultList.Clear();
            //        CheckedResultList = null;
            //    }
            //    if (_jobLogsDataTableHelper != null)
            //    {
            //        _jobLogsDataTableHelper.Dispose();
            //        _jobLogsDataTableHelper = null;
            //    }

            //    if (_imageNameList is not null)
            //    {
            //        _imageNameList.Clear();
            //        _imageNameList = null;
            //    }

            //    GC.Collect();
            //    GC.WaitForPendingFinalizers();
            //});
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
                TextBlockUnk.Text = Num_Printed.ToString("N0"); // Printed
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
            if (_paginator == null) return;
            try
            {
                var currentPage = (_paginator?.CurrentPage + 1);
                _pageInfo = string.Format("Page {0} / {1} ({2})", currentPage, _paginator?.TotalPages, countDataPerPage);
                TextBlockPageInfo.Text = _pageInfo;
                TextBoxPage.Text = currentPage.ToString();
            }
            catch (Exception)
            {

            }
        }

        private async void PageAction_Click(object sender, RoutedEventArgs e)
        {
            if (_paginator == null) return;
            try
            {
                Button button = (Button)sender;
                switch (button.Name)
                {
                    case "ButtonFirst":
                        await LoadPageAsync(0);
                        break;

                    case "ButtonBack":
                        if (_paginator.CurrentPage > 0)
                            await LoadPageAsync(--_paginator.CurrentPage);
                        break;

                    case "ButtonNext":
                        if (_paginator.CurrentPage < _paginator.TotalPages - 1)
                            await LoadPageAsync(++_paginator.CurrentPage);
                        break;

                    case "ButtonEnd":
                        await LoadPageAsync(_paginator.TotalPages - 1);
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
            catch (Exception)
            {

            }
        }

        private void ButtonPaginationVis()
        {
            if (_paginator == null) return;
            try
            {
                //Button Next - End
                if (_paginator.CurrentPage + 1 == _paginator.TotalPages)
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
                if (_paginator.CurrentPage + 1 == 1)
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
            catch (Exception)
            {

            }
        }


        private async void GotoPageAction()
        {
            if (_paginator == null) return;
            try
            {
                if (int.TryParse(TextBoxPage.Text, out int page))
                {
                    if (page > 0 && page <= _paginator.TotalPages)
                    {
                        await LoadPageAsync(page - 1);
                        return;
                    }
                }
                CusMsgBox.Show("Page not found !", "Goto Page", Enums.ViewEnums.ButtonStyleMessageBox.OK, Enums.ViewEnums.ImageStyleMessageBox.Warning);
            }
            catch (Exception)
            {
            }
        }

        private void ComboBoxFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_printingInfo?.list == null || _paginator == null) return;
            try
            {
                ComboBox cbb = (ComboBox)sender;
                switch (cbb.SelectedIndex)
                {
                    case 0: // All
                        filterList = _printingInfo.list;
                        break;
                    case 1: // Valid
                        var listValid = _printingInfo.list.Where(item => item.Result == "Valid");
                        filterList = new ObservableCollection<CheckedResultModel>(listValid);
                        break;
                    case 2: // Inva
                        var listInvalided = _printingInfo.list.Where(item => item.Result == "Invalided");
                        filterList = new ObservableCollection<CheckedResultModel>(listInvalided);
                        break;
                    case 3: // Dup
                        var listDuplicated = _printingInfo.list.Where(item => item.Result == "Duplicated");
                        filterList = new ObservableCollection<CheckedResultModel>(listDuplicated);
                        break;
                    case 4: // null
                        var listNull = _printingInfo.list.Where(item => item.Result == "Null");
                        filterList = new ObservableCollection<CheckedResultModel>(listNull);
                        break;
                    case 5: //Unk
                        filterList = FindUnkList(_printingInfo.RawList.Skip(1).ToList(), _printingInfo.list);
                        break;
                    case 6: //Fail
                        var listFailed = _printingInfo.list.Where(item => item.Result != "Valid");
                        filterList = new ObservableCollection<CheckedResultModel>(listFailed);
                        break;
                    default: break;
                }

                if (filterList != null && filterList?.Count > 0)
                {
                    _paginator = new Paginator<CheckedResultModel>(new ObservableCollection<CheckedResultModel>(filterList), _MaxDatabaseLine);
                    _ = LoadPageAsync(0);
                    UpdatePageInfo();
                    ButtonPaginationVis();

                }

            }
            catch (Exception)
            {

            }
        }

        public ObservableCollection<CheckedResultModel> FindUnkList(List<string[]> rawList, ObservableCollection<CheckedResultModel> checkedList)
        {
            if (rawList == null || checkedList == null) return new ObservableCollection<CheckedResultModel>();
            try
            {
                var duplicateCountDict = checkedList
                    .Where(x => x.Result == "Duplicated")
                    .GroupBy(x => x.ResultData)
                    .ToDictionary(g => g.Key, g => g.Count());

                var checkedResultDict = checkedList
                    .Where(x => x.Result == "Valid")
                    .GroupBy(x => x.ResultData)
                    .ToDictionary(g => g.Key, g => g.First().Result);

                var resultList = new ObservableCollection<CheckedResultModel>();
                foreach (var rawRecord in rawList)
                {
                    var resultData = SharedFunctions.GetCompareDataByPODFormat(rawRecord, _printingInfo.PodFormat);
                    if (!checkedResultDict.ContainsKey(resultData) && !duplicateCountDict.ContainsKey(resultData))
                    {
                        resultList.Add(new CheckedResultModel()
                        {
                            Index = rawRecord[0],
                            ResultData = resultData,
                            Result = "Missed",
                            ProcessingTime = rawRecord[2],
                            DateTime = rawRecord[3]
                        });
                    }
                }
                return resultList;
            }
            catch (Exception)
            {
                return new ObservableCollection<CheckedResultModel>();
            }
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchAction();
        }

        private void ButtonRF_Click(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Text = string.Empty;
            if (ComboBoxFilter.SelectedIndex == 0)
            {
                GetOriginalList();
            }
            else
            {
                ComboBoxFilter.SelectedIndex = 0;
            }

            ButtonPaginationVis();
        }

        private void SearchAction()
        {
            if (_printingInfo == null || _printingInfo.list == null || _paginator?.SourceData == null) return;
            try
            {
                var searchValue = TextBoxSearch.Text;
                var searchResults = filterList?.Where(item => item.GetType().GetProperties()
                    .Any(prop => prop.GetValue(item)?.ToString().Contains(searchValue) == true))
                    .ToList();
                if (searchResults != null && searchResults?.Count> 0)
                {
                    _paginator = new Paginator<CheckedResultModel>(new ObservableCollection<CheckedResultModel>(searchResults), _MaxDatabaseLine);
                    _ = LoadPageAsync(0);

                    UpdatePageInfo();
                    ButtonPaginationVis();

                }
                else
                {
                    CusMsgBox.Show("No results were found !", "Search", Enums.ViewEnums.ButtonStyleMessageBox.OK, Enums.ViewEnums.ImageStyleMessageBox.Warning);
                }
            }
            catch (Exception)
            {

            }
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

               // var curJob = CurrentViewModel<JobOverview>();
                if (_currentJob != null)
                {
                    string? imgFileName = _imageNameList?.Find(x => x.Contains(imageId));
                    if (imgFileName == null) { _currentJob.PathOfFailedImage = "pack://application:,,,/Images/Image_Not_Found.jpg"; return; }
                    string? imgPath =
                           _currentJob.ImageExportPath +
                           $"Job{_currentJob.Index + 1}\\" +
                           _currentJob.Name + "\\" +
                           imgFileName;
                    _currentJob.PathOfFailedImage = imgPath;
                    _currentJob.NameOfFailedImage = imgFileName;
                }
            }
            catch (Exception) { }
        }

        private List<string> GetImageNameList()
        {
            try
            {
                var cur = CurrentViewModel<JobOverview>();
                string folderPath =
                    CurrentViewModel<JobOverview>()?.ImageExportPath +
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

       

        private void ButtonRePrint_Click(object sender, RoutedEventArgs e)
        {
            var currentJob = CurrentViewModel<JobOverview>();
            currentJob?.RaiseReprint(currentJob.Index);
        }

        private void Search_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchAction();
            }

        }

     

        private bool isRowDetailShow = false;
        private void DataGridCheckedLog_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string imageId = GetValueCellInDataGrid(sender);
            GetCurrentImage(imageId);

            // Close all detail
            foreach (var item in DataGridCheckedLog.Items)
            {
                DataGridRow row = (DataGridRow)DataGridCheckedLog.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null) row.DetailsVisibility = Visibility.Collapsed;
            }

            // Show Detail for Failed result in datagrid
            var dataRow = DataGridCheckedLog.SelectedItem as CheckedResultModel;
            if (dataRow != null && dataRow.Result != "Valid" && dataRow.Result != "Missed")
            {
                DataGridRow selectedRow = (DataGridRow)DataGridCheckedLog.ItemContainerGenerator.ContainerFromItem(DataGridCheckedLog.SelectedItem);
                if (selectedRow != null && !isRowDetailShow)
                {
                    selectedRow.DetailsVisibility = Visibility.Visible;
                    isRowDetailShow = true;
                }
                else
                {
                    selectedRow.DetailsVisibility = Visibility.Collapsed;
                    isRowDetailShow = false ;
                }
                
            }
        }
    }
}
