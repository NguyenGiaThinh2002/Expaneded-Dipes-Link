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
            if (SharedValues.SelectedJob == null) return;
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
                MemoryTransfer.SendDatabaseToUIFirstTime(_ipcDeviceToUISharedMemory_DB, 
                    JobIndex, 
                    DataConverter.ToByteArray(SharedValues.ListPrintedCodeObtainFromFile)));
                Task transferCheckedDatabase = Task.Run(() => //Send checked db to UI
                    MemoryTransfer.SendCheckedDatabaseToUIFirstTime(_ipcDeviceToUISharedMemory_DB, 
                    JobIndex, 
                    DataConverter.ToByteArray(SharedValues.ListCheckedResultCode)));
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
            else
            {
                SharedValues.ListCheckedResultCode = await InitCheckedResultDataAsync(selectedJob);
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
            string? pathDatabase = jobModel.DatabasePath;
            string pathBackupPrintedResponse = SharedPaths.PathPrintedResponse + $"Job{JobIndex + 1}\\" + jobModel.PrintedResponePath;

            List<string[]> tempDatabaseWithStatus = await Task.Run(() => { return SharedFunctions.InitDatabaseWithStatus(pathDatabase); });

            if (jobModel.PrintedResponePath != "" && File.Exists(jobModel.DatabasePath) && tempDatabaseWithStatus.Count > 1)
            {
                await Task.Run(() => { SharedFunctions.InitPrintedStatus(pathBackupPrintedResponse, tempDatabaseWithStatus); });
            }
            return tempDatabaseWithStatus;
        }

        #endregion  LOAD DB AND PRINTED LIST

        #region LOAD CHECKED RESULT LIST

        private async Task<List<string[]>> InitCheckedResultDataAsync(JobModel selectedJob)
        {
            string path = SharedPaths.PathCheckedResult + $"Job{JobIndex + 1}\\" + selectedJob.CheckedResultPath;                                                                                                      
            if (selectedJob.CheckedResultPath != "")
            {
                Task<List<string[]>> task = Task.Run(() => { return InitCheckedResultData(path); });
                return await task;
            }
            return new List<string[]>();
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

        private string[] GetTheRightString(string[] line)
        {
            string[] code = new string[SharedValues.ColumnNames.Length];
            for (int i = 0; i < code.Length; i++)
            {
                code[i] = (i < line.Length) ? line[i] : "";
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
