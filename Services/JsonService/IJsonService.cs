using GeneralsZeroHourEditor.Models;
using GeneralsZeroHourEditor.Models.Weapon;

namespace GeneralsZeroHourEditor.Services.JsonService;

public interface IJsonService
{
    Task SaveToFileAsync(string filePath, string jsonContent);

    // High-level loaders for project data (clean, page-agnostic)
    Task<IReadOnlyList<GameObjectModel>> LoadInfantryAsync(string dataDir);

    Task<IReadOnlyList<GameObjectModel>> LoadVehiclesAsync(string dataDir);

    Task<IReadOnlyList<GameObjectModel>> LoadStructuresAsync(string dataDir);

    // Catalog: Sides come from PlayerTemplate entries (Content: Key = "Side") with BaseSide association
    Task<IReadOnlyList<SideModel>> LoadSidesAsync(string dataDir);

    // Weapons: aggregate from all JSON files (both Generals and Zero Hour) into full objects
    Task<IReadOnlyList<WeaponDefinition>> LoadWeaponsAsync(string dataDir);

    // Utility: enumerate all unique INI keys used by Weapon blocks across Project\Data
    Task<IReadOnlyCollection<string>> GetWeaponKeysAsync(string dataDir);
}
