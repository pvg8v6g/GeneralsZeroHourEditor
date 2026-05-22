using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using GeneralsZeroHourEditor.Models;

namespace GeneralsZeroHourEditor.Services.LocationService;

public class LocationService : ILocationService
{
    #region Directory

    public string? ProjectDirectory { get; private set; }

    public string? DataDirectory { get; private set; }

    public string? CurrentDirectory
    {
        get
        {
            field ??= CalculateDirectory();
            return field;
        }
    }

    private string? GameDirectory { get; set; }

    public string GraphicsDirectory => @$"{CurrentDirectory}Resources\";

    private static readonly JsonSerializerOptions Options = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public GeneralsEditorConfig GeneralsEditorConfig
    {
        get
        {
            field ??= CreateEditorDirectory();
            return field;
        }
    }

    private string CalculateDirectory()
    {
        const string myRelativePath = nameof(LocationService) + ".cs";
        var pathName = GetSourceFilePathName();
        var path = pathName[..^myRelativePath.Length];
        return Path.GetFullPath(Path.Combine(path, @"..\..\"));
    }

    private string GetSourceFilePathName([CallerFilePath] string? callerFilePath = null) => callerFilePath ?? "";

    private GeneralsEditorConfig CreateEditorDirectory()
    {
        GameDirectory = @$"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\GeneralsEditor\";
        if (!Directory.Exists(GameDirectory))
        {
            Directory.CreateDirectory(GameDirectory);
        }

        var configPath = $"{GameDirectory}config.json";
        if (!File.Exists(configPath))
        {
            File.Create(configPath).Dispose();
            WriteDefaultConfigFile(configPath);
        }

        try
        {
            var text = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<GeneralsEditorConfig>(text) ?? throw new NullReferenceException();
        }
        catch (JsonException)
        {
            WriteDefaultConfigFile(configPath);
            var text = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<GeneralsEditorConfig>(text) ?? throw new NullReferenceException();
        }
    }

    private void WriteDefaultConfigFile(string configPath)
    {
        // Default config: only Location is set to the editor root; game paths remain empty until chosen by user.
        var config = new GeneralsEditorConfig
        {
            Location = GameDirectory!,
            GeneralsPath = string.Empty,
            ZeroHourPath = string.Empty,
            Configured = false
        };
        var jsonString = JsonSerializer.Serialize(config, Options);
        File.WriteAllText(configPath, jsonString);
    }

    public bool CreateProjectDirectory()
    {
        CreateEditorDirectory();
        ProjectDirectory = @$"{GameDirectory}Project\";
        if (Directory.Exists(ProjectDirectory)) return false;
        Directory.CreateDirectory(ProjectDirectory);
        return true;
    }

    public bool CreateDataDirectory()
    {
        CreateEditorDirectory();
        DataDirectory = @$"{GameDirectory}Data\";
        if (Directory.Exists(DataDirectory)) return false;
        Directory.CreateDirectory(DataDirectory);
        return true;
    }

    #endregion

    public void SaveConfig(GeneralsEditorConfig config)
    {
        CreateEditorDirectory();
        var configPath = $"{GameDirectory}config.json";
        var json = JsonSerializer.Serialize(config, Options);
        File.WriteAllText(configPath, json);
    }
}
