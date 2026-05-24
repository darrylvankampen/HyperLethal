using HyperLethal.Items;
using HyperLethal.Utilities;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Servers;

namespace HyperLethal.Services;

[Injectable]
public sealed class HyperLethalTraderService(
    DatabaseServer databaseServer)
{
    /// Injects one item into a trader assort with barter and loyalty requirements.
    public void InjectAssort(HyperLethalItemDefinition definition, string traderId, int loyaltyLevelRequired = 1)
    {
        var tables = databaseServer.GetTables();
        if (!tables.Traders.ContainsKey(traderId))
        {
            HyperLethalLog.Warning("Trader", $"Trader not found for inject: {traderId}");
            return;
        }

        var assort = tables.Traders[traderId].Assort;
        var item = TraderAssortFactory.CreateSingleItem(definition.NewTpl);
        var barterScheme = BarterSchemeFactory.CreateRoubleBarter(definition.Price);

        assort.Items.Add(item);
        assort.BarterScheme[item.Id] = barterScheme;
        assort.LoyalLevelItems[item.Id] = loyaltyLevelRequired;
        HyperLethalLog.Info("Trader", $"Injected {definition.Name} into trader {traderId}, price: {definition.Price}");
    }
}
