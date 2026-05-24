using HyperLethal.Utilities;
using SPTarkov.DI.Annotations;
using System.Reflection;
using WTTServerCommonLib;

namespace HyperLethal.Services;

[Injectable]
public sealed class HyperLethalBootstrapService(
    WTTServerCommonLib.WTTServerCommonLib wttCommon)
{
    public async Task Initialize()
    {
        HyperLethalLog.Info("Init", "Starting mod initialization.");

        var assembly = Assembly.GetExecutingAssembly();
        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly, Path.Join("db", "items"));
        await wttCommon.CustomAssortSchemeService.CreateCustomAssortSchemes(assembly, Path.Join("db", "assorts"));

        HyperLethalLog.LoadedSuccessfully();
    }
}
