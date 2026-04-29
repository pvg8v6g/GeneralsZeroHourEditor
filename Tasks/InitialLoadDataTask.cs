using GeneralsZeroHourEditor.Services.DataService;
using GeneralsZeroHourEditor.Services.JsonService;
using GeneralsZeroHourEditor.Services.LocationService;

namespace GeneralsZeroHourEditor.Tasks;

public class InitialLoadDataTask(ILocationService locationService, IJsonService jsonService, IDataService dataService) : EngineTask
{
    #region Fields

    private string[] _iniFiles = [];

    #endregion

    public override async Task Call()
    {
        MaxWork = GetWorkLoad();
        await DoWork();
        Work = MaxWork;
    }

    private int GetWorkLoad()
    {
        var baseDir = Path.Combine(locationService.GeneralsEditorConfig.Location, "Data", "INI");
        var defaultDir = Path.Combine(baseDir, "Default") + Path.DirectorySeparatorChar;

        _iniFiles = Directory.GetFiles(baseDir, "*.ini", SearchOption.AllDirectories)
            .Where(file => !file.StartsWith(defaultDir, StringComparison.OrdinalIgnoreCase))
            .OrderBy(Path.GetFileName)
            .ToArray();

        return _iniFiles.Length;
    }

    private async Task DoWork()
    {
        locationService.CreateDataDirectory();
        locationService.CreateProjectDirectory();
        var projectDataDir = Path.Combine(locationService.ProjectDirectory!, "Data");
        var schemaDir = Path.Combine(locationService.ProjectDirectory!, "Schema");
        var baseIniDir = Path.Combine(locationService.GeneralsEditorConfig.Location, "Data", "INI");

        if (!Directory.Exists(projectDataDir)) Directory.CreateDirectory(projectDataDir);
        foreach (var file in _iniFiles)
        {
            var json = dataService.ParseIniToJson(file);

            // Maintain a relative directory structure
            var relativePath = Path.GetRelativePath(baseIniDir, file);
            var destPath = Path.Combine(projectDataDir, Path.ChangeExtension(relativePath, ".json"));

            var destDir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

            await jsonService.SaveToFileAsync(destPath, json);
            Work++;
        }

        // After exporting JSON, generate the initial infantry schema registry
        var schemaPath = Path.Combine(schemaDir, "infantry.schema.json");
        dataService.GenerateInfantrySchema(projectDataDir, schemaPath);
    }
}
