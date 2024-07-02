using Cognex.DataMan.SDK;
using SharedProgram.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml;

namespace SharedProgram.Shared
{
    public class SharedFunctions
    {
        public static JobModel? GetJob(string? templateNameWithExtension, int jobIndex)
        {
            string filePath = $"{SharedPaths.PathSubJobsApp}{jobIndex + 1}\\{templateNameWithExtension}";
            return JobModel.LoadFile(filePath);
        }

        /// <summary>
        /// Get all file name job in selected job path (usually there is only one job)
        /// </summary>
        /// <param name="jobIndex"></param>
        /// <returns></returns>
        public static ObservableCollection<string> GetSelectedJobNameList(int jobIndex)
        {
            try
            {
                string folderPath = SharedPaths.PathSelectedJobApp + $"Job{jobIndex + 1}";
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

        /// <summary>
        /// Get selected job from selected job path
        /// </summary>
        /// <param name="templateNameWithExtension"></param>
        /// <param name="jobIndex"></param>
        /// <returns></returns>
        public static JobModel? GetJobSelected(string? templateNameWithExtension, int jobIndex)
        {
            string filePath = $"{SharedPaths.PathSelectedJobApp}{"Job" + (jobIndex + 1)}\\{templateNameWithExtension}";
            return JobModel.LoadFile(filePath);
        }

        public static bool CheckJobHasExist(string? templateNameWithoutExtension, int jobIndex)
        {
            string filePath = $"{SharedPaths.PathSubJobsApp}{jobIndex + 1}\\{templateNameWithoutExtension}{SharedValues.Settings.JobFileExtension}";
            return File.Exists(filePath);
        }
        public static bool CheckExitTemplate(string printerTemplate, List<string> compareList)
        {
            try
            {
                if (compareList.Count <= 0)
                {
                    return false;
                }
                if (compareList.Contains(printerTemplate))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {
                return false;
            }

        }

        public static string GetReadStringFromResultXml(string resultXml)
        {
            try
            {
                XmlDocument? doc = new();
                doc.LoadXml(resultXml);
                XmlNode? fullStringNode = doc.SelectSingleNode("result/general/full_string");
                if (fullStringNode != null)
                {
                    XmlAttribute? encoding = fullStringNode?.Attributes?["encoding"];
                    if (encoding != null && encoding.InnerText == "base64")
                    {
                        if (!string.IsNullOrEmpty(fullStringNode?.InnerText))
                        {
                            byte[] code = Convert.FromBase64String(fullStringNode.InnerText);
                            return Encoding.UTF8.GetString(code, 0, code.Length);
                        }
                        else { return ""; }
                    }
                    return fullStringNode.InnerText;
                }
            }
            catch (Exception) { }
            return "";
        }

        public static Image GetImageFromImageByte(byte[]? inputImgData)
        {
            return ImageArrivedEventArgs.GetImageFromImageBytes(inputImgData);
        }
        public static BitmapImage ConvertToBitmapImage(Image image)
        {
            if (image == null) return new BitmapImage();
            try
            {
                using (MemoryStream memoryStream = new())
                {
                    image.Save(memoryStream, image.RawFormat);
                    memoryStream.Position = 0;
                    BitmapImage bitmapImage = new();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); //Important to be able to use bitmapImage on different threads.
                    return bitmapImage;
                }
            }
            catch (Exception)
            {
                return new BitmapImage();
            }

        }
        public static int DeviceTransferStartProcess(int index, string fullPath, string arguments)
        {
            try
            {
                int processID = 0;
                ProcessStartInfo? startInfo = new()
                {
                    FileName = fullPath,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    // Comment out this line if admin privileges are not necessary
                    // Verb = "runas",
                    Arguments = arguments,
                    CreateNoWindow = true, // Ensures no window is created
                    UseShellExecute = false // Ensure using shell execute is disabled
                };
#if DEBUG
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                startInfo.CreateNoWindow = false;
#endif
                if (startInfo != null)
                {
                    Process? process = Process.Start(startInfo);
                    if (process != null)
                    {
                        processID = process.Id;
                    }
                }
                return processID;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Kill process by process id
        /// </summary>
        /// <param name="id"></param>
        public static void DeviceTransferKillProcess(int id)
        {
            try
            {
                Process processes = Process.GetProcessById(id);
                processes.Kill();
            }
            catch (Exception) { }
        }

        public static bool IsValidIPAddress(string ipString, out IPAddress? ipd)
        {
          
            bool isValid = IPAddress.TryParse(ipString, out ipd);
            return isValid;
        }

        public static void ShowFolderPickerDialog(out string? folderPath)
        {
            folderPath = null;
            using var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                folderPath = dialog.SelectedPath + "\\";
            }
        }
        public static void AutoGenerateFileName(int index,out string? jobName)
        {
            jobName = string.Format("{1}_{0}", DateTime.Now.ToString("yyyyMMdd_HHmmss"), $"JobFile_{index+1}");
        }

        public static byte[] StringToFixedLengthByteArray(string inputString, int fixedLength)
        {
            byte[] byteArray;
            // Determine the string length, if the string is longer than a fixed length, truncate it, if shorter, add whitespace characters
            string truncatedString = inputString.Length > fixedLength ? inputString[..fixedLength] : inputString.PadRight(fixedLength);
            //Convert string to byte array
            byteArray = Encoding.ASCII.GetBytes(truncatedString);
            return byteArray;
        }

        public static byte[] CombineArrays(params byte[][] arrays)
        {
            int combinedArrayLength = 0;
            foreach (byte[] array in arrays)
            {
                combinedArrayLength += array.Length;
            }

            byte[] combinedArray = new byte[combinedArrayLength];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, combinedArray, offset, array.Length);
                offset += array.Length;
            }

            return combinedArray;
        }

       public static string ReadStringOfPrintedResponePath(string filePath)
        {
          
            if (!File.Exists(filePath))
            {
                return "";
            }
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }

        }

        public static void SaveStringOfPrintedResponePath(string directoryPath, string fileName, string content)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filePath = System.IO.Path.Combine(directoryPath, fileName);
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            writer.Write(content);
        }

        public static void SaveStringOfCheckedPath(string directoryPath, string fileName, string content)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filePath = System.IO.Path.Combine(directoryPath, fileName);
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            writer.Write(content);
        }


        public static void PrintDebugMessage(string message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }

    }
}
