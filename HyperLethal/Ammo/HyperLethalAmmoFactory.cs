using HyperLethal.Constants;
using HyperLethal.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;

namespace HyperLethal.Ammo;

[Injectable]
public sealed class HyperLethalAmmoFactory(
    ISptLogger<HyperLethalAmmoFactory> logger,
    HyperLethalValidationService validationService,
    HyperLethalItemRegistrationService itemRegistrationService,
    HyperLethalTraderService traderService)
{
    public void RegisterAll(IEnumerable<HyperLethalAmmoDefinition> definitions)
    {
        foreach (var definition in definitions)
        {
            RegisterAmmoInternal(definition, validate: false);
        }
    }
    
    public void RegisterAmmo(HyperLethalAmmoDefinition definition)
    {
        RegisterAmmoInternal(definition, validate: true);
    }
    
    private void RegisterAmmoInternal(HyperLethalAmmoDefinition definition, bool validate)
    {
        if (validate && !validationService.ValidateDefinition(definition))
        {
            logger.Warning($"[HyperLethal][Ammo] Skipping invalid definition: {definition.Name}");
            return;
        }

        logger.Info($"[HyperLethal][Ammo] Registering {definition.Name} with Price={definition.Price}");

        var registered = itemRegistrationService.RegisterItemFromClone(
            definition,
            new TemplateItemProperties
            {
                Damage = definition.Damage,
                PenetrationPower = definition.Penetration,
                ArmorDamage = definition.ArmorDamage,
                FragmentationChance = definition.FragmentationChance,
                InitialSpeed = definition.InitialSpeed
            });

        if (!registered)
        {
            return;
        }

        traderService.InjectAssort(definition, HyperLethalDefaults.TraderPeacekeeper, 1);
    }
}
