using System.Text.Json;
using GeneralsZeroHourEditor.Command;
using GeneralsZeroHourEditor.Models;
using GeneralsZeroHourEditor.Services.LocationService;
using GeneralsZeroHourEditor.Services.GameRegistryService;
using System.Collections.ObjectModel;

namespace GeneralsZeroHourEditor.UX.ViewModels.InfantryPage;

public class InfantryPageViewModel(ILocationService locationService, IGameRegistryService gameRegistryService) : BaseViewModel
{
    public GameObjectPageModel Model { get; } = new();

    public RelayCommand<object> ItemInvokedCommand => new(OnItemInvoked);

    public RelayCommand<string> AddPrerequisiteCommand => new(AddPrerequisite);
    public RelayCommand<string> RemovePrerequisiteCommand => new(RemovePrerequisite);
    public RelayCommand<string> AddKindOfCommand => new(AddKindOf);
    public RelayCommand<string> RemoveKindOfCommand => new(RemoveKindOf);

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
        LoadInfantryList();
        await Task.CompletedTask;
    }

    private void LoadSchema()
    {
        try
        {
            var schemaPath = Path.Combine(locationService.ProjectDirectory!, "Schema", "game.schema.json");
            if (File.Exists(schemaPath))
            {
                gameRegistryService.LoadFromSchema(schemaPath);
            }
            else
            {
                var projectDataDir = Path.Combine(locationService.ProjectDirectory!, "Data");
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
        var projectDataDir = Path.Combine(locationService.ProjectDirectory!, "Data");
        if (!Directory.Exists(projectDataDir)) return;

        var sideMap = new Dictionary<string, SideGroupModel>(StringComparer.OrdinalIgnoreCase);

        foreach (var jsonPath in Directory.EnumerateFiles(projectDataDir, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                using var stream = File.OpenRead(jsonPath);
                using var doc = JsonDocument.Parse(stream);
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

    private static string? GetSide(JsonElement content)
    {
        foreach (var prop in content.EnumerateArray())
        {
            if (prop.ValueKind != JsonValueKind.Object) continue;
            if (!prop.TryGetProperty("Key", out var keyProp)) continue;
            if (keyProp.GetString() != "Side") continue;
            if (!prop.TryGetProperty("Value", out var valArr) || valArr.ValueKind != JsonValueKind.Array) continue;
            var first = valArr.EnumerateArray().FirstOrDefault();
            return first.ValueKind == JsonValueKind.String ? first.GetString() : null;
        }

        return null;
    }

    private static bool HasKindOf(JsonElement content, string flag)
    {
        foreach (var prop in content.EnumerateArray())
        {
            if (prop.ValueKind != JsonValueKind.Object) continue;
            if (!prop.TryGetProperty("Key", out var keyProp)) continue;
            if (keyProp.GetString() != "KindOf") continue;
            if (!prop.TryGetProperty("Value", out var valArr) || valArr.ValueKind != JsonValueKind.Array) continue;
            foreach (var v in valArr.EnumerateArray())
            {
                if (v.ValueKind == JsonValueKind.String && string.Equals(v.GetString(), flag, StringComparison.OrdinalIgnoreCase)) return true;
            }
        }

        return false;
    }

    private static string? GetName(JsonElement obj)
    {
        if (!obj.TryGetProperty("Name", out var nameArr) || nameArr.ValueKind != JsonValueKind.Array) return null;
        var first = nameArr.EnumerateArray().FirstOrDefault();
        return first.ValueKind == JsonValueKind.String ? first.GetString() : null;
    }

    private void LoadSelectedDetail(string name)
    {
        var projectDataDir = Path.Combine(locationService.ProjectDirectory!, "Data");
        if (!Directory.Exists(projectDataDir)) return;

        foreach (var jsonPath in Directory.EnumerateFiles(projectDataDir, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                using var stream = File.OpenRead(jsonPath);
                using var doc = JsonDocument.Parse(stream);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) continue;
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.Object) continue;
                    if (!element.TryGetProperty("Type", out var typeProp) || typeProp.GetString() != "Object") continue;
                    var objName = GetName(element);
                    if (!string.Equals(objName, name, StringComparison.Ordinal)) continue;

                    if (!element.TryGetProperty("Content", out var content) || content.ValueKind != JsonValueKind.Array) continue;
                    var detail = new GameObjectDetailModel { Name = name, SourceFilePath = jsonPath };

                    // Iterate content array of key/value pairs and fill detail where relevant
                    foreach (var item in content.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Object) continue;
                        if (!item.TryGetProperty("Key", out var keyProp)) continue;
                        var key = keyProp.GetString();
                        if (string.IsNullOrEmpty(key)) continue;
                        if (!item.TryGetProperty("Value", out var valueArr) || valueArr.ValueKind != JsonValueKind.Array) continue;

                        string FirstString() => valueArr.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;

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
                                foreach (var v in valueArr.EnumerateArray()) if (v.ValueKind == JsonValueKind.String) AddIfMissing(detail.Prerequisites, v.GetString()!);
                                break;
                            case "KindOf":
                                foreach (var v in valueArr.EnumerateArray()) if (v.ValueKind == JsonValueKind.String) AddIfMissing(detail.KindOf, v.GetString()!);
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
                        }
                    }

                    Model.Detail = detail;
                    return;
                }
            }
            catch
            {
                // ignore bad file
            }
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
}
