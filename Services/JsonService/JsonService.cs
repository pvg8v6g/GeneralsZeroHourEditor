using System.Text.Json;
using GeneralsZeroHourEditor.Enumerations;
using GeneralsZeroHourEditor.Extensions;
using GeneralsZeroHourEditor.Models;

namespace GeneralsZeroHourEditor.Services.JsonService;

public class JsonService : IJsonService
{
    public async Task SaveToFileAsync(string filePath, string jsonContent)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, jsonContent);
    }

    public async Task<IReadOnlyList<GameObjectModel>> LoadInfantryAsync(string dataDir)
    {
        var results = new List<GameObjectModel>();
        if (string.IsNullOrWhiteSpace(dataDir) || !Directory.Exists(dataDir)) return results;

        var parseOptions = new JsonDocumentOptions { MaxDepth = 4096 };

        foreach (var jsonPath in Directory.EnumerateFiles(dataDir, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                await using var stream = File.OpenRead(jsonPath);
                using var doc = await JsonDocument.ParseAsync(stream, parseOptions);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) continue;

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.ValueKind is not JsonValueKind.Object) continue;
                    if (!element.TryGetProperty("Type", out var typeProp) || typeProp.GetString() is not "Object") continue;
                    if (!element.TryGetProperty("Content", out var content) || content.ValueKind is not JsonValueKind.Array) continue;
                    if (!HasKindOf(content, "INFANTRY")) continue;

                    var name = GetName(element);
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var detail = new GameObjectModel { Name = name };

                    FillTopLevelFields(content, detail);
                    ParseBodyBlock(content, detail);
                    ParseArmorSets(content, detail);
                    ParseWeaponSets(content, detail);
                    ParseLocomotors(content, detail);

                    results.Add(detail);
                }
            }
            catch
            {
                // ignore malformed file
            }
        }

        return results;
    }

    #region Private helpers

    private static bool HasKindOf(JsonElement content, string flag)
    {
        foreach (var prop in content.EnumerateArray())
        {
            if (prop.ValueKind is not JsonValueKind.Object) continue;
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

    private static void FillTopLevelFields(JsonElement content, GameObjectModel detail)
    {
        foreach (var prop in content.EnumerateArray())
        {
            if (prop.ValueKind is not JsonValueKind.Object) continue;
            if (!prop.TryGetProperty("Key", out var keyProp) || !prop.TryGetProperty("Value", out var valArr) ||
                valArr.ValueKind is not JsonValueKind.Array) continue;
            var key = keyProp.GetString();
            var first = valArr.EnumerateArray().FirstOrDefault();
            var value = first.ValueKind is JsonValueKind.String ? first.GetString() : null;
            if (string.IsNullOrWhiteSpace(key)) continue;

            if (string.Equals(key, "Side", StringComparison.OrdinalIgnoreCase)) detail.Side = value ?? string.Empty;
            else if (string.Equals(key, "EditorSorting", StringComparison.OrdinalIgnoreCase)) detail.EditorSorting = value ?? string.Empty;
            else if (string.Equals(key, "CommandSet", StringComparison.OrdinalIgnoreCase)) detail.CommandSet = value ?? string.Empty;
            else if (string.Equals(key, "SelectPortrait", StringComparison.OrdinalIgnoreCase)) detail.SelectPortrait = value ?? string.Empty;
            else if (string.Equals(key, "ButtonImage", StringComparison.OrdinalIgnoreCase)) detail.ButtonImage = value ?? string.Empty;
            else if (string.Equals(key, "BuildCost", StringComparison.OrdinalIgnoreCase)) detail.BuildCost = value ?? string.Empty;
            else if (string.Equals(key, "BuildTime", StringComparison.OrdinalIgnoreCase)) detail.BuildTime = value ?? string.Empty;
            else if (string.Equals(key, "BuildCompletion", StringComparison.OrdinalIgnoreCase)) detail.BuildCompletion = value ?? string.Empty;
            else if (string.Equals(key, "VisionRange", StringComparison.OrdinalIgnoreCase)) detail.VisionRange = value ?? string.Empty;
            else if (string.Equals(key, "ShroudClearingRange", StringComparison.OrdinalIgnoreCase))
                detail.ShroudClearingRange = value ?? string.Empty;
            else if (string.Equals(key, "Geometry", StringComparison.OrdinalIgnoreCase)) detail.Geometry = value ?? string.Empty;
            else if (string.Equals(key, "GeometryMajorRadius", StringComparison.OrdinalIgnoreCase))
                detail.GeometryMajorRadius = value ?? string.Empty;
            else if (string.Equals(key, "GeometryMinorRadius", StringComparison.OrdinalIgnoreCase))
                detail.GeometryMinorRadius = value ?? string.Empty;
            else if (string.Equals(key, "GeometryHeight", StringComparison.OrdinalIgnoreCase)) detail.GeometryHeight = value ?? string.Empty;
            else if (string.Equals(key, "GeometryIsSmall", StringComparison.OrdinalIgnoreCase)) detail.GeometryIsSmall = value ?? string.Empty;
            else if (string.Equals(key, "Shadow", StringComparison.OrdinalIgnoreCase)) detail.Shadow = value ?? string.Empty;
            else if (string.Equals(key, "ShadowSizeX", StringComparison.OrdinalIgnoreCase)) detail.ShadowSizeX = value ?? string.Empty;
            else if (string.Equals(key, "ShadowSizeY", StringComparison.OrdinalIgnoreCase)) detail.ShadowSizeY = value ?? string.Empty;
            else if (string.Equals(key, "ShadowTexture", StringComparison.OrdinalIgnoreCase)) detail.ShadowTexture = value ?? string.Empty;
            else if (string.Equals(key, "DisplayName", StringComparison.OrdinalIgnoreCase)) detail.DisplayName = value ?? string.Empty;
            else if (string.Equals(key, "Description", StringComparison.OrdinalIgnoreCase)) detail.Description = value ?? string.Empty;
            else if (string.Equals(key, "KindOf", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var v in valArr.EnumerateArray())
                {
                    if (v.ValueKind is not JsonValueKind.String) continue;
                    var parsed = Enum.TryParse<KindOf>(v.GetString(), ignoreCase: true, out var e);
                    if (!parsed) continue;
                    detail.KindOf.Add(e);
                }
            }
            else if (string.Equals(key, "Prerequisites", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var v in valArr.EnumerateArray())
                {
                    if (v.ValueKind is not JsonValueKind.Array) continue;
                    var it = v.EnumerateArray().ToArray();
                    if (it is not [{ ValueKind: JsonValueKind.String }, { ValueKind: JsonValueKind.String }, ..]) continue;
                    var prereqParse = Enum.TryParse<PrerequisiteType>(it[0].GetString(), ignoreCase: true, out var type);
                    if (!prereqParse) continue;
                    detail.PrereqEntries.Add(new KeyValuePair<PrerequisiteType, string>(type, it[1].GetString() ?? string.Empty));
                }
            }
        }
    }

    private static void ParseBodyBlock(JsonElement content, GameObjectModel detail)
    {
        foreach (var item in content.EnumerateArray())
        {
            if (item.ValueKind is not JsonValueKind.Object) continue;
            if (!item.TryGetProperty("Type", out var blockTypeProp) || !item.TryGetProperty("Content", out var blockContent) ||
                blockContent.ValueKind is not JsonValueKind.Array) continue;
            if (!string.Equals(blockTypeProp.GetString(), "Body", StringComparison.OrdinalIgnoreCase)) continue;

            foreach (var sub in blockContent.EnumerateArray())
            {
                if (sub.ValueKind is not JsonValueKind.Object) continue;
                if (!sub.TryGetProperty("Key", out var sk) || !sub.TryGetProperty("Value", out var sv) ||
                    sv.ValueKind is not JsonValueKind.Array) continue;
                var sKey = sk.GetString();
                if (string.Equals(sKey, "MaxHealth", StringComparison.OrdinalIgnoreCase))
                    detail.MaxHealth = sv.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                else if (string.Equals(sKey, "InitialHealth", StringComparison.OrdinalIgnoreCase))
                    detail.InitialHealth = sv.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
            }
        }
    }

    private static void ParseArmorSets(JsonElement content, GameObjectModel detail)
    {
        foreach (var item in content.EnumerateArray())
        {
            if (item.ValueKind is not JsonValueKind.Object) continue;
            if (!item.TryGetProperty("Type", out var blockTypeProp) || !item.TryGetProperty("Content", out var blockContent) ||
                blockContent.ValueKind is not JsonValueKind.Array) continue;
            if (!string.Equals(blockTypeProp.GetString(), "ArmorSet", StringComparison.OrdinalIgnoreCase)) continue;

            var model = new ArmorSetModel();
            foreach (var sub in blockContent.EnumerateArray())
            {
                if (sub.ValueKind is not JsonValueKind.Object) continue;
                if (!sub.TryGetProperty("Key", out var sk) || !sub.TryGetProperty("Value", out var sv) ||
                    sv.ValueKind is not JsonValueKind.Array) continue;
                var sKey = sk.GetString();
                if (string.Equals(sKey, "Armor", StringComparison.OrdinalIgnoreCase))
                {
                    model.Armor = sv.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                }
                else if (string.Equals(sKey, "Conditions", StringComparison.OrdinalIgnoreCase))
                {
                    var parsed = sv
                        .EnumerateArray()
                        .Where(n => n.ValueKind is JsonValueKind.String)
                        .Select(n => n.GetString())
                        .OfType<string>()
                        .Select(s => Enum.TryParse<ArmorConditions>(s, ignoreCase: true, out var v) ? (ArmorConditions?) v : null)
                        .OfType<ArmorConditions>()
                        .ToArray();

                    model.Conditions.SetRange(parsed);
                }
            }

            detail.ArmorSets.Add(model);
        }
    }

    private static void ParseWeaponSets(JsonElement content, GameObjectModel detail)
    {
        foreach (var item in content.EnumerateArray())
        {
            if (item.ValueKind is not JsonValueKind.Object) continue;
            if (!item.TryGetProperty("Type", out var blockTypeProp) || !item.TryGetProperty("Content", out var blockContent) ||
                blockContent.ValueKind is not JsonValueKind.Array) continue;
            if (!string.Equals(blockTypeProp.GetString(), "WeaponSet", StringComparison.OrdinalIgnoreCase)) continue;

            var model = new WeaponSlotModel();
            foreach (var sub in blockContent.EnumerateArray())
            {
                if (sub.ValueKind is not JsonValueKind.Object) continue;
                if (!sub.TryGetProperty("Key", out var sk) || !sub.TryGetProperty("Value", out var sv) ||
                    sv.ValueKind is not JsonValueKind.Array) continue;
                var key = sk.GetString() ?? string.Empty;
                switch (key)
                {
                    case var _ when key.Equals("Conditions", StringComparison.OrdinalIgnoreCase):
                    {
                        var parsed = sv
                            .EnumerateArray()
                            .Where(n => n.ValueKind is JsonValueKind.String)
                            .Select(n => n.GetString())
                            .OfType<string>()
                            .Select(s => Enum.TryParse<WeaponConditions>(s, ignoreCase: true, out var v) ? (WeaponConditions?) v : null)
                            .OfType<WeaponConditions>()
                            .ToArray();

                        model.Conditions.SetRange(parsed);
                        break;
                    }
                    default:
                    {
                        var slot = key.ToUpperInvariant() switch
                        {
                            "PRIMARY" => WeaponSlot.PRIMARY,
                            "SECONDARY" => WeaponSlot.SECONDARY,
                            "TERTIARY" => WeaponSlot.TERTIARY,
                            _ => (WeaponSlot?) null
                        };

                        if (slot is null) break;
                        var weapon = sv.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                        model.Weapons.Add(new KeyValuePair<WeaponSlot, string>(slot.Value, weapon));
                        break;
                    }
                }
            }

            detail.WeaponSets.Add(model);
        }
    }

    private static void ParseLocomotors(JsonElement content, GameObjectModel detail)
    {
        foreach (var item in content.EnumerateArray())
        {
            if (item.ValueKind is not JsonValueKind.Object) continue;
            if (!item.TryGetProperty("Type", out var blockTypeProp) || !item.TryGetProperty("Content", out var blockContent) ||
                blockContent.ValueKind is not JsonValueKind.Array) continue;
            if (!string.Equals(blockTypeProp.GetString(), "Locomotor", StringComparison.OrdinalIgnoreCase)) continue;

            var condition = LocomotorConditions.SET_NORMAL;
            var locomotor = string.Empty;
            foreach (var sub in blockContent.EnumerateArray())
            {
                if (sub.ValueKind is not JsonValueKind.Object) continue;
                if (!sub.TryGetProperty("Key", out var sk) || !sub.TryGetProperty("Value", out var sv) ||
                    sv.ValueKind is not JsonValueKind.Array) continue;
                var sKey = sk.GetString();
                if (string.Equals(sKey, "Locomotor", StringComparison.OrdinalIgnoreCase))
                {
                    locomotor = sv.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                }
                else if (string.Equals(sKey, "Condition", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(sKey, "Conditions", StringComparison.OrdinalIgnoreCase))
                {
                    var parsed = sv.EnumerateArray()
                        .Where(n => n.ValueKind is JsonValueKind.String)
                        .Select(n => n.GetString())
                        .OfType<string>()
                        .Select(s => Enum.TryParse<LocomotorConditions>(s, ignoreCase: true, out var v) ? (LocomotorConditions?) v : null)
                        .OfType<LocomotorConditions>()
                        .ToArray();

                    if (parsed.Length is 0) continue;
                    condition = parsed.First();
                }
            }

            detail.LocomotorSets.Add(new KeyValuePair<LocomotorConditions, string>(condition, locomotor));
        }
    }

    #endregion
}
