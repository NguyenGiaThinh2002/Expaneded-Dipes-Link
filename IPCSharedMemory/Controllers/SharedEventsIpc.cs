using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCSharedMemory.Controllers
{
    public class SharedEventsIpc
    {
        public static event EventHandler? CameraStatusChanged;
        public static event EventHandler? PrinterStatusChanged;
        public static event EventHandler? ControllerStatusChanged;
        public static event EventHandler? ScannerStatusChanged;


        public static void RaiseCameraStatusChanged(object sender, EventArgs e)
        {
            CameraStatusChanged?.Invoke(sender, e);
        }
        public static void RaisePrinterStatusChanged(object sender, EventArgs e)
        {
            PrinterStatusChanged?.Invoke(sender, e);
        }
        public static void RaiseControllerStatusChanged(object sender, EventArgs e)
        {
            ControllerStatusChanged?.Invoke(sender, e);
        }
        public static void RaiseScannerStatusChanged(object sender, EventArgs e)
        {
            ScannerStatusChanged?.Invoke(sender, e);
        }
    }
}
