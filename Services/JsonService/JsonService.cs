using System.Text.Json;
using GeneralsZeroHourEditor.Enumerations;
using GeneralsZeroHourEditor.Extensions;
using GeneralsZeroHourEditor.Models;
using GeneralsZeroHourEditor.Models.Weapon;
using GeneralsZeroHourEditor.Models.WeaponSet;

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
        // Standardized: delegate to the shared loader using KindOf flag(s)
        return await LoadByKindOfAsync(dataDir, "INFANTRY");
    }

    public async Task<IReadOnlyList<GameObjectModel>> LoadVehiclesAsync(string dataDir)
    {
        return await LoadByKindOfAsync(dataDir, "VEHICLE");
    }

    public async Task<IReadOnlyList<GameObjectModel>> LoadStructuresAsync(string dataDir)
    {
        return await LoadByKindOfAsync(dataDir, "STRUCTURE");
    }

    public async Task<IReadOnlyList<SideModel>> LoadSidesAsync(string dataDir)
    {
        // Parse all PlayerTemplate entries across the folder tree and produce a list of models
        // holding both Side and BaseSide values.
        var sideTuples = new List<(string? BaseSide, string Side)>();
        if (string.IsNullOrWhiteSpace(dataDir) || !Directory.Exists(dataDir)) return [];

        var parseOptions = new JsonDocumentOptions { MaxDepth = 4096 };
        foreach (var jsonPath in Directory.EnumerateFiles(dataDir, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                await using var stream = File.OpenRead(jsonPath);
                using var doc = await JsonDocument.ParseAsync(stream, parseOptions);
                if (doc.RootElement.ValueKind is not JsonValueKind.Array) continue;

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.ValueKind is not JsonValueKind.Object) continue;
                    if (!element.TryGetProperty("Type", out var typeProp) || !typeProp.ValueEquals("PlayerTemplate")) continue;
                    if (!element.TryGetProperty("Content", out var content) || content.ValueKind is not JsonValueKind.Array) continue;

                    string? baseSide = null;

                    // First pass: capture BaseSide if present
                    foreach (var prop in content.EnumerateArray())
                    {
                        if (prop.ValueKind is not JsonValueKind.Object) continue;
                        if (!prop.TryGetProperty("Key", out var keyProp)) continue;
                        if (!keyProp.ValueEquals("BaseSide")) continue;
                        if (!prop.TryGetProperty("Value", out var valArr) || valArr.ValueKind is not JsonValueKind.Array) continue;
                        var first = valArr.EnumerateArray().FirstOrDefault();
                        baseSide = first.ValueKind is JsonValueKind.String ? first.GetString() : null;
                        break;
                    }

                    // Second pass: collect Side values
                    foreach (var prop in content.EnumerateArray())
                    {
                        if (prop.ValueKind is not JsonValueKind.Object) continue;
                        if (!prop.TryGetProperty("Key", out var keyProp)) continue;
                        if (!prop.TryGetProperty("Value", out var valArr) || valArr.ValueKind is not JsonValueKind.Array) continue;
                        if (!keyProp.ValueEquals("Side")) continue;

                        foreach (var v in valArr.EnumerateArray())
                        {
                            if (v.ValueKind is not JsonValueKind.String) continue;
                            var side = v.GetString();
                            if (!string.IsNullOrEmpty(side))
                            {
                                sideTuples.Add((baseSide, side));
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore malformed files
            }
        }

        // Deduplicate by Side, preserving first BaseSide association
        var map = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (baseSide, side) in sideTuples)
        {
            map.TryAdd(side, baseSide);
        }

        var ordered = map
            .Select(kv => new SideModel(kv.Key, kv.Value))
            .OrderBy(s => s.Side)
            .ToList();

        return ordered;
    }

    public async Task<IReadOnlyList<WeaponDefinition>> LoadWeaponsAsync(string dataDir)
    {
        var list = new List<WeaponDefinition>();
        if (string.IsNullOrWhiteSpace(dataDir) || !Directory.Exists(dataDir)) return list;

        var parseOptions = new JsonDocumentOptions { MaxDepth = 4096 };
        foreach (var jsonPath in Directory.EnumerateFiles(dataDir, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                await using var stream = File.OpenRead(jsonPath);
                using var doc = await JsonDocument.ParseAsync(stream, parseOptions);
                if (doc.RootElement.ValueKind is not JsonValueKind.Array) continue;

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.ValueKind is not JsonValueKind.Object) continue;
                    if (!element.TryGetProperty("Type", out var typeProp) || !typeProp.ValueEquals("Weapon")) continue;
                    if (!element.TryGetProperty("Name", out var nameArr) || nameArr.ValueKind is not JsonValueKind.Array) continue;
                    if (!element.TryGetProperty("Content", out var content) || content.ValueKind is not JsonValueKind.Array) continue;

                    var nameToken = nameArr.EnumerateArray().FirstOrDefault();
                    var name = nameToken.ValueKind is JsonValueKind.String ? nameToken.GetString() ?? string.Empty : string.Empty;
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var weapon = new WeaponDefinition { Name = name };

                    foreach (var prop in content.EnumerateArray())
                    {
                        if (prop.ValueKind is not JsonValueKind.Object) continue;
                        if (!prop.TryGetProperty("Key", out var keyProp) || !prop.TryGetProperty("Value", out var valArr) || valArr.ValueKind is not JsonValueKind.Array)
                            continue;

                        var key = keyProp.GetString() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(key)) continue;

                        var values = valArr.EnumerateArray()
                            .Where(v => v.ValueKind is JsonValueKind.String)
                            .Select(v => v.GetString() ?? string.Empty)
                            .ToArray();

                        switch (key)
                        {
                            case var _ when key.Equals("PrimaryDamage", StringComparison.OrdinalIgnoreCase):
                                weapon.PrimaryDamage = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("PrimaryDamageRadius", StringComparison.OrdinalIgnoreCase):
                                weapon.PrimaryDamageRadius = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("AttackRange", StringComparison.OrdinalIgnoreCase):
                                weapon.AttackRange = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("MinimumAttackRange", StringComparison.OrdinalIgnoreCase):
                                weapon.MinimumAttackRange = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("PreAttackDelay", StringComparison.OrdinalIgnoreCase):
                                weapon.PreAttackDelay = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("DelayBetweenShots", StringComparison.OrdinalIgnoreCase):
                                weapon.DelayBetweenShots = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("ClipSize", StringComparison.OrdinalIgnoreCase):
                                weapon.ClipSize = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("ClipReloadTime", StringComparison.OrdinalIgnoreCase):
                                weapon.ClipReloadTime = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("ProjectileObject", StringComparison.OrdinalIgnoreCase):
                                weapon.ProjectileObject = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("ProjectileDetonationOCL", StringComparison.OrdinalIgnoreCase):
                                weapon.ProjectileDetonationOCL = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("ProjectileCollidesWith", StringComparison.OrdinalIgnoreCase):
                                weapon.ProjectileCollidesWith = string.Join(' ', values); break;
                            case var _ when key.Equals("FireFX", StringComparison.OrdinalIgnoreCase):
                                weapon.FireFX = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("RadiusDamageAffects", StringComparison.OrdinalIgnoreCase):
                                weapon.RadiusDamageAffects = string.Join(' ', values); break;
                            case var _ when key.Equals("ScatterRadius", StringComparison.OrdinalIgnoreCase):
                                weapon.ScatterRadius = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("AcceptableAimDelta", StringComparison.OrdinalIgnoreCase):
                                weapon.AcceptableAimDelta = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("WeaponSpeed", StringComparison.OrdinalIgnoreCase):
                                weapon.WeaponSpeed = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("DamageType", StringComparison.OrdinalIgnoreCase):
                                weapon.DamageType = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("DeathType", StringComparison.OrdinalIgnoreCase):
                                weapon.DeathType = values.FirstOrDefault() ?? string.Empty; break;
                            case var _ when key.Equals("WeaponBonusDamageScalar", StringComparison.OrdinalIgnoreCase):
                                weapon.WeaponBonusDamageScalar = string.Join(' ', values); break;
                            case var _ when key.Equals("Meta", StringComparison.OrdinalIgnoreCase):
                                weapon.Meta = string.Join(' ', values); break;
                            case var _ when key.Equals("Report", StringComparison.OrdinalIgnoreCase):
                                weapon.Report = string.Join(' ', values); break;
                            default:
                                // Unmapped key — simply skip for now; we will extend mapping iteratively
                                break;
                        }
                    }

                    list.Add(weapon);
                }
            }
            catch
            {
                // ignore malformed files
            }
        }

        return list.OrderBy(w => w.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<IReadOnlyCollection<string>> GetWeaponKeysAsync(string dataDir)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(dataDir) || !Directory.Exists(dataDir)) return keys;

        var parseOptions = new JsonDocumentOptions { MaxDepth = 4096 };
        foreach (var jsonPath in Directory.EnumerateFiles(dataDir, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                await using var stream = File.OpenRead(jsonPath);
                using var doc = await JsonDocument.ParseAsync(stream, parseOptions);
                if (doc.RootElement.ValueKind is not JsonValueKind.Array) continue;

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.ValueKind is not JsonValueKind.Object) continue;
                    if (!element.TryGetProperty("Type", out var typeProp) || !typeProp.ValueEquals("Weapon")) continue;
                    if (!element.TryGetProperty("Content", out var content) || content.ValueKind is not JsonValueKind.Array) continue;

                    foreach (var prop in content.EnumerateArray())
                    {
                        if (prop.ValueKind is not JsonValueKind.Object) continue;
                        if (!prop.TryGetProperty("Key", out var keyProp)) continue;
                        var key = keyProp.GetString();
                        if (string.IsNullOrWhiteSpace(key)) continue;
                        keys.Add(key);
                    }
                }
            }
            catch
            {
                // ignore malformed files
            }
        }

        return keys;
    }

    #region Private helpers

    private static bool HasKindOf(JsonElement content, params string[]? flags)
    {
        if (flags is null || flags.Length == 0) return false;

        foreach (var prop in content.EnumerateArray())
        {
            if (prop.ValueKind is not JsonValueKind.Object) continue;
            if (!prop.TryGetProperty("Key", out var keyProp)) continue;
            if (keyProp.GetString() is not "KindOf") continue;
            if (!prop.TryGetProperty("Value", out var valArr) || valArr.ValueKind is not JsonValueKind.Array) continue;

            foreach (var v in valArr.EnumerateArray())
            {
                if (v.ValueKind is not JsonValueKind.String) continue;
                var current = v.GetString();
                if (current is null) continue;
                if (flags.Any(f => string.Equals(current, f, StringComparison.OrdinalIgnoreCase))) return true;
            }
        }

        return false;
    }

    private static string? GetName(JsonElement obj)
    {
        if (!obj.TryGetProperty("Name", out var nameArr) || nameArr.ValueKind is not JsonValueKind.Array) return null;
        var first = nameArr.EnumerateArray().FirstOrDefault();
        return first.ValueKind is JsonValueKind.String ? first.GetString() : null;
    }

    private async Task<IReadOnlyList<GameObjectModel>> LoadByKindOfAsync(string dataDir, params string[] kindFlags)
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
                    if (!HasKindOf(content, kindFlags)) continue;

                    var name = GetName(element);
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var detail = new GameObjectModel { Name = name };

                    FillTopLevelFields(content, detail);
                    ParseBodyBlock(content, detail);
                    ParseArmorSets(content, detail);
                    ParseWeaponSets(content, detail);
                    ParseLocomotors(content, detail);
                    ParsePrerequisites(content, detail);

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
            else if (string.Equals(key, "TransportSlotCount", StringComparison.OrdinalIgnoreCase)) detail.TransportSlotCount = value ?? string.Empty;
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

    private static void ParsePrerequisites(JsonElement content, GameObjectModel detail)
    {
        foreach (var item in content.EnumerateArray())
        {
            if (item.ValueKind is not JsonValueKind.Object) continue;
            if (!item.TryGetProperty("Type", out var blockTypeProp) || !item.TryGetProperty("Content", out var blockContent) ||
                blockContent.ValueKind is not JsonValueKind.Array) continue;
            if (!string.Equals(blockTypeProp.GetString(), "Prerequisites", StringComparison.OrdinalIgnoreCase)) continue;
            foreach (var sub in blockContent.EnumerateArray())
            {
                if (sub.ValueKind is not JsonValueKind.Object) continue;
                if (!sub.TryGetProperty("Key", out var sk) || !sub.TryGetProperty("Value", out var sv) ||
                    sv.ValueKind is not JsonValueKind.Array) continue;
                var key = sk.GetString() ?? string.Empty;

                // Create a new model per prerequisite entry to avoid shared-reference overwrites
                var model = new PrerequisiteSetModel();

                var typeParse = Enum.TryParse<PrerequisiteType>(key, ignoreCase: true, out var prerequisiteType);
                if (!typeParse) continue;
                model.PrerequisiteType = prerequisiteType;
                model.Prerequisite = sv.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                detail.PrereqEntries.Add(model);
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
                var key = sk.GetString() ?? string.Empty;
                switch (key)
                {
                    case var _ when key.Equals("Armor", StringComparison.OrdinalIgnoreCase):
                    {
                        model.Armor = sv.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                        break;
                    }
                    case var _ when key.Equals("Conditions", StringComparison.OrdinalIgnoreCase):
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
                        break;
                    }
                    case var _ when key.Equals("DamageFX", StringComparison.OrdinalIgnoreCase):
                    {
                        model.DamageFX = sv.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                        break;
                    }
                }


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
                    case var _ when key.Equals("Weapon", StringComparison.OrdinalIgnoreCase):
                    {
                        var elements = sv.EnumerateArray()
                            .Select(x => x.GetString() ?? string.Empty)
                            .ToArray();
                        if (elements.Length is not 2) break;
                        var slot = elements[0].ToUpperInvariant() switch
                        {
                            "PRIMARY" => WeaponSlot.PRIMARY,
                            "SECONDARY" => WeaponSlot.SECONDARY,
                            "TERTIARY" => WeaponSlot.TERTIARY,
                            _ => (WeaponSlot?) null
                        };

                        if (slot is null) break;
                        var weapon = elements[1];
                        var weaponModel = new WeaponModel { WeaponSlot = slot.Value, Weapon = weapon };
                        model.Weapons.Add(weaponModel);
                        break;
                    }
                    case var _ when key.Equals("AutoChooseSources", StringComparison.OrdinalIgnoreCase):
                    {
                        var line = sv.EnumerateArray()
                            .Where(n => n.ValueKind is JsonValueKind.String)
                            .Select(n => n.GetString())
                            .Where(x => x is not null)
                            .Cast<string>()
                            .ToArray();
                        var slotParse = Enum.TryParse<WeaponSlot>(line[0], ignoreCase: true, out var slot);
                        if (!slotParse) break;
                        var sources = line
                            .Select(s => Enum.TryParse<AutoChooseSources>(s, ignoreCase: true, out var v) ? (AutoChooseSources?) v : null)
                            .OfType<AutoChooseSources>()
                            .ToArray();
                        var weaponAutoChooseSourceModel = new WeaponAutoChooseSourceModel { WeaponSlot = slot };
                        weaponAutoChooseSourceModel.AutoChooseSources.SetRange(sources);
                        model.AutoChooseSources.GuardedAdd(weaponAutoChooseSourceModel);
                        break;
                    }
                    case var _ when key.Equals("WeaponLockSharedAcrossSets", StringComparison.OrdinalIgnoreCase):
                    {
                        var sharedString = sv.EnumerateArray().FirstOrDefault().GetString() ?? string.Empty;
                        var sharedParse = Enum.TryParse<WeaponLockSharedAcrossSets>(sharedString, ignoreCase: true, out var shared);
                        if (!sharedParse) break;
                        model.WeaponLockSharedAcrossSets = shared;
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
            if (!item.TryGetProperty("Key", out var sk) || !item.TryGetProperty("Value", out var sv) ||
                sv.ValueKind is not JsonValueKind.Array) continue;
            var key = sk.GetString() ?? string.Empty;
            if (!string.Equals(key, "Locomotor", StringComparison.OrdinalIgnoreCase)) continue;

            var line = sv.EnumerateArray()
                .Select(x => x.GetString())
                .Where(x => x is not null)
                .Cast<string>()
                .ToArray();

            var conditionParse = Enum.TryParse<LocomotorConditions>(line[0], ignoreCase: true, out var condition);
            if (!conditionParse) continue;

            var locomotor = line[1];
            if (string.IsNullOrEmpty(locomotor)) continue;

            detail.LocomotorSets.Add(new KeyValuePair<LocomotorConditions, string>(condition, locomotor));
        }
    }

    #endregion
}
