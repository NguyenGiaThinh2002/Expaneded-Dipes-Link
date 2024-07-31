using DipesLink.Languages;
using DipesLink.Views.Extension;
using DipesLink.Views.SubWindows;
using Microsoft.VisualBasic.FileIO;
using SharedProgram.Models;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace DipesLink.ViewModels
{

    public partial class MainViewModel
    {
        internal void SavePODFormat()
        {
            try
            {
                _PODFormat.Clear();
                if (SelectedHeadersList != null && SelectedHeadersList.Count > 0)
                {
                    foreach (var item in SelectedHeadersList)
                    {
                        _PODFormat.Add(item);
                    }
                    CreateNewJob.PODFormat = _PODFormat;
                    CreateNewJob.DataCompareFormat = TextPODResult;
                }

                else
                {
                    MessageBox.Show("Please Select POD format !", "POD Selection Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show("Please Select POD format");
            }
            // PreviewPODList();
        }

        /// <summary>
        /// Get user input for item Text Type
        /// </summary>
        /// <param name="text"></param>
        internal void TextInputChange(string text)
        {
            try
            {
                if (SelectedColumnItem2 != null)
                    if (SelectedColumnItem2.Type == PODModel.TypePOD.TEXT)
                    {
                        if (SelectedHeadersList[SelectedHeaderIndex].Value != text)
                            SelectedHeadersList[SelectedHeaderIndex].Value = text;
                    }

            }
            catch (Exception) { }
            UpdateResultTextBoxPOD();
        }

        private void UpdateResultTextBoxPOD()
        {
            string tempStr = "";
            foreach (PODModel item in SelectedHeadersList)
            {
                tempStr += item.ToStringSample();
            }
            TextPODResult = tempStr;
        }

        /// <summary>
        /// Enable and get value of item text for text field textbox
        /// </summary>
        internal void EnableTextFieldInput()
        {
            try
            {
                TextFieldFeedback = SelectedHeadersList[SelectedHeaderIndex].Value;
                if (SelectedColumnItem2.Type == PODModel.TypePOD.TEXT)
                {
                    TextFieldPodVis = true;
                }
                else
                {
                    TextFieldPodVis = false;
                }
                UpdateResultTextBoxPOD();
            }
            catch (Exception)
            {
                TextFieldPodVis = false;
            }

        }

        internal async Task LoadCsvIntoDataGrid()
        {
            if (CreateNewJob.DatabasePath == null) return;
            string csvFilePath = CreateNewJob.DatabasePath;
            PODList.Clear();
            try
            {
                var dataTab = await Task.Run(() =>
                {
                    var dataTable = new DataTable();
                    int rowCount = 0;
                    if (csvFilePath == "")
                    {
                        return dataTable;
                    }
                    using (var parser = new TextFieldParser(csvFilePath))
                    {

                        var numberOfLine = File.ReadLines(csvFilePath).Count() - 1;
                        CreateNewJob.TotalRecDb = numberOfLine;
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",", ";");
                        if (!parser.EndOfData)
                        {
                            string[]? fields = parser.ReadFields();
                            if (fields != null)
                            {
                                RawDataList.Add(fields); // Add header
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    int podCount = 0;
                                    PODList.Add(new PODModel
                                    {
                                        Index = podCount,
                                        Type = PODModel.TypePOD.TEXT,
                                        PODName = "",
                                        Value = ""
                                    });

                                    foreach (string field in fields)
                                    {
                                        PODList.Add(new PODModel
                                        {
                                            Index = ++podCount,
                                            Type = PODModel.TypePOD.FIELD,
                                            PODName = field,
                                            Value = ""
                                        });
                                        dataTable.Columns.Add(field);
                                    }
                                });
                            }
                        }
                        while (!parser.EndOfData && rowCount < _MaxDatabaseLine)
                        {
                            string[]? values = parser.ReadFields();
                            if (values != null)
                            {
                                RawDataList.Add(values);
                                dataTable.Rows.Add(values);
                                rowCount++;
                            }
                        }
                    }
                    return dataTable;
                });
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    DataViewPODFormat = dataTab.DefaultView;
                    CloneDataView = dataTab.DefaultView;
                });

            }
            catch (IOException ioex)
            {
                var mess = ioex.Message;
                _ = CusMsgBox.Show(mess,
                    LanguageModel.GetLanguage("WarningDialogCaption"),
                    Views.Enums.ViewEnums.ButtonStyleMessageBox.OK,
                    Views.Enums.ViewEnums.ImageStyleMessageBox.Warning);
            }
            catch (Exception)
            {

            }

        }

        internal void DBView()
        {
            try
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    DataViewPODFormat = CloneDataView;
                });
            }
            catch (Exception) { }
        }

        internal void PreviewPODList()
        {
            try
            {
                _TempPODFormat.Clear();
                if (SelectedHeadersList != null && SelectedHeadersList.Count > 0)
                {
                    foreach (var item in SelectedHeadersList)
                    {
                        _TempPODFormat.Add(item);
                    }
                    CreateNewJob.PODFormat = _TempPODFormat;
                    CreateNewJob.DataCompareFormat = TextPODResult;
                }

                else
                {
                    MessageBox.Show("Please Select POD format !", "POD Selection Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                var formatList = new List<object>();
                foreach (PODModel pod in _TempPODFormat)
                {
                    if (pod.Type == PODModel.TypePOD.FIELD)
                    {
                        formatList.Add(pod.Index - 1);
                    }
                    else // as text
                    {
                        formatList.Add(pod.Value);
                    }
                }
                List<string> formattedList = RawDataList.Select(row =>
                string.Concat(formatList.Select(item => item is int index ? row[index] : item))).ToList();

                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    DataViewPODFormat = ConvertListToDataTable(formattedList).DefaultView;
                });
            }
            catch (Exception)
            {
                //MessageBox.Show("Please Select POD format");
            }
        }

        private static DataTable ConvertListToDataTable(List<string> list)
        {
            DataTable dataTable = new();
            dataTable.Columns.Add("Index", typeof(int));
            dataTable.Columns.Add("POD Formated", typeof(string));
            for (int i = 1; i < list.Count; i++)
            {
                dataTable.Rows.Add(i, list[i]);
            }
            return dataTable;
        }

        internal void OperationPODFormat(string buttonName)
        {
            try
            {
                switch (buttonName)
                {
                    case "ButtonAdd":
                        if (SelectedColumnItem1 != null)
                        {
                            bool isExistItemAdd = SelectedHeadersList.Any(x => x == SelectedColumnItem1);
                            SelectedHeadersList.Add(SelectedColumnItem1.Clone());
                        }
                        break;
                    case "ButtonRemove":
                        bool isExistItemRemove = SelectedHeadersList.Any(x => x == SelectedColumnItem2);
                        if (isExistItemRemove)
                        {
                            SelectedHeadersList.RemoveAt(SelectedHeaderIndex);
                        }
                        break;
                    case "ButtonUp":
                        MoveUp(SelectedHeadersList, SelectedHeaderIndex);

                        break;
                    case "ButtonDown":
                        MoveDown(SelectedHeadersList, SelectedHeaderIndex);
                        break;
                    case "ButtonClearAll":
                        SelectedHeadersList.Clear();
                        break;
                    default: break;
                }
                UpdateResultTextBoxPOD();
            }
            catch (Exception) { }

        }

        public static void MoveUp<T>(ObservableCollection<T> list, T selectedItem)
        {
            try
            {
                int index = list.IndexOf(selectedItem);
                if (index > 0) // Make sure it's not the first element
                {
                    list.RemoveAt(index);
                    list.Insert(index - 1, selectedItem);
                }
            }
            catch (Exception) { }
        }

        public static void MoveDown<T>(ObservableCollection<T> list, T selectedItem)
        {
            try
            {
                int index = list.IndexOf(selectedItem);
                if (index < list.Count - 1) //Make sure not the last element
                {
                    list.RemoveAt(index);
                    list.Insert(index + 1, selectedItem);
                }
            }
            catch (Exception) { }
        }

        public static void MoveUp<T>(ObservableCollection<T> list, int index)
        {
            try
            {
                if (index > 0)
                {
                    T item = list[index];
                    list.RemoveAt(index);
                    list.Insert(index - 1, item);
                }
            }
            catch (Exception)
            {
            }

        }

        public static void MoveDown<T>(ObservableCollection<T> list, int index)
        {
            try
            {
                if (index < list.Count - 1)
                {
                    T item = list[index];
                    list.RemoveAt(index);
                    list.Insert(index + 1, item);
                }
            }
            catch (Exception)
            {
            }
        }

    }
}
