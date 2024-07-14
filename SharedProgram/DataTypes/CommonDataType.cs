using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedProgram.DataTypes
{
    public class CommonDataType
    {
        public enum RestartStatus
        {
            Failed, Successful
        }
       public enum LoadDataStatus
        {
            StartLoad,
            Completed
        }
        public enum JobType
        {
            None,
            AfterProduction,
            OnProduction,
            VerifyAndPrint,
            StandAlone,
            
        }
        public enum CompareType
        {
            None,
            CanRead,
            StaticText,
            Database
        }

        public enum PrinterSeries
        {
            None,
            RynanSeries,
            Standalone
        }

        public  enum CameraSeries
        {
            Dataman,
            InsightVision
        }

        public enum CompleteCondition
        {
            None,
            TotalPassed,
            TotalChecked
        }
        public enum JobStatus
        {
            None,
            NewlyCreated,
            Unfinished,
            Accomplished,
            Deleted
        }
        public enum ProcessCheckeType
        {
            None, TotalChecked = 0, TotalPassed = 1
        }
        public enum RoleOfStation
        {
            ForProduct,
            ForBox
        }

        public enum ComparisonResult
        {
            Valid,
            Invalided,
            Duplicated,
            Null,
            Missed,
            None
        }
        public enum OperationStatus
        {
           
            Stopped,
            Running,
            Processing,
            Startup
        }
        public enum PrinterStatus
        {
            //Stop, Processing, Ready, Printing, Connected, Disconnected, Error, Disable
            Stop,
            Processing,
            Ready,
            Printing,
            WaitingData,
            Connected,
            Disconnected,
            Error,
            Disable,
            Start,
            Null
        }

        public enum ControllerStatus
        {
            Connected,
            Disconnected
        }

        public enum ActionButtonType
        {
            //max name length = 10
            Unknown = -1,
            LoadDB,
            StartWithDB,
            Start,
            Stop,
            Trigger,
            ReloadTemplate,
            Reprint,
            Pause,
            ExportResult,
            Simulate
        }
        public enum InitDataError
        {
            DatabaseUnknownError,
            PrintedStatusUnknownError,
            CheckedResultUnknownError,
            CannotAccessDatabase,
            CannotAccessCheckedResult,
            CannotAccessPrintedResponse,
            DatabaseDoNotExist,
            CheckedResultDoNotExist,
            PrintedResponseDoNotExist,
            CannotCreatePodDataList,
            Unknown
        }
        public enum CheckCondition
        {
            Success,
            NotLoadDatabase,
            NotConnectCamera,
            NotLoadTemplate,
            MissingParameter,
            NotConnectPrinter,
            NotConnectServer,
            LeastOneAction,
            MissingParameterActivation,
            MissingParameterPrinting,
            MissingParameterWarehouseInput,
            MissingParameterWarehouseOutput,
            CreatingWarehouseInputReceipt,
            NoJobsSelected
        }
        public  enum LoadDBStatus
        {
            Completed,
            Failed
        }
        public enum NotifyType
        {
            Unk,
            CameraConnectionFail,
            PrinterConnectionFail,
            StartSync,
            CompleteLoadDB,
            DatabaseUnknownError,
            PrintedStatusUnknownError, 
            CheckedResultUnknownError,
            CannotAccessDatabase,
            CannotAccessCheckedResult,
            CannotAccessPrintedResponse,
            DatabaseDoNotExist,
            CheckedResultDoNotExist,
            PrintedResponseDoNotExist,
            NoJobsSelected,
            NotLoadDatabase,
            NotLoadTemplate,
            NotConnectCamera,
            MissingParameter, 
            NotConnectPrinter, 
            LeastOneAction, 
            MissingParameterActivation,
            MissingParameterPrinting, 
            Unknown, 
            ProcessCompleted,
            CannotCreatePodDataList,
            NotConnectServer,
            MissingParameterWarehouseInput,
            MissingParameterWarehouseOutput,
            CreatingWarehouseInputReceipt,
            PauseSystem,
            StopSystem,
            PrinterSuddenlyStop,
            DeviceDBLoaded,
            StartEndPageInvalid,
            NoPrintheadSelected,
            PrinterSpeedLimit,
            PrintheadDisconnected,
            UnknownPrinthead,
            NoCartridges,
            InvalidCartridges,
            OutOfInk,
            CartridgesLocked,
            InvalidVersion,
            IncorrectPrinthead,
            DuplicateData,
            ExportResultFail
        }
        public enum EventsLogType
        {
            Info,
            Warning,
            Error
        }
    }
}
