using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using GeneralsZeroHourEditor.Models;

namespace GeneralsZeroHourEditor.Services.DataService;

public partial class DataService : IDataService
{
    #region Fields

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly Regex _lineRegex = CompiledLineRegex();

    private readonly HashSet<string> _blockStarters = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draw", "Body", "Behavior", "WeaponSet", "ArmorSet", "Prerequisites",
        "ExperienceScalarUpgrade", "SlowDeathBehavior", "TransitionDamageFX",
        "FlammableUpdate", "ConditionState", "DefaultConditionState",
        "UnitSpecificSounds", "LocomotorSet", "VisionData", "ProjectileSet",
        "SideSet", "EngineeringParameters", "UnitAbsorbAbilitiesUpgrade",
        "WeaponBonus", "ExperienceLevel", "AutoDepositUpdate", "GrantUpgradeCreate",
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
        "VertexWaterGridSize1"
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

    private readonly HashSet<string> _parentBlocksPreventingObject = new(StringComparer.OrdinalIgnoreCase)
    {
        "Prerequisites", "ArmorSet", "WeaponSet"
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

        foreach (var rawLine in lines)
        {
            var lineWithComments = rawLine.Trim();
            var line = rawLine.Split(';')[0].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.Equals("End", StringComparison.OrdinalIgnoreCase))
            {
                if (lineWithComments.Split(';')[0].Trim().Length <= 3)
                {
                    if (stack.Count > 0)
                    {
                        currentList = stack.Pop();
                        blockTypeStack.Pop();
                    }

                    continue;
                }
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

        return JsonSerializer.Serialize(currentList, _jsonOptions);
    }

    public string[] CollectTopLevelNames(string projectDataDir, string topLevelType)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
                    if (!element.TryGetProperty("Type", out var typeProp)) continue;
                    if (!typeProp.ValueEquals(topLevelType)) continue;
                    if (element.TryGetProperty("Name", out var nameArr) && nameArr.ValueKind == JsonValueKind.Array)
                    {
                        var first = nameArr.EnumerateArray().FirstOrDefault();
                        if (first.ValueKind == JsonValueKind.String)
                        {
                            var n = first.GetString();
                            if (!string.IsNullOrWhiteSpace(n)) names.Add(n);
                        }
                    }
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
            if (blockTypeStack.Count == 0) return true;
            if (_parentBlocksPreventingObject.Contains(parentBlock)) return false;

            // HEURISTIC: If it's a top level block keyword but inside another block and HAS an assignment,
            // it's almost certainly a property (e.g. Locomotor = ..., CommandSet = ...)
            if (hasAssignment) return false;

            return true;
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
        if (!hasAssignment)
        {
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
        }

        return false;
    }

    #endregion

    #region Regex Functions

    [GeneratedRegex(@"^\s*([\w]+)\s*(?:=|(?:\s+))?\s*(.*)$", RegexOptions.Compiled)]
    private static partial Regex CompiledLineRegex();

    #endregion
}
