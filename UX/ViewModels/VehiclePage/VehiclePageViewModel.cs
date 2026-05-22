using System.Text.Json;
using GeneralsZeroHourEditor.Command;
using GeneralsZeroHourEditor.Models;
using GeneralsZeroHourEditor.Services.GameRegistryService;
using GeneralsZeroHourEditor.Services.LocationService;

namespace GeneralsZeroHourEditor.UX.ViewModels.VehiclePage;

public class VehiclePageViewModel(ILocationService locationService, IGameRegistryService gameRegistryService) : BaseViewModel
{
    public GameObjectPageModel Model { get; } = new();

    public RelayCommand<object> ItemInvokedCommand => new(OnItemInvoked);

    private void OnItemInvoked(object? invokedItem)
    {
        if (invokedItem is GameObjectItemModel item)
        {
            Model.SelectedNode = item;
        }
    }

    protected override async Task LoadedAction()
    {
        LoadSchema();
        LoadVehicleList();
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

    private void LoadVehicleList()
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
                    if (!HasKindOf(content, "VEHICLE")) continue;

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
}
