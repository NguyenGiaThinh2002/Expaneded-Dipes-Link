using System.IO;

namespace SharedProgram.Shared
{
    public class SharedPaths
    {

        public static void InitCommonPathByIndex(int i)
        {
            var pathSubJobsApp = PathSubJobsApp + $"{i + 1}"; // C:\\ProgramData\\DP-Link\\Jobs\\Job1
            if (!Directory.Exists(pathSubJobsApp))
                Directory.CreateDirectory(pathSubJobsApp);

            var pathSelectedJob = PathSelectedJobApp + $"Job{i + 1}";
            if (!Directory.Exists(pathSelectedJob))
                Directory.CreateDirectory(pathSelectedJob);

            var pathPrintedResponse = PathPrintedResponse + $"Job{i + 1}";
            if (!Directory.Exists(pathPrintedResponse))
                Directory.CreateDirectory(pathPrintedResponse);

            var pathCheckedResult = PathCheckedResult + $"Job{i + 1}";
            if (!Directory.Exists(pathCheckedResult))
                Directory.CreateDirectory(pathCheckedResult);

            var pathEventLog = PathEventsLog + $"Job{i + 1}";
            if (!Directory.Exists(pathEventLog))
                Directory.CreateDirectory(pathEventLog);
        }

        public static string AppPath = AppDomain.CurrentDomain.BaseDirectory;

        private static string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private static string PathProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        public static string PathProgramDataApp => PathProgramData + "\\DP-Link\\"; // C:\ProgramData\R-Link
        public static string PathAppData => AppData + "\\DP-Link\\"; // C:\Users\{username}\AppData\Roaming\R-Link
        public static string PathJobsApp => PathProgramDataApp + "Jobs\\"; // C:\ProgramData\R-Link\Jobs
        public static string PathSubJobsApp => PathJobsApp + "Job";  // C:\ProgramData\R-Link\Jobs\Job{index}

        // Add directories by index manually with these paths
        public static string PathSelectedJobApp => PathJobsApp + "SelectedJob\\"; // C:\ProgramData\R-Link\Jobs\SelectedJob
        public static string PathPrintedResponse => PathProgramDataApp + "PrintedResponse\\"; //C:\ProgramData\R-Link\PrintedResponse
        public static string PathCheckedResult => PathProgramDataApp + "CheckedResult\\"; // C:\ProgramData\R-Link\CheckedResult
        public static string PathSettingsFile => PathProgramDataApp + "Settings"; //// C:\ProgramData\R-Link\Settings
        public static string PathAccountsDb => PathProgramDataApp + "Accounts";
        public static string PathEventsLog => PathAppData + "EventsLog\\"; // C:\Users\{username}\AppData\Roaming\R-Link\EventsLog



        public static string GetSettingPath()
        {
            string folderPath = PathSettingsFile;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            return Path.Combine(folderPath, "setting.cfg");
        }

    }
}
