using Cloudtoid;
using DipesLink.Languages;
using DipesLink.Models;
using DipesLink.Views.SubWindows;
using IPCSharedMemory;
using Microsoft.Win32;
using SharedProgram.Models;
using SharedProgram.Shared;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using static DipesLink.Views.Enums.ViewEnums;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLink.ViewModels
{
    /// <summary>
    /// Job Setting
    /// </summary>

    public partial class MainViewModel
    {

        private string _searchText = string.Empty;
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
        internal void UpdateSearchText(string text)
        {
            SearchText = text;
        }

        internal void LockUI(int stationIndex)
        {
            try
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    switch (JobList[stationIndex].OperationStatus)
                    {
                        case OperationStatus.Running:
                            JobList[stationIndex].IsLockUISetting = false;
                            break;
                        case OperationStatus.Processing:
                            JobList[stationIndex].IsLockUISetting = false;
                            break;
                        case OperationStatus.Stopped:
                            JobList[stationIndex].IsLockUISetting = true;
                            break;
                        default:
                            break;
                    }
                    if (JobSelection == null) return;
                    JobSelection.IsButtonOperationJobEnable = JobList[stationIndex].IsLockUISetting;
                    ConnectParamsList[stationIndex].IsLockUISetting = JobList[stationIndex].IsLockUISetting;
                    _ = LoadJobListActionAsync(stationIndex);
                });
            }
            catch (Exception)
            {
            }
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
                ImageExportPath = ImageExpPath,
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

                        CustomMessageBox checkJobNameMsgBox = new(LanguageModel.GetLanguage("Please_Input_Job_Name"), LanguageModel.GetLanguage("ErrorDialogCaption"), ButtonStyleMessageBox.OK, ImageStyleMessageBox.Error);
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
                                CustomMessageBox checkJobMsgBox = new(LanguageModel.GetLanguage("Please_Select_Database_Path"), LanguageModel.GetLanguage("ErrorDialogCaption"), ButtonStyleMessageBox.OK, ImageStyleMessageBox.Error);
                                checkJobMsgBox.ShowDialog();
                                return;
                            }


                            // Check Data compare format
                            if (_jobModel.DataCompareFormat == null || _jobModel.DataCompareFormat == "")
                            {
                                CustomMessageBox checkJobMsgBox = new(LanguageModel.GetLanguage("Please_Select_POD_Format"), LanguageModel.GetLanguage("ErrorDialogCaption"), ButtonStyleMessageBox.OK, ImageStyleMessageBox.Error);
                                checkJobMsgBox.ShowDialog();
                                return;
                            }

                            // Enter input freely for template (Only System User)
                            if (SharedFunctions.GetCurrentUsernameAndRole()?.FirstOrDefault()[0] == "System" && 
                                SharedFunctions.GetCurrentUsernameAndRole()?.FirstOrDefault()[1] == "Administrator" &&
                                string.IsNullOrEmpty(_jobModel.PrinterTemplate))
                            {
                                _jobModel.PrinterTemplate = CreateNewJob.SearchTemplateText;
                                _jobModel.TemplateListFirstFound = new List<string>();
                                _jobModel.TemplateListFirstFound.Add(_jobModel.PrinterTemplate);
                            }

                            // Check printer template
                            if (_jobModel.PrinterTemplate == null ||
                                _jobModel.PrinterTemplate == "" ||
                                _jobModel.TemplateListFirstFound == null ||
                                !SharedFunctions.CheckExitTemplate(_jobModel.PrinterTemplate, _jobModel.TemplateListFirstFound))
                            {
                                CustomMessageBox checkJobMsgBox = new(LanguageModel.GetLanguage("Please_Select_Printer_Template"), LanguageModel.GetLanguage("ErrorDialogCaption"), ButtonStyleMessageBox.OK, ImageStyleMessageBox.Error);
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

                        if (ImageExpPath == null || ImageExpPath == "" || !Directory.Exists(ImageExpPath))
                        {
                            CustomMessageBox checkJobMsgBox = new(LanguageModel.GetLanguage("Please_Select_Image_Export_Folder_Path"), LanguageModel.GetLanguage("ErrorDialogCaption"), ButtonStyleMessageBox.OK, ImageStyleMessageBox.Error);
                            checkJobMsgBox.ShowDialog();
                            return;
                        }
                    }

                    // Save Job to file
                    CustomMessageBox saveConfirm = new(LanguageModel.GetLanguage("Save_Job"), LanguageModel.GetLanguage("InfoDialogCaption"), ButtonStyleMessageBox.OKCancel, ImageStyleMessageBox.Info);
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
                                    CustomMessageBox checkJobMsgBox = new(LanguageModel.GetLanguage("Do_You_Want_Replace_Existing_Template"), LanguageModel.GetLanguage("ErrorDialogCaption"), ButtonStyleMessageBox.OKCancel, ImageStyleMessageBox.Info);
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
                        CustomMessageBox saveJobSuccMsgBox = new(LanguageModel.GetLanguage("Save_Job_Done"), LanguageModel.GetLanguage("InfoDialogCaption"), ButtonStyleMessageBox.OK, ImageStyleMessageBox.Info);
                        saveJobSuccMsgBox.ShowDialog();
                    }
                }
            }
            catch (Exception)
            {

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
            Task.Run(() => LoadJobListActionAsync(jobIndex));
        }

        private async Task LoadJobListActionAsync(object? obj)
        {
            if (obj == null) return;
            int index = (int)obj;
            numberOfSelectedJobList = 0;
            await Application.Current.Dispatcher.InvokeAsync(new Action(() =>
            {
                JobSelection?.JobFileList?.Clear();
                JobSelection?.SelectedJobFileList?.Clear();
                ObservableCollection<string> templateJobList = GetJobNameList(index);

                foreach (string templateJobName in templateJobList)
                {
                    JobModel? jobModel = SharedFunctions.GetJob(templateJobName, index);
                    if (jobModel != null && jobModel.JobStatus != JobStatus.Deleted) // Exclude deleted templates
                        JobSelection?.JobFileList?.Add(templateJobName);  // update ListTemplate to UI 
                }

                // Get from the selected Job list (contains only 1 job)
                ObservableCollection<string> templateSelectedJobList = SharedFunctions.GetSelectedJobNameList(index);
                JobSelection.SelectedJobFileList = new ObservableCollection<string>();
                foreach (var item in templateSelectedJobList)
                {
                    JobSelection.SelectedJobFileList.Add(item);
                }
                numberOfSelectedJobList = JobSelection.SelectedJobFileList.Count;

            }));
        }

        internal void DeleteJobAction(int jobIndex)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                JobModel? jobModel;
                if (JobSelection?.SelectedJob == null) return;
                try
                {
                    jobModel = SharedFunctions.GetJob(JobSelection.SelectedJob, jobIndex);
                    if (jobModel == null) return;
                    jobModel.JobStatus = JobStatus.Deleted;
                    string filePath = SharedPaths.PathSubJobsApp + (jobIndex + 1) + "\\" + jobModel.Name + SharedValues.Settings.JobFileExtension;

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

        internal void GetDetailInfoJobList(int jobIndex, bool isSelectedWorkJob = false)
        {
            JobModel? jobModel;
            if (!isSelectedWorkJob)
            {

                jobModel = SharedFunctions.GetJob(JobSelection?.SelectedJob, jobIndex);
            }
            else
            {
                ObservableCollection<string> templateSelectedJobList = SharedFunctions.GetSelectedJobNameList(jobIndex);
                jobModel = SharedFunctions.GetJobSelected(templateSelectedJobList.FirstOrDefault(), jobIndex);
            }

            if (jobModel == null || JobSelection == null) { JobSelection = new(); return; }

            JobSelection.Name = jobModel.Name;
            JobSelection.PrinterSeries = jobModel.PrinterSeries;
            JobSelection.JobType = jobModel.JobType;
            JobSelection.CompareType = jobModel.CompareType;
            JobSelection.StaticText = jobModel.StaticText;
            JobSelection.DatabasePath = jobModel.DatabasePath;
            JobSelection.DataCompareFormat = jobModel.DataCompareFormat;
            JobSelection.PrinterTemplate = jobModel.PrinterTemplate;
            JobSelection.CompleteCondition = jobModel.CompleteCondition;
            JobSelection.OutputCamera = jobModel.OutputCamera;
            JobSelection.IsImageExport = jobModel.IsImageExport;
            JobSelection.ImageExportPath = jobModel.ImageExportPath;
            JobSelection.TotalRecDb = jobModel.TotalRecDb;
            JobSelection.JobStatus = jobModel.JobStatus;
        }

        internal void AddSelectedJob(int jobIndex)
        {
            try
            {

                if (JobSelection.SelectedJob == null) return;
                DeleteSeletedJob(jobIndex);

                // Save the selected job 
                var selectJobName = JobSelection.SelectedJob;
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

        internal void SelectImageExportPath(string oldPath)
        {
            try
            {
                SharedFunctions.ShowFolderPickerDialog(oldPath, out string? folderPath);
                if (Directory.Exists(folderPath))
                {
                    ImageExpPath = folderPath;
                    ViewModelSharedValues.Settings.FailedImagePath = folderPath;
                    ViewModelSharedFunctions.SaveSetting();
                }
            }
            catch (Exception) { }
        }

        internal void AutoNamedForJob(int index)
        {
            try
            {
                SharedFunctions.AutoGenerateFileName(index, out string? jobName, ViewModelSharedValues.Settings.DateTimeFormat, ViewModelSharedValues.Settings.TemplateName);
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
