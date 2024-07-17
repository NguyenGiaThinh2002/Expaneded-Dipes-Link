using log4net;
using log4net.Appender;
using log4net.Config;
using SharedProgram.Shared;
using System.IO;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLink.Extensions
{
    public class LoggerHelper
    {
        static object _lock = new object();
        private static readonly ILog log = LogManager.GetLogger(typeof(LoggerHelper));

        static LoggerHelper()
        {
            XmlConfigurator.Configure(LogManager.GetRepository(), new FileInfo("log4net.config"));
        }

        public static void SetLogProperties(int stationIndex, string jobName, string title, string message, EventsLogType logsType)
        {
            Task.Run(() =>
            {
                try
                {
                    lock (_lock)
                    {
                        var logDirectoryPath = Path.Combine(SharedPaths.PathEventsLog, $"Job{stationIndex}");
                        if (!Directory.Exists(logDirectoryPath))
                        {
                            Directory.CreateDirectory(logDirectoryPath);
                        }


                        LogicalThreadContext.Properties["LogDirectory"] = logDirectoryPath;
                        LogicalThreadContext.Properties["JobName"] = jobName;
                        LogicalThreadContext.Properties["Title"] = title;

                        foreach (var appender in log.Logger.Repository.GetAppenders()) // For determine the correct path
                        {
                            if (appender is RollingFileAppender fileAppender)
                            {
                                fileAppender.File = $"{logDirectoryPath}\\_JobEvents_{jobName}.csv";
                                fileAppender.ActivateOptions();
                            }
                        }
                        switch (logsType)
                        {
                            case EventsLogType.Info:
                                Info(message);
                                break;
                            case EventsLogType.Warning:
                                Warn(message);
                                break;
                            case EventsLogType.Error:
                                Error(message);
                                break;
                            default:
                                break;
                        }

                    }

                }
                catch (Exception)
                {

                }
            });
        }

        public static void Info(string message)
        {
            log.Info(message);
        }

        public static void Warn(string message)
        {
            log.Warn(message);
        }

        public static void Error(string message)
        {
            log.Error(message);
        }
    }
}
