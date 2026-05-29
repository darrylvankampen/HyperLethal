using System.Reflection;
using HyperLethal.Config;
using HyperLethal.Utilities;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;

namespace HyperLethal.Services;

[Injectable]
public sealed class ConfigService
(ModHelper modHelper)
{
    private static Config.Config? _cachedConfig;

    public Config.Config GetConfig()
    {
        if (_cachedConfig is not null)
        {
            return _cachedConfig;
        }
        
        var path = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        _cachedConfig = modHelper.GetJsonDataFromFile<Config.Config>($"{path}/config/", "config.json");
        return _cachedConfig;
    }
}
