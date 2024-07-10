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
using System.Xml.Linq;
using static SharedProgram.DataTypes.CommonDataType;

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

        public static void AutoGenerateFileName(int index, out string? jobName)
        {
            jobName = string.Format("{1}_{0}", DateTime.Now.ToString("yyyyMMdd_HHmmss"), $"JobFile_{index + 1}");
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

        public static void PrintConsoleMessage(string message)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
        }
        public static void PrintDebugMessage(string message)
        {
            Debug.WriteLine(message);
        }

        public static List<string[]> InitDatabaseWithStatus(string? csvPath)
        {
            List<(int index, string[] data)> result = new(); // List to store index and data
            if (csvPath == null || !File.Exists(csvPath))
            {
                return result.Select(t => t.data).ToList();
            }
            try
            {
                string[] lines = File.ReadAllLines(csvPath);
                int columnCount = 0;
                if (lines.Length > 0)
                {
                    string[] firstLine = SplitLine(lines[0], csvPath.EndsWith(".csv"));
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
                    string[] columns = SplitLine(line, csvPath.EndsWith(".csv"));
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
            catch (IOException) { }
            catch (Exception) { }

            List<string[]> sortedAndTransformed = result.AsParallel()
                                 .OrderBy(item => item.index)
                                 .Select(item => item.data)
                                 .ToList();
            return sortedAndTransformed;
        }

        private static string[] SplitLine(string line, bool isCsv)
        {
            return isCsv ? line.Split(',') : line.Split('\t');
        }

        public static void InitPrintedStatus(string pathBackupPrinted, List<string[]> dbList)
        {
            if (!File.Exists(pathBackupPrinted))
            {
                return;
            }

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

        public static string GetCompareDataByPODFormat(string[] row, List<PODModel> pODFormat, int addingIndex = 0)
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

        public static bool ExportResult(string fileName)
        {
            try
            {
                // Create a dictionary to count the number of occurrences of each ResultData with "Duplicated" status
                var duplicateCountDict = SharedValues.ListCheckedResultCode
                    .Where(x => x[2] == "Duplicated")
                    .GroupBy(x => x[1])
                    .ToDictionary(g => g.Key, g => g.Count());

                // Create a dictionary to store the first valid results
                var checkedResultDict = SharedValues.ListCheckedResultCode
                    .Where(x => x[2] == "Valid")
                    .GroupBy(x => x[1])
                    .ToDictionary(g => g.Key, g => g.First()[SharedValues.ColumnNames.Length - 1]);

                if (File.Exists(fileName)) File.Delete(fileName);
                using (StreamWriter writer = new(fileName, true, Encoding.UTF8))
                {
                    string header = string.Join(",", SharedValues.DatabaseColunms.Select(Csv.Escape)) + ",VerifyDate";
                    writer.WriteLine(header);

                    for (int i = 0; i < SharedValues.TotalCode; i++)
                    {
                        var record = SharedValues.ListPrintedCodeObtainFromFile[i];
                        var compareString = SharedFunctions.GetCompareDataByPODFormat(record, SharedValues.SelectedJob.PODFormat);
                        var writeValue = string.Join(",", record.Take(record.Length - 1).Select(Csv.Escape)) + ",";

                        if (checkedResultDict.TryGetValue(compareString, out string dateVerify))
                        {
                            if (duplicateCountDict.TryGetValue(compareString, out int duplicateCount) && duplicateCount >= 1)
                            {
                                writeValue += "Duplicate";
                            }
                            else
                            {
                                writeValue += "Verified";
                            }
                            writeValue += "," + Csv.Escape(dateVerify);
                            checkedResultDict.Remove(compareString);
                        }
                        else
                        {
                            string tmpValue = record[record.Length - 1];
                            writeValue += tmpValue == "Printed" ? "Unverified" : tmpValue;
                            writeValue += "," + "";
                        }
                        writer.WriteLine(writeValue);
                    }
                }
                if (File.Exists(fileName))
                {
                    // Use Process.Start to open the folder and select the file
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer",
                        Arguments = $"/select,\"{fileName}\"",
                        UseShellExecute = true
                    });
                   
                }
                else
                {
                    Console.WriteLine("File does not exist.");
                    return false;

                }
                checkedResultDict.Clear();
                return true;
            }
            catch (Exception ex)
            {
                PrintConsoleMessage(ex.Message);
                return false;
            }

        }

        public static byte[] ReadByteArrayFromFile(string filePath)
        {
            try
            {
                return File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading byte array from file: {ex.Message}");
                return null;
            }
        }
       public  static void SaveByteArrayToFile(string filePath, byte[] byteArray)
        {
            try
            {
                File.WriteAllBytes(filePath, byteArray);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving byte array to file: {ex.Message}");
            }
        }
        private string? GetResultFromXmlString(string? xmlString)
        {
            if (xmlString == null) return null;
            XDocument xmlDoc = XDocument.Parse(xmlString);
            XNamespace ns = "http://www.w3.org/2000/svg"; // For Sgv Format namespace
            IEnumerable<XElement> polygons = xmlDoc
                        .Descendants(ns + "polygon")
                        .Where(p => (string?)p
                        .Attribute("class") == "result");
            string? pointsValue = polygons.Select(x => x.Attribute("points"))?.FirstOrDefault()?.Value.ToString(); // Get Points value
            return pointsValue;
        }
    }
}
