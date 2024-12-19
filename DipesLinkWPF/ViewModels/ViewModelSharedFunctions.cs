using DipesLink.Models;
using DipesLink.Views.Extension;
using SharedProgram.Models;
using SharedProgram.Shared;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
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

                if (loadSettingsModel != null)
                {
                    loadSettingsModel.SystemParamsList ??= new List<ConnectParamsModel>(loadSettingsModel.NumberOfStation);
                    var numberStationGetFromFile = loadSettingsModel.SystemParamsList.Count;
                    var numberOfStationDesire = loadSettingsModel.NumberOfStation;
                    var numberAdjust = Math.Abs(numberOfStationDesire - numberStationGetFromFile);                  
                    bool isAdd = false;
                    if (numberOfStationDesire > numberStationGetFromFile)
                    {
                        isAdd = true;
                    }
                    if (isAdd)
                    {
                        for (int i = 0; i < numberAdjust; i++)
                        {
                            var item = new ConnectParamsModel();


                            item.PrinterIPs = Enumerable.Repeat("127.0.0.1", loadSettingsModel.NumberOfPrinter).ToList();
                            item.PrinterPorts = Enumerable.Repeat(2000.0, loadSettingsModel.NumberOfPrinter).ToList();

                            // Dưới đây là ví dụ ở trên
                            //item.PrinterIPs = new List<string>() { "127.0.0.1", "127.0.0.1", "127.0.0.1", "127.0.0.1" };
                            //item.PrinterPorts = new List<double>() { 2001, 2002, 2003, 2004 };

                            loadSettingsModel.SystemParamsList.Add(item);
                        }
                    }


                    for (int i = 0; i < loadSettingsModel.NumberOfStation; i++)
                    {
                        if (loadSettingsModel.SystemParamsList[i].PrinterIPs.Count < loadSettingsModel.NumberOfPrinter)
                        {
                            loadSettingsModel.SystemParamsList[i].PrinterIPs = Enumerable.Repeat("127.0.0.1", loadSettingsModel.NumberOfPrinter).ToList();
                            loadSettingsModel.SystemParamsList[i].PrinterPorts = Enumerable.Repeat(2000.0, loadSettingsModel.NumberOfPrinter).ToList();
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
            //item.PrinterIPs = new List<string>() { "127.0.0.1", "127.0.0.1", "127.0.0.1", "127.0.0.1" };
            // item.PrinterPorts = new List<double>() { 2001, 2002, 2003, 2004 };
            item.NumberOfPrinter = 1;
            item.PrinterIPs = Enumerable.Repeat("127.0.0.1", item.NumberOfPrinter).ToList();
            item.PrinterPorts = Enumerable.Repeat(2000.0, item.NumberOfPrinter).ToList();         
            item.IsCheckPrintersSettingsEnabled = new List<bool>() { true, false, true, false };

            var initSettings = new SettingsModel();
            initSettings.SystemParamsList.Add(item);
            ViewModelSharedValues.Settings = initSettings;            
            ViewModelSharedValues.Settings.NumberOfStation = 1;
            ViewModelSharedValues.Settings.NumberOfPrinter = 1;
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
