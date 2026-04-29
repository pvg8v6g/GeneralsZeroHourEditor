using Microsoft.UI.Xaml.Data;

namespace GeneralsZeroHourEditor.Converters;

public class ScaleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double size && double.TryParse(parameter?.ToString(), out var percent))
        {
            return size * (percent / 100.0);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
