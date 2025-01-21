using DipesLink.Languages;
using DipesLink.Models;
using DipesLink.ViewModels;
using DipesLink.Views.Converter;
using DipesLink.Views.Extension;
using SharedProgram.Shared;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TAlex.WPF.Helpers;

namespace DipesLink.Views.SubWindows
{
    public partial class PrintedLogsWindow : Window
    {

        #region Declarations
        private string? _pageInfo;
        private readonly int _maxDatabaseLine = 500;
        private readonly PrintingInfo? _printingInfo;
        private string[] _columnNames = Array.Empty<string>();
        private Paginator<ExpandoObject>? _paginator;
        private int countDataPerPage;
        private ObservableCollection<ExpandoObject>? filterList = new();
        private static int selectedPrinter = 1;
        #endregion Declarations

        #region Functions

        public PrintedLogsWindow(PrintingInfo printingInfo)
        {
            _printingInfo = printingInfo;
            InitializeComponent();
            InitPrintData();
            LoadUIPrinter();
            this.Closing += PrintedLogsWindow_Closing;
        }
        private void LoadUIPrinter()
        {
            if (ViewModelSharedValues.Settings.NumberOfPrinter > 1)
            {
                ButtonItemsControl.ItemsSource = Enumerable.Range(1, ViewModelSharedValues.Settings.NumberOfPrinter).Select(n => n.ToString()).ToList();
                PrinterSelection.Visibility = Visibility.Visible;
            }
            else
            {
                PrinterSelection.Visibility = Visibility.Collapsed;
            }
        }

        private void TemplateLoadClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in ButtonItemsControl.Items)
            {
                var container = ButtonItemsControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                if (container != null)
                {
                    var button = FindVisualChild<Button>(container);
                    if (button != null)
                    {
                        button.Tag = null; // Clear selection
                    }
                }
            }

            // Cast the sender to a Button
            if (sender is Button clickedButton && int.TryParse(clickedButton.Content.ToString(), out int printerNumber))
            {
                selectedPrinter = printerNumber;
                if (clickedButton.Tag == null || clickedButton.Tag.ToString() != "Selected")
                {
                    clickedButton.Tag = "Selected";  // Set as selected
                }
                else
                {
                    clickedButton.Tag = null;  // Deselect
                }
                ReLoadSelectedPrinterData();
            }
            else
            {
                MessageBox.Show("Unknown template selected.");
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

        private void ReLoadSelectedPrinterData()
        {
            try
            {              
                int? currentIndex = _printingInfo.CurrentJob.Index;
                var printedNameFilePath = SharedPaths.PathSubJobsApp + $"{currentIndex + 1}\\" + "printedPathString";

                var namePrintedFilePath = SharedPaths.PathPrintedResponse + $"Job{currentIndex + 1}\\"
                    + SharedFunctions.ReadStringOfPrintedResponePath(printedNameFilePath);

                string subPrinterPath = namePrintedFilePath;

                subPrinterPath = $"{(namePrintedFilePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ? namePrintedFilePath[..^4] : namePrintedFilePath)}_Printer_{selectedPrinter}.csv";

                //if (selectedPrinter > 1)
                //{
                //    subPrinterPath = $"{(namePrintedFilePath.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ? namePrintedFilePath[..^4] : namePrintedFilePath)}_Printer_{selectedPrinter}.csv";
                //}

                string csvPath = subPrinterPath;

                var rawDatabaseList = GetRawDatabaseListForPrinterReponses(csvPath, _printingInfo?.RawList);

                filterList = rawDatabaseList;
                _printingInfo.list = rawDatabaseList;

                CreateDataTemplate();
                GetOriginalList();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Export SubPrinter Data Error " + ex.Message);
            }
        }

        public static ObservableCollection<ExpandoObject> GetRawDatabaseListForPrinterReponses(string pathBackupPrinted, List<string[]> rawNewDatabase)
        {
            //List<string[]> rawDatabase = rawNewDatabase;
            List<string[]> rawDatabase = rawNewDatabase
                                                    .Select(array => array.ToArray())
                                                    .ToList();
            if (selectedPrinter > 1) 
            {
                for (int i = 1; i < rawDatabase.Count; i++)
                {
                    rawDatabase[i][^1] = "Waiting"; // Update the last element of each line except the first one
                }
            }
          

            ObservableCollection<ExpandoObject> rawDatabaseList = new ObservableCollection<ExpandoObject>();

            if (rawDatabase.Count <= 1) return rawDatabaseList; 

            var headers = rawDatabase[0]; // The first line will be used as keys

            for (int i = 0; i < rawDatabase.Count; i++)
            {
                // Create a new ExpandoObject for each row
                dynamic expando = new ExpandoObject();
                var dictionary = (IDictionary<string, object>)expando;

                var row = rawDatabase[i];
                for (int j = 0; j < headers.Length; j++)
                {
                    if (j < row.Length)
                    {
                        dictionary[headers[j]] = row[j];
                    }
                }

                rawDatabaseList.Add(expando);
            }

            if (!File.Exists(pathBackupPrinted))
            {
                rawDatabaseList.RemoveAt(0); // Remove the header
                return rawDatabaseList;
            }

            try
            {
                using FileStream fs = new(pathBackupPrinted, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                using StreamReader reader = new(fs, Encoding.UTF8, true);

                string[] lines = reader.ReadToEnd().Split(Environment.NewLine);
                if (lines.Length < 2) return rawDatabaseList; // If there are less than 2 lines, there's nothing to process

                // Skip the first line (header) and process the rest in parallel
                Parallel.For(1, lines.Length, i =>
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) return;
                    string line = lines[i];
                    string[] columns = line.Split(',');
                    if (columns.Length > 0)
                    {
                        string indexString = Csv.Unescape(columns[0]);
                        if (int.TryParse(indexString, out int index) && index < rawDatabaseList.Count)
                        {
                            // Update the last column of the corresponding row to "Printed"
                            dynamic expando = rawDatabaseList[index];
                            var dictionary = (IDictionary<string, object>)expando;
                            dictionary[headers[^1]] = "Printed";
                        }
                    }
                });
                rawDatabaseList.RemoveAt(0); // Remove the header

                return rawDatabaseList;
            }
            catch (IOException)
            {
                return rawDatabaseList;
            }
            catch (Exception)
            {
                return rawDatabaseList;
            }
    }


        private void GetOriginalList()
        {
            if (_printingInfo == null || _printingInfo.list == null) return;
            try
            {
                _paginator = new Paginator<ExpandoObject>(_printingInfo.list, _maxDatabaseLine);
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
                TextBlockTotal.Text = _printingInfo.list.Count.ToString("N0");
                int countPrinted = _printingInfo.list.Count(item => ((IDictionary<string, object>)item)["Status"].ToString() == "Printed");
                int countWaiting = _printingInfo.list.Count(item => ((IDictionary<string, object>)item)["Status"].ToString() == "Waiting");
                TextBlockPrinted.Text = countPrinted.ToString("N0");
                TextBlockWait.Text = countWaiting.ToString("N0");
            }
            catch (Exception)
            {
            }
        }

        private void CreateDataTemplate()
        {
            DataGridPrintLog.Columns.Clear();
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
                    DataGridPrintLog.Columns.Add(templateColumn);
                }
                else
                {
                    DataGridTextColumn textColumn = new()
                    {
                        Header = columnName,
                        Binding = new Binding(columnName),
                        Width = DataGridLength.Auto
                    };
                    DataGridPrintLog.Columns.Add(textColumn);
                }
            }
        }

        public async Task LoadPageAsync(int pageNumber)
        {
            ImageLoadingPrintedLog.Visibility = Visibility.Visible;
            if (_paginator == null) return;
            await Task.Run(() =>
            {
                try
                {
                    if (pageNumber < 0 || pageNumber >= _paginator.TotalPages) return;
                    ObservableCollection<ExpandoObject> pageData = _paginator.GetPage(pageNumber);
                    countDataPerPage = pageData.Count;
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        DataGridPrintLog.ItemsSource = pageData;
                        UpdatePageInfo();
                        ButtonPaginationVis();
                    });

                }
                catch (Exception)
                {

                }

            });
            ImageLoadingPrintedLog.Visibility = Visibility.Collapsed;
        }

        private async void PrintedLogsWindow_Closing(object? sender, CancelEventArgs e)
        {
            e.Cancel = true;
            await Task.Run(async () =>
            {
                await CleanupResourcesAsync();
                await Dispatcher.InvokeAsync(() =>
                {
                    Closing -= PrintedLogsWindow_Closing;
                    Close();
                });
            });
        }

        private async Task CleanupResourcesAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _printingInfo?.list?.Clear();
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

        private void UpdatePageInfo()
        {
            if (_paginator == null) return;
            try
            {

                var currentPage = (_paginator?.CurrentPage + 1);
                string page = LanguageModel.GetLanguage("Page");
                _pageInfo = $"{page} {currentPage} / {_paginator?.TotalPages} ({countDataPerPage})";
                //_pageInfo = string.Format("Page {0} / {1} ({2})", currentPage, _paginator?.TotalPages, countDataPerPage);
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
                _ = CusMsgBox.Show(LanguageModel.GetLanguage("PageNotFound"), 
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
                    case 0:
                        filterList = _printingInfo.list;
                        break;
                    case 1:
                        filterList = new ObservableCollection<ExpandoObject>(_printingInfo.list.Where(item => ((IDictionary<string, object>)item)["Status"].ToString() == "Waiting"));
                        break;
                    case 2:
                        filterList = new ObservableCollection<ExpandoObject>(_printingInfo.list.Where(item => ((IDictionary<string, object>)item)["Status"].ToString() == "Printed"));
                        break;
                    default: break;
                }

                if (filterList != null)
                {
                    _paginator = new Paginator<ExpandoObject>(new ObservableCollection<ExpandoObject>(filterList), _maxDatabaseLine);
                    _ = LoadPageAsync(0);
                    UpdatePageInfo();
                    ButtonPaginationVis();
                    if (filterList?.Count <= 0)
                    {
                        Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            DataGridPrintLog.ItemsSource = null;
                        });
                        _ = CusMsgBox.Show(LanguageModel.GetLanguage("NotFound"),
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

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchAction();

        }

        private void SearchAction()
        {
            if (_printingInfo == null || _printingInfo.list == null || _paginator?.SourceData == null) return;
            try
            {
                var searchValue = TextBoxSearch.Text;
                var searchResults = filterList?.Where(item =>
                {

                    var expandoDict = (IDictionary<string, object>)item;
                    return expandoDict.Values.Any(value => value != null && value.ToString() == searchValue);
                });

                if (searchResults != null && searchResults?.Count() > 0)
                {
                    _paginator = new Paginator<ExpandoObject>(new ObservableCollection<ExpandoObject>(searchResults), _maxDatabaseLine);
                    _ = LoadPageAsync(0);
                    UpdatePageInfo();
                    ButtonPaginationVis();
                }
                else
                {
                    CusMsgBox.Show(LanguageModel.GetLanguage("NotFoundSearchResult"), 
                        LanguageModel.GetLanguage("WarningDialogCaption"),
                        Enums.ViewEnums.ButtonStyleMessageBox.OK, 
                        Enums.ViewEnums.ImageStyleMessageBox.Warning);
                }
            }
            catch (Exception)
            {

            }
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

        private void ButtonRePrint_Click(object sender, RoutedEventArgs e)
        {
            var currentJob = CurrentViewModel<JobOverview>();
            currentJob?.RaiseReprint(currentJob.Index);
        }

        private void TextBoxSearch_KeyDownAsync(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchAction();
            }
        }

        #endregion Functions

    }
}
