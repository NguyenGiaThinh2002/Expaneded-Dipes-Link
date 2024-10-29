using System.Diagnostics;
using System.Windows.Markup;
using static IPCSharedMemory.Datatypes.Enums;
using static SharedProgram.DataTypes.CommonDataType;

namespace IPCSharedMemory
{
    public class MemoryTransfer
    {
        #region Procedural Function
        public static void SendCommandToUI(IPCSharedHelper? ipc, byte[] command, long capacity = 1024*1024*1) // 1MB
        {
                ipc?.SendData(command);
        }

        public static void SendDatabaseCommandToUI(IPCSharedHelper? ipc, byte[] command) // 200MB
        {
            ipc?.SendData(command);
        }

        public static void SendCheckedDatabaseCommandToUI(IPCSharedHelper? ipc, byte[] command) // 200MB
        {
            ipc?.SendData(command);
        }

        public static void SendRealTimeDataCommandToUI(IPCSharedHelper? ipc, byte[] command) // 1MB
        {
            ipc?.SendData(command);
        }

        public static void SendCommandToDevice(IPCSharedHelper? ipc, byte[] command)
        {
            ipc?.SendData(command);
        }
        #endregion Procedural Function

        #region IPC UI -> Device Transfer
 
        public static void SendConnectionParamsToDevice(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.UICommand ,
                    (byte)index,
                    (byte)SharedMemoryType.ParamsSetting,
                }; // This is header

                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1
              //  Debug.WriteLine("gui den index"+ index);
                SendCommandToDevice(ipc, newCommand); // send memory map file
            }
            catch (Exception) { }
        }

        public static void SendActionButtonToDevice(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command =
                {
                    (byte)SharedMemoryCommandType.UICommand,
                    (byte)index,
                    (byte)SharedMemoryType.ActionButton,
                };

                var newCommand = new byte[command.Length + data.Length];
                Array.Copy(command, 0, newCommand, 0, command.Length);
                Array.Copy(data, 0, newCommand, command.Length, data.Length);
                SendCommandToDevice(ipc, newCommand);
            }
            catch (Exception) { }
        }

        #endregion IPC UI -> Device Transfer

        #region IPC Device Transfer -> UI

        public static void SendLoadingStatusToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.LoadingStatus,
                };
                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1
                                                                              // Console.WriteLine("Send: "+ newCommand.Length);
                SendCommandToUI(ipc, newCommand); // send memory map file
            }
            catch (Exception) { }
        }

        public static void SendRestartStatusToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.RestartStatus,
                };
                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1
                                                                              // Console.WriteLine("Send: "+ newCommand.Length);
                SendCommandToUI(ipc, newCommand); // send memory map file
            }
            catch (Exception) { }
        }

        public static void SendScannerStatusToUI(IPCSharedHelper? ipc, int index, ScannerStatus scannerSts)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.ScannerStatus,
                    (byte)scannerSts
                };
                SendCommandToUI(ipc, command);
            }
            catch (Exception) { }
        }

        public static void SendDatabaseToUIFirstTime(IPCSharedHelper? ipc,int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.DatabaseList,
                };
                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1
                                                                              // Console.WriteLine("Send: "+ newCommand.Length);
                SendDatabaseCommandToUI(ipc, newCommand); // send memory map file
              //  Console.WriteLine("Send DB:" + newCommand.Length);
            }
            catch (Exception) { }
        }


        public static void SendCheckedDatabaseToUIFirstTime(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.CheckedList,
                };
                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1
                SendDatabaseCommandToUI(ipc, newCommand);
               Console.WriteLine("Send Checked DB:" + newCommand.Length);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public static void SendPrinterStatusToUI(IPCSharedHelper? ipc, int index, PrinterStatus printerStatus)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.PrinterStatus,
                    (byte)printerStatus
                };
                SendCommandToUI(ipc, command);
            }
            catch (Exception) { }
        }

        public static void SendControllerStatusToUI(IPCSharedHelper? ipc, int index, ControllerStatus controllerStatus)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.ControllerStatus,
                    (byte)controllerStatus
                };
                SendCommandToUI(ipc, command);
            }
            catch (Exception) { }
        }

        //public static void SendCameraInforToUI(IPCSharedHelper? ipc, int index, CameraStatus camStatus)
        //{
        //    try
        //    {
        //        byte[] command = {
        //            (byte)SharedMemoryCommandType.DeviceCommand ,
        //            (byte)index,
        //            (byte)SharedMemoryType.CamStatus,
        //            (byte)camStatus
        //        };
        //        SendCommandToUI(ipc, command);
        //    }
        //    catch (Exception) { }
        //}


        public static void SendCameraStatusToUI(IPCSharedHelper? ipc,int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.CameraInfo
                };
                var newCommand = new byte[command.Length + data.Length];
                Array.Copy(command, 0, newCommand, 0, command.Length);
                Array.Copy(data, 0, newCommand, command.Length, data.Length);
               
                SendCommandToUI(ipc, newCommand);
            }
            catch (Exception) { }
        }

        public static void SendControllerResponseMessageToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.ControllerResponseMess,
                };
                var newCommand = new byte[command.Length + data.Length]; 
                Array.Copy(command, 0, newCommand, 0, command.Length);
                Array.Copy(data, 0, newCommand, command.Length, data.Length);
                SendCommandToUI(ipc, newCommand);
            }
            catch (Exception) { }
        }

        public static void SendPrinterTemplateListToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.PrinterTemplate,
                };
                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1

                SendCommandToUI(ipc, newCommand); // send memory map file
            }
            catch (Exception){}
        }

        #region Sent/Received/Printed Counter
        //Sent Number
        public static void SendCounterSentToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.StatisticsCounterSent,
                };

                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1

                SendCommandToUI(ipc, newCommand); // send memory map file
            }
            catch (Exception) { }
        }

        //Received Number
        public static void SendCounterReceivedToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.StatisticsCounterReceived,
                };

                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1

                SendCommandToUI(ipc, newCommand); // send memory map file
            }
            catch (Exception) { }
        }

        //Printed Number
        public static void SendCounterPrintedToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.StatisticsCounterPrinted,
                };

                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1

                SendCommandToUI(ipc, newCommand); // send memory map file
            }
            catch (Exception) { }
        }
        #endregion end Sent/Received/Printed Counter


        public static void SendCurrentPosDatabaseToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.CurrentPosDb,
                };

                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1

                SendCommandToUI(ipc, newCommand); // send memory map file
            }
            catch (Exception) { }
        }
        public static void SendCycleTimePODTransferToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.CycleTimePOD,
                };

                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1

                SendCommandToUI(ipc, newCommand); // send memory map file
            }
            catch (Exception) { }
        }

        public static void SendDetectModelToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.DetectModel,
                };
                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1

                SendRealTimeDataCommandToUI(ipc, newCommand);

            }
            catch (Exception)
            {
            }
        }

        public static void SendCheckedStatisticsToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.CheckedStatistics,
                };
                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1

                SendRealTimeDataCommandToUI(ipc, newCommand);
            }
            catch (Exception)
            {
            }
        }

        public static void SendCheckedResultRawToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.CheckedResultRaw,
                };

                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1

                SendRealTimeDataCommandToUI(ipc, newCommand); // send memory map file
            }
            catch (Exception) { }
        }

        public static void SendPrintedCodeRawToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.PrintedCodeRaw,
                };

                var newCommand = new byte[command.Length + data.Length]; // Create new array
                Array.Copy(command, 0, newCommand, 0, command.Length); // copy array 1 to new array
                Array.Copy(data, 0, newCommand, command.Length, data.Length); // copy array 2 to new array start from length array 1
                        
                SendCommandToUI(ipc, newCommand); // send memory map file
            }
            catch (Exception){ }
        }

        public static void SendOperationStatusToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.JobOperationStatus
                };

                var newCommand = new byte[command.Length + data.Length]; 
                Array.Copy(command, 0, newCommand, 0, command.Length); 
                Array.Copy(data, 0, newCommand, command.Length, data.Length); 

                SendCommandToUI(ipc, newCommand); 
            }
            catch (Exception) { }
        }

        public static void SendMessageJobStatusToUI(IPCSharedHelper? ipc, int index, byte[] data)
        {
            try
            {
                byte[] command = {
                    (byte)SharedMemoryCommandType.DeviceCommand ,
                    (byte)index,
                    (byte)SharedMemoryType.JobMessageStatus,
                };

                var newCommand = new byte[command.Length + data.Length];
                Array.Copy(command, 0, newCommand, 0, command.Length);
                Array.Copy(data, 0, newCommand, command.Length, data.Length);

                SendCommandToUI(ipc, newCommand);
            }
            catch (Exception) { }
        }


        #endregion IPC Device Transfer -> UI


    }
}
