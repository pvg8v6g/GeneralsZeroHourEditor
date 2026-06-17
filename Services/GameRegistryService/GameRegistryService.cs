using System.Text.Json;
using GeneralsZeroHourEditor.Models.Schema;
using GeneralsZeroHourEditor.Services.DataService;
using GeneralsZeroHourEditor.Services.GameDataService;

namespace GeneralsZeroHourEditor.Services.GameRegistryService;

public class GameRegistryService(IDataService dataService, IGameDataService gameDataService) : IGameRegistryService
{
    public GameRegistryModel Registry { get; } = new();

    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public void Initialize(string projectDataDir)
    {
        if (string.IsNullOrWhiteSpace(projectDataDir) || !Directory.Exists(projectDataDir)) return;

        // 1. Collect and cache catalogs in GameDataService (load once)
        //    Populate central, long-lived collections if they are not yet loaded.

        // Weapons are now loaded as full objects elsewhere during initial load

        if (gameDataService.GameArmors.Count == 0)
        {
            PopulateCatalog(gameDataService.GameArmors, dataService.CollectTopLevelNames(projectDataDir, "Armor"));
        }

        if (gameDataService.FXLists.Count == 0)
        {
            PopulateCatalog(gameDataService.FXLists, dataService.CollectTopLevelNames(projectDataDir, "FXList"));
        }

        if (gameDataService.GameLocomotors.Count == 0)
        {
            PopulateCatalog(gameDataService.GameLocomotors, dataService.CollectTopLevelNames(projectDataDir, "Locomotor"));
        }

        //    No longer mirror into Registry (catalogs now live solely in GameDataService)

        // 2. Mine Modules and Fields (Generic Schema Mining)
        MineSchema(projectDataDir);
    }

    private static void PopulateCatalog(System.Collections.ObjectModel.ObservableCollection<string> catalog, string[] names)
    {
        catalog.Clear();
        foreach (var name in names.OrderBy(n => n))
        {
            catalog.Add(name);
        }
    }

    private void MineSchema(string projectDataDir)
    {
        var moduleMap = new Dictionary<string, ModuleDefinitionModel>(StringComparer.OrdinalIgnoreCase);

        var parseOptions = new JsonDocumentOptions { MaxDepth = 4096 };
        foreach (var jsonPath in Directory.EnumerateFiles(projectDataDir, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                using var stream = File.OpenRead(jsonPath);
                using var doc = JsonDocument.Parse(stream, parseOptions);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) continue;

                foreach (var element in doc.RootElement.EnumerateArray().Where(element => element.ValueKind == JsonValueKind.Object))
                {
                    if (!element.TryGetProperty("Type", out var typeProp) || !typeProp.ValueEquals("Object")) continue;
                    if (!element.TryGetProperty("Content", out var content) || content.ValueKind != JsonValueKind.Array) continue;

                    foreach (var item in content.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object)
                                 .Where(item => !item.TryGetProperty("Key", out _)))
                    {
                        if (!item.TryGetProperty("Type", out var subTypeProp)) continue;

                        var moduleType = subTypeProp.GetString() ?? string.Empty;
                        if (string.IsNullOrEmpty(moduleType)) continue;

                        if (!moduleMap.TryGetValue(moduleType, out var def))
                        {
                            def = new ModuleDefinitionModel { Name = moduleType };
                            moduleMap[moduleType] = def;
                        }

                        // Collect variant names (e.g., Behavior = <Variant> ModuleTag_...)
                        if (item.TryGetProperty("Name", out var nameArr) && nameArr.ValueKind == JsonValueKind.Array)
                        {
                            var variant = nameArr.EnumerateArray().FirstOrDefault().GetString();
                            if (!string.IsNullOrWhiteSpace(variant) && !def.Variants.Contains(variant))
                            {
                                def.Variants.Add(variant);
                            }
                        }

                        if (item.TryGetProperty("Content", out var subContent) && subContent.ValueKind == JsonValueKind.Array)
                        {
                            AnalyzeModuleContent(def, subContent);
                        }
                    }
                }
            }
            catch
            {
                /* skip corrupted files */
            }
        }

        Registry.ModuleDefinitions.Clear();
        foreach (var def in moduleMap.Values.OrderBy(m => m.Name))
        {
            Registry.ModuleDefinitions.Add(def);
        }
    }

    private void AnalyzeModuleContent(ModuleDefinitionModel def, JsonElement content)
    {
        foreach (var item in content.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object))
        {
            if (item.TryGetProperty("Key", out var keyProp))
            {
                var field = keyProp.GetString();
                if (!string.IsNullOrWhiteSpace(field) && !def.Fields.Contains(field))
                {
                    def.Fields.Add(field);
                }
            }
            else if (item.TryGetProperty("Type", out var typeProp))
            {
                var subBlockName = typeProp.GetString();
                if (string.IsNullOrWhiteSpace(subBlockName)) continue;

                var subDef = def.SubBlocks.FirstOrDefault(s => s.Name == subBlockName);
                if (subDef == null)
                {
                    subDef = new SubBlockDefinitionModel { Name = subBlockName };
                    def.SubBlocks.Add(subDef);
                }

                if (!item.TryGetProperty("Content", out var subContent) || subContent.ValueKind != JsonValueKind.Array) continue;
                foreach (var subItem in subContent.EnumerateArray())
                {
                    if (subItem.ValueKind != JsonValueKind.Object || !subItem.TryGetProperty("Key", out var subKeyProp)) continue;
                    var subField = subKeyProp.GetString();
                    if (!string.IsNullOrWhiteSpace(subField) && !subDef.Fields.Contains(subField)) subDef.Fields.Add(subField);
                }
            }
        }
    }

    public void LoadFromSchema(string schemaPath)
    {
        if (!File.Exists(schemaPath)) return;
        var json = File.ReadAllText(schemaPath);
        var loaded = JsonSerializer.Deserialize<GameRegistryModel>(json);
        if (loaded == null) return;

        Registry.ModuleDefinitions.Clear();
        foreach (var item in loaded.ModuleDefinitions) Registry.ModuleDefinitions.Add(item);
    }

    public void SaveSchema(string schemaPath)
    {
        var json = JsonSerializer.Serialize(Registry, _jsonOptions);
        var dir = Path.GetDirectoryName(schemaPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(schemaPath, json);
    }
}
