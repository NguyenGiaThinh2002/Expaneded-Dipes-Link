using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DongleKeyVerification.Models
{
    [DataContract]
    public class HardwareIDModel
    {
        #region Properties
        [DataMember(Name = "_HardwareID")]
        public string HardwareID { get; set; }
        #endregion Properties
    }
}
