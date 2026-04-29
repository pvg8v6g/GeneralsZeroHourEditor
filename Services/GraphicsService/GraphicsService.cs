using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using GeneralsZeroHourEditor.Models;
using GeneralsZeroHourEditor.Services.LocationService;
using Microsoft.UI.Xaml.Media.Imaging;

namespace GeneralsZeroHourEditor.Services.GraphicsService;

public class GraphicsService(ILocationService locationService) : IGraphicsService
{
    #region Engine Graphics

    public string GetEngineIconsPath()
    {
        return Path.Combine(locationService.GraphicsDirectory, "Icons.png");
    }

    public async Task<CroppedImage> GetEngineIcon(int index)
    {
        var path = GetEngineIconsPath();
        var source = await GetImage(path);
        var dimensions = await GetImageDimensions(path);
        var w = dimensions.width / 48.0d;
        var viewport = new Rect(((int) (index % w)) * 48.0, ((int) (index / w)) * 48.0, 48.0, 48.0);
        return new CroppedImage { ImagePath = path, ImageSource = source, Rect = viewport };
    }

    #endregion

    #region Images

    public async Task<(double width, double height)> GetImageDimensions(string imagePath)
    {
        var file = await StorageFile.GetFileFromPathAsync(imagePath);
        using var stream = await file.OpenReadAsync();
        var decoder = await BitmapDecoder.CreateAsync(stream);

        return (decoder.PixelWidth, decoder.PixelHeight);
    }

    private async Task<BitmapImage> GetImage(string imagePath)
    {
        var path = Path.IsPathRooted(imagePath) ? imagePath : Path.Combine(locationService.GraphicsDirectory, imagePath);

        if (ImagesCache.Count > 200) ImagesCache.Clear(); // Clear cache if it gets too big
        if (ImagesCache.TryGetValue(path, out var cachedImage))
        {
            return cachedImage;
        }

        var bitmapImage = new BitmapImage(new Uri(path, UriKind.Absolute));
        ImagesCache[path] = bitmapImage;
        return await Task.FromResult(bitmapImage);
    }

    #endregion

    #region Cache

    private Dictionary<string, BitmapImage> ImagesCache { get; } = new();

    #endregion
}
