using DipesLink.ViewModels;
using DipesLink.Views.Converter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System.Windows.Input;
using DipesLink.Models;
using System.Diagnostics;
using System.Windows.Threading;
using System.Collections.Concurrent;
using Microsoft.VisualBasic;
using System.Windows.Markup;

namespace DipesLink.Views.Extension
{
    public class PrintObserHelper : ViewModelBase
    {
        private DataGrid _dataGrid;
        private ObservableCollection<ExpandoObject>? printList = new();
        public ObservableCollection<ExpandoObject>? PrintList { get => printList; set { printList = value; OnPropertyChanged(); } }
        private Paginator paginator;
        string[] columnNames = Array.Empty<string>();
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(50);
        private readonly List<string[]> _batchUpdateList = new();
        private ConcurrentQueue<string[]> _batchUpdateQueue = new();
        private readonly object _lock = new();
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private readonly Dictionary<string, (ExpandoObject Item, int PageNumber)> _dataLookup = new();


        public PrintObserHelper(List<string[]> list, int currentPage, DataGrid dataGrid)
        {
            _dataGrid = dataGrid;
            columnNames = list[0];
            CreateDataTemplate();
            AddDataToCollection(PrintList, list[0], list.Skip(1).ToList());
            paginator = new Paginator(PrintList, 500);
            _ = LoadPageAsync(currentPage);
            CreateDataLookup();
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

        public async Task LoadPageAsync(int pageNumber)
        {
            if (pageNumber < 0 || pageNumber >= paginator.TotalPages) return;
            ObservableCollection<ExpandoObject> pageData = await Task.Run(() => paginator.GetPage(pageNumber));
            Application.Current.Dispatcher.Invoke(() =>
            {
                _dataGrid.ItemsSource = pageData;
            });
        }

        public async void CheckAndUpdateStatusAsync(string[] incomingData)
        {
            _batchUpdateQueue.Enqueue(incomingData);
            var now = DateTime.UtcNow;
            if (now - _lastUpdateTime < _updateInterval)
            {
                return;
            }
            _lastUpdateTime = now;
            await Task.Run(async () =>
            {
                while (_batchUpdateQueue.TryDequeue(out string[]? dataListPartial))
                {
                    await ProcessDataAndDisplayAsync(dataListPartial);
                }
            });
        }

        private async Task ProcessDataAndDisplayAsync(string[] incomingData)
        {
            if (paginator.SourceData == null) return;
            string key = incomingData[0];
            string newStatus = incomingData[^1];
            bool dataUpdated = false;

            if (_dataLookup.TryGetValue(key, out var data))
            {
                var modelDict = data.Item as IDictionary<string, object>;
                if (modelDict != null && modelDict.ContainsKey("Status") && modelDict["Status"].ToString() != newStatus)
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
                    await LoadPageAsync(curPageNumber);
                }
                _ = Application.Current.Dispatcher.BeginInvoke(() =>
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
