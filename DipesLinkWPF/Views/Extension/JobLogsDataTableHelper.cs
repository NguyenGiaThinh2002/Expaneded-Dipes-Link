using DipesLink.Models;
using DipesLink.Views.Converter;
using Microsoft.SqlServer.Server;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DipesLink.Views.Extension
{
    public class JobLogsDataTableHelper : IDisposable
    {
        public enum FilteredKeyword { All, Valid, Invalided, Duplicated, Null, Missed, Failed,
            Printed,
            Waiting
        }
      //  public Paginator? Paginator { get; set; }
        private DataTable? _miniDataTable;
        private DataTable? _originalDataTable;
        private bool disposedValue = false;
        public int NumberItemInCurPage;

        public async Task InitDatabaseAsync(DataTable dataTable, DataGrid dataGrid, JobOverview currentViewModel = null)
        {
            _originalDataTable = dataTable; // saved original DB
            await ProcessMiniPageAsync(dataGrid, dataTable);
        }

        public async Task DatabaseSearchForPrintStatusAsync(DataGrid dataGrid, string keyword)
        {
            DataTable? searchTable = null;
            try
            {
                searchTable = await Task.Run(() =>
                {
                    // Creates a list containing filter condition strings
                    List<string> filterConditions = new();

                    // Browse through all columns in the DataTable
                    foreach (DataColumn column in _originalDataTable.Columns)
                    {
                        string columnName = column.ColumnName;
                        filterConditions.Add($"[{columnName}] LIKE '%{keyword}%'"); // special symbol must put it in []
                    }

                    // Combine all filter conditions into a single string
                    string filterString = string.Join(" OR ", filterConditions);
                    if (keyword == "") filterString = ""; // Refresh button

                    DataView dataView = new(_originalDataTable)
                    {
                        RowFilter = filterString
                    };
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        dataGrid.Columns.Clear();
                    });
                    return dataView.ToTable();
                });
              
                await ProcessMiniPageAsync(dataGrid, searchTable);
            }
            catch (Exception) { }
            finally
            {
                searchTable?.Dispose();
            }
        }

        public async Task DatabaseSearchAsync(DataGrid dataGrid,string keyword)
        {
            DataTable? searchTable = null;
            try
            {
                searchTable = await Task.Run(() =>
                {
                    string filterString = $"Index LIKE '%{keyword}%' OR ResultData LIKE '%{keyword}%' OR Result LIKE '%{keyword}%' OR ProcessingTime LIKE '%{keyword}%' OR DateTime LIKE '%{keyword}%'";


                    if (keyword == "") filterString = ""; // Refresh button
                    DataView dataView = new(_originalDataTable)
                    {
                        RowFilter = filterString
                    };

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        dataGrid.Columns.Clear();
                    });
                    return dataView.ToTable();
                });
                await ProcessMiniPageAsync(dataGrid, searchTable);
            }
            catch (Exception){ }
            finally
            {
                searchTable?.Dispose();
            }
        }

        public async Task DatabaseFilteredForPrintStatusAsync(DataGrid dataGrid, FilteredKeyword keyword)
        {
           await Task.Run(() => { 

            try
            {
                string key = "";
                switch (keyword)
                {
                    case FilteredKeyword.All:
                        key = ""; // No filter 
                        break;
                    case FilteredKeyword.Printed:
                        key = "Printed";
                        break;
                    case FilteredKeyword.Waiting:
                        key = "Waiting";
                        break;

                    default:
                        break;
                }
                DataView dataView = new(_originalDataTable)
                {
                    RowFilter = $"Status = '{key}'"
                };
                if (key == "") // No Filter
                {
                    dataView.RowFilter = key;
                }
 
                DataTable filteredTable = dataView.ToTable();
                   Application.Current.Dispatcher.Invoke(async () =>
                   {
                       dataGrid.Columns.Clear();
                       await ProcessMiniPageAsync(dataGrid, filteredTable);
                   });
               }
            catch (Exception) { }

            });
        }

        public async Task DatabaseFilteredAsync(DataGrid dataGrid, FilteredKeyword keyword)
        {
            try
            {
                string key = "";
                switch (keyword)
                {
                    case FilteredKeyword.All:
                        key = ""; // No filter 
                        break;
                    case FilteredKeyword.Valid:
                        key = "Valid";
                        break;
                    case FilteredKeyword.Invalided:
                        key = "Invalided";
                        break;
                    case FilteredKeyword.Duplicated:
                        key = "Duplicated";
                        break;
                    case FilteredKeyword.Null:
                        key = "Null";
                        break;
                    case FilteredKeyword.Missed:
                        key = "Missed";
                        break;
                    case FilteredKeyword.Failed:
                        key = "Failed";
                        break;
                    default:
                        break;
                }
                DataView dataView = new(_originalDataTable)
                {
                    RowFilter = $"Result = '{key}'"
                };
                if (key == "") // No Filter
                {
                    dataView.RowFilter = key;
                }
                if (key == "Failed") // All failed data
                {
                    dataView.RowFilter =
                        "Result = 'Invalided' OR " +
                        "Result = 'Duplicated' OR " +
                        "Result = 'Null' OR " +
                        "Result = 'Missed'";
                }
                DataTable filteredTable = dataView.ToTable();
                dataGrid.Columns.Clear();
                await ProcessMiniPageAsync(dataGrid, filteredTable);
            }
            catch (Exception) { }
        }

        public async Task ProcessMiniPageAsync(DataGrid dataGrid, DataTable dataTable)
        {
         ///   Paginator = new Paginator(dataTable);
          //  if (Paginator == null) return;
            DataTable? pageTable = null;
            try
            {
              //  pageTable = await Task.Run(() => Paginator.GetPage(Paginator.CurrentPage));

                foreach (DataColumn column in pageTable.Columns)
                {
                    if (column.ColumnName == "Result" || column.ColumnName == "Status")
                    {
                        DataGridTemplateColumn templateColumn = new() { Header = column.ColumnName, Width = DataGridLength.Auto };
                        DataTemplate template = new();
                        FrameworkElementFactory factory = new(typeof(Image)); // Create Image UI by Code behind instead XAML
                        Binding binding = new();
                        if (column.ColumnName == "Result")
                        {
                            binding = new(column.ColumnName) { Converter = new ResultCheckedImgConverter() };
                        }
                        else if (column.ColumnName == "Status")
                        {
                            binding = new(column.ColumnName) { Converter = new StatusToIconConverter() };
                        }

                        factory.SetValue(Image.SourceProperty, binding); // Set binding for Image.
                        factory.SetValue(Image.HeightProperty, 20.0); // Image Height
                        factory.SetValue(Image.WidthProperty, 20.0);  // Image Width

                        template.VisualTree = factory; // add UI to VisualTree Template
                        templateColumn.CellTemplate = template; // CellTemplate = Template
                        dataGrid.Columns.Add(templateColumn); // Add DataGridTemplateColumn
                    }
                    else
                    {
                        DataGridTextColumn textColumn = new()
                        {
                            Header = column.ColumnName,
                            Binding = new Binding(column.ColumnName),
                            Width = 100
                        };
                        dataGrid.Columns.Add(textColumn);
                    }
                }
            }
            catch (Exception)
            {
               
            }
            finally
            {
                pageTable?.Dispose();
            }
             
            await UpdateDataGridAsync(dataGrid);
        }

        public async Task UpdateDataGridAsync(DataGrid dataGrid, int customPage = 0)
        {
            //if (Paginator != null)
            //{
            //    if (customPage == 0) { }
            //    else Paginator.CurrentPage = customPage - 1;
            //    try
            //    {
            //      //  _miniDataTable = await Task.Run(() => Paginator.GetPage(Paginator.CurrentPage)); // Load mini datatable by current page
            //        if (_miniDataTable != null)
            //        {
            //            Interlocked.Exchange(ref NumberItemInCurPage, _miniDataTable.Rows.Count);
            //            Application.Current.Dispatcher.Invoke(() =>  //Update UI Flow
            //            {
            //                dataGrid.AutoGenerateColumns = false;
            //                dataGrid.ItemsSource = _miniDataTable.DefaultView;
            //            });
            //        }
            //    }
            //    catch (Exception)
            //    {

            //    }
            //    finally
            //    {
            //        _miniDataTable?.Dispose();
            //    }
            //}
        }
       
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Free managed resources (managed resources) here.
                    _miniDataTable?.Dispose();
                    _originalDataTable?.Dispose();
                    _miniDataTable = null;
                    _originalDataTable = null;
                    //Paginator?.Dispose();
                    //Paginator = null;
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
    }
}
