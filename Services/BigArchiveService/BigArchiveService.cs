using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Text;

namespace GeneralsZeroHourEditor.Services.BigArchiveService;

public class BigArchiveService : IBigArchiveService
{
    private readonly ConcurrentDictionary<string, Entry> _index = new(StringComparer.OrdinalIgnoreCase);

    public void IndexArchives(string generalsDir, string zeroHourDir)
    {
        _index.Clear();

        // 1) Index Generals base game first (lower priority)
        foreach (var big in EnumerateBigFilesSafe(generalsDir))
        {
            TryIndexArchive(big, preferExisting: false);
        }

        // 2) Index Zero Hour last (higher priority overrides duplicates)
        foreach (var big in EnumerateBigFilesSafe(zeroHourDir))
        {
            TryIndexArchive(big, preferExisting: true);
        }
    }

    public IEnumerable<BigEntryInfo> EnumerateEntries(params string[]? extensions)
    {
        HashSet<string>? exts = null;
        if (extensions is { Length: > 0 })
        {
            exts = new HashSet<string>(extensions.Select(e => e.StartsWith('.') ? e : "." + e), StringComparer.OrdinalIgnoreCase);
        }

        return _index.Values
            .Where(e => exts == null || exts.Contains(Path.GetExtension(e.Path)))
            .OrderBy(e => e.Path)
            .Select(e => new BigEntryInfo(e.Path, e.SourceArchive, e.Offset, e.Size));
    }

    public Stream? OpenEntryStream(string entryPath)
    {
        if (!_index.TryGetValue(NormalizePath(entryPath), out var e)) return null;
        var fs = new FileStream(e.SourceArchive, FileMode.Open, FileAccess.Read, FileShare.Read);
        return new SubStream(fs, e.Offset, e.Size, leaveOpenParent: false);
    }

    public string? TryReadText(string entryPath)
    {
        using var s = OpenEntryStream(entryPath);
        if (s == null) return null;
        using var reader = new StreamReader(s, Encoding.ASCII, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static IEnumerable<string> EnumerateBigFilesSafe(string? root)
    {
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) yield break;
        string[] patterns = ["*.big", "*.BIG"]; // Windows FS usually case-insensitive but be safe for indexing
        foreach (var pattern in patterns)
        {
            IEnumerable<string> files = [];
            try { files = Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories); }
            catch { /* ignore */ }

            foreach (var f in files) yield return f;
        }
    }

    private void TryIndexArchive(string bigPath, bool preferExisting)
    {
        try
        {
            using var fs = new FileStream(bigPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: true);

            var fourCc = new string(br.ReadChars(4));
            if (fourCc != "BIG4" && fourCc != "BIGF") return; // not a BIG we support

            // total archive size (LE)
            var sizeLe = br.ReadUInt32();
            _ = sizeLe; // not needed further

            // NumEntries (BE) and OffsetFirst (BE)
            Span<byte> beBuf = stackalloc byte[4];
            fs.ReadExactly(beBuf);
            var numEntries = BinaryPrimitives.ReadUInt32BigEndian(beBuf);
            fs.ReadExactly(beBuf);
            var _offsetFirst = BinaryPrimitives.ReadUInt32BigEndian(beBuf);

            for (var i = 0; i < numEntries; i++)
            {
                fs.ReadExactly(beBuf);
                var entryOffset = BinaryPrimitives.ReadUInt32BigEndian(beBuf);
                fs.ReadExactly(beBuf);
                var entrySize = BinaryPrimitives.ReadUInt32BigEndian(beBuf);

                // read CSTRING (null-terminated)
                var name = ReadCString(br) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(name)) continue;

                var norm = NormalizePath(name);
                var e = new Entry(norm, bigPath, entryOffset, checked((int)entrySize));

                if (preferExisting)
                {
                    // Overwrite if already present (Zero Hour wins)
                    _index[norm] = e;
                }
                else
                {
                    _index.TryAdd(norm, e);
                }
            }
        }
        catch
        {
            // Corrupt or locked archive — ignore gracefully
        }
    }

    private static string? ReadCString(BinaryReader br)
    {
        var sb = new StringBuilder(128);
        try
        {
            while (true)
            {
                var b = br.ReadByte();
                if (b == 0) break;
                sb.Append((char)b);
            }
            return sb.ToString();
        }
        catch
        {
            return null;
        }
    }

    private static string NormalizePath(string p)
    {
        var s = p.Replace('\\', '/');
        // BIG entries often store relative-like paths without drive letters
        return s.TrimStart('/');
    }

    private sealed class Entry(string path, string sourceArchive, long offset, int size)
    {
        public string Path { get; } = path;
        public string SourceArchive { get; } = sourceArchive;
        public long Offset { get; } = offset;
        public int Size { get; } = size;
    }

    /// Stream that restricts reading to a subrange of a parent FileStream.
    private sealed class SubStream : Stream
    {
        private readonly FileStream _parent;
        private readonly long _start;
        private readonly long _length;
        private long _position;
        private readonly bool _leaveOpenParent;

        public SubStream(FileStream parent, long start, long length, bool leaveOpenParent)
        {
            _parent = parent; _start = start; _length = length; _position = 0; _leaveOpenParent = leaveOpenParent;
            _parent.Seek(_start, SeekOrigin.Begin);
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length;
        public override long Position { get => _position; set => Seek(value, SeekOrigin.Begin); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count)
        {
            var remain = (int)Math.Min(count, _length - _position);
            if (remain <= 0) return 0;
            _parent.Seek(_start + _position, SeekOrigin.Begin);
            var read = _parent.Read(buffer, offset, remain);
            _position += read;
            return read;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            long target = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => _position
            };
            if (target < 0 || target > _length) throw new IOException("Seek out of bounds");
            _position = target;
            return _position;
        }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_leaveOpenParent) _parent.Dispose();
        }
    }
}
