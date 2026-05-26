using GeneralsZeroHourEditor.Services.DataService;
using GeneralsZeroHourEditor.Services.JsonService;
using GeneralsZeroHourEditor.Services.LocationService;
using Microsoft.Extensions.DependencyInjection;
using GeneralsZeroHourEditor.Services.BigArchiveService;
using System.Text.Json;
using GeneralsZeroHourEditor.Models;

namespace GeneralsZeroHourEditor.Tasks;

public class InitialLoadDataTask(ILocationService locationService, IJsonService jsonService, IDataService dataService) : EngineTask
{
    #region Fields

    private string[] _iniFiles = [];

    #endregion

    public override async Task Call()
    {
        MaxWork = GetWorkLoad();
        await DoWork();
        Work = MaxWork;
    }

    private int GetWorkLoad()
    {
        var baseDir = Path.Combine(locationService.GeneralsEditorConfig.Location, "Data", "INI");
        var defaultDir = Path.Combine(baseDir, "Default") + Path.DirectorySeparatorChar;

        if (Directory.Exists(baseDir))
        {
            _iniFiles = Directory.GetFiles(baseDir, "*.ini", SearchOption.AllDirectories)
                .Where(file => !file.StartsWith(defaultDir, StringComparison.OrdinalIgnoreCase))
                .OrderBy(Path.GetFileName)
                .ToArray();
        }
        else
        {
            _iniFiles = [];
        }

        // If no extracted INI files exist, we will calculate workload later from BIG entries.
        return _iniFiles.Length;
    }

    private async Task DoWork()
    {
        locationService.CreateDataDirectory();
        locationService.CreateProjectDirectory();
        var projectDataDir = Path.Combine(locationService.ProjectDirectory!, "Data");
        var baseIniDir = Path.Combine(locationService.GeneralsEditorConfig.Location, "Data", "INI");

        if (!Directory.Exists(projectDataDir)) Directory.CreateDirectory(projectDataDir);

        if (_iniFiles.Length > 0)
        {
            // Legacy path: convert already-extracted INI files
            foreach (var file in _iniFiles)
            {
                var json = dataService.ParseIniToJson(file);

                // Maintain a relative directory structure
                var relativePath = Path.GetRelativePath(baseIniDir, file);
                var destPath = Path.Combine(projectDataDir, Path.ChangeExtension(relativePath, ".json"));

                var destDir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                await jsonService.SaveToFileAsync(destPath, json);
                Work++;
            }
        }
        else
        {
            // New path: scrape directly from BIG archives
            // Read a fresh copy of config from disk to avoid stale cached values
            var currentCfg = locationService.GeneralsEditorConfig;
            var cfgPath = Path.Combine(currentCfg.Location, "config.json");
            var cfgText = File.Exists(cfgPath) ? File.ReadAllText(cfgPath) : string.Empty;
            GeneralsEditorConfig? freshCfg = null;
            try { if (!string.IsNullOrWhiteSpace(cfgText)) freshCfg = JsonSerializer.Deserialize<GeneralsEditorConfig>(cfgText); }
            catch { /* ignore malformed */ }
            var effectiveCfg = freshCfg ?? currentCfg;

            var generalsDir = effectiveCfg.GeneralsPath;
            var zhDir = effectiveCfg.ZeroHourPath;

            // If still not configured, exit gracefully (first-run prompts are handled earlier in the ViewModel)
            if (string.IsNullOrWhiteSpace(generalsDir) || string.IsNullOrWhiteSpace(zhDir))
            {
                return;
            }

            // Build separate indices for Generals and Zero Hour to allow object-level merging
            var genIndex = new Services.BigArchiveService.BigArchiveService();
            genIndex.IndexArchives(generalsDir, string.Empty);
            var zhIndex = new Services.BigArchiveService.BigArchiveService();
            zhIndex.IndexArchives(string.Empty, zhDir);

            // Collect union of INI entry paths from both indices, prefer paths under Data/INI/
            var genPaths = genIndex.EnumerateEntries(".ini").Select(e => e.Path);
            var zhPaths = zhIndex.EnumerateEntries(".ini").Select(e => e.Path);
            var allPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var p in genPaths) if (p.Contains("Data/INI/", StringComparison.OrdinalIgnoreCase)) allPaths.Add(p);
            foreach (var p in zhPaths) if (p.Contains("Data/INI/", StringComparison.OrdinalIgnoreCase)) allPaths.Add(p);

            // Fallback: include any .ini if none were under Data/INI/
            if (allPaths.Count == 0)
            {
                foreach (var p in genPaths) allPaths.Add(p);
                foreach (var p in zhPaths) allPaths.Add(p);
            }

            MaxWork = allPaths.Count;

            foreach (var path in allPaths.OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
            {
                var zhText = zhIndex.TryReadText(path);
                var genText = genIndex.TryReadText(path);

                string mergedJson;
                if (!string.IsNullOrWhiteSpace(zhText) && !string.IsNullOrWhiteSpace(genText))
                {
                    // Merge at top-level object/module granularity: ZH overrides same-named blocks; Generals fills gaps
                    var zhJson = dataService.ParseIniContentToJson(zhText);
                    var genJson = dataService.ParseIniContentToJson(genText);
                    mergedJson = MergeTopLevelBlocks(genJson, zhJson); // ZH wins on duplicates
                }
                else if (!string.IsNullOrWhiteSpace(zhText))
                {
                    mergedJson = dataService.ParseIniContentToJson(zhText);
                }
                else if (!string.IsNullOrWhiteSpace(genText))
                {
                    mergedJson = dataService.ParseIniContentToJson(genText);
                }
                else
                {
                    Work++; continue;
                }

                // Compute relative path after Data/INI/
                var idx = path.IndexOf("/Data/INI/", StringComparison.OrdinalIgnoreCase);
                var rel = idx >= 0 ? path[(idx + "/Data/INI/".Length)..] : Path.GetFileName(path);
                var destPath = Path.Combine(projectDataDir, Path.ChangeExtension(rel, ".json"));

                var destDir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                await jsonService.SaveToFileAsync(destPath, mergedJson);
                Work++;
            }
        }

        // Initialize and save the Game Registry
        var gameRegistryService = AppMain.App.Services?.GetRequiredService<GeneralsZeroHourEditor.Services.GameRegistryService.IGameRegistryService>();
        if (gameRegistryService != null)
        {
            gameRegistryService.Initialize(projectDataDir);
            var schemaPath = Path.Combine(locationService.ProjectDirectory!, "Schema", "game.schema.json");
            gameRegistryService.SaveSchema(schemaPath);
        }
    }

    private static string MergeTopLevelBlocks(string generalsJson, string zhJson)
    {
        var parseOptions = new JsonDocumentOptions { MaxDepth = 4096 };
        using var zhDoc = JsonDocument.Parse(string.IsNullOrWhiteSpace(zhJson) ? "[]" : zhJson, parseOptions);
        using var genDoc = JsonDocument.Parse(string.IsNullOrWhiteSpace(generalsJson) ? "[]" : generalsJson, parseOptions);

        var dict = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        static string? KeyFor(JsonElement el)
        {
            if (el.ValueKind != JsonValueKind.Object) return null;
            if (!el.TryGetProperty("Type", out var tProp)) return null;
            var type = tProp.GetString() ?? string.Empty;
            if (string.IsNullOrEmpty(type)) return null;
            if (!el.TryGetProperty("Name", out var nameArr) || nameArr.ValueKind != JsonValueKind.Array) return null;
            var first = nameArr.EnumerateArray().FirstOrDefault();
            if (first.ValueKind != JsonValueKind.String) return null;
            var name = first.GetString() ?? string.Empty;
            if (string.IsNullOrEmpty(name)) return null;
            return type + "::" + name;
        }

        // Start with Generals
        var genArray = genDoc.RootElement.ValueKind == JsonValueKind.Array ? genDoc.RootElement.EnumerateArray() : Enumerable.Empty<JsonElement>();
        var genMisc = new List<string>();
        foreach (var el in genArray)
        {
            var key = KeyFor(el);
            if (key != null) dict[key] = el; else genMisc.Add(el.GetRawText());
        }

        // Override/add from Zero Hour
        var zhArray = zhDoc.RootElement.ValueKind == JsonValueKind.Array ? zhDoc.RootElement.EnumerateArray() : Enumerable.Empty<JsonElement>();
        var zhMisc = new HashSet<string>();
        foreach (var el in zhArray)
        {
            var key = KeyFor(el);
            if (key != null) dict[key] = el; else zhMisc.Add(el.GetRawText());
        }

        // Build final array: deterministically ordered by key
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartArray();
            foreach (var kv in dict.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                kv.Value.WriteTo(writer);
            }
            // Add ZH-only misc entries first, then Generals-only ones that are not duplicates
            foreach (var raw in zhMisc)
            {
                using var t = JsonDocument.Parse(raw, parseOptions);
                t.RootElement.WriteTo(writer);
            }
            foreach (var raw in genMisc)
            {
                if (zhMisc.Contains(raw)) continue;
                using var t = JsonDocument.Parse(raw, parseOptions);
                t.RootElement.WriteTo(writer);
            }
            writer.WriteEndArray();
        }
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }
}
