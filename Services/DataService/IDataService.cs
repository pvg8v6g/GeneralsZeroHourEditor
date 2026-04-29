namespace GeneralsZeroHourEditor.Services.DataService;

public interface IDataService
{
    string ParseIniToJson(string filePath);

    /// <summary>
    /// Scans the parsed project JSON files to generate a first-cut Infantry schema registry.
    /// </summary>
    /// <param name="projectDataDir">Path to the Project/Data directory containing parsed INI JSON files.</param>
    /// <param name="outputSchemaPath">Destination file path for the generated infantry schema JSON.</param>
    void GenerateInfantrySchema(string projectDataDir, string outputSchemaPath);
}
