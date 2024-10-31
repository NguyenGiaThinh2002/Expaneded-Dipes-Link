using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DipesLink_SDK_Cameras.Models
{
    public class DataISCamera
    {
        public int Index { get; set; }
        public JToken JTokenData { get; set; }
        public string ImageUrl { get; set; }
    }
}
