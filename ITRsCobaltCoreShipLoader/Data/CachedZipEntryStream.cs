using System.IO.Compression;

namespace ITRsCobaltCoreShipLoader.Data;

public class CachedZipEntryStream
{
    private static readonly HashSet<string> LoadedArchives = new ();
    private static (string, ZipArchive)? _cachedArchive = default;

    private string _zipPath, _entryPath;
    
    public CachedZipEntryStream(string zipPath, string entryPath)
    {
        _zipPath = zipPath;
        _entryPath = entryPath;
    }

    public Stream GetStream()
    {
        if (_cachedArchive == null || _cachedArchive.Value.Item1 != _zipPath)
        {
            _cachedArchive?.Item2.Dispose();
            _cachedArchive = (_zipPath, ZipFile.OpenRead(_zipPath));

            if (!LoadedArchives.Add(_zipPath))
            {
                Console.WriteLine($"Warning: Loaded archive {_zipPath} multiple times!");
            }
        }

        var entry = _cachedArchive.Value.Item2.GetEntry(_entryPath);
        if (entry == null)
        {
            throw new FileLoadException($"Failed to find entry {_entryPath} in zip {_zipPath}");
        }
        return entry.Open();
    }
}