using System.Text.Json;
using GeneralsZeroHourEditor.Models.Weapon;
using GeneralsZeroHourEditor.Services.DataService;

namespace GeneralsZeroHourEditor.Services.WeaponService;

public class WeaponService(IDataService dataService) : IWeaponService
{
    public async Task<IReadOnlyList<WeaponDefinition>> LoadWeaponsAsync(string gameRootDir)
    {
        // Expecting standard Zero Hour path layout: <root>\Data\INI\Weapon.ini
        if (string.IsNullOrWhiteSpace(gameRootDir)) return Array.Empty<WeaponDefinition>();
        var iniPath = Path.Combine(gameRootDir, "Data", "INI", "Weapon.ini");
        if (!File.Exists(iniPath)) return Array.Empty<WeaponDefinition>();

        // Reuse existing INI->JSON converter so we stay consistent with the rest of the pipeline
        var json = await Task.Run(() => dataService.ParseIniToJson(iniPath));

        var results = new List<WeaponDefinition>();

        try
        {
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 4096 });
            if (doc.RootElement.ValueKind is not JsonValueKind.Array) return results;

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.ValueKind is not JsonValueKind.Object) continue;
                if (!element.TryGetProperty("Type", out var typeProp) || !typeProp.ValueEquals("Weapon")) continue;
                if (!element.TryGetProperty("Name", out var nameArr) || nameArr.ValueKind is not JsonValueKind.Array) continue;
                if (!element.TryGetProperty("Content", out var content) || content.ValueKind is not JsonValueKind.Array) continue;

                var nameToken = nameArr.EnumerateArray().FirstOrDefault();
                var name = nameToken.ValueKind is JsonValueKind.String ? nameToken.GetString() ?? string.Empty : string.Empty;
                if (string.IsNullOrWhiteSpace(name)) continue;

                var model = new WeaponDefinition { Name = name };

                foreach (var prop in content.EnumerateArray())
                {
                    if (prop.ValueKind is not JsonValueKind.Object) continue;

                    // Ignore nested blocks for now (we only map flat key/value weapon properties)
                    if (prop.TryGetProperty("Type", out _) && prop.TryGetProperty("Content", out _)) continue;

                    if (!prop.TryGetProperty("Key", out var keyProp) || !prop.TryGetProperty("Value", out var valArr) || valArr.ValueKind is not JsonValueKind.Array)
                        continue;

                    var key = keyProp.GetString() ?? string.Empty;
                    var values = valArr.EnumerateArray()
                        .Where(v => v.ValueKind is JsonValueKind.String)
                        .Select(v => v.GetString() ?? string.Empty)
                        .ToArray();

                    // Map a set of common keys into strongly-typed fields only; ignore the rest here
                    switch (key)
                    {
                        case var _ when key.Equals("PrimaryDamage", StringComparison.OrdinalIgnoreCase):
                            model.PrimaryDamage = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("PrimaryDamageRadius", StringComparison.OrdinalIgnoreCase):
                            model.PrimaryDamageRadius = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("AttackRange", StringComparison.OrdinalIgnoreCase):
                            model.AttackRange = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("MinimumAttackRange", StringComparison.OrdinalIgnoreCase):
                            model.MinimumAttackRange = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("PreAttackDelay", StringComparison.OrdinalIgnoreCase):
                            model.PreAttackDelay = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("DelayBetweenShots", StringComparison.OrdinalIgnoreCase):
                            model.DelayBetweenShots = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("ClipSize", StringComparison.OrdinalIgnoreCase):
                            model.ClipSize = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("ClipReloadTime", StringComparison.OrdinalIgnoreCase):
                            model.ClipReloadTime = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("ProjectileObject", StringComparison.OrdinalIgnoreCase):
                            model.ProjectileObject = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("ProjectileDetonationOCL", StringComparison.OrdinalIgnoreCase):
                            model.ProjectileDetonationOCL = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("ProjectileCollidesWith", StringComparison.OrdinalIgnoreCase):
                            model.ProjectileCollidesWith = string.Join(' ', values); break;
                        case var _ when key.Equals("FireFX", StringComparison.OrdinalIgnoreCase):
                            model.FireFX = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("RadiusDamageAffects", StringComparison.OrdinalIgnoreCase):
                            model.RadiusDamageAffects = string.Join(' ', values); break;
                        case var _ when key.Equals("ScatterRadius", StringComparison.OrdinalIgnoreCase):
                            model.ScatterRadius = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("AcceptableAimDelta", StringComparison.OrdinalIgnoreCase):
                            model.AcceptableAimDelta = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("WeaponSpeed", StringComparison.OrdinalIgnoreCase):
                            model.WeaponSpeed = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("DamageType", StringComparison.OrdinalIgnoreCase):
                            model.DamageType = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("DeathType", StringComparison.OrdinalIgnoreCase):
                            model.DeathType = values.FirstOrDefault() ?? string.Empty; break;
                        case var _ when key.Equals("WeaponBonusDamageScalar", StringComparison.OrdinalIgnoreCase):
                            model.WeaponBonusDamageScalar = string.Join(' ', values); break;
                        case var _ when key.Equals("Meta", StringComparison.OrdinalIgnoreCase):
                            model.Meta = string.Join(' ', values); break;
                        case var _ when key.Equals("Report", StringComparison.OrdinalIgnoreCase):
                            model.Report = string.Join(' ', values); break;
                        default:
                            break;
                    }
                }

                results.Add(model);
            }
        }
        catch
        {
            // Ignore malformed files; return whatever we could parse
        }

        // Order results by name for consistent UI
        return results.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
