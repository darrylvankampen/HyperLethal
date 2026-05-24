using HyperLethal.Ammo;
using HyperLethal.Items;
using HyperLethal.Utilities;
using SPTarkov.DI.Annotations;

namespace HyperLethal.Services;

[Injectable]
public sealed class HyperLethalValidationService(
    SptDatabaseAccessor databaseAccessor)
{
    /// <summary>
    /// Validates the complete built-in ammo definition set.
    /// </summary>
    public void ValidateBuiltInData(IEnumerable<HyperLethalAmmoDefinition> ammoDefinitions)
    {
        var definitions = ammoDefinitions.ToList();
        var duplicates = definitions
            .GroupBy(x => x.NewTpl)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var duplicateId in duplicates)
        {
            HyperLethalLog.Error("Validation", $"Duplicate item id detected: {duplicateId}");
        }

        var hasErrors = duplicates.Count > 0;
        foreach (var definition in definitions)
        {
            hasErrors |= !ValidateDefinition(definition);
        }

        if (hasErrors)
        {
            HyperLethalLog.Warning("Validation", "One or more definitions failed validation checks.");
        }
        else
        {
            HyperLethalLog.Success("Validation", "All definitions passed validation.");
        }
    }
    
    public bool ValidateDefinition(HyperLethalItemDefinition definition)
    {
        var valid = true;

        var resolved = databaseAccessor.TryTemplateExists(definition.BaseTpl, out var exists);
        if (resolved && !exists)
        {
            HyperLethalLog.Warning("Validation", $"Base tpl does not exist: {definition.BaseTpl} ({definition.Name})");
            valid = false;
        }
        else if (!resolved)
        {
            HyperLethalLog.Warning("Validation", $"Base tpl validation skipped (templates unavailable): {definition.BaseTpl} ({definition.Name})");
        }

        return valid;
    }
    
    /// Validates that a trader id exists in the loaded database. not used at the moment
    public bool ValidateTrader(string traderId)
    {
        var resolved = databaseAccessor.TryTraderExists(traderId, out var exists);
        if (!resolved)
        {
            HyperLethalLog.Warning("Validation", $"Trader validation skipped (traders unavailable): {traderId}");
            return true;
        }

        if (!exists)
        {
            HyperLethalLog.Warning("Validation", $"Trader does not exist: {traderId}");
        }

        return exists;
    }
}
