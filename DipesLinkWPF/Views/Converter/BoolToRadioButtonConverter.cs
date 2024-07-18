using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DipesLink.Views.Converter
{
    public class BoolToRadioButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;
            int radioParams = int.Parse(parameter.ToString());
            if(radioParams == 0) // Rad 1
            {
                if(isChecked) return true;
                return false;
            }
            else // Rad 2
            {
                if (isChecked) return false;
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;
            int radioParams = int.Parse(parameter.ToString());
            if (radioParams == 0) // Rad 1
            {
                if (isChecked) return true;
                return false;
            }
            else // Rad 2
            {
                if (isChecked) return false;
                return true;
            }
        }
    }
}
