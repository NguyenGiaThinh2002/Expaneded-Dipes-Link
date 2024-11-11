using DipesLink.Languages;
using System.Globalization;
using System.Windows.Data;
using static SharedProgram.DataTypes.CommonDataType;


namespace DipesLink.Views.Converter
{
    public class FromEnumtoStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            // Handle Value Type
            switch (value)
            {
                case JobType jobType when jobType == JobType.None:
                case CompareType compareType when compareType == CompareType.None:
                case PrinterSeries printerSeries when printerSeries == PrinterSeries.None:
                case CompleteCondition completeCondition when completeCondition == CompleteCondition.None:
                case JobStatus jobStatus when jobStatus == JobStatus.None:
                case ComparisonResult comparisonResult when comparisonResult == ComparisonResult.None:
                case int v when v == 0:
                    return "";
                case CompareType compareType:
                    return LanguageModel.GetLanguage(compareType.ToString());
                case JobType jobType:
                    return LanguageModel.GetLanguage(jobType.ToString());
                case CompleteCondition completeCondition:
                    return LanguageModel.GetLanguage(completeCondition.ToString());
                case bool OutputCamera:
                    return LanguageModel.GetLanguage(OutputCamera.ToString());
                case JobStatus jobStatus:
                    return LanguageModel.GetLanguage(jobStatus.ToString());

                default:
                    return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
