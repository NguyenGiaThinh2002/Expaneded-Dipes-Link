using System.Text.Json.Serialization;
using static SharedProgram.DataTypes.CommonDataType;

namespace SharedProgram.Models
{
    public class DetectModel
    {
        private int _index;
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        private byte[]? _imageBytes;
       //  [JsonIgnore]
        public byte[]? ImageBytes
        {
            get { return _imageBytes; }
            set { _imageBytes = value; }
        }

        private string? _imagePolyRes;

        public string? ImagePolyResult
        {
            get { return _imagePolyRes; }
            set { _imagePolyRes = value; }
        }


        private Bitmap? _image = null;
        [JsonIgnore]
        public Bitmap? Image
        {
            get { return _image; }
            set { _image = value; }
        }

        private ComparisonResult _cmpRes;
        public ComparisonResult CompareResult
        {
            get { return _cmpRes; }
            set { _cmpRes = value; }
        }

        private long _CompareTime;
        public long CompareTime
        {
            get { return _CompareTime; }
            set { _CompareTime = value; }
        }

        private string _ProcessingDateTime = "";
        public string ProcessingDateTime
        {
            get { return _ProcessingDateTime; }
            set { _ProcessingDateTime = value; }
        }

        private string _Text = "";
        public string Text
        {
            get { return _Text; }
            set { _Text = value; }
        }

    }
}
