namespace DipesLink_SDK_Cameras
{
    public interface  Cameras
    {
        public void Connect();
        public void Disconnect();
        public abstract bool ManualInputTrigger();
        public abstract bool OutputAction();

        public bool IsConnected { get; }

    }
}
