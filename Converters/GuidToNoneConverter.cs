using Microsoft.UI.Xaml.Data;

namespace GeneralsZeroHourEditor.Converters;

public partial class GuidToNoneConverter : IValueConverter
{
    private static readonly Guid PlaceholderGuid = Guid.NewGuid();

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is null)
        {
            return PlaceholderGuid;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is Guid guid && guid == Guid.Empty)
        {
            return null;
        }

        return value;
    }
}
