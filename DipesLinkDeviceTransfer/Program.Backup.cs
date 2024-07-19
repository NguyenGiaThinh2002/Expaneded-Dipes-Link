using SharedProgram.Shared;
using System.Drawing;
using System.Text;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLinkDeviceTransfer
{
    public partial class Program
    {

        private readonly string _DateTimeFormat = "yyMMddHHmmss";

        private async void ExportCheckedResultToFileAsync()
        {
            if (SharedValues.SelectedJob == null) return;
            _CTS_BackupCheckedResult = new();
            var token = _CTS_BackupCheckedResult.Token;

            await Task.Run(() =>
            {
                //CREATE NEW PATH if not exist
                if (SharedValues.SelectedJob.CheckedResultPath == "")
                {
                    string fileNameCheckedRes = DateTime.Now.ToString(_DateTimeFormat) + "_" + SharedValues.SelectedJob.Name; // Create file name
                    string jobPath = SharedPaths.PathSubJobsApp + $"{JobIndex + 1}\\" + SharedValues.SelectedJob.Name + SharedValues.Settings.JobFileExtension; // Path for job file
                    string selectedJobPath = SharedPaths.PathSelectedJobApp + $"Job{JobIndex + 1}\\" + SharedValues.SelectedJob.Name + SharedValues.Settings.JobFileExtension; // Path for job file
                    string pathCheckedRes = SharedPaths.PathCheckedResult + $"Job{JobIndex + 1}\\" + fileNameCheckedRes + ".csv"; // path for Checked Result

                    if (!File.Exists(pathCheckedRes))
                    {
                        // If not found checked Result path then create new file with column template
                        using StreamWriter streamWriter = new(pathCheckedRes, true, new UTF8Encoding(true));
                        streamWriter.WriteLine(string.Join(",", SharedValues.ColumnNames));
                    }
                    //Save Job
                    SharedValues.SelectedJob.CheckedResultPath = fileNameCheckedRes + ".csv";

                    SharedValues.SelectedJob.SaveJobFile(jobPath);
                    SharedValues.SelectedJob.SaveJobFile(selectedJobPath);
                }
                try
                {
                    string checkedResultPath = SharedPaths.PathCheckedResult + $"Job{JobIndex + 1}\\" + SharedValues.SelectedJob.CheckedResultPath;
                    while (true)
                    {
                        // Only stop if all data is handled
                        if (token.IsCancellationRequested)
                            if (_QueueBufferBackupCheckedResult.IsEmpty)
                                token.ThrowIfCancellationRequested();

                        _ = _QueueBufferBackupCheckedResult.TryDequeue(out var valueArr);
                        if (valueArr == null) { Thread.Sleep(1); continue; };
                        if (valueArr.Count > 0)
                        {
                            SaveResultToFile(valueArr, checkedResultPath);
                        }
                        valueArr.Clear();
                        Thread.Sleep(5);
                    }
                }
                catch (OperationCanceledException)
                {
#if DEBUG
                    Console.WriteLine("ExportCheckedResultToFileAsync Task was Canceled !");
#endif
                }
                catch (Exception)
                {
#if DEBUG
                    Console.WriteLine("ExportCheckedResultToFileAsync was Failed !");
#endif
                }
            });
        }

        private async void ExportPrintedResponseToFileAsync()
        {
            if (SharedValues.SelectedJob == null) return;
            _CTS_BackupPrintedResponse = new();
            var token = _CTS_BackupPrintedResponse.Token;

            await Task.Run(async () =>
            {

                if (SharedValues.SelectedJob.PrintedResponePath == "") // if not exist path then create new 
                {
                    string fileNamePrintedResponse = DateTime.Now.ToString(_DateTimeFormat) + "_Printed_" + SharedValues.SelectedJob.Name;
                    string jobPath = SharedPaths.PathSubJobsApp + $"{JobIndex + 1}\\" + SharedValues.SelectedJob.Name + SharedValues.Settings.JobFileExtension;
                    string selectedJobPath = SharedPaths.PathSelectedJobApp + $"Job{JobIndex + 1}\\" + SharedValues.SelectedJob.Name + SharedValues.Settings.JobFileExtension; //
                    string printedResponsePath = SharedPaths.PathPrintedResponse + $"Job{JobIndex + 1}\\" + fileNamePrintedResponse + ".csv";

                    if (!File.Exists(printedResponsePath))
                    {
                        using StreamWriter streamWriter = new(printedResponsePath, true, new UTF8Encoding(true));
                        streamWriter.WriteLine(string.Join(",", SharedValues.DatabaseColunms));
                    }
                    SharedValues.SelectedJob.PrintedResponePath = fileNamePrintedResponse + ".csv";
                    SharedFunctions.SaveStringOfPrintedResponePath(
                          SharedPaths.PathSubJobsApp + $"{JobIndex + 1}\\",
                          "printedPathString",
                          SharedValues.SelectedJob.PrintedResponePath);
                    SharedValues.SelectedJob.SaveJobFile(jobPath);
                    SharedValues.SelectedJob.SaveJobFile(selectedJobPath);
                }
                try
                {
                    string printedResponsePath = SharedPaths.PathPrintedResponse + $"Job{JobIndex + 1}\\" + SharedValues.SelectedJob.PrintedResponePath;
                    while (true)
                    {
                        // Only stop if handled all data
                        if (token.IsCancellationRequested)
                            if (_QueueBufferBackupPrintedCode.IsEmpty)
                                token.ThrowIfCancellationRequested();

                        _ = _QueueBufferBackupPrintedCode.TryDequeue(out List<string[]>? valueArr);
                        if (valueArr == null) { Thread.Sleep(1); continue; };
                        if (valueArr.Count > 0)
                        {
                            SaveResultToFile(valueArr, printedResponsePath);
                        }
                        valueArr.Clear();
                        await Task.Delay(1, token);
                    }
                }
                catch (OperationCanceledException)
                {
#if DEBUG
                    Console.WriteLine("ExportPrintedResponseToFileAsync Thread was Canceled !");
#endif
                }
                catch (Exception)
                {
#if DEBUG
                    Console.WriteLine("ExportPrintedResponseToFileAsync Thread Failed !");
#endif
                }
            });
        }

        private static void SaveResultToFile(List<string[]> list, string path)
        {
            try
            {
                using StreamWriter streamWriter = new(path, true, new UTF8Encoding(true));
                foreach (string[] strArr in list)       //Add row result
                {
                    streamWriter.WriteLine(string.Join(",", strArr.Select(x => Csv.Escape(x))));
                }
            }
            catch (Exception)
            {
                // Todo
            }
        }

        private async void ExportImagesToFileAsync()
        {
            _CTS_BackupFailedImage = new();
            var token = _CTS_BackupFailedImage.Token;
            await Task.Run(async () =>
             {
                 try
                 {
                     while (true)
                     {
                         if (token.IsCancellationRequested)
                         {
                             if (_QueueBackupFailedImage.IsEmpty)
                             {
                                 token.ThrowIfCancellationRequested();
                             }
                         }
                         if (_QueueBackupFailedImage.TryDequeue(out var exImageModel))
                         {
                             try
                             {
                                 string fileName = string.Format("\\{0}_Job_{1}_Image_{2:D7}.jpg",
                                 SharedValues.SelectedJob?.ExportNamePrefix,
                                 SharedValues.SelectedJob?.Name,
                                 exImageModel.Index);
                                 if (SharedValues.SelectedJob?.ImageExportPath != null && exImageModel.Image != null)
                                 {
                                     string path = SharedValues.SelectedJob?.ImageExportPath + $"Job{JobIndex + 1}\\" + SharedValues.SelectedJob?.Name + fileName;
                                     using (exImageModel.Image)
                                     {
                                         SaveBitmap(exImageModel.Image, path);
                                     }
                                 }
                             }
                             catch { }
                         }
                         await Task.Delay(5);
                     }

                 }
                 catch (OperationCanceledException)
                 {
#if DEBUG
                     Console.WriteLine("ExportImagesToFileAsync Task was Canceled !");
#endif
                 }
                 catch (Exception ex)
                 {
#if DEBUG
                     Console.WriteLine("save image failed: " + ex);
#endif
                 }
             });
        }

        public static void SaveBitmap(Bitmap bitmap, string filePath)
        {
            try
            {
                string? directory = System.IO.Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory) && directory != null)
                {
                    Directory.CreateDirectory(directory);
                }
                using var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                memoryStream.Position = 0;
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                memoryStream.CopyTo(fileStream);
            }
            catch (Exception) { }
        }

       
     
       

    }
}
