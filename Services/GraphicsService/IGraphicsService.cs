using GeneralsZeroHourEditor.Models;

namespace GeneralsZeroHourEditor.Services.GraphicsService;

public interface IGraphicsService
{
    string GetEngineIconsPath();

    Task<CroppedImage> GetEngineIcon(int index);

    Task<(double width, double height)> GetImageDimensions(string imagePath);
}
