using GeneralsZeroHourEditor.Models;

namespace GeneralsZeroHourEditor.Services.LocationService;

public interface ILocationService
{
    string? ProjectDirectory { get; }

    string? DataDirectory { get; }

    string? CurrentDirectory { get; }

    string GraphicsDirectory { get; }

    GeneralsEditorConfig GeneralsEditorConfig { get; }

    bool CreateProjectDirectory();

    bool CreateDataDirectory();

    void SaveConfig(GeneralsEditorConfig config);
}
