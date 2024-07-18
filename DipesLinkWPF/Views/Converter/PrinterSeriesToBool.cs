using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static SharedProgram.DataTypes.CommonDataType;
using static DipesLink.Views.Enums.ViewEnums;

namespace DipesLink.Views.Converter
{
    public class PrinterSeriesToBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) // value => state
        {
            var val = (PrinterSeries)value;
            var par = int.Parse((string)parameter);
            if (par == 0) // rad 1
            {
                if (val == PrinterSeries.RynanSeries) { return true; } // in rad 1, value == rnp => true (view)
                else return false;
            }
            else if (par == 1) // rad 2
            {
                if (val == PrinterSeries.RynanSeries) { return false; }  // int rad 2 ,  value != rnp => true (view)
                else return true;
            }
            else { return false; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) // state => value
        {
            var val = (bool)value;
            var par = int.Parse((string)parameter);
            if (par == 0) // rad 1
            {
                return val == true ? PrinterSeries.RynanSeries : PrinterSeries.Standalone; // value
            }
            else //rad 2
            {
                return val == true ? PrinterSeries.Standalone : PrinterSeries.RynanSeries;
            }
           
        }
    }
}
