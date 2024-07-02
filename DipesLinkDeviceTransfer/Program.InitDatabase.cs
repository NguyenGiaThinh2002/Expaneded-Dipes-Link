using IPCSharedMemory;
using IPCSharedMemory.Controllers;
using SharedProgram.DeviceTransfer;
using SharedProgram.Models;
using SharedProgram.Shared;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Shapes;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLinkDeviceTransfer
{
    /// <summary>
    /// INIT DATABASE
    /// </summary>
    public partial class Program
    {
        private List<NotifyType> _InitDataErrorList = new();

        private int _TotalMissed = 0; // Use for RePrint Condition

        private readonly int _StartIndex = 1;



        private async Task InitDataAsync(JobModel selectedJob)
        {
            //  Console.WriteLine("Toi bat dau load");

            if (_SelectedJob == null) return;
            if (selectedJob.CompareType == CompareType.Database)
            {
                MemoryTransfer.SendLoadingStatusToUI(_ipcDeviceToUISharedMemory_DT, JobIndex, DataConverter.ToByteArray(LoadDataStatus.StartLoad));
                Task<List<string[]>> databaseTsk = InitDatabaseAndPrintedStatusAsync(selectedJob); //Load database and update print status
                Task<List<string[]>> checkedResultTsk = InitCheckedResultDataAsync(selectedJob); //Load list of checked result by cameras
                await Task.WhenAll(databaseTsk, checkedResultTsk); // Wait for the lists to finish loading
                //Console.WriteLine("Toi da load");
                // Save to List
                _ListPrintedCodeObtainFromFile = databaseTsk.Result;
                _ListCheckedResultCode = checkedResultTsk.Result;

                Task a = Task.Run(() =>
                 {
                     //Console.WriteLine("Task a work");
                     MemoryTransfer.SendDatabaseToUIFirstTime(_ipcDeviceToUISharedMemory_DB, JobIndex, DataConverter.ToByteArray(_ListPrintedCodeObtainFromFile)); // Send saved DB to UI
                 });
                Task b = Task.Run(() =>
                {
                    //Console.WriteLine("Task b work");
                    MemoryTransfer.SendCheckedDatabaseToUIFirstTime(_ipcDeviceToUISharedMemory_DB, JobIndex, DataConverter.ToByteArray(_ListCheckedResultCode)); // Send checked list to UI
                });

                await Task.WhenAll(a, b);
                // đi tiếp
#if DEBUG
                Console.WriteLine("\nDatabase: {0} raw", _ListPrintedCodeObtainFromFile.Count - 1);
                Console.WriteLine("Printed: " + _ListPrintedCodeObtainFromFile.Count(item => item.Last() == "Printed")); // show row number was printed
                Console.WriteLine("Checked: {0}", _ListCheckedResultCode.Count);
#endif

                if (_ListPrintedCodeObtainFromFile != null && _ListPrintedCodeObtainFromFile.Count > 1)
                {
                    _DatabaseColunms = _ListPrintedCodeObtainFromFile[0]; // get header column
                    _ListPrintedCodeObtainFromFile.RemoveAt(0);  // remove header for get only data
                    await InitVerifyAndPrindSendDataMethod(); // Init Verify and Print cond send
                    if (_SelectedJob.CompareType == CompareType.Database)
                    {
                        await InitCompareDataAsync(_ListPrintedCodeObtainFromFile, _ListCheckedResultCode);

#if DEBUG
                        await Console.Out.WriteLineAsync($"POD filter: {_CodeListPODFormat.Count}");
                        //  await Console.Out.WriteLineAsync("Complete load !");
#endif
                    }
                    _TotalCode = _ListPrintedCodeObtainFromFile.Count; // tổng số dữ liệu
                    NumberPrinted = _ListPrintedCodeObtainFromFile.Where(x => x[^1] == "Printed").Count(); // tổng số đã in
                    int firstWaiting = _ListPrintedCodeObtainFromFile.IndexOf(_ListPrintedCodeObtainFromFile.Find(x => x[^1] == "Waiting")); // Xác định index của dữ liệu waiting đầu tiên
                    _CurrentPage = _TotalCode > _MaxDatabaseLine ?
                                   (firstWaiting > 0 ? firstWaiting / _MaxDatabaseLine : (firstWaiting == 0 ? 0 : _TotalCode / _MaxDatabaseLine - 1)) : 0;
                    string[] lastCode = _ListPrintedCodeObtainFromFile[^1];
                    if (_NumberOfDuplicate > 0)
                    {
                        //Duplicate error message
                        NotificationProcess(NotifyType.DuplicateData);
                        SharedFunctions.PrintDebugMessage($"Database Duplicate: {_NumberOfDuplicate} row");
                    }
                }
            }
            else
            {
                _ListCheckedResultCode = await InitCheckedResultDataAsync(selectedJob);
            }
            TotalChecked = _ListCheckedResultCode.Count; // Get total checked for remembering index
            NumberOfCheckPassed = _ListCheckedResultCode.Where(x => x[2] == "Valid").Count();
            NumberOfCheckFailed = TotalChecked - NumberOfCheckPassed;
#if DEBUG
            await Console.Out.WriteLineAsync($"Total checked: {TotalChecked}, Total passed: {NumberOfCheckPassed}, Total failed: {NumberOfCheckFailed}\n");
#endif
        }

        #region LOAD DB AND PRINTED LIST
        private async Task<List<string[]>> InitDatabaseAndPrintedStatusAsync(JobModel jobModel)
        {
            // Get path db and printed list
            var pathDatabase = jobModel.DatabasePath;
            var pathBackupPrintedResponse = SharedPaths.PathPrintedResponse + $"Job{JobIndex + 1}\\" + jobModel.PrintedResponePath;

            // Init Databse from file, add index column, status column, and string "Feild"
            List<string[]> tmp = await Task.Run(() => { return InitDatabase(pathDatabase); });

            // Update Printed status
            if (jobModel.PrintedResponePath != "" && File.Exists(jobModel.DatabasePath) && tmp.Count > 1)
            {
                await Task.Run(() => { InitPrintedStatus(pathBackupPrintedResponse, tmp); });
            }
            return tmp;
        }



        /// <summary>
        /// Load database
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<string[]> InitDatabase(string? path)
        {
            List<string[]> result = new(); // List result
            if (!File.Exists(path))
            {
                _InitDataErrorList.Add(NotifyType.DatabaseDoNotExist);
                return result;
            }
            try
            {
                using var reader = new StreamReader(path, Encoding.UTF8, true);
                var rexCsvSplitter = path.EndsWith(".csv") ? new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))") : new Regex(@"[\t]");
                int lineCounter = -1;
                int columnCount = 0;

                if (reader.ReadLine() is not null)
                    while (!reader.EndOfStream)
                    {
                        var readLineRes = reader.ReadLine(); 
                        if (readLineRes is not null)
                        {
                            string[] line = rexCsvSplitter.Split(readLineRes).Select(x => Csv.Unescape(x)).ToArray();
                            lineCounter++;
                            if (lineCounter == 0)
                            {
                                // Create additional database columns
                                var tmp = new string[line.Length + 2];
                                tmp[0] = "Index"; // thêm cột Index ở đầu
                                tmp[^1] = "Status"; // Thêm cột status ở cuối (^1 đại diện phần tử cuối)
                                for (int i = 1; i < tmp.Length - 1; i++)
                                {
                                    tmp[i] = line[i - 1] + $" - Field{i}"; // Thêm chữ field vào cạnh column
                                }
                                columnCount = tmp.Length;
                                result.Add(tmp);
                            }
                            else
                            {
                                var tmp = new string[columnCount];
                                tmp[0] = "" + lineCounter;
                                tmp[columnCount - 1] = "Waiting"; // thêm trạng thái waiting cho trường status
                                for (int i = 1; i < tmp.Length - 1; i++)
                                {
                                    if (i - 1 < line.Length)
                                    {
                                        tmp[i] = line[i - 1];
                                    }
                                    else
                                    {
                                        tmp[i] = "";
                                    }
                                }
                                result.Add(tmp);
                            }
                        }
                    }
            }
            catch (IOException) { _InitDataErrorList.Add(NotifyType.CannotAccessDatabase); }
            catch (Exception) { _InitDataErrorList.Add(NotifyType.DatabaseUnknownError); }
            return result;
        }

        /// <summary>
        /// Cập nhật trạng thái Printed cho database vừa load vào
        /// </summary>
        /// <param name="pathBackupPrinted"></param>
        /// <param name="dbList"></param>
        private void InitPrintedStatus(string pathBackupPrinted, List<string[]> dbList)
        {
            if (!File.Exists(pathBackupPrinted))
            {
                _InitDataErrorList.Add(NotifyType.PrintedResponseDoNotExist);
                return;
            }
            try
            {
                using StreamReader reader = new(pathBackupPrinted, Encoding.UTF8, true);
                int i = -1;
                var rexCsvSplitter = pathBackupPrinted.EndsWith(".csv") ? new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))") : new Regex(@"[\t]");
                while (!reader.EndOfStream)
                {
                    i++;
                    if (i == 0) // Leave out the first row
                    {
                        reader.ReadLine();
                    }
                    else
                    {
                        string line = Csv.Unescape(rexCsvSplitter.Split(reader.ReadLine())[0]);
                        if (int.TryParse(line, out int index))
                        {
                            dbList[index][^1] = "Printed"; // Get rows by index and last column ^1 Updated with the content Printed
                        }
                    }
                    //  await Task.Delay(1);
                }
            }
            catch (IOException) { _InitDataErrorList.Add(NotifyType.CannotAccessPrintedResponse); }
            catch (Exception) { _InitDataErrorList.Add(NotifyType.PrintedStatusUnknownError); }
        }

        #endregion  LOAD DB AND PRINTED LIST

        #region LOAD CHECKED RESULT LIST

        private async Task<List<string[]>> InitCheckedResultDataAsync(JobModel selectedJob)
        {
            Console.WriteLine("Job Index: " + selectedJob.Index);
            string path = SharedPaths.PathCheckedResult + $"Job{JobIndex + 1}\\" + selectedJob.CheckedResultPath; // Get path of checked result file (.csv)
                                                                                                                  // await Console.Out.WriteLineAsync("path checked" + selectedJob.CheckedResultPath);
            if (selectedJob.CheckedResultPath != "")
            {
                Task<List<string[]>> task = Task.Run(() => { return InitCheckedResultData(path); });
                return await task;
            }
            return new List<string[]>();
        }

        /// <summary>
        /// Read data from checked result backup file 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private List<string[]> InitCheckedResultData(string path)
        {
            List<string[]> result = new();
            if (!File.Exists(path))
            {
                _InitDataErrorList.Add(NotifyType.CheckedResultDoNotExist);
                return result;
            }
            try
            {
                Regex rexCsvSplitter = path.EndsWith(".csv") ? new Regex(@",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))") : new Regex(@"[\t]");
                using var reader = new StreamReader(path, Encoding.UTF8, true);
                bool isFirstline = false;
                while (!reader.EndOfStream)
                {
                    var data = reader.ReadLine();
                    if (data == null) { /*await Task.Delay(1);*/ continue; };

                    if (!isFirstline)
                    {
                        isFirstline = true; // Reject first line (header line)
                    }
                    else
                    {
                        string[] line = rexCsvSplitter.Split(data).Select(x => Csv.Unescape(x)).ToArray();
                        if (line.Length == 1 && line[0] == "") { /*await Task.Delay(1);*/ continue; }; // ignore empty line 
                        if (line.Length < _ColumnNames.Length)
                        {
                            string[] checkedResult = GetTheRightString(line);
                            result.Add(checkedResult);
                        }
                        else
                        {
                            result.Add(line); // Add checked result to list
                        }
                    }
                }
            }
            catch (IOException)
            {
                _InitDataErrorList.Add(NotifyType.CannotAccessCheckedResult);
            }
            catch (Exception)
            {
                _InitDataErrorList.Add(NotifyType.CheckedResultUnknownError);
            }
            return result;
        }

        private string[] GetTheRightString(string[] line)
        {
            var code = new string[_ColumnNames.Length];
            for (int i = 0; i < code.Length; i++)
            {
                if (i < line.Length)
                    code[i] = line[i];
                else
                    code[i] = "";
            }
            return code;
        }

        #endregion LOAD CHECKED RESULT LIST


        #region INIT COMPARE DATA

        private async Task InitCompareDataAsync(List<string[]> dbList, List<string[]> checkedResultList)
        {
            await Task.Run(() => { InitCompareData(dbList, checkedResultList); });
        }

        private void InitCompareData(List<string[]> dbList, List<string[]> checkedResultList)
        {
            HashSet<string> _ValidCheckedResultCodeSet = new();
            string validCond = ComparisonResult.Valid.ToString();
            int columnCount = 5;
            try
            {
                if (_SelectedJob == null) return;
                foreach (var result in checkedResultList)
                {
                    // Get the code content field and add it to the valid list
                    if (columnCount == result.Length && result[2] == validCond)
                    {
                        _ValidCheckedResultCodeSet.Add(result[1]);
                    }
                }
                if (dbList.Count > 0)
                {
                    for (int i = 0; i < dbList.Count; i++)
                    {
                        string[] rowData = dbList[i].ToArray();
                        string dataByPod = "";
                        dataByPod = GetCompareDataByPODFormat(rowData, _SelectedJob.PODFormat);

                        // If data has exist in _ValidCheckedResultCodeSet => status : checked = true
                        // Add data to _CodeListPODFormat
                        if (_ValidCheckedResultCodeSet.Contains(dataByPod)) // Compared POD data
                        {
                            bool tryAdd = _CodeListPODFormat.TryAdd(dataByPod, new SharedProgram.Controller.CompareStatus(i, true));
                            if (!tryAdd)
                            {
                                _ListPrintedCodeObtainFromFile[i][_DatabaseColunms.Length - 1] = "Duplicate"; // same key => duplicate code
                                _NumberOfDuplicate++;
                            }
                        }
                        else // Not yet compare POD data
                        {
                            bool tryAdd = _CodeListPODFormat.TryAdd(dataByPod, new SharedProgram.Controller.CompareStatus(i, false));
                            if (!tryAdd)
                            {
                                _ListPrintedCodeObtainFromFile[i][_DatabaseColunms.Length - 1] = "Duplicate";
                                _NumberOfDuplicate++;
                            }
                            if (_IsVerifyAndPrintMode)
                            {
                                string tmp = "";
                                for (int j = 1; j < rowData.Length - 1; j++)
                                {
                                    var tmpPOD = DeviceSharedValues.VPObject.PrintFieldForVerifyAndPrint.Find(x => x.Index == j);
                                    if (tmpPOD != null)
                                    {
                                        tmp += rowData[tmpPOD.Index];
                                    }
                                }
                                //     var tryAdd2 = _Emergency.TryAdd(tmp, i);
                            }
                        }


                    }
                }

                _ValidCheckedResultCodeSet.Clear();
            }
            catch (Exception)
            {
                _InitDataErrorList.Add(NotifyType.CannotCreatePodDataList);
            }
        }


        /// <summary>
        /// Transfer raw code to UI
        /// </summary>


        private static string GetCompareDataByPODFormat(string[] row, List<PODModel> pODFormat, int addingIndex = 0)
        {
            if (row.Length == 0) return "";
            var compareString = "";
            foreach (var item in pODFormat)
            {
                if (item.Type == PODModel.TypePOD.FIELD) // In case it is a FIELD column
                {
                    compareString += row[item.Index + addingIndex];
                }
                else if (item.Type == PODModel.TypePOD.TEXT) // In case of a custom text Column
                {
                    compareString += item.Value;
                }
            }
            return compareString;
        }

        #endregion INIT COMPARE DATA


    }
}
