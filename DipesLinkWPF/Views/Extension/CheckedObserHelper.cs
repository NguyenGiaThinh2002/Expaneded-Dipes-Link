using DipesLink.Models;
using DipesLink.ViewModels;
using DipesLink.Views.Converter;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DipesLink.Views.Extension
{
    public class CheckedObserHelper : ViewModelBase, IDisposable
    {
        #region Declarations    

        public int TotalChecked { get; set; }

        public int TotalPassed { get; set; }

        public int TotalFailed { get; set; }

        public ObservableCollection<CheckedResultModel>? CheckedList { get; set; } = new();

        private ObservableCollection<CheckedResultModel>? _DisplayList = new();
        public ObservableCollection<CheckedResultModel>? DisplayList
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

        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(50);
        private readonly List<CheckedResultModel> _batchUpdateList = new();
        private readonly object _lock = new();
        private DateTime _lastUpdateTime = DateTime.MinValue;

        #endregion End Declarations 

        #region Functionals
        private void GetStatistic()
        {
            if (CheckedList == null) return;
            //Get Number Checked
            TotalChecked = CheckedList.Count;
            TotalPassed = CheckedList.Count(x => x.Result == "Valid");
            if (TotalChecked >= TotalPassed)
            {
                TotalFailed = TotalChecked - TotalPassed;
            }
        }

        public void ConvertListToObservableCol(List<string[]> list)
        {
            CheckedList = new ObservableCollection<CheckedResultModel>(list.Select(data => new CheckedResultModel(data)));
            GetStatistic();
        }

        public async Task<DataTable> GetDataTableDBAsync()
        {
            return await Task.Run(() =>
            {
                string[] columnNames = new string[5] { "Index", "ResultData", "Result", "ProcessingTime", "DateTime" }; // Default header col
                var dataTable = new DataTable();
                var list = CheckedList?.ToList();
                if (list == null) return dataTable;

                try
                {
                    // Add Header columns
                    foreach (var columnName in columnNames)
                    {
                        dataTable.Columns.Add(columnName);
                    }

                    // Add rows
                    foreach (CheckedResultModel array in list)
                    {
                        DataRow row = dataTable.NewRow();

                        row["Index"] = array.Index != null ? array.Index : DBNull.Value;
                        row["ResultData"] = array.ResultData != null ? array.ResultData : DBNull.Value;
                        row["Result"] = array.Result != null ? array.Result : DBNull.Value;
                        row["ProcessingTime"] = array.ProcessingTime != null ? array.ProcessingTime : DBNull.Value;
                        row["DateTime"] = array.DateTime != null ? array.DateTime : DBNull.Value;

                        dataTable.Rows.Add(row);
                    }
                    return dataTable;
                }
                catch (Exception)
                {
                    return dataTable;
                }
            });
        }

        public static void CreateDataTemplate(DataGrid dataGrid)
        {
            dataGrid.Columns.Clear();
            var properties = typeof(CheckedResultModel).GetProperties(); // get all properties of CheckedResultModel

            foreach (var property in properties)
            {
                if (property.Name == "Result") // Create template for "Result" column
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

        public async void AddNewData(string[] newData)
        {
            var data = new CheckedResultModel(newData);
            lock (_lock)
            {
                _batchUpdateList.Add(data);
            }

            await Application.Current.Dispatcher.InvokeAsync(() => //Batch Update and Throttle Updates 50ms
            {
                var now = DateTime.UtcNow;
                if (now - _lastUpdateTime < _updateInterval)
                {
                    return;
                }
                _lastUpdateTime = now;

                List<CheckedResultModel> updates;

                lock (_lock)
                {
                    updates = new List<CheckedResultModel>(_batchUpdateList);
                    _batchUpdateList.Clear();
                }

                foreach (var update in updates)
                {
                    CheckedList?.Add(update);
                    UpdateDisplayedData(update);
                }
                GetStatistic();
            });
        }

        public void TakeFirtloadCollection()
        {
            var newestItems = CheckedList?.OrderByDescending(x => x.DateTime).Take(100).ToList();
            if (DisplayList != null && newestItems != null)
            {
                DisplayList.Clear();
                foreach (var item in newestItems)
                {
                    DisplayList.Add(item);
                }
            }
        }

        public void UpdateDisplayedData(CheckedResultModel data)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (CheckedList != null)
                {
                    DisplayList?.Insert(0, data);
                    while (DisplayList?.Count > 100)
                    {
                        DisplayList.RemoveAt(DisplayList.Count - 1);
                        Thread.Sleep(1);
                    }
                }
            });
        }


        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Release managed resources here.
                    CheckedList?.Clear();
                    CheckedList = null;

                    Application.Current.Dispatcher?.Invoke(() =>
                    {
                        DisplayList?.Clear();
                        DisplayList = null;

                    });
                }

                // Release unmanaged resources here.

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
