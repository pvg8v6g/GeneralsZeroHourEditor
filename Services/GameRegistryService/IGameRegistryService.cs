using GeneralsZeroHourEditor.Models.Schema;

namespace GeneralsZeroHourEditor.Services.GameRegistryService;

public interface IGameRegistryService
{
    GameRegistryModel Registry { get; }
    void Initialize(string projectDataDir);
    void LoadFromSchema(string schemaPath);
    void SaveSchema(string schemaPath);
}
