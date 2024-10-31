using Cognex.InSight.Remoting.Serialization;
using Cognex.InSight.Web;
using Cognex.InSight.Web.Controls;
using SharedProgram.DeviceTransfer;
using System.Collections.Concurrent;

namespace DipesLink_SDK_Cameras.Models
{
    public class ISCameraController : IDisposable
    {
        #region Variables Definition
        public string _ipAddress;
        public string _port;
        public bool IsConnected { get; set; }
        public string ModelNumber { get; set; }
        public string SerialNumber { get; set; }
        public int Index { get; set; }
        internal CvsInSight _inSight;
        internal CvsDisplay _cvsDisplay;
        internal event EventHandler AutoAddSufixEvent;
        internal event EventHandler UpdateLabelStatusEvent;
        public event EventHandler ChangeStatusEvent;
        internal Role _role;
        public enum Role
        {
            master,
            slave
        }
        public readonly BlockingCollection<DataISCamera> _blockColResult = new();
        private int _indexCounter;


      //  internal readonly CameraModel _CameraModel = Shared.Settings.CameraList.FirstOrDefault();

        #endregion Variables Definition

        public ISCameraController(string ipAddress, string port, Role role)
        {
            _ipAddress = ipAddress;
            _port = port;
            _role = role;
            _inSight = new CvsInSight();
            _inSight.ConnectedChanged += InSight_ConnectedChanged;
            _inSight.ResultsChanged += InSight_ResultsChangedAsync;

         //   Shared.OnStopButtonClick -= Shared_OnStopButtonClick;
          //  Shared.OnStopButtonClick += Shared_OnStopButtonClick;
        }
        private void EventsRegister()
        {

        }

        /// <summary>
        /// Detect Stop Process Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shared_OnStopButtonClick(object sender, EventArgs e)
        {
            _indexCounter = 0; // Reset counter 
        }

        /// <summary>
        /// Detect Add Suffix Event by Start Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>


        /// <summary>
        /// Connection First Time
        /// </summary>
        /// <returns></returns>
        public async Task FirstConnection()
        {
            try
            {
                var sessionInfo = new HmiSessionInfo
                {
                    SheetName = "Inspection",
                    CellNames = new string[1] { "A0:Z599" },
                    EnableQueuedResults = true,
                    IncludeCustomView = true,
                    AutoReady = true
                };
                await _inSight.Connect(_ipAddress + ":" + _port, "admin", "", sessionInfo);
             //   Shared.RaiseOnCameraStatusChangeEvent();// Sts change
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Event Handler: Connection state and information of the camera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void InSight_ConnectedChanged(object? sender, EventArgs e)
        {
            try
            {
                await Task.Delay(500);
                if (_inSight.Connected)
                {
                    IsConnected = true;
                    ModelNumber = _inSight.CameraInfo.ModelNumber;
                    SerialNumber = _inSight.CameraInfo.SerialNumber;

                }
                else
                {
                    IsConnected = false;
                    ModelNumber = "";
                    SerialNumber = "";
                }
                ChangeStatusEvent?.Invoke(IsConnected, EventArgs.Empty);
                // Shared.RaiseOnCameraStatusChangeEvent(); // Sts change
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Event Handler: Results changed of the camera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InSight_ResultsChangedAsync(object? sender, EventArgs e)
        {
            /* Some resolution options
             240*160
             320*240
             480*360
             */
            //int wimg = Shared.Settings.CameraList.FirstOrDefault().WidthImage;
            //int himg = Shared.Settings.CameraList.FirstOrDefault().HeigthImage;
            int wimg = DeviceSharedValues.WidthImage;
            int himg = DeviceSharedValues.HeigthImage;

            var cognexIs = sender as CvsInSight;
            if (cognexIs == null) return;
            var imageUrl = cognexIs.GetMainImageUrl(wimg, himg);

            if (cognexIs.Online && SharedProgram.Shared.SharedValues.OperStatus == SharedProgram.DataTypes.CommonDataType.OperationStatus.Running ||
                SharedProgram.Shared.SharedValues.OperStatus == SharedProgram.DataTypes.CommonDataType.OperationStatus.Processing)
            {
                var data = new DataISCamera
                {
                    Index = ++_indexCounter,
                    ImageUrl = imageUrl,
                    JTokenData = cognexIs.Results
                };
                Index = _role.Equals(Role.master) ? 0 : 1;
              //  Shared.RaiseOnNumberEventISCountEvent(new CountEventISCamera(Index, data.Index));
                _blockColResult.Add(data);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private bool _disposed = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free managed resources here
                if (_inSight != null)
                {
                    _inSight.ConnectedChanged -= InSight_ConnectedChanged;
                    _inSight.ResultsChanged -= InSight_ResultsChangedAsync;
                    _inSight = null;
                }

                if (_cvsDisplay != null)
                {
                    _cvsDisplay.Dispose();
                    _cvsDisplay = null;
                }


           //     Shared.OnStopButtonClick -= Shared_OnStopButtonClick;

                _blockColResult.Dispose();

            }

            // Free unmanaged resources here (if any)

            _disposed = true;
        }
        ~ISCameraController()
        {
            Dispose(false);
        }
    }
}
