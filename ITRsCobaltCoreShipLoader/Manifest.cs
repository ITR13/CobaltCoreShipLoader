using System.IO.Compression;
using System.Reflection;
using CobaltCoreModding.Definitions.ExternalItems;
using CobaltCoreModding.Definitions.ModContactPoints;
using CobaltCoreModding.Definitions.ModManifests;
using ITRsCobaltCoreShipLoader.Data;
using Newtonsoft.Json;

namespace ITRsCobaltCoreShipLoader;

// ReSharper disable once UnusedType.Global
public class Manifest : IRawShipManifest, IRawStartershipManifest, ISpriteManifest, IShipPartManifest
{
    public string Name => "ITR's Ship Loader";

    public DirectoryInfo? ModRootFolder { get; set; }
    public DirectoryInfo? GameRootFolder { get; set; }

    private readonly List<SimpleStarterShip> _loadedShips = new();
    private readonly Dictionary<string, ExternalSprite> _loadedParts = new();
    private readonly Dictionary<string, ExternalSprite> _offParts = new();

    private string _shipModFolder = "";

    private string GetShipModFolder()
    {
        if (!string.IsNullOrEmpty(_shipModFolder)) return _shipModFolder;
        _shipModFolder = Path.Combine(GameRootFolder!.FullName, "ShipMods");

        if (Directory.Exists(_shipModFolder)) return _shipModFolder;
        Directory.CreateDirectory(_shipModFolder);
        ExportSampleShips(_shipModFolder);

        return _shipModFolder;
    }
        
    private static void ExportSampleShips(string folderPath)
    {
        var currentAssembly = Assembly.GetExecutingAssembly();

        using var resourceStream = currentAssembly.GetManifestResourceStream("ITRsCobaltCoreShipLoader.SampleShips.zip");
        if (resourceStream == null)
        {
            Console.WriteLine("Failed to find sample ships in assembly.");
            return;
        }

        var path = Path.Join(folderPath, "SampleShips.zip");
        using var fileStream = File.Create(path);
        resourceStream.CopyTo(fileStream);
    }

    public void LoadManifest(IRawShipRegistry registry)
    {
        var shipModFolder = GetShipModFolder();
        var shipJsons = GetShipJsons(shipModFolder);
        Console.WriteLine($"Found {shipJsons.Count} ShipMods in {shipModFolder}");
        LoadManifestFromFiles(shipJsons, registry);
    }

    public void LoadManifest(IRawStartershipRegistry registry)
    {
        var loaded = new List<string>();
        foreach (var starterShip in _loadedShips)
        {
            var key = starterShip.ship.key;
            try
            {
                if (!registry.RegisterStartership(starterShip, key))
                {
                    continue;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to load ship {key}");
                Console.WriteLine(e);
                continue;
            }

            loaded.Add(key);
            var descriptionPrefix = string.IsNullOrEmpty(starterShip.MetaData.Author)
                ? ""
                : $"Author: <b>{starterShip.MetaData.Author}</b>\n";
            registry.AddRawLocalization(
                key,
                starterShip.MetaData.Name,
                descriptionPrefix + starterShip.MetaData.Description
            );

            foreach (var (language, loc) in starterShip.MetaData.ExtraLocalization)
            {
                registry.AddRawLocalization(
                    key,
                    loc.Name,
                    descriptionPrefix + loc.Description,
                    language
                );
            }
        }

        if (loaded.Count == 0)
        {
            Console.WriteLine("Failed to load ShipMods");
            return;
        }

        var loadedString = string.Join(", ", loaded);
        Console.WriteLine($"Successfully loaded {loaded.Count} ShipMods:\n{loadedString}");
    }

    private List<(string, string)> GetShipJsons(string shipModFolder)
    {
        var files = Directory.GetFiles(shipModFolder, "*.startership", SearchOption.AllDirectories);
        Console.WriteLine($"Found {files.Length} startership files");
        var filePairs = new List<(string, string)>();
        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file);
                filePairs.Add((file, content));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {file}: {ex.Message}");
            }
        }

        var zipFiles = Directory.GetFiles(shipModFolder, "*.zip", SearchOption.AllDirectories);
        Console.WriteLine($"Found {zipFiles.Length} zip files");
        foreach (var zipFile in zipFiles)
        {
            try
            {
                using var archive = ZipFile.OpenRead(zipFile);
                foreach (var entry in archive.Entries)
                {
                    if (!entry.FullName.EndsWith(".startership", StringComparison.OrdinalIgnoreCase)) continue;
                    using var stream = entry.Open();
                    using var reader = new StreamReader(stream);
                    var content = reader.ReadToEnd();
                    filePairs.Add(($"{zipFile}\0{entry.FullName}", content));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading zip file {zipFile}: {ex.Message}");
            }
        }

        return filePairs;
    }

    private void LoadManifestFromFiles(List<(string, string)> shipJsons, IRawShipRegistry registry)
    {
        if (shipJsons.Count == 0) return;
        var jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };

        foreach (var (path, json) in shipJsons)
        {
            try
            {
                var starterShip = JsonConvert.DeserializeObject<SimpleStarterShip>(json, jsonSettings)!;
                var key = $"@@{starterShip.MetaData.Author}.{starterShip.MetaData.Name}";

                var ship = starterShip.ship;
                ship.key = key;
                ship.isPlayerShip = true;

                foreach (var part in ship.parts)
                {
                    if (part?.skin == null || !part.skin.StartsWith("@@")) continue;
                    part.skin = "@mod_extra_part:" + part.skin.ToLower();
                }

                if (ship.chassisOver != null && ship.chassisOver.StartsWith("@@"))
                {
                    ship.chassisOver = "@mod_extra_part:" + ship.chassisOver.ToLower();
                }

                if (ship.chassisUnder != null && ship.chassisUnder.StartsWith("@@"))
                {
                    ship.chassisUnder = "@mod_extra_part:" + ship.chassisUnder.ToLower();
                }

                if (registry.RegisterShip(ship, key))
                {
                    Console.WriteLine($"Loaded ship from {path}");
                    _loadedShips.Add(starterShip);
                }
                else
                {
                    Console.WriteLine($"Failed to load ship from {path}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ship from {path}:\n{ex.Message}");
            }
        }

        Console.WriteLine($"Successfully loaded {_loadedShips.Count} ships");
    }

    public void LoadManifest(IArtRegistry artRegistry)
    {
        var shipModFolder = GetShipModFolder();
        var files = Directory.GetFiles(shipModFolder, "*.png", SearchOption.AllDirectories);
        Console.WriteLine($"Found {files.Length} .png files");
        var shipModFolderUri = new Uri(
            shipModFolder.EndsWith(Path.DirectorySeparatorChar)
                ? shipModFolder
                : (shipModFolder + Path.DirectorySeparatorChar)
        );
        foreach (var file in files)
        {
            var uri = "@@" + shipModFolderUri.MakeRelativeUri(new Uri(file[..^4]));
            uri = uri.ToLower();
            if (!uri.Contains('/'))
            {
                Console.WriteLine($"Can't register root sprite {uri}, put it in a folder!");
                continue;
            }

            var partDict = uri.EndsWith("_off") ? _offParts : _loadedParts;
            if (partDict.ContainsKey(uri))
            {
                Console.WriteLine($"Duplicate sprite with key {uri} at path {file}");
                continue;
            }

            var fileInfo = new FileInfo(file);

            try
            {
                var externalSprite = new ExternalSprite(uri, fileInfo);
                if (artRegistry.RegisterArt(externalSprite))
                {
                    partDict.Add(uri, externalSprite);
                }
                else
                {
                    Console.WriteLine($"Modloader failed to register {uri}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {file}: {ex.Message}");
            }
        }

        var zipFiles = Directory.GetFiles(shipModFolder, "*.zip", SearchOption.AllDirectories);
        Console.WriteLine($"Found {zipFiles.Length} zip files");
        foreach (var zipFile in zipFiles)
        {
            try
            {
                using var archive = ZipFile.OpenRead(zipFile);
                foreach (var entry in archive.Entries)
                {
                    var fullName = entry.FullName;
                    if (!fullName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;

                    var uri = "@@" + new Uri(fullName[..^4], UriKind.Relative);
                    uri = uri.ToLower();
                    if (!uri.Contains('/'))
                    {
                        Console.WriteLine($"Can't register root sprite {uri} in {zipFile}, put it in a folder!");
                        continue;
                    }

                    var partDict = uri.EndsWith("_off") ? _offParts : _loadedParts;
                    if (partDict.ContainsKey(uri))
                    {
                        Console.WriteLine($"Duplicate sprite with key {uri} at {entry.FullName} in {zipFile}");
                        continue;
                    }

                    var cachedZipEntry = new CachedZipEntryStream(zipFile, fullName);
                    var externalSprite = new ExternalSprite(
                        uri,
                        cachedZipEntry.GetStream
                    );

                    if (artRegistry.RegisterArt(externalSprite))
                    {
                        partDict.Add(uri, externalSprite);
                    }
                    else
                    {
                        Console.WriteLine($"Modloader failed to register {uri}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading zip file {zipFile}: {ex.Message}");
            }
        }
    }

    public void LoadManifest(IShipPartRegistry registry)
    {
        var registered = new List<string>();
        foreach (var (key, sprite) in _loadedParts)
        {
            var offSprite = _offParts.TryGetValue(key + "_off", out var offPart) ? offPart : null;
            try
            {
                if (registry.RegisterRawPart(key, sprite.Id!.Value, offSprite?.Id))
                {
                    registered.Add(key);
                }
                else
                {
                    Console.WriteLine($"Failed to register raw part {key}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        if (registered.Count == 0)
        {
            Console.WriteLine($"Failed to register any ship part sprites");
            return;
        }

        Console.WriteLine($"Registered {registered.Count} ship part sprites:\n" + string.Join("\n", registered));
    }

    public IEnumerable<string> Dependencies => ArraySegment<string>.Empty;
}