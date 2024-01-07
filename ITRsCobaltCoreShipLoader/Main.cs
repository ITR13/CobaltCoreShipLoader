using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;

namespace ITRsCobaltCoreShipLoader;

public class Main : Mod
{
    public Main(
        ExtendablePluginLoader<IModManifest, Mod> extendablePluginLoader,
        Func<IPluginPackage<IModManifest>, ILogger> modLoggerGetter,
        Func<IPluginPackage<IModManifest>, IModHelper> modHelperGetter
    )
    {
        extendablePluginLoader.RegisterPluginLoader(
            new ShipLoader(modLoggerGetter, modHelperGetter)
        );
    }
}