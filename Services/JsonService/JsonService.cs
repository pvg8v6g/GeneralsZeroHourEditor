namespace GeneralsZeroHourEditor.Services.JsonService;

public class JsonService : IJsonService
{
    public async Task SaveToFileAsync(string filePath, string jsonContent)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, jsonContent);
    }
}
