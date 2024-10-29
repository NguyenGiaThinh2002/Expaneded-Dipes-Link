using SharedProgram.Models;
using SharedProgram.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static SharedProgram.DataTypes.CommonDataType;

namespace DipesLinkDeviceTransfer
{
    public partial class Program
    {
        public void ScannerEventInit()
        {
            SharedEvents.OnScannerReadDataChange -= SharedEvents_OnScannerReadDataChange;
            SharedEvents.OnScannerReadDataChange += SharedEvents_OnScannerReadDataChange; // Camera Data change event
        }

        private void SharedEvents_OnScannerReadDataChange(object? sender, EventArgs e)
        {

            if (SharedValues.OperStatus != OperationStatus.Running &&
                SharedValues.OperStatus != OperationStatus.Processing)
            {
                return; // Only implement when Running
            }

            if (sender is DetectModel detectModel)
            {
                //MessageBox.Show(detectModel.Text);
                _QueueBufferScannerReceivedData.Enqueue(detectModel); // Add camera data read to buffer
            }

        }
    }
}
