using HyperLethal.Utilities;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;

namespace HyperLethal.Services;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class HyperLethalTraderService
(DatabaseService databaseService, ICloner cloner, LocaleService localeService)
{
    public void SetTraderUpdateTime(TraderConfig traderConfig, TraderBase baseTrader, int refreshTimeMin, int refreshTimeMax)
    {
        var traderRefreshRecord = new UpdateTime()
        {
            TraderId = baseTrader.Id,
            Seconds = new MinMax<int>(refreshTimeMin, refreshTimeMax),
        };
        traderConfig.UpdateTime.Add(traderRefreshRecord);
    }

    public void AddTraderToDatabase(TraderBase trader)
    {
        var emptyAssorts = new TraderAssort
        {
            Items = [],
            BarterScheme = new Dictionary<MongoId, List<List<BarterScheme>>>(),
            LoyalLevelItems = new Dictionary<MongoId, int>()
        };

        var traderData = new Trader
        {
            Assort = emptyAssorts,
            Base = cloner.Clone(trader),
            QuestAssort = new()
            {
                { "Started", new() },
                { "Success", new() },
                { "Fail", new() }
            },
            Dialogue = []
        };

        if (!databaseService.GetTables().Traders.TryAdd(trader.Id, traderData))
        {
            HyperLethalLog.Error("Trader", "Failed to add trader!");
        }
    }

    public void AddLocales(TraderBase traderBase, string firstName, string desc)
    {
        var locales = databaseService.GetTables().Locales.Global;
        var newTraderId = traderBase.Id;
        var fullName = traderBase.Name;
        var nickName =  traderBase.Nickname;
        var location =  traderBase.Location;

        foreach (var (key, value) in locales)
        {
            value.AddTransformer(loadedData =>
            {
                loadedData.Add($"{newTraderId} FullName", fullName);
                loadedData.Add($"{newTraderId} FirstName", firstName);
                loadedData.Add($"{newTraderId} Nickname", nickName);
                loadedData.Add($"{newTraderId} Location", location);
                loadedData.Add($"{newTraderId} Description", desc);
                return loadedData;
            });
        }
    }

    public void OverwriteAssort(string TraderId, TraderAssort assorts)
    {
        if (!databaseService.GetTables().Traders.TryGetValue(TraderId, out var traderToEdit))
        {
            HyperLethalLog.Error("Trader", $"Cannot find trader {TraderId}!");
            return;
        }
        HyperLethalLog.Success("Trader", $"Trader {TraderId} updated!");
        traderToEdit.Assort = assorts;
    }
}