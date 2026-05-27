using HyperLethal.Utilities;
using SPTarkov.DI.Annotations;
using System.Reflection;
using System.Text.Json;
using HyperLethal.Config;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using WTTServerCommonLib;
using Path = System.IO.Path;

namespace HyperLethal.Services;

[Injectable]
public sealed class HyperLethalBootstrapService(
    WTTServerCommonLib.WTTServerCommonLib wttCommon, ModHelper modHelper, ImageRouter imageRouter, ConfigServer configServer, TimeUtil timeUtil, HyperLethalTraderService hyperLethalTraderService, HyperLethalConfigService config)
{
    private readonly TraderConfig _traderConfig = configServer.GetConfig<TraderConfig>();
    private readonly RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();
    
    public async Task Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var itemsPath = Path.Join("db", "items");
        var assortsPath = Path.Join("db", "assorts");
        var tradersPath = modHelper.GetAbsolutePathToModFolder(assembly);
        var version = new ModMetadata().Version.ToString();
        var loadedItems = CountTopLevelEntries(assembly, itemsPath);
        var loadedAssorts = CountAssortItems(assembly, assortsPath);

        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly, itemsPath);

        if (config.GetConfig().EnableTrader)
        {
            AddTrader(tradersPath);
        }
        else
        {
            await wttCommon.CustomAssortSchemeService.CreateCustomAssortSchemes(assembly, assortsPath);
        }
        HyperLethalLog.LoadedSuccessfully(version, loadedItems, loadedAssorts);
    }

    private static int CountTopLevelEntries(Assembly assembly, string relativePath)
    {
        var modRootPath = Path.GetDirectoryName(assembly.Location);
        if (string.IsNullOrWhiteSpace(modRootPath))
        {
            return 0;
        }

        var absolutePath = Path.Combine(modRootPath, relativePath);
        if (!Directory.Exists(absolutePath))
        {
            return 0;
        }

        var count = 0;
        var files = Directory.GetFiles(absolutePath, "*.json", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            using var stream = File.OpenRead(file);
            using var document = JsonDocument.Parse(stream);
            if (document.RootElement.ValueKind == JsonValueKind.Object)
            {
                count += document.RootElement.EnumerateObject().Count();
            }
        }

        return count;
    }

    private static int CountAssortItems(Assembly assembly, string relativePath)
    {
        var modRootPath = Path.GetDirectoryName(assembly.Location);
        if (string.IsNullOrWhiteSpace(modRootPath))
        {
            return 0;
        }

        var absolutePath = Path.Combine(modRootPath, relativePath);
        if (!Directory.Exists(absolutePath))
        {
            return 0;
        }

        var count = 0;
        var files = Directory.GetFiles(absolutePath, "*.json", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            using var stream = File.OpenRead(file);
            using var document = JsonDocument.Parse(stream);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var traderEntry in document.RootElement.EnumerateObject())
            {
                if (traderEntry.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                if (!traderEntry.Value.TryGetProperty("items", out var itemsElement))
                {
                    continue;
                }

                if (itemsElement.ValueKind == JsonValueKind.Array)
                {
                    count += itemsElement.GetArrayLength();
                }
            }
        }

        return count;
    }

    public void AddTrader(string path)
    {
        string traderImage;
        var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(path, "db/trader/base.json");
        if (config.GetConfig().ReplaceTraderImage)
        {
            HyperLethalLog.Info("Trader", "Replaced the trader image with a cute little cat");
            traderImage = Path.Combine(path, "db/trader/cutecat.jpg");
            traderBase.Avatar = "/files/trader/avatar/cutecat.jpg";
        }
        else
        {
            traderImage = Path.Combine(path, "db/trader/keres.jpg");
        }
        
        
        imageRouter.AddRoute(traderBase.Avatar.Replace(".jpg", ""), traderImage);
        hyperLethalTraderService.SetTraderUpdateTime(_traderConfig, traderBase, timeUtil.GetHoursAsSeconds(1), timeUtil.GetHoursAsSeconds(2));

        _ragfairConfig.Traders.TryAdd(traderBase.Id, true);
        
        hyperLethalTraderService.AddTraderToDatabase(traderBase);
        
        hyperLethalTraderService.AddLocales(traderBase, "Keres", "Keres is a former engineer turned underground dealer. Supplier for experimental items that were never meant to leave the testing range.");

        var assorts = modHelper.GetJsonDataFromFile<TraderAssort>(path, "db/trader/assorts.json");
        
        hyperLethalTraderService.OverwriteAssort(traderBase.Id, assorts);
    }
}
