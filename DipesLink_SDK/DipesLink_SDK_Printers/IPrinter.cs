using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DipesLink_SDK_Printers
{
    public interface IPrinter
    {
        public bool Connect();
        public bool Disconnect();

        public bool IsConnected();
        public void SendData(string data);
    }
}
