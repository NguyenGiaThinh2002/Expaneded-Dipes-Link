using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DipesLink.Views.Converter
{
    public class ForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() is string str)
            {
                switch (str)
                {
                    case "Running":
                        return Brushes.Green;
                    case "Processing":
                        return Brushes.OrangeRed;
                    case "Stopped":
                        return Brushes.Red;
                    default:
                        return Brushes.White;
                }
                
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
