using System.Collections.ObjectModel;
using GeneralsZeroHourEditor.Command;
using GeneralsZeroHourEditor.Extensions;
using GeneralsZeroHourEditor.Models;
using GeneralsZeroHourEditor.Services.GameDataService;
using GeneralsZeroHourEditor.Services.GameRegistryService;
using GeneralsZeroHourEditor.Services.JsonService;
using GeneralsZeroHourEditor.Services.LocationService;

namespace GeneralsZeroHourEditor.UX.ViewModels.InfantryPage;

public class InfantryPageViewModel(
    ILocationService locationService,
    IGameRegistryService gameRegistryService,
    IGameDataService gameDataService,
    IJsonService jsonService) : BaseViewModel
{
    #region Properties

    public ObservableCollection<TreeViewModel> EntityCollection { get; } = [];

    public GameObjectModel? SelectedItem
    {
        get;
        set => SetField(ref field, value);
    }

    #endregion

    #region Commands

    public RelayCommand<object> SelectedItemChangedCommand => new(SelectedItemChanged);

    #endregion

    #region Actions & Listeners

    protected override async Task LoadedAction()
    {
        var t0 = gameDataService.Infantry.Select(x => x.Name).OrderBy(x => x).ToArray();
        EntityCollection.Clear();
        EntityCollection.AddRange(gameDataService.Infantry
            .OrderBy(x => x.Name)
            .GroupBy(x => x.Side)
            .OrderBy(x => x.Key)
            .Select(x =>
            {
                var models = x.Select(y => new TreeViewModel { Name = y.Name, GameObject = y });
                var model = new TreeViewModel
                {
                    Name = x.Key
                };
                model.Children.AddRange(models);
                return model;
            }));
        await Task.CompletedTask;
    }

    private void SelectedItemChanged(object? selectedItem)
    {
        if (selectedItem is not TreeViewModel item) return;
        if (item.GameObject is null) return;
        SelectedItem = item.GameObject;
        var t0 = 0;
        // if (invokedItem is not GameObjectItemModel item) return;
        // Model.SelectedNode = item;
        // LoadSelectedDetail(item.Name);
    }

    #endregion

    // #region Fields
    //
    // // Cached registries discovered from project Data JSONs
    // private readonly SortedSet<string> _armorTemplates = new(StringComparer.OrdinalIgnoreCase);
    // private readonly SortedSet<string> _weaponTemplates = new(StringComparer.OrdinalIgnoreCase);
    // private readonly SortedSet<string> _locomotors = new(StringComparer.OrdinalIgnoreCase);
    //
    // // Note: event subscriptions are registered in LoadedAction to avoid constructor syntax conflicts.
    //
    // private static readonly string[] _prereqTypes = ["Object", "Science"];
    //
    // #endregion
    //
    // #region Commands
    //
    // public RelayCommand<object> ItemInvokedCommand => new(SelectedItemChanged);
    //
    // public RelayCommand AddPrerequisiteCommand => new(() => AddPrerequisite(null));
    //
    // public RelayCommand<PrerequisiteEntryModel> RemovePrerequisiteCommand => new(RemovePrerequisite);
    //
    // public RelayCommand<string> AddKindOfCommand => new(AddKindOf);
    //
    // public RelayCommand<string> RemoveKindOfCommand => new(RemoveKindOf);
    //
    // // Armor & Weapon CRUD
    // // Use non-generic RelayCommand so buttons without CommandParameter are enabled
    // public RelayCommand AddArmorSetCommand => new(AddArmorSet);
    //
    // public RelayCommand<ArmorSetModel> RemoveArmorSetCommand => new(RemoveArmorSet);
    //
    // public RelayCommand AddWeaponSetCommand => new(AddWeaponSet);
    //
    // public RelayCommand<WeaponSetModel> RemoveWeaponSetCommand => new(RemoveWeaponSet);
    //
    // public RelayCommand AddLocomotorCommand => new(AddLocomotorSet);
    //
    // public RelayCommand<LocomotorSetModel> RemoveLocomotorCommand => new(RemoveLocomotorSet);
    //
    // #endregion
    //
    // #region Actions & Listeners
    //
    // protected override async Task LoadedAction()
    // {
    //     // Subscribe to GameDataService catalogs (single source of truth for templates)
    //     gameDataService.GameWeapons.CollectionChanged += (_, _) => OnRegistriesChanged();
    //     gameDataService.GameArmors.CollectionChanged += (_, _) => OnRegistriesChanged();
    //     gameDataService.GameLocomotors.CollectionChanged += (_, _) => OnRegistriesChanged();
    //
    //     LoadModulesFromRegistry();
    //     LoadTemplateRegistries();
    //     // Ensure any existing selection (if restored) gets refreshed template lists
    //     RefreshAvailableLists();
    //     LoadInfantryList();
    //     await Task.CompletedTask;
    // }
    //
    // private void SelectedItemChanged(object? invokedItem)
    // {
    //     if (invokedItem is not GameObjectItemModel item) return;
    //     Model.SelectedNode = item;
    //     LoadSelectedDetail(item.Name);
    // }
    //
    // #endregion
    //
    // #region Initialization
    //
    // private void LoadModulesFromRegistry()
    // {
    //     Model.Modules.Clear();
    //     foreach (var def in gameRegistryService.Registry.ModuleDefinitions)
    //     {
    //         var moduleModel = new GameObjectModuleModel { Name = def.Name };
    //         foreach (var field in def.Fields) moduleModel.Fields.Add(field);
    //         foreach (var subDef in def.SubBlocks)
    //         {
    //             var sub = new GameObjectSubBlockModel { Name = subDef.Name };
    //             foreach (var f in subDef.Fields) sub.Fields.Add(f);
    //             moduleModel.SubBlocks.Add(sub);
    //         }
    //
    //         Model.Modules.Add(moduleModel);
    //     }
    // }
    //
    // private void LoadInfantryList()
    // {
    //     // Build the tree view from preloaded in-memory objects (populated by LoadProjectDataTask)
    //     Model.GameObjectGroups.Clear();
    //     var sideMap = new Dictionary<string, SideGroupModel>(StringComparer.OrdinalIgnoreCase);
    //
    //     foreach (var unit in gameDataService.Infantry)
    //     {
    //         var side = string.IsNullOrWhiteSpace(unit.Side) ? "Unknown" : unit.Side;
    //         if (!sideMap.TryGetValue(side, out var group))
    //         {
    //             group = new SideGroupModel { Name = side };
    //             sideMap[side] = group;
    //             Model.GameObjectGroups.Add(group);
    //         }
    //
    //         if (group.Children.All(i => i.Name != unit.Name))
    //         {
    //             group.Children.Add(new GameObjectItemModel { Name = unit.Name });
    //         }
    //     }
    //
    //     // Sort groups and items
    //     var sortedGroups = Model.GameObjectGroups.OrderBy(g => g.Name).ToList();
    //     Model.GameObjectGroups.Clear();
    //     foreach (var group in sortedGroups)
    //     {
    //         var sortedItems = group.Children.OrderBy(i => i.Name).ToList();
    //         group.Children.Clear();
    //         foreach (var item in sortedItems) group.Children.Add(item);
    //         Model.GameObjectGroups.Add(group);
    //     }
    // }
    //
    // private void LoadTemplateRegistries()
    // {
    //     _armorTemplates.Clear();
    //     _weaponTemplates.Clear();
    //     _locomotors.Clear();
    //
    //     // Source solely from GameDataService (populated once by GameRegistryService.Initialize)
    //     foreach (var a in gameDataService.GameArmors) _armorTemplates.Add(a);
    //     foreach (var w in gameDataService.GameWeapons) _weaponTemplates.Add(w);
    //     foreach (var l in gameDataService.GameLocomotors) _locomotors.Add(l);
    //
    //     RefreshAvailableLists();
    // }
    //
    // private void OnRegistriesChanged()
    // {
    //     // Merge latest registry values and resync detail pickers
    //     LoadTemplateRegistries();
    // }
    //
    // private void RefreshAvailableLists()
    // {
    //     // Push cached sets into the current detail model so ComboBoxes get ItemsSource
    //     Model.Detail ??= new GameObjectDetailModel();
    //     Model.Detail.AvailableArmorTemplates.Clear();
    //     Model.Detail.AvailableWeaponTemplates.Clear();
    //     Model.Detail.AvailableLocomotors.Clear();
    //     foreach (var a in _armorTemplates) Model.Detail.AvailableArmorTemplates.Add(a);
    //     foreach (var w in _weaponTemplates) Model.Detail.AvailableWeaponTemplates.Add(w);
    //     foreach (var l in _locomotors) Model.Detail.AvailableLocomotors.Add(l);
    // }
    //
    // #endregion
    //
    // #region Detail Loading
    //
    // private void LoadSelectedDetail(string name)
    // {
    //     // Use preloaded in-memory object (populated by LoadProjectDataTask)
    //     var picked = gameDataService.Infantry.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal));
    //     if (picked is not null)
    //     {
    //         Model.Detail = picked;
    //
    //         // Ensure templates are synced
    //         LoadTemplateRegistries();
    //         RefreshAvailableLists();
    //
    //         // Populate prerequisite catalogs on demand
    //         if (locationService.ProjectDirectory is not null)
    //         {
    //             var projectDataDir = Path.Combine(locationService.ProjectDirectory, "Data");
    //             PopulatePrereqCatalogs(projectDataDir, picked);
    //         }
    //     }
    // }
    //
    // #endregion
    //
    // #region Catalog Population
    //
    // private static void PopulatePrereqCatalogs(string projectDataDir, GameObjectDetailModel detail)
    // {
    //     void ReadNames(string filePath, string typeName, ICollection<string> destination)
    //     {
    //         if (!File.Exists(filePath)) return;
    //         try
    //         {
    //             using var stream = File.OpenRead(filePath);
    //             using var doc = JsonDocument.Parse(stream);
    //             if (doc.RootElement.ValueKind != JsonValueKind.Array) return;
    //             foreach (var element in doc.RootElement.EnumerateArray())
    //             {
    //                 if (element.ValueKind != JsonValueKind.Object) continue;
    //                 if (!element.TryGetProperty("Type", out var typeProp) || typeProp.ValueKind != JsonValueKind.String) continue;
    //                 if (!string.Equals(typeProp.GetString(), typeName, StringComparison.OrdinalIgnoreCase)) continue;
    //                 if (!element.TryGetProperty("Name", out var nameArr) || nameArr.ValueKind != JsonValueKind.Array) continue;
    //                 var first = nameArr.EnumerateArray().FirstOrDefault();
    //                 if (first.ValueKind == JsonValueKind.String)
    //                 {
    //                     var n = first.GetString();
    //                     if (!string.IsNullOrWhiteSpace(n) && !destination.Contains(n)) destination.Add(n);
    //                 }
    //             }
    //         }
    //         catch
    //         {
    //             /* ignore */
    //         }
    //     }
    //
    //     // Objects: filter to structures only (KindOf contains STRUCTURE). Scan all project Data JSONs;
    //     // there might not be a single consolidated Object.json at the root.
    //     if (detail.AvailableObjects.Count == 0 && Directory.Exists(projectDataDir))
    //     {
    //         var parseOptions = new JsonDocumentOptions { MaxDepth = 4096 };
    //         foreach (var jsonPath in Directory.EnumerateFiles(projectDataDir, "*.json", SearchOption.AllDirectories))
    //         {
    //             try
    //             {
    //                 using var stream = File.OpenRead(jsonPath);
    //                 using var doc = JsonDocument.Parse(stream, parseOptions);
    //                 if (doc.RootElement.ValueKind != JsonValueKind.Array) continue;
    //                 foreach (var el in doc.RootElement.EnumerateArray())
    //                 {
    //                     if (el.ValueKind != JsonValueKind.Object) continue;
    //                     if (!el.TryGetProperty("Type", out var t) || !t.ValueEquals("Object")) continue;
    //                     // name
    //                     if (!el.TryGetProperty("Name", out var nameArr) || nameArr.ValueKind != JsonValueKind.Array) continue;
    //                     var first = nameArr.EnumerateArray().FirstOrDefault();
    //                     if (first.ValueKind != JsonValueKind.String) continue;
    //                     var objName = first.GetString();
    //                     if (string.IsNullOrWhiteSpace(objName)) continue;
    //                     // look for KindOf containing STRUCTURE
    //                     if (!el.TryGetProperty("Content", out var content) || content.ValueKind != JsonValueKind.Array) continue;
    //                     bool isStructure = false;
    //                     foreach (var item in content.EnumerateArray())
    //                     {
    //                         if (item.ValueKind != JsonValueKind.Object) continue;
    //                         if (!item.TryGetProperty("Key", out var keyProp)) continue;
    //                         if (!keyProp.ValueEquals("KindOf")) continue;
    //                         if (!item.TryGetProperty("Value", out var valArr) || valArr.ValueKind != JsonValueKind.Array) continue;
    //                         foreach (var v in valArr.EnumerateArray())
    //                         {
    //                             if (v.ValueKind == JsonValueKind.String &&
    //                                 string.Equals(v.GetString(), "STRUCTURE", StringComparison.OrdinalIgnoreCase))
    //                             {
    //                                 isStructure = true;
    //                                 break;
    //                             }
    //                         }
    //
    //                         if (isStructure) break;
    //                     }
    //
    //                     if (isStructure && !detail.AvailableObjects.Contains(objName)) detail.AvailableObjects.Add(objName);
    //                 }
    //             }
    //             catch
    //             {
    //                 /* ignore unreadable/malformed file */
    //             }
    //         }
    //     }
    //
    //     if (detail.AvailableSciences.Count == 0)
    //     {
    //         var sciPath = Path.Combine(projectDataDir, "Science.json");
    //         ReadNames(sciPath, "Science", detail.AvailableSciences);
    //     }
    // }
    //
    // public IReadOnlyList<string> GetPrereqItems(string type)
    // {
    //     var d = Model.Detail;
    //     if (d is null) return [];
    //     return string.Equals(type, "Science", StringComparison.OrdinalIgnoreCase)
    //         ? (IReadOnlyList<string>) d.AvailableSciences
    //         : (IReadOnlyList<string>) d.AvailableObjects;
    // }
    //
    // #endregion
    //
    // #region CRUD Actions
    //
    // private void AddPrerequisite(string? _)
    // {
    //     if (Model.Detail == null) return;
    //     var entry = new PrerequisiteEntryModel { Type = "Object", Value = string.Empty };
    //     entry.AttachCatalogs(Model.Detail.AvailableObjects, Model.Detail.AvailableSciences);
    //     // Value will be auto-initialized to first available by AttachCatalogs/RebuildItems
    //     Model.Detail.PrereqEntries.Add(entry);
    // }
    //
    // private void RemovePrerequisite(PrerequisiteEntryModel? entry)
    // {
    //     if (entry is null) return;
    //     Model.Detail?.PrereqEntries.Remove(entry);
    // }
    //
    // private void AddKindOf(string? value)
    // {
    //     if (Model.Detail == null) return;
    //     if (string.IsNullOrWhiteSpace(value)) return;
    //     if (!Model.Detail.KindOf.Contains(value)) Model.Detail.KindOf.Add(value);
    // }
    //
    // private void RemoveKindOf(string flag)
    // {
    //     Model.Detail?.KindOf.Remove(flag);
    // }
    //
    // private void AddArmorSet()
    // {
    //     Model.Detail?.ArmorSets.Add(new ArmorSetModel { Armor = string.Empty, ConditionsCsv = string.Empty });
    // }
    //
    // private void RemoveArmorSet(ArmorSetModel? set)
    // {
    //     if (set == null) return;
    //     Model.Detail?.ArmorSets.Remove(set);
    // }
    //
    // private void AddWeaponSet()
    // {
    //     Model.Detail?.WeaponSets.Add(new WeaponSetModel
    //     {
    //         ConditionsCsv = string.Empty,
    //         Primary = string.Empty,
    //         Secondary = string.Empty,
    //         Tertiary = string.Empty
    //     });
    // }
    //
    // private void RemoveWeaponSet(WeaponSetModel? set)
    // {
    //     if (set is null) return;
    //     Model.Detail?.WeaponSets.Remove(set);
    // }
    //
    // private void AddLocomotorSet()
    // {
    //     Model.Detail?.LocomotorSets.Add(new LocomotorSetModel { Locomotor = string.Empty });
    // }
    //
    // private void RemoveLocomotorSet(LocomotorSetModel? set)
    // {
    //     if (set is null) return;
    //     Model.Detail?.LocomotorSets.Remove(set);
    // }
    //
    // #endregion
}
