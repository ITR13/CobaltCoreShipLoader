using System.Text;
using ITRsCobaltCoreShipLoader.Data;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Newtonsoft.Json;
using Nickel;
using OneOf;
using OneOf.Types;

namespace ITRsCobaltCoreShipLoader;

// ReSharper disable once UnusedType.Global
public class ShipLoader(
    Func<IPluginPackage<IModManifest>, ILogger> modLoggerGetter,
    Func<IPluginPackage<IModManifest>, IModHelper> modHelperGetter
)
    : IPluginLoader<IModManifest, Mod>
{
    private readonly struct SplitEntry(IFileInfo fileInfo, string[] split)
    {
        public readonly IFileInfo FileInfo = fileInfo;
        public readonly string[] Split = split;
    }

    private readonly HashSet<string> _registeredParts = new();

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
    };


    public OneOf<Yes, No, Error<string>> CanLoadPlugin(IPluginPackage<IModManifest> package)
    {
        return package.Manifest.ModType == "ShipMod" ? new Yes() : new No();
    }

    public OneOf<Mod, Error<string>> LoadPlugin(IPluginPackage<IModManifest> package)
    {
        var logger = modLoggerGetter(package);
        var log = (object msg) => logger.Log(LogLevel.Information, "{}", msg);
        var warn = (object msg) => logger.Log(LogLevel.Warning, "{}", msg);
        var err = (object msg) => logger.Log(LogLevel.Error, "{}", msg);

        var modHelper = modHelperGetter(package);
        var modSprites = modHelper.Content.Sprites;
        var modShips = modHelper.Content.Ships;

        var (pngEntries, offPngEntries, shipModEntries) = FindEntries(package, log);
        var successfulParts = LoadSprites(pngEntries, modSprites, offPngEntries, modShips, log, err);
        var successfulShips = LoadShips(shipModEntries, warn, modShips, log, err);

        if (successfulParts == 0 && successfulShips == 0)
        {
            return new Error<string>($"Failed to load any sprites or ships from {package.Manifest.UniqueName}");
        }

        log($"Successfully loaded {successfulParts} part and {successfulShips} ships!");
        return new ShipMod();
    }

    private int LoadShips(
        List<SplitEntry> shipModEntries,
        Action<object> warn,
        IModShips modShips,
        Action<object> log,
        Action<object> err
    )
    {
        var successfulShips = 0;
        foreach (var entry in shipModEntries)
        {
            try
            {
                var json = ReadFile(entry.FileInfo);
                var ship = LoadShip(warn, json, entry.Split);
                var definition = new ShipConfiguration
                {
                    Ship = ship,
                };
                var registeredShip = modShips.RegisterShip(ship.ship.key, definition);

                successfulShips++;
                log($"Loaded ship {registeredShip.UniqueName}");
            }
            catch (Exception ex)
            {
                err($"Failed loading ship at entry {entry.FileInfo}:\n{ex}");
            }
        }

        return successfulShips;
    }

    private int LoadSprites(
        List<(string, IFileInfo)> pngEntries,
        IModSprites modSprites,
        Dictionary<string, IFileInfo> offPngEntries,
        IModShips modShips,
        Action<object> log,
        Action<object> err
    )
    {
        var successfulParts = 0;
        foreach (var (cleanedEntry, fileInfo) in pngEntries)
        {
            try
            {
                var cachedEntry = fileInfo;
                var sprite = modSprites.RegisterSprite(
                    cleanedEntry,
                    () => cachedEntry.OpenRead()
                );

                var offEntry = cleanedEntry + "_off";
                ISpriteEntry? offSprite = null;
                if (offPngEntries.TryGetValue(offEntry, out var offFileInfo))
                {
                    offSprite = modSprites.RegisterSprite(
                        cleanedEntry + "_off",
                        () => offFileInfo.OpenRead()
                    );
                }

                var part = modShips.RegisterPart(
                    cleanedEntry,
                    new PartConfiguration
                    {
                        Sprite = sprite.Sprite,
                        DisabledSprite = offSprite?.Sprite,
                    }
                );
                _registeredParts.Add(part.UniqueName);
                successfulParts++;

                log($"Loaded part {part.UniqueName}");
            }
            catch (Exception ex)
            {
                err($"Failed loading sprite at entry {fileInfo}:\n{ex}");
            }
        }

        return successfulParts;
    }

    private (List<(string, IFileInfo)> pngEntries, Dictionary<string, IFileInfo> offPngEntries, List<SplitEntry>
        shipModEntries) FindEntries(IPluginPackage<IModManifest> package, Action<object> log)
    {
        var pngEntries = new List<(string, IFileInfo)>();
        var offPngEntries = new Dictionary<string, IFileInfo>();
        var shipModEntries = new List<SplitEntry>();
        foreach (var fileInfo in package.PackageRoot.Files)
        {
            var entryPath = fileInfo.GetRelativePathTo(package.PackageRoot);
            if (entryPath == "nickel.json") continue;

            var cleanedEntry = FixPath(entryPath.ToLower());
            var extension = Path.GetExtension(cleanedEntry);
            cleanedEntry = "/" + cleanedEntry[..^extension.Length];

            switch (extension)
            {
                case ".png" when cleanedEntry.EndsWith("_off"):
                    offPngEntries.Add(cleanedEntry, fileInfo);
                    break;
                case ".png":
                    pngEntries.Add((cleanedEntry, fileInfo));
                    break;
                case ".startership":
                    shipModEntries.Add(
                        new SplitEntry(
                            fileInfo: fileInfo,
                            split: $"{package.Manifest.UniqueName}::{cleanedEntry}".Split("/")
                        )
                    );
                    break;
                default:
                    log($"Skipping entry '{cleanedEntry}' with extension {extension}");
                    break;
            }
        }

        return (pngEntries, offPngEntries, shipModEntries);
    }

    private string ReadFile(IFileInfo file)
    {
        using var streamReader = new StreamReader(file.OpenRead(), Encoding.UTF8);
        return streamReader.ReadToEnd();
    }

    private SimpleStarterShip LoadShip(Action<object> warn, string json, string[] folders)
    {
        var starterShip = JsonConvert.DeserializeObject<SimpleStarterShip>(json, JsonSettings)!;
        var key = $"{starterShip.MetaData.Author}.{starterShip.MetaData.Name}";

        var ship = starterShip.ship;
        ship.key = key;
        ship.isPlayerShip = true;

        foreach (var part in ship.parts)
        {
            part.skin = GetSpriteName(warn, part.skin, folders);
        }

        ship.chassisOver = GetSpriteName(warn, ship.chassisOver, folders);
        ship.chassisUnder = GetSpriteName(warn, ship.chassisUnder, folders);

        return starterShip;
    }

    private string? GetSpriteName(Action<object> warn, string? original, string[] folders)
    {
        if (original == null || !original.StartsWith("@@"))
        {
            return original;
        }

        original = original[2..].ToLower();
        for (var i = 0; i < folders.Length - 1; i++)
        {
            var path = string.Join("/", folders[i..^1]) + "/" + original;
            if (_registeredParts.Contains(path))
            {
                return path;
            }
        }

        warn($"Failed to remap sprite '{original}'");
        return original;
    }

    private string FixPath(string path)
    {
        return path.Replace(Path.DirectorySeparatorChar, '/');
    }
}