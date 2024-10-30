namespace DipesLink_SDK_BarcodeScanner
{
    public interface IBarcodeScanner
    {
        bool Connect();
        void Disconnect();
        bool IsConnected();
    }
}
