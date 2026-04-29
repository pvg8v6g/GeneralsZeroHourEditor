using Microsoft.UI.Xaml.Data;

namespace GeneralsZeroHourEditor.Converters;

public partial class NegateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double doubleValue)
        {
            return -doubleValue;
        }

        if (value is int intValue)
        {
            return -intValue;
        }

        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
