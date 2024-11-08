using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DipesLink_SDK_PLC
{
    public interface IPLC_TCPIP
    {
        bool Connect();
        bool Disconnect();
        void ReceiveData();
        void SendData(string data);
        bool IsChangeParams();
        bool IsConnected();
        bool PingIP(string ip,int port);
    }
}
