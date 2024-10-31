using DipesLink.Views.Converter;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DipesLink.Views.Extension
{
    public class CheckedDataTableHelper
    {
        public DataTable? DataTable { get; set; }
        public int TotalChecked { get; set; }

        public int TotalPassed { get; set; }

        public int TotalFailed { get; set; }
        /// <summary>
        /// Create manual template for DataGrid
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="dataGrid"></param>
        public void CreateDataTemplate(DataGrid dataGrid)
        {
            if (DataTable == null) return;
            dataGrid.Columns.Clear();
            foreach (DataColumn column in DataTable.Columns)
            {
                if (column.ColumnName == "Result") // Create template for column has name "Result"
                {
                    DataGridTemplateColumn templateColumn = new() { Header = column.ColumnName, Width = DataGridLength.Auto };
                    DataTemplate template = new();
                    FrameworkElementFactory factory = new(typeof(Image)); // Create Image UI by Code behind instead XAML
                    Binding binding = new(column.ColumnName) { Converter = new ResultCheckedImgConverter() };

                    factory.SetValue(Image.SourceProperty, binding); // Set binding for Image
                    factory.SetValue(Image.HeightProperty, 20.0); // Image Height
                    factory.SetValue(Image.WidthProperty, 20.0);  // Image Width

                    template.VisualTree = factory; // add UI to VisualTree Template
                    templateColumn.CellTemplate = template; // CellTemplate = Template
                    dataGrid.Columns.Add(templateColumn); // Add DataGridTemplateColumn
                }
                else  // The remaining columns will be TextColumn
                {
                    DataGridTextColumn textColumn = new()
                    {
                        Header = column.ColumnName,
                        Binding = new Binding(column.ColumnName),
                        Width = DataGridLength.Auto
                    };
                    dataGrid.Columns.Add(textColumn);
                }
            }
        }

        /// <summary>
        /// Convert List<string[]> to Datatable
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public void ConvertListToDataTable(List<string[]> list)
        {
            string[] columnNames = new string[6] { "Index", "ResultData", "Result", "ProcessingTime", "DateTime", "Device" }; // Default header col
            DataTable = new DataTable();
            try
            {
                // Add Header column
                int numberOfColumns = 6; // or get dynamic with list[0].Length;
                for (int i = 0; i < numberOfColumns; i++)
                {
                    string columnName = columnNames.Length > i ? columnNames[i] : $"Column{i + 1}";
                    DataTable.Columns.Add(columnName);
                }

                // Add row
                foreach (var array in list)
                {
                    DataRow row = DataTable.NewRow();
                    for (int i = 0; i < array.Length; i++)
                    {
                        row[i] = array[i];
                    }
                    DataTable.Rows.Add(row);
                }
                GetStatistic();
            }
            catch (Exception){}
        }

        private void GetStatistic()
        {
            if (DataTable == null) return;
            //Get Number Checked
            TotalChecked = DataTable.Rows.Count;
            TotalPassed = DataTable.Select("Result = 'Valid'").Length;
            if (TotalChecked >= TotalPassed)
            {
                TotalFailed = TotalChecked - TotalPassed;
            }
        }

        /// <summary>
        /// Add new data to Datatabel
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="dataGrid"></param>
        /// <param name="table"></param>
        public void AddNewData(string[] newData, DataGrid dataGrid)
        {
            try
            {
                if (DataTable == null) return;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DataRow newRow = DataTable.NewRow(); // Create new row
                    newRow.ItemArray = newData; // add data to new row
                    DataTable.Rows.Add(newRow); // add to datatable
                    GetStatistic();
                    var dataView = CreateLimitDataView();
                    dataGrid.ItemsSource = dataView;

                    //if (dataGrid.Items.Count > 0)
                    //{
                    //    dataGrid.SelectedIndex = 0; // Select the first row
                    //    dataGrid.ScrollIntoView(0);
                    //    //dataGrid.Focus(); // Give the DataGrid focus
                    //    DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(0);
                    //    if (row != null)
                    //    {
                    //        row.Focus();
                    //    }
                    //}
                });
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Update DataGrid by DataView
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="dt"></param>
        public void UpdateDataGrid(DataGrid dataGrid)
        {
            if (DataTable == null) return;
            dataGrid.ItemsSource = CreateLimitDataView();
        }

        /// <summary>
        /// Create Limit DataView
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public DataView CreateLimitDataView()
        {
            try
            {
                DataView view = new(DataTable)
                {
                    Sort = "DateTime DESC"  // Sort by DateTime
                };

                DataTable newTable = view.ToTable().Clone(); // Clone DataView Sorted to Table
                int rowsToAdd = Math.Min(100, view.Count); // Get new row (in max: view.Count)
                for (int i = 0; i < rowsToAdd; i++)
                {
                    newTable.ImportRow(view[i].Row);
                }
                return newTable.DefaultView; // Get dataview
            }
            catch (Exception) { return new DataView(); }
        }
    }
}
