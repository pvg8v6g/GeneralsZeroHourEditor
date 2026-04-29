using System.Collections.ObjectModel;
using System.Text.Json;
using GeneralEditor.UX.Models.Infantry;
using GeneralsZeroHourEditor.Services.LocationService;

namespace GeneralsZeroHourEditor.UX.ViewModels.InfantryPage;

public class InfantryPageViewModel(ILocationService locationService) : BaseViewModel
{
    public InfantryPageModel Model { get; } = new();

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
            var schemaPath = Path.Combine(locationService.ProjectDirectory!, "Schema", "infantry.schema.json");
            if (!File.Exists(schemaPath)) return;
            using var stream = File.OpenRead(schemaPath);
            using var doc = JsonDocument.Parse(stream);
            if (!doc.RootElement.TryGetProperty("modules", out var modules) || modules.ValueKind != JsonValueKind.Object) return;

            Model.Modules.Clear();
            foreach (var moduleProp in modules.EnumerateObject())
            {
                var moduleModel = new InfantryModuleModel { Name = moduleProp.Name };
                var moduleObj = moduleProp.Value;
                if (moduleObj.ValueKind == JsonValueKind.Object)
                {
                    if (moduleObj.TryGetProperty("fields", out var fields) && fields.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var f in fields.EnumerateArray())
                        {
                            if (f.ValueKind == JsonValueKind.String) moduleModel.Fields.Add(f.GetString()!);
                        }
                    }

                    if (moduleObj.TryGetProperty("blocks", out var blocks) && blocks.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var blockProp in blocks.EnumerateObject())
                        {
                            var sub = new InfantrySubBlockModel { Name = blockProp.Name };
                            if (blockProp.Value.ValueKind == JsonValueKind.Object && blockProp.Value.TryGetProperty("fields", out var subFields) &&
                                subFields.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var sf in subFields.EnumerateArray())
                                {
                                    if (sf.ValueKind == JsonValueKind.String) sub.Fields.Add(sf.GetString()!);
                                }
                            }

                            moduleModel.SubBlocks.Add(sub);
                        }
                    }
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
        Model.Infantry.Clear();
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
                    if (!element.TryGetProperty("Content", out var content) || content.ValueKind != JsonValueKind.Array) continue;
                    if (!HasKindOf(content, "INFANTRY")) continue;
                    var name = GetName(element);
                    if (!string.IsNullOrWhiteSpace(name) && !Model.Infantry.Contains(name)) Model.Infantry.Add(name);
                }
            }
            catch
            {
                // ignore file
            }
        }

        // simple sort
        var sorted = new ObservableCollection<string>(new List<string>(Model.Infantry).OrderBy(s => s, StringComparer.OrdinalIgnoreCase));
        Model.Infantry.Clear();
        foreach (var s in sorted) Model.Infantry.Add(s);
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
