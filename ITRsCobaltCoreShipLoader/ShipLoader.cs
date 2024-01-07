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
    private readonly struct SplitEntry(string entryPath, string[] split)
    {
        public readonly string EntryPath = entryPath;
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
        void Log(object msg) => logger.Log(LogLevel.Information, "{}", msg);
        var warn = (object msg) => logger.Log(LogLevel.Warning, "{}", msg);
        void Err(object msg) => logger.Log(LogLevel.Error, "{}", msg);

        var modHelper = modHelperGetter(package);
        var modSprites = modHelper.Content.Sprites;
        var modShips = modHelper.Content.Ships;

        var pngEntries = new List<(string, string)>();
        var offPngEntries = new HashSet<string>();
        var shipModEntries = new List<SplitEntry>();
        var rootPath = package.PackageRoot.FullName + Path.DirectorySeparatorChar;
        var rootUri = new Uri(rootPath);
        foreach (var fileInfo in package.PackageRoot.Files)
        {
            var entry = rootUri.MakeRelativeUri(new Uri(fileInfo.FullName)).ToString();
            if (entry == "nickel.json") continue;

            var cleanedEntry = entry.ToLower();
            var extension = Path.GetExtension(cleanedEntry);
            cleanedEntry = "/" + cleanedEntry[..^extension.Length];

            switch (extension)
            {
                case ".png" when cleanedEntry.EndsWith("_off"):
                    offPngEntries.Add(entry);
                    break;
                case ".png":
                    pngEntries.Add((entry, cleanedEntry));
                    break;
                case ".startership":
                    shipModEntries.Add(
                        new SplitEntry(
                            entryPath: entry,
                            split: $"{package.Manifest.UniqueName}::{cleanedEntry}".Split("/")
                        )
                    );
                    break;
                default:
                    Log($"Skipping entry '{cleanedEntry}' with extension {extension}");
                    break;
            }
        }

        var successfulParts = 0;
        foreach (var (entry, cleanedEntry) in pngEntries)
        {
            try
            {
                var cachedEntry = entry;
                var sprite = modSprites.RegisterSprite(
                    cleanedEntry,
                    () => new FileStream(Path.Combine(rootPath, cachedEntry), FileMode.Open)
                );

                var offEntry = entry[..^4] + "_off.png";
                ISpriteEntry? offSprite = null;
                if (offPngEntries.Contains(offEntry))
                {
                    offSprite = modSprites.RegisterSprite(
                        cleanedEntry + "_off",
                        () => new FileStream(Path.Combine(rootPath, offEntry), FileMode.Open)
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

                Log($"Loaded part {part.UniqueName}");
            }
            catch (Exception ex)
            {
                Err($"Failed loading sprite at entry {entry}:\n{ex}");
            }
        }

        var successfulShips = 0;
        foreach (var entry in shipModEntries)
        {
            try
            {
                var json = File.ReadAllText(Path.Combine(rootPath, entry.EntryPath));
                var ship = LoadShip(warn, json, entry.Split);
                var definition = new ShipConfiguration
                {
                    Ship = ship,
                };
                var registeredShip = modShips.RegisterShip(ship.ship.key, definition);

                successfulShips++;
                Log($"Loaded ship {registeredShip.UniqueName}");
            }
            catch (Exception ex)
            {
                Err($"Failed loading ship at entry {entry.EntryPath}:\n{ex}");
            }
        }

        if (successfulParts == 0 && successfulShips == 0)
        {
            return new Error<string>($"Failed to load any sprites or ships from {package.Manifest.UniqueName}");
        }

        Log($"Successfully loaded {successfulParts} part and {successfulShips} ships!");
        return new ShipMod();
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
}