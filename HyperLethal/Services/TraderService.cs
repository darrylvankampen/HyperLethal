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
public class TraderService
(DatabaseService databaseService, ICloner cloner)
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

    public void AddTraderToDatabase(TraderBase baseTrader)
    {
        var traderData = new Trader
        {
            Assort = CreateNewTraderAssort(),
            Base = cloner.Clone(baseTrader),
            QuestAssort = CreateNewQuestAssort(),
            Dialogue = []
        };

        if (!databaseService.GetTables().Traders.TryAdd(baseTrader.Id, traderData))
        {
            LogService.Error("Trader", "Failed to add trader!");
        }
    }

    private static TraderAssort CreateNewTraderAssort()
    {
        return new TraderAssort()
        {
            Items = [],
            BarterScheme = new Dictionary<MongoId, List<List<BarterScheme>>>(),
            LoyalLevelItems = new Dictionary<MongoId, int>()
        };
    }
    
    private static Dictionary<string, Dictionary<MongoId, MongoId>> CreateNewQuestAssort()
    {
        return new()
        {
            { "Started", new() },
            { "Success", new() },
            { "Fail", new() }
        };
    }

    public void AddLocales(TraderBase baseTrader, string firstName, string desc)
    {
        var locales = databaseService.GetTables().Locales.Global;
        var newTraderId = baseTrader.Id;
        var fullName = baseTrader.Name;
        var nickName =  baseTrader.Nickname;
        var location =  baseTrader.Location;

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

    public void OverwriteAssort(string traderId, TraderAssort assorts)
    {
        if (!databaseService.GetTables().Traders.TryGetValue(traderId, out var traderToEdit))
        {
            LogService.Error("Trader", $"Cannot find trader {traderId}!");
            return;
        }
        LogService.Success("Trader", $"Trader {traderId} updated!");
        traderToEdit.Assort = assorts;
    }
}