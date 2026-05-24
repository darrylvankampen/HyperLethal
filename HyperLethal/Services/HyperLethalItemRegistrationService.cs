using HyperLethal.Items;
using HyperLethal.Utilities;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services.Mod;

namespace HyperLethal.Services;

[Injectable]
public sealed class HyperLethalItemRegistrationService(
    ISptLogger<HyperLethalItemRegistrationService> logger,
    CustomItemService customItemService,
    SptDatabaseAccessor databaseAccessor)
{
    // register item using clone
    public bool RegisterItemFromClone(
        HyperLethalItemDefinition definition,
        TemplateItemProperties overrideProperties)
    {
        EnsureTemplateTypeIsSet(overrideProperties, definition.Name);

        var resolved = databaseAccessor.TryTemplateExists(definition.NewTpl, out var exists);
        if (resolved && exists)
        {
            logger.Warning($"[HyperLethal][Register] Duplicate id skipped: {definition.NewTpl} ({definition.Name})");
            return false;
        }

        var request = new NewItemFromCloneDetails
        {
            ItemTplToClone = definition.BaseTpl,
            NewId = definition.NewTpl,
            ParentId = definition.ParentId,
            FleaPriceRoubles = definition.Price,
            HandbookPriceRoubles = definition.Price,
            HandbookParentId = definition.HandbookParentId,
            Locales = new Dictionary<string, LocaleDetails>
            {
                ["en"] = new LocaleDetails
                {
                    Name = definition.Name,
                    ShortName = definition.ShortName,
                    Description = definition.Description
                }
            },
            OverrideProperties = overrideProperties
        };

        customItemService.CreateItemFromClone(request);
        logger.Success($"[HyperLethal][Register] Registered item {definition.Name} ({definition.NewTpl})");
        return true;
    }

    // check type property because somehow it crashed on this -> error not found yet
    private void EnsureTemplateTypeIsSet(TemplateItemProperties overrideProperties, string itemName)
    {
        var typeProp = overrideProperties.GetType().GetProperty("type")
                       ?? overrideProperties.GetType().GetProperty("Type");
        if (typeProp is null || !typeProp.CanWrite)
        {
            return;
        }

        var current = typeProp.GetValue(overrideProperties)?.ToString();
        if (!string.IsNullOrWhiteSpace(current))
        {
            return;
        }

        typeProp.SetValue(overrideProperties, "Item");
        logger.Warning($"[HyperLethal][Debug] Forced override type=Item for {itemName} to prevent empty taxonomy type.");
    }
}
