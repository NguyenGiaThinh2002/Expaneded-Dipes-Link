using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharedProgram.DataTypes.CommonDataType;

namespace SharedProgram.Models
{
    public class PODDataModel : ViewModelBase
    {
        public int Index { get; set; }
        public string IP { get; set; } = "";
        public string Port { get; set; } = "0";
        public RoleOfStation RoleOfPrinter { get; set; } = RoleOfStation.ForProduct;
        public string Text { get; set; } = "";
    }
}
