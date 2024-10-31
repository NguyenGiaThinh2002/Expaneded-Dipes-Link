using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DipesLink.Models
{
    public class CheckedResultModel
    {
      
        public string Index { get; set; } = string.Empty;
        public string ResultData { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string ProcessingTime { get; set; } = string.Empty;
        public string DateTime { get; set; } = string.Empty;
        public string Device { get; set; } = string.Empty;

        public CheckedResultModel(string[] data)
        {
            if (data.Length >= 6)
            {
                Index = data[0];
                ResultData = data[1];
                Result = data[2];
                ProcessingTime = data[3];
                DateTime = data[4];
                Device = data[5];

            }

        }
        public CheckedResultModel()
        {
            
        }

    }
}
