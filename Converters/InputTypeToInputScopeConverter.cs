using GeneralsZeroHourEditor.Enumerations;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;

namespace GeneralsZeroHourEditor.Converters;

public class InputTypeToInputScopeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not InputType inputType) return new InputScope { Names = { new InputScopeName { NameValue = InputScopeNameValue.Default } } };
        var scope = new InputScope();
        var name = new InputScopeName
        {
            NameValue = inputType switch
            {
                InputType.Integer or InputType.Double => InputScopeNameValue.Number,
                _ => InputScopeNameValue.Default
            }
        };

        scope.Names.Add(name);
        return scope;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
