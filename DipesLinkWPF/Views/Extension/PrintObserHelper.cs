using DipesLink.Models;
using DipesLink.ViewModels;
using DipesLink.Views.Converter;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;

namespace DipesLink.Views.Extension
{
    public class PrintObserHelper : ViewModelBase
    {
        private DataGrid _dataGrid;
        private ObservableCollection<ExpandoObject>? printList = new();
        public ObservableCollection<ExpandoObject>? PrintList { get => printList; set { printList = value; OnPropertyChanged(); } }
        private readonly Paginator paginator;
        readonly string[] columnNames = Array.Empty<string>();
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(2000);
        private readonly List<string[]> _batchUpdateList = new();
        private ConcurrentQueue<string[]> _batchUpdateQueue = new();
        private readonly object _lock = new();
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private readonly Dictionary<string, (ExpandoObject Item, int PageNumber)> _dataLookup = new();
        private CancellationTokenSource cts_UpdateUI = new();
        private int _MaxDatabaseLine = 500;
        private DispatcherTimer _dispatcherTimer;

        public PrintObserHelper(List<string[]> list, int currentPage, DataGrid dataGrid)
        {
            _dataGrid = dataGrid;
            columnNames = list[0];
            CreateDataTemplate();
            AddDataToCollection(PrintList, list[0], list.Skip(1).ToList());
            paginator = new Paginator(PrintList, _MaxDatabaseLine);
            LoadPageAsync(currentPage);
            CreateDataLookup();
            _dispatcherTimer = InitializeDispatcherTimer();

        }

        private void CreateDataLookup()
        {
            if (paginator.SourceData == null) return;
            for (int i = 0; i < paginator.SourceData.Count; i++)
            {
                var item = paginator.SourceData[i];
                if (item is IDictionary<string, object> modelDict && modelDict.ContainsKey(columnNames[0]))
                {

                    string? key = modelDict[columnNames[0]].ToString();
                    int pageNumber = paginator.GetCurrentPageNumber(i);
                    if (key != null) _dataLookup[key] = (item, pageNumber);
                }
            }
        }
        private static void AddDataToCollection(ObservableCollection<ExpandoObject>? collection, string[] properties, List<string[]> dataList)
        {
            foreach (var data in dataList)
            {
                dynamic model = new ExpandoObject();
                var modelDict = (IDictionary<string, object>)model;

                for (int i = 0; i < properties.Length; i++)
                {
                    modelDict[properties[i]] = data[i];
                }
                collection?.Add(model);
            }
        }

        private void CreateDataTemplate()
        {
            _dataGrid.Columns.Clear();
            foreach (var columnName in columnNames)
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
                    _dataGrid.Columns.Add(templateColumn);
                }
                else
                {
                    DataGridTextColumn textColumn = new()
                    {
                        Header = columnName,
                        Binding = new Binding(columnName),
                        Width = DataGridLength.Auto
                    };
                    _dataGrid.Columns.Add(textColumn);
                }
            }
        }

        public void LoadPageAsync(int pageNumber)
        {
            if (pageNumber < 0 || pageNumber >= paginator.TotalPages) return;
            ObservableCollection<ExpandoObject> pageData = paginator.GetPage(pageNumber);
            Application.Current.Dispatcher.Invoke(() =>
             {
                 _dataGrid.ItemsSource = pageData;
             });
        }

        public void CheckAndUpdateStatusAsync(string[] incomingData)
        {
            _batchUpdateQueue.Enqueue(incomingData);
        }

        private DispatcherTimer InitializeDispatcherTimer()
        {
            var dispatcherTimer = new DispatcherTimer
            {
                Interval = _updateInterval
            };
            dispatcherTimer.Tick += (sender, args) => ProcessBatchUpdates();
            dispatcherTimer.Start();
            return dispatcherTimer;
        }

        private void ProcessBatchUpdates()
        {
            if (_batchUpdateQueue.IsEmpty)
            {
                return;
            }
            var updates = new List<string[]>();
            while (_batchUpdateQueue.TryDequeue(out var data))
            {
                updates.Add(data);
            }
            foreach (var update in updates)
            {
                ProcessDataAndDisplayAsync(update);
            }
        }

        private void ProcessDataAndDisplayAsync(string[] incomingData)
        {
            if (paginator.SourceData == null) return;
            string key = incomingData[0];
            string newStatus = incomingData[^1];
            bool dataUpdated = false;

            if (_dataLookup.TryGetValue(key, out var data))
            {
                if (data.Item is IDictionary<string, object> modelDict && modelDict.ContainsKey("Status") && modelDict["Status"].ToString() != newStatus)
                {
                    modelDict["Status"] = newStatus;
                    dataUpdated = true;
                }
            }

            if (dataUpdated)
            {
                int curPageNumber = _dataLookup[key].PageNumber;
                if (paginator != null && curPageNumber != paginator.CurrentPage)
                {
                    paginator.CurrentPage = curPageNumber;
                    LoadPageAsync(curPageNumber);
                }

                Application.Current.Dispatcher.InvokeAsync(() =>
                 {
                     ScrollToData(key);
                 }, DispatcherPriority.Background);
            }
        }

        private void ScrollToData(string key)
        {
            foreach (var item in _dataGrid.Items)
            {
                if (item is IDictionary<string, object> modelDict && modelDict.ContainsKey(columnNames[0]) && modelDict[columnNames[0]].ToString() == key)
                {
                    _dataGrid.ScrollIntoView(item);

                    break;
                }
            }
        }

    }
}
