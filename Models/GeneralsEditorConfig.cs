namespace GeneralsZeroHourEditor.Models;

public record GeneralsEditorConfig
{
    // Existing field used elsewhere: editor storage root (e.g., Documents\GeneralsEditor)
    public string Location { get; init; } = string.Empty;

    // New: absolute paths to installed games
    public string GeneralsPath { get; init; } = string.Empty;
    public string ZeroHourPath { get; init; } = string.Empty;

    // Optional flag to indicate paths were chosen at least once
    public bool Configured { get; init; }
}
