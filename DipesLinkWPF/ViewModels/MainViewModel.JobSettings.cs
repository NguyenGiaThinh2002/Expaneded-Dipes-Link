using Cloudtoid;
using DipesLink.Views.SubWindows;
using IPCSharedMemory;
using IPCSharedMemory.Datatypes;
using Microsoft.Win32;
using SharedProgram.Models;
using SharedProgram.Shared;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using static SharedProgram.DataTypes.CommonDataType;
using static DipesLink.Views.Enums.ViewEnums;
using DipesLink.Views.Extension;
using System;
using System.Windows.Media;
using System.Windows.Controls;
using DipesLink.Views.Models;
using DipesLink.Views.UserControls.MainUc;
using DipesLink.Models;
using System.Collections.Concurrent;

namespace DipesLink.ViewModels
{
    /// <summary>
    /// Job Setting
    /// </summary>

    public partial class MainViewModel
    {

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterListViewPrinterTemplate();
            }
        }

        internal void LockChoosingStation()
        {
            int t = 0;
            for (int i = 0; i < JobList.Count; i++)
                if (JobList[i].OperationStatus != OperationStatus.Stopped) t++;

            for (int i = 0; i < JobList.Count; i++)
                ConnectParamsList[i].LockChoosingStation = t <= 0;  // use ternary
        }
        internal void LockUI(int stationIndex)
        {
            switch (JobList[stationIndex].OperationStatus)
            {
                case OperationStatus.Running:
                    ConnectParamsList[stationIndex].LockUISetting = false;
                    break;
                case OperationStatus.Processing:
                    ConnectParamsList[stationIndex].LockUISetting = false;
                    break;
                case OperationStatus.Stopped:
                    ConnectParamsList[stationIndex].LockUISetting = true;
                    break;
                default:
                    break;
            }
            EnableButtons(ConnectParamsList[stationIndex].LockUISetting);
            LoadJobListAction(stationIndex);
        }

        #region Job Selection and create new
        private JobModel InitJobModel(int index)
        {
            var job = new JobModel
            {
                Index = index,
                Name = CreateNewJob.Name,
                PrinterSeries = CreateNewJob.PrinterSeries,
                JobType = CreateNewJob.JobType,
                CompareType = CreateNewJob.CompareType,
                StaticText = CreateNewJob.StaticText,
                DatabasePath = CreateNewJob.DatabasePath,
                DataCompareFormat = CreateNewJob.DataCompareFormat,
                TotalRecDb = CreateNewJob.TotalRecDb,
                PrinterTemplate = CreateNewJob.PrinterTemplate,
                TemplateListFirstFound = CreateNewJob.TemplateListFirstFound,
                PODFormat = CreateNewJob.PODFormat,
                CompleteCondition = CreateNewJob.CompleteCondition,
                OutputCamera = CreateNewJob.OutputCamera,
                IsImageExport = CreateNewJob.IsImageExport,
                ImageExportPath = CreateNewJob.ImageExportPath,
                PrinterIP = CreateNewJob.PrinterIP,
                PrinterPort = CreateNewJob.PrinterPort,
                PrinterWebPort = CreateNewJob.PrinterWebPort,
                CameraIP = CreateNewJob.CameraIP,
                ControllerIP = CreateNewJob.ControllerIP,
                ControllerPort = CreateNewJob.ControllerPort
            };
            return job;
        }

        internal void SaveJob(int jobIndex)
        {
            isSaveJob = false;
            try
            {
                _jobModel = InitJobModel(jobIndex);
                if (_jobModel != null)
                {
                    _jobModel.JobStatus = JobStatus.NewlyCreated;

                    // Check job name 
                    if (_jobModel.Name == null || _jobModel.Name == "")
                    {
                        CustomMessageBox checkJobNameMsgBox = new("Please input Job name", "Job create fail", ButtonStyleMessageBox.OK, ImageStyleMessageBox.Error);
                        checkJobNameMsgBox.ShowDialog();
                        return;
                    }

                    // Check params for PrinterSeries is R Printer
                    if (_jobModel.PrinterSeries == PrinterSeries.RynanSeries)
                    {
                        if (_jobModel.CompareType == CompareType.Database)
                        {
                            if (_jobModel.DatabasePath == null || _jobModel.DatabasePath == "")
                            {
                                CustomMessageBox checkJobMsgBox = new("Please select database path", "Job create fail", ButtonStyleMessageBox.OK, ImageStyleMessageBox.Error);
                                checkJobMsgBox.ShowDialog();
                                return;
                            }


                            // Check Data compare format
                            if (_jobModel.DataCompareFormat == null || _jobModel.DataCompareFormat == "")
                            {
                                CustomMessageBox checkJobMsgBox = new("Please select POD format", "Job create fail", ButtonStyleMessageBox.OK, ImageStyleMessageBox.Error);
                                checkJobMsgBox.ShowDialog();
                                return;
                            }

#if DEBUG
                            _jobModel.PrinterTemplate = "podtest";
                            _jobModel.TemplateListFirstFound = new List<string>();
                            _jobModel.TemplateListFirstFound.Add("podtest");
#endif
                            // Check printer template
                            if (_jobModel.PrinterTemplate == null ||
                                _jobModel.PrinterTemplate == "" ||
                                _jobModel.TemplateListFirstFound == null ||
                                !SharedFunctions.CheckExitTemplate(_jobModel.PrinterTemplate, _jobModel.TemplateListFirstFound))
                            {
                                CustomMessageBox checkJobMsgBox = new("Please select printer template", "Job create fail", ButtonStyleMessageBox.OK, ImageStyleMessageBox.Error);
                                checkJobMsgBox.ShowDialog();
                                return;
                            }
                        }
                    }
                    else
                    {
                        // todo
                    }

                    // Check Image Export Path 
                    if (_jobModel.IsImageExport == true)
                    {

                        if (_jobModel.ImageExportPath == null || _jobModel.ImageExportPath == "" || !Directory.Exists(_jobModel.ImageExportPath))
                        {
                            CustomMessageBox checkJobMsgBox = new("Please select image export folder path", "Job create fail", ButtonStyleMessageBox.OK, ImageStyleMessageBox.Error);
                            checkJobMsgBox.ShowDialog();
                            return;
                        }
                    }



                    // Save Job to file
                    CustomMessageBox saveConfirm = new("Save Job", "Confirm", ButtonStyleMessageBox.OKCancel, ImageStyleMessageBox.Info);
                    var isSaveConfirm = saveConfirm.ShowDialog();
                    if (isSaveConfirm == true)
                    {
                        // Check Job exist
                        if (SharedFunctions.CheckJobHasExist(_jobModel.Name, jobIndex))
                        {
                            JobModel? tempJob = SharedFunctions.GetJob(_jobModel.Name + SharedValues.Settings.JobFileExtension, jobIndex);
                            if (tempJob != null)
                            {
                                // Check deleted template
                                if (tempJob.JobStatus == JobStatus.Deleted)
                                {

                                    string oldJobPath = SharedPaths.PathJobsApp
                                        + _jobModel.Name
                                        + SharedValues.Settings.JobFileExtension;
                                    string newJobPath = SharedPaths.PathJobsApp
                                        + _jobModel.Name
                                        + "_Old_"
                                        + DateTime.Now.ToString("yyMMddHHmmss")
                                        + SharedValues.Settings.JobFileExtension;

                                    try
                                    {
                                        File.Move(oldJobPath, newJobPath); // Change File Name
                                    }
                                    catch { }
                                }
                                else
                                {
                                    CustomMessageBox checkJobMsgBox = new("Do you want replace existing template ?", "Job create fail", ButtonStyleMessageBox.OKCancel, ImageStyleMessageBox.Info);
                                    var isReplace = checkJobMsgBox.ShowDialog();
                                    if (isReplace == true) { } else return;
                                }
                            }
                        }

                        string filePath = SharedPaths.PathSubJobsApp + (jobIndex + 1) + "\\" + _jobModel.Name + SharedValues.Settings.JobFileExtension;
                        string selectedfilePath = SharedPaths.PathSelectedJobApp + $"Job{jobIndex + 1}" + "\\" + _jobModel.Name + SharedValues.Settings.JobFileExtension;

                        _jobModel.SaveJobFile(filePath); // Save in Job folder

                        if (File.Exists(selectedfilePath)) // Save in Selected Job folder
                        {
                            _jobModel.SaveJobFile(selectedfilePath);

                        }
                        isSaveJob = true;
                        CustomMessageBox saveJobSuccMsgBox = new("Save Job done !", "Notification", ButtonStyleMessageBox.OK, ImageStyleMessageBox.Info);
                        saveJobSuccMsgBox.ShowDialog();
                    }
                }
            }
            catch (Exception)
            {

            }

            switch (jobIndex)
            {
                case 0:

                    break;
            }
        }

        internal void BrowseDatabasePath()
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Title = "Select a Database file";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "CSV file (*.csv)|*.csv|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                string filename = openFileDialog.FileName;
                CreateNewJob.DatabasePath = filename;
            }
        }

        internal void LoadJobList(int jobIndex)
        {
            Thread ThreadLoadJobList = new(new ParameterizedThreadStart(LoadJobListAction));
            ThreadLoadJobList.Start(jobIndex);
        }

        public void EnableButtons(bool isEnable)
        {
            // thinh
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                SelectJob = new();
                SelectJob.IsButtonEnable = new();
                SelectJob.IsButtonEnable = isEnable;
            }));

            //ConnectParamsList[stationIndex].LockUISetting
        }
      
        private void LoadJobListAction(object? obj)
        {
            // thinh 
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                numberOfSelectedJobList = 0;
                int index = (int)obj;
                SelectJob = new();
                ObservableCollection<string> templateJobList = GetJobNameList(index);
                SelectJob.JobFileList = new();
                // thinh
                SelectJob.IsButtonEnable = new();
                SelectJob.IsButtonEnable = ConnectParamsList[index].LockUISetting;
                foreach (string templateJobName in templateJobList)
                {
                    JobModel? jobModel = SharedFunctions.GetJob(templateJobName, index);
                    if (jobModel != null && jobModel.JobStatus != JobStatus.Deleted) // Exclude deleted templates
                        SelectJob.JobFileList.Add(templateJobName);  // update ListTemplate to UI 
                }

                // Lấy trong danh sách Job được chọn (chỉ chứa 1 job)
                ObservableCollection<string> templateSelectedJobList = SharedFunctions.GetSelectedJobNameList(index);
                SelectJob.SelectedJobFileList = new ObservableCollection<string>();
                foreach (var item in templateSelectedJobList)
                {
                    SelectJob.SelectedJobFileList.Add(item);
                }
            
                numberOfSelectedJobList = SelectJob.SelectedJobFileList.Count;
                //if (SelectJob.SelectedJobFileList.Count <= 0)
                //{
                //    //SelectJob.JobType = ;

                //}

            }));
        }



        internal void DeleteJobAction(int jobIndex)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                JobModel? jobModel;
                if (SelectJob.SelectedJob == null) return;
                try
                {
                    jobModel = SharedFunctions.GetJob(SelectJob.SelectedJob, jobIndex);
                    if (jobModel == null) return;
                    jobModel.JobStatus = JobStatus.Deleted;
                    string filePath = SharedPaths.PathSubJobsApp + (jobIndex + 1) + "\\" + jobModel.Name + SharedValues.Settings.JobFileExtension;
                    // string selectedfilePath = SharedPaths.PathSelectedJobApp + $"Job{jobIndex + 1}" + "\\" + jobModel.Name + SharedValues.Settings.JobFileExtension;

                    var MsgBoxDelJob = CusMsgBox.Show("Do you want to delete this Job ? ", "Job Deletion", ButtonStyleMessageBox.OKCancel, ImageStyleMessageBox.Warning);
                    if (MsgBoxDelJob)
                    {
                        // Check Job Delete whether is selected job and Delete at the same time Selected Job
                        string folderPath = SharedPaths.PathSelectedJobApp + $"Job{jobIndex + 1}";
                        string[] files = Directory.GetFiles(folderPath);
                        foreach (string file in files)
                        {
                            if (jobModel.Name == Path.GetFileNameWithoutExtension(file))
                                File.Delete(file);
                        }
                        //Deleted
                        jobModel.SaveJobFile(filePath);
                    }

                }
                catch (Exception)
                {
                }
            }));
        }

        private static ObservableCollection<string> GetJobNameList(int jobIndex)
        {
            try
            {
                string folderPath = SharedPaths.PathSubJobsApp + $"{jobIndex + 1}";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                DirectoryInfo dir = new(folderPath);
                string strFileNameExtension = string.Format("*{0}", SharedValues.Settings.JobFileExtension);
                FileInfo[] files = dir.GetFiles(strFileNameExtension).OrderByDescending(x => x.CreationTime).ToArray();
                ObservableCollection<string> result = new();
                foreach (FileInfo file in files)
                {
                    result.Add(file.Name);
                }
                return result;
            }
            catch (Exception) { return new ObservableCollection<string>(); }
        }


        internal void GetDetailJobList(int jobIndex, bool isSelectedWorkJob = false)
        {
            JobModel? jobModel;
            if (!isSelectedWorkJob)
            {
                jobModel = SharedFunctions.GetJob(SelectJob.SelectedJob, jobIndex);
            }
            else
            {
                jobModel = SharedFunctions.GetJobSelected(SelectJob.SelectedWorkJob, jobIndex);
            }

            if (jobModel == null) return;
            // thinh

            SelectJob.Name = jobModel.Name;
            SelectJob.PrinterSeries = jobModel.PrinterSeries;
            SelectJob.JobType = jobModel.JobType;
            SelectJob.CompareType = jobModel.CompareType;
            SelectJob.StaticText = jobModel.StaticText;
            SelectJob.DatabasePath = jobModel.DatabasePath;
            SelectJob.DataCompareFormat = jobModel.DataCompareFormat;
            SelectJob.PrinterTemplate = jobModel.PrinterTemplate;
            SelectJob.CompleteCondition = jobModel.CompleteCondition;
            SelectJob.OutputCamera = jobModel.OutputCamera;
            SelectJob.IsImageExport = jobModel.IsImageExport;
            SelectJob.ImageExportPath = jobModel.ImageExportPath;
            SelectJob.TotalRecDb = jobModel.TotalRecDb;
            SelectJob.JobStatus = jobModel.JobStatus;
        }


        // Thêm selected job vào thư mục 
        internal void AddSelectedJob(int jobIndex)
        {
            try
            {
                //if (SelectJob.SelectedJob == null) return;
                //// Empty the folder 
                //string folderPath = SharedPaths.PathSelectedJobApp + $"Job{jobIndex + 1}";
                //if (!Directory.Exists(folderPath))
                //{
                //    Directory.CreateDirectory(folderPath);
                //}
                //else
                //{
                //    string[] files = Directory.GetFiles(folderPath);
                //    foreach (string file in files)
                //    {
                //        File.Delete(file);
                //    }
                //}
                if (SelectJob.SelectedJob == null) return;
                DeleteSeletedJob(jobIndex);
                
                // Save the selected job 
                var selectJobName = SelectJob.SelectedJob;
                JobModel? _jobModel = SharedFunctions.GetJob(selectJobName, jobIndex);
                string filePath = SharedPaths.PathSelectedJobApp + $"Job{jobIndex + 1}\\" + _jobModel?.Name + SharedValues.Settings.JobFileExtension; // Save Job
                _jobModel?.SaveJobFile(filePath);
            }
            catch (Exception) { }
        }

        internal void DeleteSeletedJob(int stationIndex)
        {
            try
            {
                // Empty the folder 
                string folderPath = SharedPaths.PathSelectedJobApp + $"Job{stationIndex + 1}";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                else
                {
                    string[] files = Directory.GetFiles(folderPath);
                    foreach (string file in files) { File.Delete(file); }
                }
                JobList[stationIndex].CurrentCodeData = ""; 
                JobList[stationIndex].CompareResult = ComparisonResult.None;
                JobList[stationIndex].ProcessingTime = 0;
                
                JobList[stationIndex].SentDataNumber = "0";  
                JobList[stationIndex].ReceivedDataNumber = "0";
                JobList[stationIndex].PrintedDataNumber = "0";

                JobOverview jobOverview = new()
                {
                    SentDataNumber = "0",
                    CompareResult = ComparisonResult.None,
                    ProcessingTime = 0
                };
            }
            catch (Exception) { }
        }




        internal void SelectImageExportPath()
        {
            try
            {
                SharedFunctions.ShowFolderPickerDialog(out string? folderPath);
                CreateNewJob.ImageExportPath = folderPath;
            }
            catch (Exception) { }

        }

        internal void AutoNamedForJob(int index)
        {
            try
            {
                SharedFunctions.AutoGenerateFileName(index, out string? jobName);
                CreateNewJob.Name = jobName;
            }
            catch (Exception) { }

        }

        internal void RefreshTemplatName(int stationIndex)
        {
            try
            {
                byte[] indexBytes = SharedFunctions.StringToFixedLengthByteArray(stationIndex.ToString(), 1);
                byte[] actionTypeBytes = SharedFunctions.StringToFixedLengthByteArray(((int)ActionButtonType.ReloadTemplate).ToString(), 1);
                byte[] combineBytes = SharedFunctions.CombineArrays(indexBytes, actionTypeBytes);
                MemoryTransfer.SendActionButtonToDevice(listIPCUIToDevice1MB[stationIndex], stationIndex, combineBytes);
            }
            catch (Exception) { }
        }

        internal void SaveAllJob()
        {
            ViewModelSharedFunctions.SaveSetting();
        }

        private void FilterListViewPrinterTemplate()
        {
            if (CreateNewJob.TemplateListFirstFound != null)
            {
                IEnumerable<string> filteredItems;
                if (SearchText != "")
                {
                    filteredItems = CreateNewJob.TemplateListFirstFound.Cast<string>().Where(x => x.ToLower().StartsWithOrdinal(SearchText.ToLower()));
                    CreateNewJob.TemplateList = filteredItems.ToList();
                }
                else
                {
                    CreateNewJob.TemplateList = CreateNewJob.TemplateListFirstFound;
                }
            }
        }
        #endregion
    }
}
