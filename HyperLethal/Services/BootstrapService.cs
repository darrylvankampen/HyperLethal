using HyperLethal.Utilities;
using SPTarkov.DI.Annotations;
using System.Reflection;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using Path = System.IO.Path;

namespace HyperLethal.Services;

[Injectable]
public sealed class BootstrapService(
    WTTServerCommonLib.WTTServerCommonLib wttCommon, ModHelper modHelper, ImageRouter imageRouter, ConfigServer configServer, TimeUtil timeUtil, TraderService traderService, ConfigService config, OverridePropertiesService harderItemService)
{
    private readonly TraderConfig _traderConfig = configServer.GetConfig<TraderConfig>();
    private readonly RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();
    
    public async Task Initialize()
    {
        LogService.Configure(config);

        var assembly = Assembly.GetExecutingAssembly();
        var itemsPath = Path.Join("db", "items");
        var tradersPath = modHelper.GetAbsolutePathToModFolder(assembly);
        var version = new ModMetadata().Version.ToString();

        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly, itemsPath);
        harderItemService.ApplyOverrides(assembly, itemsPath);
        
        AddTrader(tradersPath);
        
        LogService.LoadedSuccessfully(version);
    }

    public void AddTrader(string path)
    {
        string traderImage;
        var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(path, "db/trader/base.json");
        if (config.GetConfig().ReplaceTraderImage)
        {
            LogService.Info("Trader", "Replaced the trader image with a cute little cat");
            traderImage = Path.Combine(path, "db/trader/cutecat.jpg");
            traderBase.Avatar = "/files/trader/avatar/cutecat.jpg";
        }
        else
        {
            traderImage = Path.Combine(path, "db/trader/keres.jpg");
        }
        
        
        imageRouter.AddRoute(traderBase.Avatar.Replace(".jpg", ""), traderImage);
        traderService.SetTraderUpdateTime(_traderConfig, traderBase, timeUtil.GetHoursAsSeconds(1), timeUtil.GetHoursAsSeconds(2));

        _ragfairConfig.Traders.TryAdd(traderBase.Id, true);
        
        traderService.AddTraderToDatabase(traderBase);
        
        traderService.AddLocales(traderBase, "Keres", "Keres is a former engineer turned underground dealer. Supplier for experimental items that were never meant to leave the testing range.");

        var assorts = modHelper.GetJsonDataFromFile<TraderAssort>(path, "db/trader/assorts.json");

        traderService.OverwriteAssort(traderBase.Id, assorts);
    }
}
