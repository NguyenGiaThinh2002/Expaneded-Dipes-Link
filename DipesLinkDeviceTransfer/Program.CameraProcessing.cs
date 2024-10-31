using IPCSharedMemory;
using SharedProgram.Controller;
using SharedProgram.Models;
using SharedProgram.Shared;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLinkDeviceTransfer
{
    public partial class Program
    {
        public void CameraEventInit()
        {
            SharedEvents.OnCameraReadDataChange -= SharedEvents_OnCameraReadDataChange;
            SharedEvents.OnCameraReadDataChange += SharedEvents_OnCameraReadDataChange; // Camera Data change event
        }

        private void SharedEvents_OnCameraReadDataChange(object? sender, EventArgs e)
        {

#if DEBUG
           // SharedValues.OperStatus = OperationStatus.Running; //Supposed to be running
#endif

            if (SharedValues.OperStatus != OperationStatus.Running && 
                SharedValues.OperStatus != OperationStatus.Processing &&
                SharedValues.SelectedJob?.JobType != JobType.StandAlone)
            {
                return; // Only implement when Running
            }
            if (sender is DetectModel detectModel)
            {
                _QueueBufferCameraReceivedData.Enqueue(detectModel); // Add camera data read to buffer
            }
        }

        public void SimulateValidDataCamera()
        {
            if (SharedValues.SelectedJob == null) return;
            DetectModel dtm = new();
            string[] data = Array.Empty<string>();
            if (_IsAfterProductionMode)
            {
                for (int i = 0; i < SharedValues.TotalCode; i++)
                {
                    string tmp = SharedFunctions.GetCompareDataByPODFormat(SharedValues.ListPrintedCodeObtainFromFile[i], SharedValues.SelectedJob.PODFormat);
                    if (_CodeListPODFormat.TryGetValue(tmp, out CompareStatus? compareStatus))
                    {
                        if (!compareStatus.Status) // only send data not yet compare 
                        {
                            data = SharedValues.ListPrintedCodeObtainFromFile[i];
                            break;
                        }
                    }
                }
            }
            if (SharedValues.SelectedJob.CompareType == CompareType.Database)
            {
                dtm.Text = SharedFunctions.GetCompareDataByPODFormat(data, SharedValues.SelectedJob.PODFormat);
            }

            SharedEvents.RaiseOnCameraReadDataChangeEvent(dtm); //Trigger event for send data camera
        }
        public enum TypeOfSimulateInvalidDataCamera
        {
            Fail,
            Duplicate,
            Null
        }
        public void SimulateInvalidDataCamera(TypeOfSimulateInvalidDataCamera typeOfInvalidData)
        {
            if (SharedValues.SelectedJob == null) return;
            DetectModel dtm = new();
            switch (typeOfInvalidData)
            {
                case TypeOfSimulateInvalidDataCamera.Fail:
                    dtm.Text = "Trigger"; // A specific string
                    break;
                case TypeOfSimulateInvalidDataCamera.Duplicate:
                    if(SharedValues.ListCheckedResultCode != null && SharedValues.ListCheckedResultCode.Count >0)
                    {
                        string[]? validRowData = SharedValues.ListCheckedResultCode.Where(x => x[2] == "Valid").FirstOrDefault(); // Get valid code from checked list 
                        if (validRowData != null)
                        {
                            dtm.Text = validRowData[1]; // Get column 1 (data)
                        }
                    }
                    break;
                case TypeOfSimulateInvalidDataCamera.Null:
                    dtm.Text = "";  // Null string
                    break;
                default:
                    break;
            }

            SharedEvents.RaiseOnCameraReadDataChangeEvent(dtm); //Trigger event for send data camera
        }
        public static void SendCameraInfoAndStatusToUi(int index, IPCSharedHelper? ipc, CameraInfos? camInfo)
        {
            try
            {
                byte[] arrInfo = DataConverter.ToByteArray(camInfo);
                MemoryTransfer.SendCameraStatusToUI(ipc, index, arrInfo);
            }
            catch (Exception)
            {
            }
           
        }
    }
}
