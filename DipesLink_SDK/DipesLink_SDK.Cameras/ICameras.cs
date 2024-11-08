using SharedProgram.Models;

namespace DipesLink_SDK_Cameras
{
    public interface ICameras
    {
        public void Connect();
        public void Disconnect();

        public void Reconnect();
        public void Dispose();
        public abstract bool ManualInputTrigger();
        public abstract bool OutputAction();

        public CameraInfos CameraInfo { get; set; }
        public bool IsConnected { get; }

    }
}
