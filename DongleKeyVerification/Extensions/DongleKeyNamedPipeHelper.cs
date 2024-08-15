using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DongleKeyVerification
{
    public class DongleKeyNamedPipeHelper
    {
        public void SendKeyLevel(int level)
        {

            try
            {
                using (var pipeClient = new NamedPipeClientStream(".", "NamedPipeUSBLicense", PipeDirection.Out))
                {
                    pipeClient.Connect(10); 
                    using (var writer = new StreamWriter(pipeClient))
                    {
                        writer.AutoFlush = true; 
                        writer.WriteLine(level.ToString()); 
                        return;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
