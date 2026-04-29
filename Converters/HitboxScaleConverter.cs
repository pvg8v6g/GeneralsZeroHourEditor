using Microsoft.UI.Xaml.Data;

namespace GeneralsZeroHourEditor.Converters;

public class HitboxScaleConverter : IValueConverter
{
    private const double Scale = 148.0 / 300.0;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value switch
        {
            int i => i * Scale,
            double d => d * Scale,
            _ => 0.0
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
