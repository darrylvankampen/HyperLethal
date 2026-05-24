using HyperLethal.Items;
using HyperLethal.Utilities;
using SPTarkov.DI.Annotations;

namespace HyperLethal.Services;

[Injectable]
public sealed class HyperLethalMarketService(
    SptDatabaseAccessor databaseAccessor)
{

    // public void RegisterHandbook(HyperLethalItemDefinition definition)
    // {}
    //
    // public void RegisterFlea(HyperLethalItemDefinition definition)
    // {
    //     if (databaseAccessor.TryEnableFlea(definition.NewTpl))
    //     {
    //         HyperLethalLog.Info("Flea", $"Enabled flea market entry for {definition.Name}");
    //     }
    // }
}
