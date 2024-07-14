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
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Dynamic;
using TAlex.WPF.Helpers;
using System.DirectoryServices;

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
        private int _MaxDatabaseLine = 500;
        private PrintingInfo? _printingInfo;
        private string[] _columnNames = Array.Empty<string>();
        private Paginator<ExpandoObject>? _paginator;
        public PrintedLogsWindow(PrintingInfo printingInfo)
        {
            _printingInfo = printingInfo;

            InitializeComponent();
            InitPrintData();

            //Loaded += PrintedLogsWindow_Loaded;
            //this.Closing += PrintedLogsWindow_Closing;
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
                _paginator = new Paginator<ExpandoObject>(_printingInfo.list, _MaxDatabaseLine);
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
                TextBlockWait.Text = countWaiting.ToString();
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

        private int countDataPerPage;
        public async Task LoadPageAsync(int pageNumber)
        {
            ImageLoadingPrintedLog.Visibility = Visibility.Visible;
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


        //private async void PrintedLogsWindow_Closing(object? sender, CancelEventArgs e)
        //{
        //    //e.Cancel = true; // Cancel the default close operation
        //    //await Task.Run(async () =>
        //    // {
        //    //     await CleanupResourcesAsync();
        //    //     Dispatcher.Invoke(() =>  // Close the window on the UI thread after cleanup
        //    //     {
        //    //         Closing -= PrintedLogsWindow_Closing; // Prevent re-entry
        //    //         Close(); // Now close the window
        //    //     });
        //    // });
        //}

        //private async Task CleanupResourcesAsync()
        //{
        //    //await Task.Run(() =>
        //    //{
        //    //    try
        //    //    {
        //    //        if (PrintedDataTable != null)
        //    //        {
        //    //            PrintedDataTable?.Clear();
        //    //            PrintedDataTable?.Dispose();
        //    //            PrintedDataTable = null;
        //    //        }

        //    //        if (_jobLogsDataTableHelper != null)
        //    //        {
        //    //            _jobLogsDataTableHelper.Dispose();
        //    //            _jobLogsDataTableHelper = null;
        //    //        }

        //    //        _currentJob = null;
        //    //        GC.Collect();
        //    //        //GC.WaitForPendingFinalizers();
        //    //    }
        //    //    catch (Exception)
        //    //    {

        //    //    }

        //    //});
        //}

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

        ObservableCollection<ExpandoObject>? filterList = new();
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

                if (filterList != null && filterList?.Count > 0)
                {
                    _paginator = new Paginator<ExpandoObject>(new ObservableCollection<ExpandoObject>(filterList), _MaxDatabaseLine);
                    _ = LoadPageAsync(0);
                    UpdatePageInfo();
                    ButtonPaginationVis();

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
                var searchResults = filterList.Where(item =>
                {

                    var expandoDict = (IDictionary<string, object>)item;
                    return expandoDict.Values.Any(value => value != null && value.ToString() == searchValue);
                });

                if (searchResults != null && searchResults?.Count() > 0)
                {
                    _paginator = new Paginator<ExpandoObject>(new ObservableCollection<ExpandoObject>(searchResults), _MaxDatabaseLine);
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

        private void DataGridResult_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // todo
        }
    }
}
