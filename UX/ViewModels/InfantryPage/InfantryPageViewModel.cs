using System.Text.Json;
using GeneralsZeroHourEditor.Command;
using GeneralsZeroHourEditor.Models;
using GeneralsZeroHourEditor.Services.LocationService;
using GeneralsZeroHourEditor.Services.GameRegistryService;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace GeneralsZeroHourEditor.UX.ViewModels.InfantryPage;

public class InfantryPageViewModel(ILocationService locationService, IGameRegistryService gameRegistryService) : BaseViewModel
{
    public GameObjectPageModel Model { get; } = new();

    // Cached registries discovered from project Data JSONs
    private readonly SortedSet<string> _armorTemplates = new(StringComparer.OrdinalIgnoreCase);
    private readonly SortedSet<string> _weaponTemplates = new(StringComparer.OrdinalIgnoreCase);
    private readonly SortedSet<string> _locomotors = new(StringComparer.OrdinalIgnoreCase);

    // Note: event subscriptions are registered in LoadedAction to avoid constructor syntax conflicts.

    public RelayCommand<object> ItemInvokedCommand => new(OnItemInvoked);

    public RelayCommand<string> AddPrerequisiteCommand => new(AddPrerequisite);

    public RelayCommand<string> RemovePrerequisiteCommand => new(RemovePrerequisite);

    public RelayCommand<string> AddKindOfCommand => new(AddKindOf);

    public RelayCommand<string> RemoveKindOfCommand => new(RemoveKindOf);

    // Armor & Weapon CRUD
    // Use non-generic RelayCommand so buttons without CommandParameter are enabled
    public RelayCommand AddArmorSetCommand => new(AddArmorSet);

    public RelayCommand<ArmorSetModel> RemoveArmorSetCommand => new(RemoveArmorSet);

    public RelayCommand AddWeaponSetCommand => new(AddWeaponSet);

    public RelayCommand<WeaponSetModel> RemoveWeaponSetCommand => new(RemoveWeaponSet);

    private void OnItemInvoked(object? invokedItem)
    {
        if (invokedItem is GameObjectItemModel item)
        {
            Model.SelectedNode = item;
            LoadSelectedDetail(item.Name);
        }
    }

    protected override async Task LoadedAction()
    {
        LoadSchema();
        // Subscribe to registry changes so dropdowns update live
        gameRegistryService.Registry.Weapons.CollectionChanged += (_, _) => OnRegistriesChanged();
        gameRegistryService.Registry.Armors.CollectionChanged += (_, _) => OnRegistriesChanged();
        gameRegistryService.Registry.Locomotors.CollectionChanged += (_, _) => OnRegistriesChanged();

        LoadTemplateRegistries();
        // Ensure any existing selection (if restored) gets refreshed template lists
        RefreshAvailableLists();
        LoadInfantryList();
        await Task.CompletedTask;
    }

    private void LoadSchema()
    {
        try
        {
            if (locationService.ProjectDirectory is null) return;
            var schemaPath = Path.Combine(locationService.ProjectDirectory, "Schema", "game.schema.json");
            if (File.Exists(schemaPath))
            {
                gameRegistryService.LoadFromSchema(schemaPath);
            }
            else
            {
                var projectDataDir = Path.Combine(locationService.ProjectDirectory, "Data");
                gameRegistryService.Initialize(projectDataDir);
            }

            Model.Modules.Clear();
            foreach (var def in gameRegistryService.Registry.ModuleDefinitions)
            {
                var moduleModel = new GameObjectModuleModel { Name = def.Name };
                foreach (var field in def.Fields) moduleModel.Fields.Add(field);
                foreach (var subDef in def.SubBlocks)
                {
                    var sub = new GameObjectSubBlockModel { Name = subDef.Name };
                    foreach (var f in subDef.Fields) sub.Fields.Add(f);
                    moduleModel.SubBlocks.Add(sub);
                }

                Model.Modules.Add(moduleModel);
            }
        }
        catch
        {
            // ignore schema errors for first cut
        }
    }

    private void LoadInfantryList()
    {
        Model.GameObjectGroups.Clear();
        if (locationService.ProjectDirectory is null) return;
        var projectDataDir = Path.Combine(locationService.ProjectDirectory, "Data");
        if (!Directory.Exists(projectDataDir)) return;

        var sideMap = new Dictionary<string, SideGroupModel>(StringComparer.OrdinalIgnoreCase);

        foreach (var jsonPath in Directory.EnumerateFiles(projectDataDir, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                using var stream = File.OpenRead(jsonPath);
                // Use higher parse depth to match data generation tolerances
                var parseOptions = new JsonDocumentOptions { MaxDepth = 4096 };
                using var doc = JsonDocument.Parse(stream, parseOptions);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) continue;
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.Object) continue;
                    if (!element.TryGetProperty("Type", out var typeProp) || typeProp.GetString() != "Object") continue;
                    if (!element.TryGetProperty("Content", out var content) || content.ValueKind != JsonValueKind.Array) continue;
                    if (!HasKindOf(content, "INFANTRY")) continue;

                    var name = GetName(element);
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var side = GetSide(content) ?? "Unknown";

                    if (!sideMap.TryGetValue(side, out var group))
                    {
                        group = new SideGroupModel { Name = side };
                        sideMap[side] = group;
                        Model.GameObjectGroups.Add(group);
                    }

                    if (group.Children.All(i => i.Name != name))
                    {
                        group.Children.Add(new GameObjectItemModel { Name = name });
                    }
                }
            }
            catch
            {
                // ignore file
            }
        }

        // Sort groups and items
        var sortedGroups = Model.GameObjectGroups.OrderBy(g => g.Name).ToList();
        Model.GameObjectGroups.Clear();
        foreach (var group in sortedGroups)
        {
            var sortedItems = group.Children.OrderBy(i => i.Name).ToList();
            group.Children.Clear();
            foreach (var item in sortedItems) group.Children.Add(item);
            Model.GameObjectGroups.Add(group);
        }
    }

    private void LoadTemplateRegistries()
    {
        _armorTemplates.Clear();
        _weaponTemplates.Clear();
        _locomotors.Clear();

        // Prefer the centralized registry if available (built by GameRegistryService).
        // IMPORTANT: Do NOT early-return if some catalogs are empty. We merge with fallback scan per-category.
        foreach (var a in gameRegistryService.Registry.Armors) _armorTemplates.Add(a);
        foreach (var w in gameRegistryService.Registry.Weapons) _weaponTemplates.Add(w);
        foreach (var l in gameRegistryService.Registry.Locomotors) _locomotors.Add(l);

        if (locationService.ProjectDirectory is null) return;
        // Fallback: scan Data JSONs directly for any categories still empty or to catch new items not yet in schema
        var projectDataDir = Path.Combine(locationService.ProjectDirectory, "Data");
        if (!Directory.Exists(projectDataDir))
        {
            // Still refresh the UI lists from whatever we already have
            RefreshAvailableLists();
            return;
        }

        foreach (var jsonPath in Directory.EnumerateFiles(projectDataDir, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                using var stream = File.OpenRead(jsonPath);
                using var doc = JsonDocument.Parse(stream);
                if (doc.RootElement.ValueKind is not JsonValueKind.Array) continue;

                foreach (var element in doc.RootElement.EnumerateArray().Where(element => element.ValueKind is JsonValueKind.Object))
                {
                    if (!element.TryGetProperty("Type", out var typeProp) || typeProp.ValueKind is not JsonValueKind.String) continue;
                    var type = typeProp.GetString();
                    if (string.IsNullOrWhiteSpace(type)) continue;

                    var name = GetName(element);
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    if (string.Equals(type, "Armor", StringComparison.OrdinalIgnoreCase)) _armorTemplates.Add(name);
                    else if (string.Equals(type, "Weapon", StringComparison.OrdinalIgnoreCase)) _weaponTemplates.Add(name);
                    else if (string.Equals(type, "Locomotor", StringComparison.OrdinalIgnoreCase)) _locomotors.Add(name);
                }
            }
            catch
            {
                /* ignore unreadable file */
            }
        }

        RefreshAvailableLists();
    }

    private void OnRegistriesChanged()
    {
        // Merge latest registry values and resync detail pickers
        LoadTemplateRegistries();
    }

    private void RefreshAvailableLists()
    {
        // Push cached sets into the current detail model so ComboBoxes get ItemsSource
        Model.Detail ??= new GameObjectDetailModel();
        Model.Detail.AvailableArmorTemplates.Clear();
        Model.Detail.AvailableWeaponTemplates.Clear();
        Model.Detail.AvailableLocomotors.Clear();
        foreach (var a in _armorTemplates) Model.Detail.AvailableArmorTemplates.Add(a);
        foreach (var w in _weaponTemplates) Model.Detail.AvailableWeaponTemplates.Add(w);
        foreach (var l in _locomotors) Model.Detail.AvailableLocomotors.Add(l);
    }

    private static string? GetSide(JsonElement content)
    {
        foreach (var prop in content.EnumerateArray().Where(prop => prop.ValueKind == JsonValueKind.Object))
        {
            if (!prop.TryGetProperty("Key", out var keyProp)) continue;
            if (keyProp.GetString() is not "Side") continue;
            if (!prop.TryGetProperty("Value", out var valArr) || valArr.ValueKind is not JsonValueKind.Array) continue;
            var first = valArr.EnumerateArray().FirstOrDefault();
            return first.ValueKind is JsonValueKind.String ? first.GetString() : null;
        }

        return null;
    }

    private static bool HasKindOf(JsonElement content, string flag)
    {
        foreach (var prop in content.EnumerateArray().Where(prop => prop.ValueKind is JsonValueKind.Object))
        {
            if (!prop.TryGetProperty("Key", out var keyProp)) continue;
            if (keyProp.GetString() is not "KindOf") continue;
            if (!prop.TryGetProperty("Value", out var valArr) || valArr.ValueKind is not JsonValueKind.Array) continue;
            if (valArr.EnumerateArray().Any(v =>
                    v.ValueKind is JsonValueKind.String && string.Equals(v.GetString(), flag, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        return false;
    }

    private static string? GetName(JsonElement obj)
    {
        if (!obj.TryGetProperty("Name", out var nameArr) || nameArr.ValueKind is not JsonValueKind.Array) return null;
        var first = nameArr.EnumerateArray().FirstOrDefault();
        return first.ValueKind is JsonValueKind.String ? first.GetString() : null;
    }

    private void LoadSelectedDetail(string name)
    {
        if (locationService.ProjectDirectory is null) return;
        var projectDataDir = Path.Combine(locationService.ProjectDirectory, "Data");
        if (!Directory.Exists(projectDataDir)) return;

        foreach (var jsonPath in Directory.EnumerateFiles(projectDataDir, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                using var stream = File.OpenRead(jsonPath);
                using var doc = JsonDocument.Parse(stream);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) continue;
                foreach (var element in doc.RootElement.EnumerateArray().Where(element => element.ValueKind == JsonValueKind.Object))
                {
                    if (!element.TryGetProperty("Type", out var typeProp) || typeProp.GetString() != "Object") continue;
                    var objName = GetName(element);
                    if (!string.Equals(objName, name, StringComparison.Ordinal)) continue;

                    if (!element.TryGetProperty("Content", out var content) || content.ValueKind != JsonValueKind.Array) continue;
                    var detail = new GameObjectDetailModel { Name = name, SourceFilePath = jsonPath };

                    // Populate available registries for pickers (clear first for robustness)
                    // First, try cached registries gathered earlier
                    detail.AvailableArmorTemplates.Clear();
                    detail.AvailableWeaponTemplates.Clear();
                    detail.AvailableLocomotors.Clear();
                    foreach (var a in _armorTemplates) detail.AvailableArmorTemplates.Add(a);
                    foreach (var w in _weaponTemplates) detail.AvailableWeaponTemplates.Add(w);
                    foreach (var l in _locomotors) detail.AvailableLocomotors.Add(l);

                    // Belt-and-suspenders: If any list is still empty, directly parse the known catalog files.
                    PopulateAvailableDirect(projectDataDir, detail);

                    // Iterate content array of key/value pairs and block items and fill detail where relevant
                    foreach (var item in content.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object))
                    {
                        // 1) Handle block items like Body/ArmorSet/WeaponSet represented as { Type, Name?, Content }
                        if (item.TryGetProperty("Type", out var blockTypeProp) &&
                            item.TryGetProperty("Content", out var blockContent) && blockContent.ValueKind == JsonValueKind.Array)
                        {
                            var blockType = blockTypeProp.GetString() ?? string.Empty;
                            if (string.Equals(blockType, "Body", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (var sub in blockContent.EnumerateArray().Where(sub => sub.ValueKind == JsonValueKind.Object))
                                {
                                    if (!sub.TryGetProperty("Key", out var sk) || !sub.TryGetProperty("Value", out var sv) ||
                                        sv.ValueKind != JsonValueKind.Array) continue;
                                    var sKey = sk.GetString();
                                    if (string.Equals(sKey, "MaxHealth", StringComparison.OrdinalIgnoreCase))
                                    {
                                        detail.MaxHealth = sv.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                                    }
                                    else if (string.Equals(sKey, "InitialHealth", StringComparison.OrdinalIgnoreCase))
                                    {
                                        detail.InitialHealth = sv.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                                    }
                                }

                                continue;
                            }
                            else if (string.Equals(blockType, "ArmorSet", StringComparison.OrdinalIgnoreCase))
                            {
                                var armorSet = new ArmorSetModel();
                                foreach (var sub in blockContent.EnumerateArray().Where(sub => sub.ValueKind == JsonValueKind.Object))
                                {
                                    if (!sub.TryGetProperty("Key", out var sk) || !sub.TryGetProperty("Value", out var sv) ||
                                        sv.ValueKind is not JsonValueKind.Array) continue;
                                    var sKey = sk.GetString();
                                    if (string.Equals(sKey, "Armor", StringComparison.OrdinalIgnoreCase))
                                    {
                                        armorSet.Armor = sv.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                                    }
                                    else if (string.Equals(sKey, "Conditions", StringComparison.OrdinalIgnoreCase))
                                    {
                                        var conds = sv.EnumerateArray()
                                            .Where(x => x.ValueKind is JsonValueKind.String)
                                            .Select(x => x.GetString())
                                            .Where(x => x is not null)
                                            .Cast<string>()
                                            .ToList();
                                        armorSet.ConditionsCsv = string.Join(", ", conds);
                                    }
                                }

                                detail.ArmorSets.Add(armorSet);
                                continue;
                            }
                            else if (string.Equals(blockType, "WeaponSet", StringComparison.OrdinalIgnoreCase))
                            {
                                var weaponSet = new WeaponSetModel();
                                foreach (var sub in blockContent.EnumerateArray().Where(sub => sub.ValueKind == JsonValueKind.Object))
                                {
                                    if (!sub.TryGetProperty("Key", out var sk) || !sub.TryGetProperty("Value", out var sv) ||
                                        sv.ValueKind != JsonValueKind.Array) continue;
                                    var sKey = sk.GetString();
                                    if (string.Equals(sKey, "Conditions", StringComparison.OrdinalIgnoreCase))
                                    {
                                        var conds = (from c in sv.EnumerateArray() where c.ValueKind == JsonValueKind.String select c.GetString())
                                            .ToList();
                                        weaponSet.ConditionsCsv = string.Join(", ", conds);
                                    }
                                    else if (string.Equals(sKey, "Weapon", StringComparison.OrdinalIgnoreCase))
                                    {
                                        var vals = sv.EnumerateArray().Where(v => v.ValueKind == JsonValueKind.String).Select(v => v.GetString())
                                            .ToList();
                                        if (vals.Count < 2) continue;
                                        var slot = vals[0];
                                        var weap = vals[1];
                                        if (slot is null || weap is null) continue;
                                        if (slot.Equals("PRIMARY", StringComparison.OrdinalIgnoreCase)) weaponSet.Primary = weap;
                                        if (slot.Equals("SECONDARY", StringComparison.OrdinalIgnoreCase)) weaponSet.Secondary = weap;
                                        if (slot.Equals("TERTIARY", StringComparison.OrdinalIgnoreCase)) weaponSet.Tertiary = weap;
                                    }
                                }

                                detail.WeaponSets.Add(weaponSet);
                                continue;
                            }
                        }

                        // 2) Handle simple key/value properties
                        if (!item.TryGetProperty("Key", out var keyProp)) continue;
                        var key = keyProp.GetString();
                        if (string.IsNullOrEmpty(key)) continue;
                        if (!item.TryGetProperty("Value", out var valueArr) || valueArr.ValueKind != JsonValueKind.Array) continue;

                        switch (key)
                        {
                            case "Side":
                                detail.Side = FirstString();
                                break;
                            case "EditorSorting":
                                detail.EditorSorting = FirstString();
                                break;
                            case "CommandSet":
                                detail.CommandSet = FirstString();
                                break;
                            case "DisplayName":
                                detail.DisplayName = FirstString();
                                break;
                            case "Description":
                                detail.Description = FirstString();
                                break;
                            case "SelectPortrait":
                                detail.SelectPortrait = FirstString();
                                break;
                            case "ButtonImage":
                                detail.ButtonImage = FirstString();
                                break;
                            case "UpgradeCameo1":
                            case "UpgradeCameo2":
                            case "UpgradeCameo3":
                            case "UpgradeCameo4":
                                AddIfMissing(detail.UpgradeCameos, FirstString());
                                break;
                            case "BuildCost":
                                detail.BuildCost = FirstString();
                                break;
                            case "BuildTime":
                                detail.BuildTime = FirstString();
                                break;
                            case "BuildCompletion":
                                detail.BuildCompletion = FirstString();
                                break;
                            case "Prerequisites":
                                valueArr.EnumerateArray()
                                    .Where(v => v.ValueKind is JsonValueKind.String)
                                    .Select(v => v.GetString())
                                    .Where(x => x is not null)
                                    .Cast<string>()
                                    .ToList()
                                    .ForEach(x => AddIfMissing(detail.Prerequisites, x));
                                break;
                            case "KindOf":
                                valueArr.EnumerateArray()
                                    .Where(v => v.ValueKind is JsonValueKind.String)
                                    .Select(v => v.GetString())
                                    .Where(x => x is not null)
                                    .Cast<string>()
                                    .ToList()
                                    .ForEach(x => AddIfMissing(detail.KindOf, x));
                                break;
                            case "VisionRange":
                                detail.VisionRange = FirstString();
                                break;
                            case "ShroudClearingRange":
                                detail.ShroudClearingRange = FirstString();
                                break;
                            case "Geometry":
                                detail.Geometry = FirstString();
                                break;
                            case "GeometryMajorRadius":
                                detail.GeometryMajorRadius = FirstString();
                                break;
                            case "GeometryMinorRadius":
                                detail.GeometryMinorRadius = FirstString();
                                break;
                            case "GeometryHeight":
                                detail.GeometryHeight = FirstString();
                                break;
                            case "GeometryIsSmall":
                                detail.GeometryIsSmall = FirstString();
                                break;
                            case "Shadow":
                                detail.Shadow = FirstString();
                                break;
                            case "ShadowSizeX":
                                detail.ShadowSizeX = FirstString();
                                break;
                            case "ShadowSizeY":
                                detail.ShadowSizeY = FirstString();
                                break;
                            case "ShadowTexture":
                                detail.ShadowTexture = FirstString();
                                break;

                            // Body/ArmorSet/WeaponSet are now handled via block branch above

                            // Movement
                            case "Locomotor":
                                detail.Locomotor = FirstString();
                                break;
                            case "Speed":
                                detail.Speed = FirstString();
                                break;
                            case "Acceleration":
                                detail.Acceleration = FirstString();
                                break;
                            case "TurnRate":
                                detail.TurnRate = FirstString();
                                break;
                            case "MovementZone":
                                detail.MovementZone = FirstString();
                                break;
                        }

                        continue;

                        string FirstString() => valueArr.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                    }

                    // Assign before pushing available lists so ComboBoxes get ItemsSource immediately
                    Model.Detail = detail;
                    // Now push current cached registries into the detail's Available* collections
                    RefreshAvailableLists();
                    return;
                }
            }
            catch
            {
                // ignore bad file
            }
        }
    }

    private static void PopulateAvailableDirect(string projectDataDir, GameObjectDetailModel detail)
    {
        // Helper to read names from a specific file/type pair
        static void ReadNames(string filePath, string typeName, ICollection<string> destination)
        {
            if (!File.Exists(filePath)) return;
            try
            {
                using var stream = File.OpenRead(filePath);
                using var doc = JsonDocument.Parse(stream);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) return;
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.Object) continue;
                    if (!element.TryGetProperty("Type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String) continue;
                    if (!string.Equals(typeProp.GetString(), typeName, StringComparison.OrdinalIgnoreCase)) continue;
                    if (!element.TryGetProperty("Name", out var nameArr) || nameArr.ValueKind != JsonValueKind.Array) continue;
                    var first = nameArr.EnumerateArray().FirstOrDefault();
                    if (first.ValueKind == JsonValueKind.String)
                    {
                        var n = first.GetString();
                        if (!string.IsNullOrWhiteSpace(n) && !destination.Contains(n)) destination.Add(n);
                    }
                }
            }
            catch
            {
                /* ignore malformed file */
            }
        }

        // Only add if empty to avoid duplicates and reduce work
        if (detail.AvailableArmorTemplates.Count == 0)
        {
            var armorPath = Path.Combine(projectDataDir, "Armor.json");
            ReadNames(armorPath, "Armor", detail.AvailableArmorTemplates);
        }

        if (detail.AvailableWeaponTemplates.Count == 0)
        {
            var weaponPath = Path.Combine(projectDataDir, "Weapon.json");
            ReadNames(weaponPath, "Weapon", detail.AvailableWeaponTemplates);
        }

        if (detail.AvailableLocomotors.Count == 0)
        {
            var locoPath = Path.Combine(projectDataDir, "Locomotor.json");
            ReadNames(locoPath, "Locomotor", detail.AvailableLocomotors);
        }
    }

    private static void AddIfMissing(ObservableCollection<string> list, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        if (!list.Contains(value)) list.Add(value);
    }

    private void AddPrerequisite(string? value)
    {
        if (Model.Detail == null) return;
        if (string.IsNullOrWhiteSpace(value)) return;
        if (!Model.Detail.Prerequisites.Contains(value)) Model.Detail.Prerequisites.Add(value);
    }

    private void RemovePrerequisite(string key)
    {
        Model.Detail?.Prerequisites.Remove(key);
    }

    private void AddKindOf(string? value)
    {
        if (Model.Detail == null) return;
        if (string.IsNullOrWhiteSpace(value)) return;
        if (!Model.Detail.KindOf.Contains(value)) Model.Detail.KindOf.Add(value);
    }

    private void RemoveKindOf(string flag)
    {
        Model.Detail?.KindOf.Remove(flag);
    }

    private void AddArmorSet()
    {
        Model.Detail?.ArmorSets.Add(new ArmorSetModel { Armor = string.Empty, ConditionsCsv = string.Empty });
    }

    private void RemoveArmorSet(ArmorSetModel? set)
    {
        if (set == null) return;
        Model.Detail?.ArmorSets.Remove(set);
    }

    private void AddWeaponSet()
    {
        Model.Detail?.WeaponSets.Add(new WeaponSetModel
        {
            ConditionsCsv = string.Empty,
            Primary = string.Empty,
            Secondary = string.Empty,
            Tertiary = string.Empty
        });
    }

    private void RemoveWeaponSet(WeaponSetModel? set)
    {
        if (set is null) return;
        Model.Detail?.WeaponSets.Remove(set);
    }
}
