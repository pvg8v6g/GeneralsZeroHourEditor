using GeneralsZeroHourEditor.Models;

namespace GeneralsZeroHourEditor.Services.JsonService;

public interface IJsonService
{
    Task SaveToFileAsync(string filePath, string jsonContent);

    // High-level loaders for project data (clean, page-agnostic)
    Task<IReadOnlyList<GameObjectModel>> LoadInfantryAsync(string dataDir);
}
