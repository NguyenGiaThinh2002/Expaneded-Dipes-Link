using DipesLink.Languages;
using DipesLink.Models;
using DipesLink.Views.Converter;
using DipesLink.Views.Extension;
using SharedProgram.Models;
using SharedProgram.Shared;
using System.Collections.ObjectModel;
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
using static DipesLink.Views.Enums.ViewEnums;
using static SharedProgram.DataTypes.CommonDataType;
using Binding = System.Windows.Data.Binding;

namespace DipesLink.Views.SubWindows
{
    /// <summary>
    /// Interaction logic for JobLogsWindow.xaml
    /// </summary>
    public partial class CheckedLogsWindow : Window
    {

        public List<string[]>? CheckedResultList { get; set; }
        private string? _pageInfo;

        public int Num_TotalChecked { get; set; }
        public int Num_Printed { get; set; }
        public int Num_Verified { get; set; }
        public int Num_Valid { get; set; }
        public int Num_Failed { get; set; }

        private List<string>? _imageNameList;
        private readonly CheckedInfo? _printingInfo;
        private ObservableCollection<CheckedResultModel>? filterList = new();
        private Paginator<CheckedResultModel>? _paginator;
        private readonly int _maxDatabaseLine = 500;
        private JobOverview? _currentJob;
        private int countDataPerPage;


        public CheckedLogsWindow(CheckedInfo printingInfo)
        {
            _printingInfo = printingInfo;
            InitializeComponent();
            DataContext = _printingInfo.CurrentJob;
            ButtonRePrint.IsEnabled = 
                _printingInfo.CurrentJob.StatusStartButton && 
                _printingInfo.CurrentJob.PrinterSeries == SharedProgram.DataTypes.CommonDataType.PrinterSeries.RynanSeries &&
                _printingInfo.CurrentJob.JobType == SharedProgram.DataTypes.CommonDataType.JobType.AfterProduction; // Only use Re-Print for After Production
            ExportData_StackPanel.Visibility = _printingInfo.CurrentJob.CompareType == CompareType.Database ? Visibility.Visible : Visibility.Collapsed;
            InitPrintData();
            Closing += CheckedLogsWindow_Closing;
        }

        private void InitPrintData()
        {
            if (_printingInfo == null || _printingInfo.columnNames == null) return;
            try
            {
                filterList = _printingInfo.list;
                _currentJob = _printingInfo.CurrentJob;
                CreateDataTemplate();
                GetOriginalList();
                _imageNameList = GetImageNameList();
            }
            catch (Exception) { }
        }

        private void GetOriginalList()
        {
            if (_printingInfo == null || _printingInfo.list == null) return;
            try
            {
                _paginator = new Paginator<CheckedResultModel>(_printingInfo.list, _maxDatabaseLine);
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

                TextBlockTotal.Text = countTotal.ToString("N0");
                TextBlockValid.Text = countValid.ToString("N0");
                TextBlockVerified.Text = countVerified.ToString("N0");
                TextBlockFailed.Text = countFailed.ToString("N0");

                TextBlockUnk.Text = FindUnkList(_printingInfo.RawList.Skip(1).ToList(), _printingInfo.list).Count.ToString("N0");
            }
            catch (Exception)
            {
            }
        }

        private void CreateDataTemplate()
        {
            try
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
            catch (Exception)
            {

            }

        }

        public async Task LoadPageAsync(int pageNumber)
        {
            ImageLoadingJobLog.Visibility = Visibility.Visible;
            if (_paginator == null) return;
            await Task.Run(() =>
            {
                try
                {
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
            ImageLoadingJobLog.Visibility = Visibility.Collapsed;
        }

        private async Task CleanupResourcesAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _printingInfo?.list?.Clear();
                    _printingInfo?.RawList?.Clear();
                    CheckedResultList?.Clear();
                    _imageNameList?.Clear();
                    filterList?.Clear();
                    _paginator?.Dispose();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (Exception)
                {
                }
            });
        }

        private async void CheckedLogsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            await Task.Run(async () =>
            {
                await CleanupResourcesAsync();
                await Dispatcher.InvokeAsync(() =>
                {
                    Closing -= CheckedLogsWindow_Closing;
                    Close();
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
                string page = LanguageModel.GetLanguage("Page");
                _pageInfo = $"{page} {currentPage} / {_paginator?.TotalPages} ({countDataPerPage})";
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
              _ =  CusMsgBox.Show(LanguageModel.GetLanguage("PageNotFound"), 
                  LanguageModel.GetLanguage("WarningDialogCaption"), 
                  Enums.ViewEnums.ButtonStyleMessageBox.OK, 
                  Enums.ViewEnums.ImageStyleMessageBox.Warning);
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
                    case 7: //Camera
                        var listCamera = _printingInfo.list.Where(item => item.Device == Device.Camera.ToString());
                        filterList = new ObservableCollection<CheckedResultModel>(listCamera);
                        break;
                    case 8: //Scanner
                        var listScanner = _printingInfo.list.Where(item => item.Device == Device.BarcodeScanner.ToString());
                        filterList = new ObservableCollection<CheckedResultModel>(listScanner);
                        break;
                    default: break;
                }

                if (filterList != null)
                {
                    _paginator = new Paginator<CheckedResultModel>(new ObservableCollection<CheckedResultModel>(filterList), _maxDatabaseLine);
                    _ = LoadPageAsync(0);
                    UpdatePageInfo();
                    ButtonPaginationVis();
                    if (filterList?.Count <= 0)
                    {
                        Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            DataGridCheckedLog.ItemsSource = null;
                        });
                        _ = CusMsgBox.Show(
                            LanguageModel.GetLanguage("NotFound"),
                           LanguageModel.GetLanguage("WarningDialogCaption"), 
                            Enums.ViewEnums.ButtonStyleMessageBox.OK, 
                            Enums.ViewEnums.ImageStyleMessageBox.Warning);
                    }
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
                            DateTime = rawRecord[3],
                            Device = rawRecord[4],
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
            try
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
            catch (Exception)
            {
            }
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
                if (searchResults != null && searchResults?.Count > 0)
                {
                    _paginator = new Paginator<CheckedResultModel>(new ObservableCollection<CheckedResultModel>(searchResults), _maxDatabaseLine);
                    _ = LoadPageAsync(0);
                    UpdatePageInfo();
                    ButtonPaginationVis();
                }
                else
                {
                    CusMsgBox.Show(
                        LanguageModel.GetLanguage("NotFoundSearchResult"), 
                        LanguageModel.GetLanguage("WarningDialogCaption"), 
                        Enums.ViewEnums.ButtonStyleMessageBox.OK, 
                        Enums.ViewEnums.ImageStyleMessageBox.Warning);
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
            try
            {
                var dg = sender as DataGrid;
                if (dg?.SelectedItem == null) return "";
                if (dg.ItemContainerGenerator.ContainerFromItem(dg.SelectedItem) is DataGridRow row)
                {
                    DataGridCell cell = GetCell(dg, row, 0);
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
            catch (Exception)
            {
                return "";
            }
        }

        public static DataGridCell GetCell(DataGrid dataGrid, DataGridRow row, int column)
        {
            try
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
            catch (Exception)
            {
                return null;
            }

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
            var isReprint = CusMsgBox.Show(
                    LanguageModel.GetLanguage("ReprintConfirmation"),
                    LanguageModel.GetLanguage("InfoDialogCaption"),
                    ButtonStyleMessageBox.YesNo,
                    ImageStyleMessageBox.Info);
            if (isReprint.Result)
            {
                var currentJob = CurrentViewModel<JobOverview>();
                currentJob?.RaiseReprint(currentJob.Index);
                this.Close();
            }
         
        }

        private void ButtonRecheck_Click(object sender, RoutedEventArgs e)
        {
            // thinh dang lam
            var isRechecked = CusMsgBox.Show(
                    LanguageModel.GetLanguage("RecheckConfirmation"),
                    LanguageModel.GetLanguage("InfoDialogCaption"),
                    ButtonStyleMessageBox.YesNo,
                    ImageStyleMessageBox.Info);
            if(isRechecked.Result)
            {
                var currentJob = CurrentViewModel<JobOverview>();
                currentJob?.RaiseRecheck(currentJob.Index);
                this.Close();
            }      
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchAction();
            }
        }

        private void DataGridCheckedLog_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string imageId = GetValueCellInDataGrid(sender);
                GetCurrentImage(imageId);
                foreach (var item in DataGridCheckedLog.Items)
                {
                    DataGridRow row = (DataGridRow)DataGridCheckedLog.ItemContainerGenerator.ContainerFromItem(item);
                    if (row != null) row.DetailsVisibility = Visibility.Collapsed;
                }
                var dataRow = DataGridCheckedLog.SelectedItem as CheckedResultModel;
                if (dataRow != null && dataRow.Result != "Valid" && dataRow.Result != "Missed")
                {
                    DataGridRow selectedRow = (DataGridRow)DataGridCheckedLog.ItemContainerGenerator.ContainerFromItem(DataGridCheckedLog.SelectedItem);
                    if (selectedRow != null)
                    {
                        selectedRow.DetailsVisibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private async void ExportDataAsync()
        {
            try
            {
                if (_currentJob?.Name == null)
                {
                    return;
                }

                var confirmSaved = CusMsgBox.Show(LanguageModel.GetLanguage("ExportDataConfirmation"), 
                    "Export Checked Result", 
                    Enums.ViewEnums.ButtonStyleMessageBox.OKCancel, 
                    Enums.ViewEnums.ImageStyleMessageBox.Info);
                if (!confirmSaved.Result)
                {
                    return;
                }

                string docFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "DPLink_LogFiles",
                    $"Job{_currentJob.Index + 1}");

                string fileName = _currentJob?.Name + $"_Logs_{DateTime.Now:yyyyMMdd_hhmmss}.csv";

                if (!Directory.Exists(docFolderPath))
                {
                    Directory.CreateDirectory(docFolderPath);
                }

                if (fileName != null)
                {
                    string fullFilePath = Path.Combine(docFolderPath, fileName);
                    Task<bool> doneExportTask = Task.Run(() => { return ExportData(fullFilePath, _printingInfo?.RawList, _printingInfo?.list, _printingInfo?.PodFormat); });
                    if (!(await doneExportTask))
                    {

                    }
                    else
                    {
                        SharedFunctions.PrintConsoleMessage($"Job{_currentJob?.Index + 1} export successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
            }
        }

        public static bool ExportData(string fileName, List<string[]>? rawDatabaseList, ObservableCollection<CheckedResultModel>? checkedList, List<PODModel>? podList)
        {
            if (fileName == null || rawDatabaseList == null || checkedList == null || podList == null) return false;
            try
            {
                var headerColumns = rawDatabaseList.FirstOrDefault();
                var datas = rawDatabaseList.Skip(1).ToList();

                // Create a dictionary to count the number of occurrences of each ResultData with "Duplicated" status
                var duplicateCountDict = checkedList
                     .Where(x => x.Result == "Duplicated" && x.Device != Device.BarcodeScanner.ToString())
                    .GroupBy(x => x.ResultData)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Create a dictionary to store the first valid results with Device information
                var checkedResultDict = checkedList
                   .Where(x => x.Result == "Valid" && x.Device != Device.BarcodeScanner.ToString())
                    .GroupBy(x => x.ResultData)
                    .ToDictionary(g => g.Key, g => (DateTime: g.First().DateTime, Device: g.First().Device));

                if (File.Exists(fileName)) File.Delete(fileName);
                using (StreamWriter writer = new(fileName, true, Encoding.UTF8))
                {
                    string header = string.Join(",", headerColumns.Select(Csv.Escape)) + ",VerifyDate";
                    //string header = string.Join(",", headerColumns.Select(Csv.Escape)) + ",VerifyDate,Device";

                    writer.WriteLine(header);
                    for (int i = 0; i < datas.Count; i++)
                    {
                        var record = datas[i];
                        var compareString = SharedFunctions.GetCompareDataByPODFormat(record, podList);
                        var writeValue = string.Join(",", record.Take(record.Length - 1).Select(Csv.Escape)) + ",";

                        if (checkedResultDict.TryGetValue(compareString, out var checkResult))
                        {
                            var (dateVerify, device) = checkResult;
                            if (duplicateCountDict.TryGetValue(compareString, out int duplicateCount) && duplicateCount >= 1)
                            {
                                writeValue += "Printed-Duplicate";
                            }
                            else
                            {
                                writeValue += "Printed-Verified";
                            }
                            writeValue += "," + Csv.Escape(dateVerify);
                            checkedResultDict.Remove(compareString);
                        }
                        else
                        {
                            string tmpValue = record[^1];
                            writeValue += tmpValue == "Printed" ? "Printed-Unverified" : "Unprinted-Unverified";
                        }
                        writer.WriteLine(writeValue);
                    }
                }
                if (File.Exists(fileName))
                {
                    // Use Process.Start to open the folder and select the file
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer",
                        Arguments = $"/select,\"{fileName}\"",
                        UseShellExecute = true
                    });
                }
                else
                {
                    Console.WriteLine("File does not exist.");
                    return false;
                }
                checkedResultDict.Clear();
                return true;
            }
            catch (Exception ex)
            {
                SharedFunctions.PrintConsoleMessage(ex.Message);
                return false;
            }
        }

        private void ButtonExportData_Click(object sender, RoutedEventArgs e)
        {
            ExportDataAsync();
        }
        private void ButtonExportResult_Click(object sender, RoutedEventArgs e)
        {
            ExportCheckedResult();
        }
        public void ExportCheckedResult()
        {
            if (_currentJob?.Name == null)
            {
                return;
            }

            var confirmSaved = CusMsgBox.Show(LanguageModel.GetLanguage("ExportResultConfirmation"), "Export Checked Result", Enums.ViewEnums.ButtonStyleMessageBox.OKCancel, Enums.ViewEnums.ImageStyleMessageBox.Info);
            if (!confirmSaved.Result)
            {
                return;
            }

            string? selectedJobName = SharedFunctions.GetSelectedJobNameList(_currentJob.Index).FirstOrDefault();
            var SelectedJob = SharedFunctions.GetJobSelected(selectedJobName, _currentJob.Index);

            string sourceFilePath = SharedPaths.PathCheckedResult + $"Job{_currentJob?.Index + 1}\\" + SelectedJob.CheckedResultPath;
            var checkedNameFilePath = SharedPaths.PathCheckedResult + $"Job{_currentJob?.Index + 1}\\" + "checkedPathString";
            string checkedResultPath = SharedPaths.PathCheckedResult + $"Job{_currentJob?.Index + 1}\\" + SharedValues.SelectedJob.CheckedResultPath;
            //string path = SharedPaths.PathCheckedResult + $"Job{JobIndex + 1}\\" + selectedJob.CheckedResultPath;
            if (!string.IsNullOrEmpty(Path.GetFileName(sourceFilePath)))
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Select destination to save the copied file",
                    FileName = Path.GetFileName(sourceFilePath),
                    Filter = "All Files|*.*"
                };

                // Show the SaveFileDialog
                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    try
                    {
                        File.Copy(sourceFilePath, saveFileDialog.FileName, true);
                        Process.Start("explorer.exe", $"/select,\"{saveFileDialog.FileName}\"");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Result file does not exist", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }



    }
}
