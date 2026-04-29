using Microsoft.UI.Xaml.Data;

namespace GeneralsZeroHourEditor.Converters;

public partial class NullToInvalidIndexConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value == null ? -1 : value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is int index && index == -1)
        {
            return null;
        }

        return value;
    }
}
