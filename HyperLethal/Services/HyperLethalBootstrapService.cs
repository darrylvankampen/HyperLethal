using HyperLethal.Ammo;
using HyperLethal.Utilities;
using SPTarkov.DI.Annotations;

namespace HyperLethal.Services;

[Injectable]
public sealed class HyperLethalBootstrapService(
    HyperLethalValidationService validationService,
    HyperLethalAmmoFactory ammoFactory)
{
    public Task Initialize()
    {
        HyperLethalLog.Info("Init", "Starting mod initialization.");

        validationService.ValidateBuiltInData(HyperLethalAmmoDefinitions.All);
        ammoFactory.RegisterAll(HyperLethalAmmoDefinitions.All);

        HyperLethalLog.Success("Init", "Initialization complete.");
        return Task.CompletedTask;
    }
}
