namespace GeneralsZeroHourEditor.Services.JsonService;

public interface IJsonService
{
    Task SaveToFileAsync(string filePath, string jsonContent);
}
