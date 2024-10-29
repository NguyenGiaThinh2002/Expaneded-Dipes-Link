using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCSharedMemory.Datatypes
{
    public static class Enums
    {
        public enum CameraStatus
        {
            Connected = 0,
            Disconnected = 1
        }
        //public enum PrinterStatus
        //{
        //    Connected = 0,
        //    Disconnected = 1
        //}
        public enum SharedMemoryType
        {
            JobSetting = 0,
            CameraInfo =1,
            PrinterStatus =2,
            ParamsSetting =3,
            ActionButton = 4,
            PrinterTemplate = 5,
            StatisticsCounterSent = 6,
            PrintedCodeRaw =7,
            DetectModel = 8,
            CheckedResultRaw =9,
            CheckedStatistics = 10,
            JobMessageStatus,
            JobOperationStatus,
            DatabaseList,
            CurrentPosDb,
            ScannerStatus,
            CheckedList,
            StatisticsCounterReceived,
            StatisticsCounterPrinted,
            ControllerStatus,
            ControllerResponseMess,
            LoadingStatus,
            RestartStatus,
            CycleTimePOD
        }
        public enum SharedMemoryCommandType
        {
            DeviceCommand = 0,
            UICommand = 1
        }
    }
}
