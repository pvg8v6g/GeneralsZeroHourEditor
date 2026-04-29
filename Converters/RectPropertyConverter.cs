using Windows.Foundation;
using Microsoft.UI.Xaml.Data;

namespace GeneralsZeroHourEditor.Converters;

public partial class RectPropertyConverter : IValueConverter
{
    public enum Property
    {
        Width,
        Height,
        Rect,
        X,
        Y,
        NegativeX,
        NegativeY,
        NormalizedRect
    }

    public Property PropertyToExtract { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is Rect rect)
        {
            return PropertyToExtract switch
            {
                Property.Width => rect.Width,
                Property.Height => rect.Height,
                Property.Rect => rect,
                Property.X => rect.X,
                Property.Y => rect.Y,
                Property.NegativeX => -rect.X,
                Property.NegativeY => -rect.Y,
                Property.NormalizedRect => new Rect(0, 0, rect.Width, rect.Height),
                _ => 0.0
            };
        }

        return PropertyToExtract == Property.Rect ? new Rect(0, 0, 0, 0) : 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
