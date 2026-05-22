namespace GeneralsZeroHourEditor.Services.BigArchiveService;

public interface IBigArchiveService
{
    /// Build an index from the provided Generals and Zero Hour directories.
    /// Zero Hour entries override Generals on duplicate logical paths.
    void IndexArchives(string generalsDir, string zeroHourDir);

    /// Enumerate all indexed entries. Optionally filter by file extension (e.g., ".ini").
    IEnumerable<BigEntryInfo> EnumerateEntries(params string[]? extensions);

    /// Try get read-only stream for an entry path (case-insensitive, '/' or '\\'). Returns null if missing.
    Stream? OpenEntryStream(string entryPath);

    /// Convenience: read text content using ASCII fallback (INI files are ASCII in ZH/Generals).
    string? TryReadText(string entryPath);
}

public readonly record struct BigEntryInfo(string Path, string SourceArchive, long Offset, int Size);
