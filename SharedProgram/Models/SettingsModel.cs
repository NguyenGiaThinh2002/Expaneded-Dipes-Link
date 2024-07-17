using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
namespace SharedProgram.Models
{
    public class SettingsModel : CommonSettingsModel
    {
        public int NumberOfStation {  get; set; }

        public string JobFileExtension { get; set; } = ".rvis";

        public string Language { get; set; } = "en-US"; 

        public string DateTimeFormat { get; set; } = string.Empty;

        public int DateTimeFormatSelectedIndex { get; set; }

        

        public string TemplateName { get; set; } = string.Empty;
        //public List<ConnectParamsModel> SystemParamsList { get; set; } = new();

        private List<ConnectParamsModel> _SystemParamsList = new();

        public List<ConnectParamsModel> SystemParamsList
        {
            get { return _SystemParamsList; }
            set
            {
                if (_SystemParamsList != value)
                {
                    _SystemParamsList = value;
                    OnPropertyChanged();
                }
            }
        }

       // public List<SettingStationsModel> StationList { get; set; } = new();

        //public SettingStationsModel CurrentStation { get; set; } = new SettingStationsModel();

        /// <summary>
        /// Load SettingsModel Instance from file name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static SettingsModel? LoadSetting(string fileName)
        {
            SettingsModel? info = null;
            try
            {
                XmlDocument xmlDocument = new();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;
                using StringReader read = new(xmlString);
                Type outType = typeof(SettingsModel);
                XmlSerializer serializer = new(outType);
                using (XmlReader reader = new XmlTextReader(read))
                {
                    info = (SettingsModel?)serializer.Deserialize(reader);
                    reader.Close();
                }
                read.Close();
            }
            catch (Exception)
            {
                return null;
            }
            return info;
        }
    }
}
