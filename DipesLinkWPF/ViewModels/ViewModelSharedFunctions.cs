using DipesLink.Models;
using DipesLink.Views.Extension;
using SharedProgram.Models;
using SharedProgram.Shared;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using static DipesLink.Views.Enums.ViewEnums;

namespace DipesLink.ViewModels
{
    public class ViewModelSharedFunctions
    {
        /// <summary>
        /// Load SettingsModel from file
        /// </summary>
        public static void LoadSetting()
        {
			try
			{
               
                string fullFilePath = SharedPaths.GetSettingPath();
                SettingsModel? loadSettingsModel = SettingsModel.LoadSetting(fullFilePath);

                if(loadSettingsModel != null)
                {
                    loadSettingsModel.SystemParamsList ??= new List<ConnectParamsModel>(loadSettingsModel.NumberOfStation);
                    var numberStationGetFromFile = loadSettingsModel.SystemParamsList.Count;
                    var numberOfStationDesire = loadSettingsModel.NumberOfStation;
                    var numberAdjust = Math.Abs(numberOfStationDesire - numberStationGetFromFile);
                    bool isAdd = false;
                    if(numberOfStationDesire> numberStationGetFromFile)
                    {
                        isAdd = true;
                    }
                    if (isAdd)
                    {
                        for (int i = 0; i < numberAdjust; i++)
                        {
                            var item = new ConnectParamsModel();
                            loadSettingsModel.SystemParamsList.Add(item);
                        }
                    }
                 
                    ViewModelSharedValues.Settings = loadSettingsModel;
                }
                else
                {
                    InitDefaultSettings();
                }
            }
			catch (Exception)
			{
                InitDefaultSettings();
            }
        }

        private static void InitDefaultSettings() 
        {
            var item = new ConnectParamsModel();
            var initSettings = new SettingsModel();
            initSettings.SystemParamsList.Add(item);
            ViewModelSharedValues.Settings = initSettings;
            ViewModelSharedValues.Settings.NumberOfStation = 1;
        }

        /// <summary>
        /// Save SettingsModel to file
        /// </summary>
        /// <returns></returns>
        public static bool SaveSetting()
        {
            try
            {
                string fullFilePath = SharedPaths.GetSettingPath();
                ViewModelSharedValues.Settings.SaveJobFile(fullFilePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Init Device Transfer (Start new Process)
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static int InitDeviceTransfer(int i)
        {
            try
            {
                string fullPath = SharedPaths.AppPath + SharedValues.DeviceTransferName;
                JobModel? jobModel = GetJobById(i);
                string arguments = "";
                arguments += i;
                return SharedFunctions.DeviceTransferStartProcess(i, fullPath, arguments); // save transfer ID 
            }
            catch (Exception)
            {
                Debug.WriteLine("Init Device Transfer Fail");
                return 0;
            }
        }

        /// <summary>
        /// Get selected job by index
        /// </summary>
        /// <param name="jobIndex"></param>
        /// <returns></returns>
        public static JobModel? GetJobById(int jobIndex)
        {
            try
            {
                string folderPath = SharedPaths.PathSelectedJobApp + $"Job{jobIndex + 1}"; //Find selected job file by index
                if (!Directory.Exists(folderPath)) { Directory.CreateDirectory(folderPath); }
                DirectoryInfo dir = new(folderPath);
                string fileExtension = string.Format("*{0}", ".rvis"); // Get FileName with extension
                FileInfo[] files = dir.GetFiles(fileExtension).OrderByDescending(x => x.CreationTime).ToArray();
                string? fileNameWithExtension = files?.FirstOrDefault()?.Name;
                JobModel? jobModel = SharedFunctions.GetJobSelected(fileNameWithExtension, jobIndex); // Get job instance by filename
                return jobModel;
            }
            catch (Exception) { return null; }
        }

        public static void KillDeviceTransferByIndex(int index = -1)
        {
            SharedFunctions.DeviceTransferKillProcess(index);
            //if (index < 0) //Kill all process
            //{
            //    for (int i = 0; i < ViewModelSharedValues.Running.NumberOfStation; i++)
            //    {
            //        SharedFunctions.DeviceTransferKillProcess(ViewModelSharedValues.Running.StationList[i].TransferID);
            //    }
            //}
            //else // kill by id
            //{
            //    SharedFunctions.DeviceTransferKillProcess(ViewModelSharedValues.Running.StationList[index].TransferID);
            //}
        }
        public static Task RestartDeviceTransfer(JobOverview? job)
        {
            // Thread t = new Thread(new ThreadStart(() => { })); MultiThread

            return Task.Run(() => 
             {
                 KillDeviceTransferByIndex(job.DeviceTransferID); // kill old process
                //await Task.Delay(5000); // 0.5s
                job.DeviceTransferID = InitDeviceTransfer(job.Index); // start new process
             });
        }

        
    }
}
