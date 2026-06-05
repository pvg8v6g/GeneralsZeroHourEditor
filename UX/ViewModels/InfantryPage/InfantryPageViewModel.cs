using System.Collections.ObjectModel;
using System.Text.Json;
using GeneralsZeroHourEditor.Command;
using GeneralsZeroHourEditor.Models;
using GeneralsZeroHourEditor.Services.GameDataService;
using GeneralsZeroHourEditor.Services.GameRegistryService;
using GeneralsZeroHourEditor.Services.LocationService;

namespace GeneralsZeroHourEditor.UX.ViewModels.InfantryPage;

public class InfantryPageViewModel(ILocationService locationService, IGameRegistryService gameRegistryService, IGameDataService gameDataService)
    : BaseViewModel
{
    #region Properties

    public GameObjectPageModel Model { get; } = new();

    public string[] Armors
    {
        get;
        set => SetField(ref field, value);
    } = [];

    public string[] Weapons
    {
        get;
        set => SetField(ref field, value);
    } = [];

    public string[] Locomotors
    {
        get;
        set => SetField(ref field, value);
    } = [];

    public IReadOnlyList<string> PrereqTypes => _prereqTypes;

    #endregion

    #region Fields

    // Cached registries discovered from project Data JSONs
    private readonly SortedSet<string> _armorTemplates = new(StringComparer.OrdinalIgnoreCase);
    private readonly SortedSet<string> _weaponTemplates = new(StringComparer.OrdinalIgnoreCase);
    private readonly SortedSet<string> _locomotors = new(StringComparer.OrdinalIgnoreCase);

    // Note: event subscriptions are registered in LoadedAction to avoid constructor syntax conflicts.

    private static readonly string[] _prereqTypes = ["Object", "Science"];

    #endregion

    #region Commands

    public RelayCommand<object> ItemInvokedCommand => new(SelectedItemChanged);

    public RelayCommand AddPrerequisiteCommand => new(() => AddPrerequisite(null));

    public RelayCommand<PrerequisiteEntryModel> RemovePrerequisiteCommand => new(RemovePrerequisite);

    public RelayCommand<string> AddKindOfCommand => new(AddKindOf);

    public RelayCommand<string> RemoveKindOfCommand => new(RemoveKindOf);

    // Armor & Weapon CRUD
    // Use non-generic RelayCommand so buttons without CommandParameter are enabled
    public RelayCommand AddArmorSetCommand => new(AddArmorSet);

    public RelayCommand<ArmorSetModel> RemoveArmorSetCommand => new(RemoveArmorSet);

    public RelayCommand AddWeaponSetCommand => new(AddWeaponSet);

    public RelayCommand<WeaponSetModel> RemoveWeaponSetCommand => new(RemoveWeaponSet);

    public RelayCommand AddLocomotorCommand => new(AddLocomotorSet);

    public RelayCommand<LocomotorSetModel> RemoveLocomotorCommand => new(RemoveLocomotorSet);

    #endregion

    #region Actions & Listeners

    protected override async Task LoadedAction()
    {
        LoadSchema();
        // Subscribe to GameDataService catalogs (single source of truth for templates)
        gameDataService.GameWeapons.CollectionChanged += (_, _) => OnRegistriesChanged();
        gameDataService.GameArmors.CollectionChanged += (_, _) => OnRegistriesChanged();
        gameDataService.GameLocomotors.CollectionChanged += (_, _) => OnRegistriesChanged();

        LoadTemplateRegistries();
        // Ensure any existing selection (if restored) gets refreshed template lists
        RefreshAvailableLists();
        LoadInfantryList();
        await Task.CompletedTask;
    }

    private void SelectedItemChanged(object? invokedItem)
    {
        if (invokedItem is not GameObjectItemModel item) return;
        Model.SelectedNode = item;
        LoadSelectedDetail(item.Name);
    }

    #endregion

    #region Initialization

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
        // If infantry are already preloaded in memory, build from that and exit
        Model.GameObjectGroups.Clear();
        if (gameDataService.Infantry.Count > 0)
        {
            var sideMap = new Dictionary<string, SideGroupModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var unit in gameDataService.Infantry)
            {
                var side = string.IsNullOrWhiteSpace(unit.Side) ? "Unknown" : unit.Side;
                if (!sideMap.TryGetValue(side, out var group))
                {
                    group = new SideGroupModel { Name = side };
                    sideMap[side] = group;
                    Model.GameObjectGroups.Add(group);
                }

                if (group.Children.All(i => i.Name != unit.Name))
                {
                    group.Children.Add(new GameObjectItemModel { Name = unit.Name });
                }
            }

            // Sort groups and items
            var sortedGroupsMem = Model.GameObjectGroups.OrderBy(g => g.Name).ToList();
            Model.GameObjectGroups.Clear();
            foreach (var group in sortedGroupsMem)
            {
                var sortedItems = group.Children.OrderBy(i => i.Name).ToList();
                group.Children.Clear();
                foreach (var item in sortedItems) group.Children.Add(item);
                Model.GameObjectGroups.Add(group);
            }

            return;
        }

        // Fallback: legacy direct file scan (will be removed once all pages rely solely on preloaded data)
        if (locationService.ProjectDirectory is null) return;
        var projectDataDir = Path.Combine(locationService.ProjectDirectory, "Data");
        if (!Directory.Exists(projectDataDir)) return;

        var sideMapFallback = new Dictionary<string, SideGroupModel>(StringComparer.OrdinalIgnoreCase);

        foreach (var jsonPath in Directory.EnumerateFiles(projectDataDir, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                using var stream = File.OpenRead(jsonPath);
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

                    if (!sideMapFallback.TryGetValue(side, out var group))
                    {
                        group = new SideGroupModel { Name = side };
                        sideMapFallback[side] = group;
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

        // Source solely from GameDataService (populated once by GameRegistryService.Initialize)
        foreach (var a in gameDataService.GameArmors) _armorTemplates.Add(a);
        foreach (var w in gameDataService.GameWeapons) _weaponTemplates.Add(w);
        foreach (var l in gameDataService.GameLocomotors) _locomotors.Add(l);

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

    #endregion

    #region Parsing Helpers

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

    #endregion

    #region Detail Loading

    private void LoadSelectedDetail(string name)
    {
        // Prefer preloaded in-memory object
        var picked = gameDataService.Infantry.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));
        if (picked is not null)
        {
            Model.Detail = picked;
            // Prime available catalogs for pickers
            picked.AvailableArmorTemplates.Clear();
            foreach (var a in gameDataService.GameArmors) picked.AvailableArmorTemplates.Add(a);
            picked.AvailableWeaponTemplates.Clear();
            foreach (var w in gameDataService.GameWeapons) picked.AvailableWeaponTemplates.Add(w);
            picked.AvailableLocomotors.Clear();
            foreach (var l in gameDataService.GameLocomotors) picked.AvailableLocomotors.Add(l);

            // Also refresh cached sets and push
            LoadTemplateRegistries();
            RefreshAvailableLists();

            // Populate prerequisite catalogs (objects/sciences) on demand
            if (locationService.ProjectDirectory is not null)
            {
                var projectDataDir0 = Path.Combine(locationService.ProjectDirectory, "Data");
                PopulatePrereqCatalogs(projectDataDir0, picked);
            }

            return;
        }

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
                    // Bind the UI to the new detail early so templated controls (ComboBoxes/ListViews)
                    // always refer to the current Model.Detail during population.
                    Model.Detail = detail;

                    // Ensure catalogs are ready BEFORE we parse and materialize content so ComboBoxes have
                    // valid ItemsSource as rows are created and selections apply immediately.
                    // 1) Prime from GameDataService directly (single source of truth)
                    detail.AvailableArmorTemplates.Clear();
                    foreach (var a in gameDataService.GameArmors) detail.AvailableArmorTemplates.Add(a);
                    detail.AvailableWeaponTemplates.Clear();
                    foreach (var w in gameDataService.GameWeapons) detail.AvailableWeaponTemplates.Add(w);
                    detail.AvailableLocomotors.Clear();
                    foreach (var l in gameDataService.GameLocomotors) detail.AvailableLocomotors.Add(l);

                    // 2) Also refresh cached registries and push, in case schema was not yet initialized
                    LoadTemplateRegistries(); // refresh cached sets from GameDataService
                    RefreshAvailableLists(); // push cached sets into current detail (merges with step 1)

                    // 3) Populate other prerequisite catalogs (objects/sciences)
                    PopulatePrereqCatalogs(projectDataDir, detail);

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
                            else if (string.Equals(blockType, "Prerequisites", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (var sub in blockContent.EnumerateArray().Where(sub => sub.ValueKind == JsonValueKind.Object))
                                {
                                    if (!sub.TryGetProperty("Key", out var sk) || !sub.TryGetProperty("Value", out var sv) ||
                                        sv.ValueKind != JsonValueKind.Array) continue;
                                    var sKey = sk.GetString() ?? string.Empty;
                                    var first = sv.EnumerateArray().FirstOrDefault();
                                    var sVal = first.ValueKind == JsonValueKind.String ? (first.GetString() ?? string.Empty) : string.Empty;
                                    if (!string.IsNullOrWhiteSpace(sKey) && !string.IsNullOrWhiteSpace(sVal))
                                    {
                                        var entry = new PrerequisiteEntryModel { Type = sKey, Value = sVal };
                                        entry.AttachCatalogs(detail.AvailableObjects, detail.AvailableSciences);
                                        detail.PrereqEntries.Add(entry);
                                    }
                                }

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
                            // Prerequisites handled via block above
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
                            case "Locomotor":
                            {
                                var vals = valueArr.EnumerateArray()
                                    .Where(v => v.ValueKind == JsonValueKind.String)
                                    .Select(v => v.GetString() ?? string.Empty)
                                    .ToArray();
                                LocomotorSetModel? model = null;
                                switch (vals.Length)
                                {
                                    case 1:
                                        model = new LocomotorSetModel { Locomotor = vals[0] };
                                        break;
                                    case >= 2:
                                    {
                                        // Format typically: SET_NORMAL <LocomotorName>
                                        var maybeCond = vals[0];
                                        model = maybeCond.StartsWith("SET_", StringComparison.OrdinalIgnoreCase)
                                            ? new LocomotorSetModel { Locomotor = vals[1], ConditionsCsv = maybeCond }
                                            : new LocomotorSetModel { Locomotor = vals[0] };
                                        break;
                                    }
                                }

                                if (model is null) break;
                                detail.LocomotorSets.Add(model);
                                break;
                            }
                        }

                        continue;

                        string FirstString() => valueArr.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                    }

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

    #endregion

    #region Catalog Population

    private static void PopulatePrereqCatalogs(string projectDataDir, GameObjectDetailModel detail)
    {
        void ReadNames(string filePath, string typeName, ICollection<string> destination)
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
                /* ignore */
            }
        }

        // Objects: filter to structures only (KindOf contains STRUCTURE). Scan all project Data JSONs;
        // there might not be a single consolidated Object.json at the root.
        if (detail.AvailableObjects.Count == 0 && Directory.Exists(projectDataDir))
        {
            var parseOptions = new JsonDocumentOptions { MaxDepth = 4096 };
            foreach (var jsonPath in Directory.EnumerateFiles(projectDataDir, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    using var stream = File.OpenRead(jsonPath);
                    using var doc = JsonDocument.Parse(stream, parseOptions);
                    if (doc.RootElement.ValueKind != JsonValueKind.Array) continue;
                    foreach (var el in doc.RootElement.EnumerateArray())
                    {
                        if (el.ValueKind != JsonValueKind.Object) continue;
                        if (!el.TryGetProperty("Type", out var t) || !t.ValueEquals("Object")) continue;
                        // name
                        if (!el.TryGetProperty("Name", out var nameArr) || nameArr.ValueKind != JsonValueKind.Array) continue;
                        var first = nameArr.EnumerateArray().FirstOrDefault();
                        if (first.ValueKind != JsonValueKind.String) continue;
                        var objName = first.GetString();
                        if (string.IsNullOrWhiteSpace(objName)) continue;
                        // look for KindOf containing STRUCTURE
                        if (!el.TryGetProperty("Content", out var content) || content.ValueKind != JsonValueKind.Array) continue;
                        bool isStructure = false;
                        foreach (var item in content.EnumerateArray())
                        {
                            if (item.ValueKind != JsonValueKind.Object) continue;
                            if (!item.TryGetProperty("Key", out var keyProp)) continue;
                            if (!keyProp.ValueEquals("KindOf")) continue;
                            if (!item.TryGetProperty("Value", out var valArr) || valArr.ValueKind != JsonValueKind.Array) continue;
                            foreach (var v in valArr.EnumerateArray())
                            {
                                if (v.ValueKind == JsonValueKind.String &&
                                    string.Equals(v.GetString(), "STRUCTURE", StringComparison.OrdinalIgnoreCase))
                                {
                                    isStructure = true;
                                    break;
                                }
                            }

                            if (isStructure) break;
                        }

                        if (isStructure && !detail.AvailableObjects.Contains(objName)) detail.AvailableObjects.Add(objName);
                    }
                }
                catch
                {
                    /* ignore unreadable/malformed file */
                }
            }
        }

        if (detail.AvailableSciences.Count == 0)
        {
            var sciPath = Path.Combine(projectDataDir, "Science.json");
            ReadNames(sciPath, "Science", detail.AvailableSciences);
        }
    }

    public IReadOnlyList<string> GetPrereqItems(string type)
    {
        var d = Model.Detail;
        if (d is null) return [];
        return string.Equals(type, "Science", StringComparison.OrdinalIgnoreCase)
            ? (IReadOnlyList<string>) d.AvailableSciences
            : (IReadOnlyList<string>) d.AvailableObjects;
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

    #endregion

    #region CRUD Actions

    private void AddPrerequisite(string? _)
    {
        if (Model.Detail == null) return;
        var entry = new PrerequisiteEntryModel { Type = "Object", Value = string.Empty };
        entry.AttachCatalogs(Model.Detail.AvailableObjects, Model.Detail.AvailableSciences);
        // Value will be auto-initialized to first available by AttachCatalogs/RebuildItems
        Model.Detail.PrereqEntries.Add(entry);
    }

    private void RemovePrerequisite(PrerequisiteEntryModel? entry)
    {
        if (entry is null) return;
        Model.Detail?.PrereqEntries.Remove(entry);
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

    private void AddLocomotorSet()
    {
        Model.Detail?.LocomotorSets.Add(new LocomotorSetModel { Locomotor = string.Empty });
    }

    private void RemoveLocomotorSet(LocomotorSetModel? set)
    {
        if (set is null) return;
        Model.Detail?.LocomotorSets.Remove(set);
    }

    #endregion
}
