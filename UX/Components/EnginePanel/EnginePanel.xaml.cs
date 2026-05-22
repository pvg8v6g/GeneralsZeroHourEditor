using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

namespace GeneralsZeroHourEditor.UX.Components.EnginePanel;

[ContentProperty(Name = nameof(InnerContent))]
public partial class EnginePanel
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(EnginePanel), new PropertyMetadata(null));

    public string Title
    {
        get => (string) GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty InnerContentProperty = DependencyProperty.Register(
        nameof(InnerContent), typeof(object), typeof(EnginePanel), new PropertyMetadata(null));

    public object InnerContent
    {
        get => GetValue(InnerContentProperty);
        set => SetValue(InnerContentProperty, value);
    }

    public static readonly DependencyProperty ScrollerViewportHeightProperty = DependencyProperty.Register(
        nameof(ScrollerViewportHeight), typeof(double), typeof(EnginePanel), new PropertyMetadata(0.0));

    public double ScrollerViewportHeight
    {
        get => (double) GetValue(ScrollerViewportHeightProperty);
        set => SetValue(ScrollerViewportHeightProperty, value);
    }

    public EnginePanel()
    {
        InitializeComponent();

        var binding = new Binding
        {
            Source = ScrollRoot,
            Path = new PropertyPath("ViewportHeight"),
            Mode = BindingMode.OneWay
        };
        SetBinding(ScrollerViewportHeightProperty, binding);
    }
}
