namespace GeneralsZeroHourEditor.Models;

internal sealed class ModuleAggregate(string name)
{
    public string Name { get; } = name;

    public HashSet<string> Fields { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, HashSet<string>> SubBlocks { get; } = new(StringComparer.OrdinalIgnoreCase);

    public HashSet<string> Variants { get; } = new(StringComparer.OrdinalIgnoreCase);
}
