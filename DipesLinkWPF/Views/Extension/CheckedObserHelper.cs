using CommunityToolkit.Mvvm.ComponentModel;
using DipesLink.Models;
using DipesLink.ViewModels;
using DipesLink.Views.Converter;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace DipesLink.Views.Extension
{

    public class CheckedObserHelper : ObservableObject, IDisposable
    {
        public int TotalChecked { get; set; }

        public int TotalPassed { get; set; }

        public int TotalFailed { get; set; }

        public ObservableCollection<CheckedResultModel> CheckedList { get; set; } = new();

        private ObservableCollection<CheckedResultModel> _DisplayList = new();

        public ObservableCollection<CheckedResultModel> DisplayList
        {
            get { return _DisplayList; }
            set
            {
                if (_DisplayList != value)
                {
                    _DisplayList = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(2000);

        private DispatcherTimer _updateTimer;

        private readonly ConcurrentQueue<CheckedResultModel> _batchUpdateQueue = new();

        private bool disposedValue = false;

        public CheckedObserHelper()
        {
            _updateTimer = InitializeDispatcherTimer();
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

        private void GetStatistic()
        {
            if (CheckedList == null) return;
            TotalChecked = CheckedList.Count;
            TotalPassed = CheckedList.Count(x => x.Result == "Valid");
            if (TotalChecked >= TotalPassed)
            {
                TotalFailed = TotalChecked - TotalPassed;
            }
        }

        public void ConvertListToObservableCol(List<string[]> list)
        {
            var newCheckedList = new ObservableCollection<CheckedResultModel>(list.Select(data => new CheckedResultModel(data)));
            CheckedList = newCheckedList;
            GetStatistic();
        }

        public async Task<DataTable> GetDataTableDBAsync()
        {
            return await Task.Run(() =>
            {
                string[] columnNames = { "Index", "ResultData", "Result", "ProcessingTime", "DateTime" };
                var dataTable = new DataTable();

                foreach (var columnName in columnNames)
                {
                    dataTable.Columns.Add(columnName);
                }

                foreach (var array in CheckedList)
                {
                    var row = dataTable.NewRow();
                    row["Index"] = array.Index != null ? array.Index : DBNull.Value;
                    row["ResultData"] = array.ResultData != null ? array.ResultData : DBNull.Value;
                    row["Result"] = array.Result != null ? array.Result : DBNull.Value;
                    row["ProcessingTime"] = array.ProcessingTime != null ? array.ProcessingTime : DBNull.Value;
                    row["DateTime"] = array.DateTime != null ? array.DateTime : DBNull.Value;
                    dataTable.Rows.Add(row);
                }

                return dataTable;
            });
        }

        public static void CreateDataTemplate(DataGrid dataGrid)
        {
            dataGrid.Columns.Clear();
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
                    dataGrid.Columns.Add(templateColumn);
                }
                else
                {
                    DataGridTextColumn textColumn = new()
                    {
                        Header = property.Name,
                        Binding = new Binding(property.Name),
                        Width = DataGridLength.Auto
                    };
                    dataGrid.Columns.Add(textColumn);
                }
            }
        }

        public void AddNewData(string[] newData)
        {
            var data = new CheckedResultModel(newData);
            _batchUpdateQueue.Enqueue(data);
        }

        private void ProcessBatchUpdates()
        {
            if (_batchUpdateQueue.IsEmpty)
            {
                return;
            }
            var updates = new List<CheckedResultModel>();
            while (_batchUpdateQueue.TryDequeue(out var data))
            {
                updates.Add(data);
            }

            Application.Current.Dispatcher.InvokeAsync(new Action(() =>
            {
                foreach (var update in updates)
                {
                    CheckedList.Add(update);
                    DisplayList.Insert(0, update);
                    if (DisplayList.Count > 100)
                    {
                        DisplayList.RemoveAt(DisplayList.Count - 1);
                    }
                }
                GetStatistic();
            }));
        }

        public void TakeFirtloadCollection()
        {
            var newestItems = CheckedList.OrderByDescending(x => x.DateTime).Take(100).ToList();
            DisplayList.Clear();
            foreach (var item in newestItems)
            {
                DisplayList.Add(item);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CheckedList.Clear();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DisplayList.Clear();
                    });
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
}
