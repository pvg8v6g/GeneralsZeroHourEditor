using Windows.Foundation;
using MercuryLibrary.WinUI3Components;
using Microsoft.UI.Xaml.Media;

namespace GeneralsZeroHourEditor.Models;

public class CroppedImage : PropertyChangedUpdater
{
    public string? ImagePath
    {
        get;
        set => SetField(ref field, value);
    }

    public ImageSource? ImageSource
    {
        get;
        set => SetField(ref field, value);
    }

    public Rect? Rect
    {
        get;
        set => SetField(ref field, value);
    } = null;
}
