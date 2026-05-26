using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GeneralsZeroHourEditor.Services.DataService;

public partial class DataService : IDataService
{
    #region Fields

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        // Some INI files can legitimately nest many sub-blocks. Also, if a source INI
        // is malformed, we still want to serialize a best-effort tree instead of
        // crashing on default depth (64). Bump the JSON max depth generously.
        MaxDepth = 256,
        // In case of any accidental reference reuse in the constructed object graph,
        // ignore cycles instead of throwing. Our structure should be a tree, but this
        // keeps serialization robust across all INIs.
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    private readonly Regex _lineRegex = CompiledLineRegex();

    private readonly HashSet<string> _blockStarters = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draw", "Body", "Behavior", "WeaponSet", "ArmorSet", "Prerequisites",
        "ExperienceScalarUpgrade", "SlowDeathBehavior", "TransitionDamageFX",
        "FlammableUpdate", "ConditionState", "DefaultConditionState",
        "UnitSpecificSounds", "LocomotorSet", "VisionData", "ProjectileSet",
        "SideSet", "EngineeringParameters", "UnitAbsorbAbilitiesUpgrade",
        "ExperienceLevel", "AutoDepositUpdate", "GrantUpgradeCreate",
        "AttributeModifierUpgrade", "StealthUpdate", "ProductionUpdate",
        "QueueProductionExitUpdate", "SpawnBehavior", "SpecialAbility",
        "SpecialPower", "SpecialAbilityUpdate", "DynamicShroudClearingRangeUpdate",
        "StealthDetectorUpdate", "PhysicsBehavior", "DestroyDie", "FXListDie",
        "CreateCrateDie", "CreateObjectDie", "EjectPilotDie", "ObjectCreationUpgrade",
        "WeaponSetUpgrade", "MappedImage", "TransitionState", "UnitSpecificFX",
        "ClientUpdate", "AttackAreaDecal", "TargetingReticleDecal", "UpgradeTree",
        "GridDecalTemplate", "SkirmishBuildList", "Structure", "ParticleSystem",
        "Sound", "LightPulse", "TerrainScorch", "ViewShake", "AttachedModel",
        "CreateDebris", "CreateObject", "DeliverPayload", "DeliveryDecal",
        "RayEffect", "Tracer", "FXListAtBonePos", "ApplyRandomForce", "FireWeapon", "Attack"
    };

    private readonly HashSet<string> _ambiguousStarters = new(StringComparer.OrdinalIgnoreCase)
    {
        "Turret", "Sound", "ParticleSystem", "FXList", "Tracer", "FireWeapon"
    };

    private readonly HashSet<string> _nonBlockKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "AliasConditionState", "TransitionKey", "WeaponFireFXBone", "WeaponLaunchBone",
        "WeaponMuzzleFlash", "WeaponHideShowBone", "PrimaryDamage", "PrimaryDamageRadius",
        "ScatterRadiusVsInfantry", "AttackRange", "MinimumAttackRange", "DefaultStructureRubbleHeight",
        "VertexWaterAvailableMaps1", "VertexWaterXPosition1", "VertexWaterYPosition1",
        "VertexWaterZPosition1", "VertexWaterXGridCells1", "VertexWaterYGridCells1",
        "VertexWaterGridSize1", "WeaponBonus"
    };

    private readonly HashSet<string> _topLevelBlocks = new(StringComparer.OrdinalIgnoreCase)
    {
        "AIData", "Animation", "Armor", "AudioEvent", "AudioSettings", "Bridge",
        "Campaign", "CommandButton", "CommandMap", "CommandSet", "ControlBarScheme",
        "ControlBarResizer", "CrateData", "Credits", "WindowTransition", "DamageFX",
        "DialogEvent", "DrawGroupInfo", "EvaEvent", "FXList", "GameData", "InGameUI",
        "Locomotor", "Language", "MapCache", "MapData", "MappedImage", "MiscAudio",
        "Mouse", "MouseCursor", "MultiplayerColor", "OnlineChatColors", "MultiplayerSettings",
        "MusicTrack", "Object", "ObjectCreationList", "ObjectReskin", "ParticleSystem",
        "PlayerTemplate", "Road", "Science", "Rank", "SpecialPower", "ShellMenuScheme",
        "Terrain", "Upgrade", "Video", "WaterSet", "WaterTransparency", "Weapon",
        "WebpageURL", "HeaderTemplate", "StaticGameLOD", "DynamicGameLOD", "LODPreset",
        "BenchProfile", "ReallyLowMHz", "ChildObject", "ChallengeMode", "Eva", "Speech",
        "Voice", "Roads", "Water", "Weather"
    };

    #endregion

    #region Public Functions

    public string ParseIniToJson(string filePath)
    {
        if (!File.Exists(filePath)) return "{}";

        var lines = File.ReadAllLines(filePath);
        return ParseIniLinesToJson(lines);
    }

    public string ParseIniContentToJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return "[]";
        var lines = content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        return ParseIniLinesToJson(lines);
    }

    private string ParseIniLinesToJson(IEnumerable<string> lines)
    {
        var stack = new Stack<List<object>>();
        var blockTypeStack = new Stack<string>();
        List<object> currentList = [];
        // Keep a stable reference to the true root array to avoid returning a nested list
        // if an 'End' line is missed or malformed in source content.
        var rootList = currentList;

        foreach (var rawLine in lines)
        {
            // Strip common INI comment styles: ';', '//', and '#'. Use the earliest occurrence.
            var commentStripped = StripComments(rawLine);
            var line = commentStripped.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.Equals("End", StringComparison.OrdinalIgnoreCase))
            {
                // Always treat a standalone 'End' (after trimming ';' comments) as the end of the current block.
                // This makes the parser resilient to inline comments or extra whitespace.
                if (stack.Count > 0)
                {
                    currentList = stack.Pop();
                    blockTypeStack.Pop();
                }

                continue;
            }

            var match = _lineRegex.Match(line);
            if (!match.Success) continue;

            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value.Trim();
            var values = value.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

            if (IsBlockStart(key, line, blockTypeStack))
            {
                HandleBlockStart(key, values, stack, blockTypeStack, ref currentList);
            }
            else
            {
                currentList.Add(new Dictionary<string, object> { { "Key", key }, { "Value", values } });
            }
        }

        // Always serialize from the root list to avoid returning a nested list when a closing 'End' was missed.
        return JsonSerializer.Serialize(rootList, _jsonOptions);
    }

    public string[] CollectTopLevelNames(string projectDataDir, string topLevelType)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parseOptions = new JsonDocumentOptions { MaxDepth = 4096 };
        foreach (var jsonPath in Directory.EnumerateFiles(projectDataDir, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                using var stream = File.OpenRead(jsonPath);
                using var doc = JsonDocument.Parse(stream, parseOptions);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) continue;
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.ValueKind is not JsonValueKind.Object) continue;
                    if (!element.TryGetProperty("Type", out var typeProp)) continue;
                    if (!typeProp.ValueEquals(topLevelType)) continue;
                    if (!element.TryGetProperty("Name", out var nameArr) || nameArr.ValueKind is not JsonValueKind.Array) continue;
                    var first = nameArr.EnumerateArray().FirstOrDefault();
                    if (first.ValueKind is not JsonValueKind.String) continue;
                    var n = first.GetString();
                    if (!string.IsNullOrWhiteSpace(n)) names.Add(n);
                }
            }
            catch
            {
                // ignore malformed
            }
        }

        return names.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    #endregion

    #region Private Functions

    private static string StripComments(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        // Normalize to simple string scanning; do not allocate too much
        var idxSemicolon = s.IndexOf(';');
        var idxHash = s.IndexOf('#');
        var idxSlashSlash = s.IndexOf("//", StringComparison.Ordinal);

        var idx = -1;

        Consider(idxSemicolon);
        Consider(idxHash);
        Consider(idxSlashSlash);

        return idx >= 0 ? s[..idx] : s;

        void Consider(int i)
        {
            if (i < 0) return;
            if (idx < 0 || i < idx) idx = i;
        }
    }

    private void HandleBlockStart(
        string key,
        string[] values,
        Stack<List<object>> stack,
        Stack<string> blockTypeStack,
        ref List<object> currentList)
    {
        // ENGINE REFINEMENT: If the block key is followed by an '=' on the same line (e.g., ObjectNames = FireWall),
        // we need to remove the '=' from the 'values' array which contains the block 'name'.
        var nameValues = values;
        if (values.Length > 0 && values[0] == "=")
        {
            nameValues = values.Skip(1).ToArray();
        }

        var blockContent = new List<object>();
        var block = new Dictionary<string, object>
        {
            { "Type", key },
            { "Name", nameValues },
            { "Content", blockContent }
        };

        currentList.Add(block);
        stack.Push(currentList);
        blockTypeStack.Push(key);
        currentList = blockContent;
    }

    private bool IsBlockStart(string key, string currentLine, Stack<string> blockTypeStack)
    {
        var hasAssignment = currentLine.Contains('=');
        var parentBlock = blockTypeStack.Count > 0 ? blockTypeStack.Peek() : string.Empty;

        if (_topLevelBlocks.Contains(key))
        {
            // Top-level blocks (Weapon, Object, FXList, etc.) must only start at the root of a file.
            // Allowing them inside another block causes invalid nesting (e.g., Weapon inside Weapon).
            return blockTypeStack.Count == 0;
        }

        if (_blockStarters.Contains(key))
        {
            // ENGINE REFINEMENT: Many block starters (like CreateObject, CreateDebris, ParticleSystem)
            // can appear without an '=' sign even when they ARE blocks.
            // If they are in _blockStarters and have no assignment, they are almost certainly blocks.
            // EXCEPT if they are known to be ambiguous and often used as properties with '='.
            if (!hasAssignment) return true;
            return !_ambiguousStarters.Contains(key);
        }

        if (_ambiguousStarters.Contains(key))
        {
            return !hasAssignment;
        }

        if (_nonBlockKeywords.Contains(key)) return false;

        // Final Heuristic: If there's no assignment, and it's not a common property keyword, it might be a block.
        // In the engine, many sub-blocks (nuggets) are identified this way.
        if (hasAssignment) return false;
        // However, we must be careful. Many properties also don't have '='.
        // For now, if it's not in our explicit non-block list, we'll try treating it as a block
        // ONLY if it's inside one of the known "nugget containers" like FXList or ObjectCreationList.
        if (parentBlock.Equals("FXList", StringComparison.OrdinalIgnoreCase) ||
            parentBlock.Equals("ObjectCreationList", StringComparison.OrdinalIgnoreCase))
        {
            // SPECIAL CASE: Some keywords in these blocks are definitely properties even without '='.
            // If it's not a known block starter and we are in OCL/FXList, we only treat it as a block
            // if it's one of the known "nugget" types.
            return _blockStarters.Contains(key);
        }

        return false;
    }

    #endregion

    #region Regex Functions

    [GeneratedRegex(@"^\s*([\w]+)\s*(?:=|(?:\s+))?\s*(.*)$", RegexOptions.Compiled)]
    private static partial Regex CompiledLineRegex();

    #endregion
}
