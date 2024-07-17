using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SharedProgram.Models
{
    public class CommonSettingsModel : ObservableObject
    {
        private static void CheckPathAndCreateNew(string path)
        {
            string? pathWithoutExt = Path.GetDirectoryName(path);
            if (pathWithoutExt is not null && !Directory.Exists(pathWithoutExt))
            {
                Directory.CreateDirectory(pathWithoutExt);
            }
        }

        public void SaveJobFile(string fileName)
        {
            try
            {
                CheckPathAndCreateNew(fileName);
                var xs = new XmlSerializer(GetType());
                using TextWriter sw = new StreamWriter(fileName);
                xs.Serialize(sw, this);
            }
            catch { }
        }
    }
}
