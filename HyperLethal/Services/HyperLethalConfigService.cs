using System.Reflection;
using HyperLethal.Config;
using HyperLethal.Utilities;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;

namespace HyperLethal.Services;

[Injectable]
public sealed class HyperLethalConfigService
(ModHelper modHelper)
{
    private static HLConfig? _cachedConfig;

    public HLConfig GetConfig()
    {
        if (_cachedConfig is not null)
        {
            return _cachedConfig;
        }
        
        var path = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        _cachedConfig = modHelper.GetJsonDataFromFile<HLConfig>($"{path}/config/", "config.json");
        return _cachedConfig;
    }
}
