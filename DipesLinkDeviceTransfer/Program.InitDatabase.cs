using IPCSharedMemory;
using SharedProgram.DeviceTransfer;
using SharedProgram.Models;
using SharedProgram.Shared;
using System.Text;
using System.Text.RegularExpressions;
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

            if (SharedValues.SelectedJob == null)
            {
                return;
            }
            if (selectedJob.CompareType == CompareType.Database)
            {
                // Database list init
                MemoryTransfer.SendLoadingStatusToUI(_ipcDeviceToUISharedMemory_DT, JobIndex, DataConverter.ToByteArray(LoadDataStatus.StartLoad));
                Task<List<string[]>> initDatabaseTask = InitDatabaseAndPrintedStatusAsync(selectedJob);
                Task<List<string[]>> initCheckedResultTask = InitCheckedResultDataAsync(selectedJob);
                await Task.WhenAll(initDatabaseTask, initCheckedResultTask);
                SharedValues.ListPrintedCodeObtainFromFile = initDatabaseTask.Result;
                SharedValues.ListCheckedResultCode = initCheckedResultTask.Result;

                //Send Database to UI async
                Task transferDatabase = Task.Run(() =>
                MemoryTransfer.SendDatabaseToUIFirstTime(_ipcDeviceToUISharedMemory_DB, JobIndex, DataConverter.ToByteArray(SharedValues.ListPrintedCodeObtainFromFile)));

                Task transferCheckedDatabase = Task.Run(() => //Send checked db to UI
                    MemoryTransfer.SendCheckedDatabaseToUIFirstTime(_ipcDeviceToUISharedMemory_DB, JobIndex, DataConverter.ToByteArray(SharedValues.ListCheckedResultCode)));
                await Task.WhenAll(transferDatabase, transferCheckedDatabase);


#if DEBUG
                Console.Write("\nDatabase: {0} row", SharedValues.ListPrintedCodeObtainFromFile.Count - 1);
                Console.Write(", Printed: " + SharedValues.ListPrintedCodeObtainFromFile.Count(item => item.Last() == "Printed")); // show row number was printed
                Console.Write(" ,Checked: {0}", SharedValues.ListCheckedResultCode.Count);
#endif

                if (SharedValues.ListPrintedCodeObtainFromFile != null && SharedValues.ListPrintedCodeObtainFromFile.Count > 1)
                {
                    SharedValues.DatabaseColunms = SharedValues.ListPrintedCodeObtainFromFile[0];
                    SharedValues.ListPrintedCodeObtainFromFile.RemoveAt(0);
                    await InitVerifyAndPrindSendDataMethod();
                    if (SharedValues.SelectedJob.CompareType == CompareType.Database)
                    {
                        await InitCompareDataAsync(SharedValues.ListPrintedCodeObtainFromFile, SharedValues.ListCheckedResultCode);
                    }
                    SharedValues.TotalCode = SharedValues.ListPrintedCodeObtainFromFile.Count;
                    NumberPrinted = SharedValues.ListPrintedCodeObtainFromFile.Where(x => x[^1] == "Printed").Count();

                    if (SharedValues.ListPrintedCodeObtainFromFile != null)
                    {
                        string[]? foundItem = SharedValues.ListPrintedCodeObtainFromFile.Find(x => x[^1] == "Waiting");
                        if (foundItem != null)
                        {
                            int firstWaiting = SharedValues.ListPrintedCodeObtainFromFile.IndexOf(foundItem);
                            _CurrentPage = SharedValues.TotalCode > _MaxDatabaseLine ?
                                  (firstWaiting > 0 ? firstWaiting / _MaxDatabaseLine :
                                  (firstWaiting == 0 ? 0 : SharedValues.TotalCode / _MaxDatabaseLine - 1)) : 0;
                            string[] lastCode = SharedValues.ListPrintedCodeObtainFromFile[^1];
                        }
                        else
                        {
                            SharedFunctions.PrintConsoleMessage("Not found this waiting data !");
                        }
                    }
                    else
                    {
                        SharedFunctions.PrintConsoleMessage("Not found database list !");
                    }

                    // Notify input database is duplicate
                    if (_NumberOfDuplicate > 0)
                    {
                        NotificationProcess(NotifyType.DuplicateData);
                        SharedFunctions.PrintConsoleMessage($"Database Duplicate: {_NumberOfDuplicate} row");
                    }
                }
            }
            else // for mode No use database
            {
                SharedValues.ListCheckedResultCode = await InitCheckedResultDataAsync(selectedJob);
                Task transferCheckedDatabase = Task.Run(() => //Send checked db to UI
                    MemoryTransfer.SendCheckedDatabaseToUIFirstTime(_ipcDeviceToUISharedMemory_DB, JobIndex, DataConverter.ToByteArray(SharedValues.ListCheckedResultCode)));
            }

            TotalChecked = SharedValues.ListCheckedResultCode.Count;
            NumberOfCheckPassed = SharedValues.ListCheckedResultCode.Where(x => x[2] == "Valid").Count();
            NumberOfCheckFailed = TotalChecked - NumberOfCheckPassed;

#if DEBUG
            await Console.Out.WriteLineAsync($"\nTotal checked: {TotalChecked}, Total passed: {NumberOfCheckPassed}, Total failed: {NumberOfCheckFailed}\n");
#endif
        }


        #region LOAD DB AND PRINTED LIST
        private async Task<List<string[]>> InitDatabaseAndPrintedStatusAsync(JobModel jobModel)
        {
            if (jobModel == null) return new List<string[]>();
            try
            {
                string? pathDatabase = jobModel.DatabasePath;
                string pathBackupPrintedResponse = SharedPaths.PathPrintedResponse + $"Job{JobIndex + 1}\\" + jobModel.PrintedResponePath;
                List<string[]> tempDatabaseWithStatus = await Task.Run(() => { return InitDatabase(pathDatabase); });
                if (jobModel.PrintedResponePath != "" && File.Exists(jobModel.DatabasePath) && tempDatabaseWithStatus.Count > 1)
                {
                    await Task.Run(() => { InitPrintedStatus(pathBackupPrintedResponse, tempDatabaseWithStatus); });
                }

                return tempDatabaseWithStatus;
            }
            catch (Exception)
            {
                return new List<string[]>();
            }

        }
        public List<string[]> InitDatabase(string? csvPath)
        {
            List<(int index, string[] data)> result = new(); // List to store index and data
            if (csvPath == null || !File.Exists(csvPath))
            {
             
                _InitDataErrorList.Add(NotifyType.DatabaseDoNotExist);
                return result.Select(t => t.data).ToList();
            }
            try
            {
                string[] lines = File.ReadAllLines(csvPath);
                int columnCount = 0;
                if (lines.Length > 0)
                {
                    string[] firstLine = SharedFunctions.SplitLine(lines[0], csvPath.EndsWith(".csv"));
                    columnCount = firstLine.Length + 2;
                    string[] headerRow = new string[columnCount];
                    headerRow[0] = "Index";
                    headerRow[^1] = "Status";
                    for (int i = 1; i < headerRow.Length - 1; i++)
                    {
                        headerRow[i] = firstLine[i - 1] + $" - Field{i}";
                    }
                    result.Add((0, headerRow));
                }

                Parallel.ForEach(lines.Skip(1), (line, state, index) =>
                {
                    string[] columns = SharedFunctions.SplitLine(line, csvPath.EndsWith(".csv"));
                    string[] row = new string[columnCount];
                    row[0] = (index + 1).ToString();
                    row[^1] = "Waiting";
                    for (int i = 1; i < row.Length - 1; i++)
                    {
                        row[i] = i - 1 < columns.Length ? Csv.Unescape(columns[i - 1]) : "";
                    }
                    lock (result)
                    {
                        result.Add(((int)index + 1, row));
                    }
                });
            }
            catch (IOException) 
            { 
                _InitDataErrorList.Add(NotifyType.CannotAccessDatabase); 
            }
            catch (Exception)
            { 
                _InitDataErrorList.Add(NotifyType.DatabaseUnknownError); 
            }

            List<string[]> sortedAndTransformed = result.AsParallel()
                                 .OrderBy(item => item.index)
                                 .Select(item => item.data)
                                 .ToList();
            return sortedAndTransformed;
        }
        public void InitPrintedStatus(string pathBackupPrinted, List<string[]> dbList)
        {
            if (!File.Exists(pathBackupPrinted))
            {
                _InitDataErrorList.Add(NotifyType.PrintedResponseDoNotExist);
                return;
            }
            try
            {
                // Use FileStream with buffering
                using FileStream fs = new(pathBackupPrinted, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                using StreamReader reader = new(fs, Encoding.UTF8, true);

                // Read all lines at once
                string[] lines = reader.ReadToEnd().Split(Environment.NewLine);

                if (lines.Length < 2) return; // If there are less than 2 lines, there's nothing to process

                // Skip the first line (header) and process the rest in parallel
                Parallel.For(1, lines.Length, i =>
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) return;
                    string line = lines[i];
                    string[] columns = line.Split(',');
                    if (columns.Length > 0)
                    {
                        string indexString = Csv.Unescape(columns[0]);
                        if (int.TryParse(indexString, out int index))
                        {
                            dbList[index][^1] = "Printed"; // Get rows by index and update the last column with "Printed"
                        }
                    }
                });

            }
            catch (IOException)
            {
                _InitDataErrorList.Add(NotifyType.CannotAccessPrintedResponse);
            }
            catch (Exception)
            {
                _InitDataErrorList.Add(NotifyType.PrintedStatusUnknownError);
            }
        }
        #endregion  LOAD DB AND PRINTED LIST


        #region LOAD CHECKED RESULT LIST

        private async Task<List<string[]>> InitCheckedResultDataAsync(JobModel selectedJob)
        {
            if(selectedJob == null) return new List<string[]>();
            try
            {
                string path = SharedPaths.PathCheckedResult + $"Job{JobIndex + 1}\\" + selectedJob.CheckedResultPath;
                if (selectedJob.CheckedResultPath != "")
                {
                    return await Task.Run(() => { return InitCheckedResultData(path); });
                }
                return new List<string[]>();
            }
            catch (Exception)
            {
                return new List<string[]>();
            }
        }

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
                    if (data == null) { continue; };

                    if (!isFirstline)
                    {
                        isFirstline = true;
                    }
                    else
                    {
                        string[] line = rexCsvSplitter.Split(data).Select(x => Csv.Unescape(x)).ToArray();
                        if (line.Length == 1 && line[0] == "") { continue; };
                        if (line.Length < SharedValues.ColumnNames.Length)
                        {
                            string[] checkedResult = GetTheRightString(line);
                            result.Add(checkedResult);
                        }
                        else
                        {
                            result.Add(line);
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

        private static string[] GetTheRightString(string[] line)
        {
            try
            {
                string[] code = new string[SharedValues.ColumnNames.Length];
                for (int i = 0; i < code.Length; i++)
                {
                    code[i] = (i < line.Length) ? line[i] : "";
                }
                return code;
            }
            catch (Exception)
            {
                return Array.Empty<string>();
            }
         
        }

        #endregion LOAD CHECKED RESULT LIST


        #region INIT COMPARE DATA
        private async Task InitCompareDataAsync(List<string[]> dbList, List<string[]> checkedResultList)
        {
            await Task.Run(() => { InitCompareData(dbList, checkedResultList); });
        }
        private void InitCompareData(List<string[]> dbList, List<string[]> checkedResultList)
        {
            _NumberOfDuplicate = 0;
            HashSet<string> _ValidCheckedResultCodeSet = new();
            string validCond = ComparisonResult.Valid.ToString();
            int columnCount = 5;
            try
            {
                if (SharedValues.SelectedJob == null) return;
                foreach (var result in checkedResultList)
                {
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
                        dataByPod = SharedFunctions.GetCompareDataByPODFormat(rowData, SharedValues.SelectedJob.PODFormat);

                        if (_ValidCheckedResultCodeSet.Contains(dataByPod))
                        {
                            bool tryAdd = _CodeListPODFormat.TryAdd(dataByPod, new SharedProgram.Controller.CompareStatus(i, true));
                            if (!tryAdd)
                            {
                                SharedValues.ListPrintedCodeObtainFromFile[i][SharedValues.DatabaseColunms.Length - 1] = "Duplicate";
                                _NumberOfDuplicate++;
                            }
                        }
                        else
                        {
                            bool tryAdd = _CodeListPODFormat.TryAdd(dataByPod, new SharedProgram.Controller.CompareStatus(i, false));
                            if (!tryAdd)
                            {
                                SharedValues.ListPrintedCodeObtainFromFile[i][SharedValues.DatabaseColunms.Length - 1] = "Duplicate";
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

        #endregion INIT COMPARE DATA

    }
}
