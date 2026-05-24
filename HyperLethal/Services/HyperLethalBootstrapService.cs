using HyperLethal.Utilities;
using SPTarkov.DI.Annotations;
using System.Reflection;
using System.Text.Json;
using WTTServerCommonLib;

namespace HyperLethal.Services;

[Injectable]
public sealed class HyperLethalBootstrapService(
    WTTServerCommonLib.WTTServerCommonLib wttCommon)
{
    public async Task Initialize()
    {
        HyperLethalLog.Info("Init", "Starting mod initialization.");

        var assembly = Assembly.GetExecutingAssembly();
        var itemsPath = Path.Join("db", "items");
        var assortsPath = Path.Join("db", "assorts");
        var version = new ModMetadata().Version.ToString();
        var loadedItems = CountTopLevelEntries(assembly, itemsPath);
        var loadedAssorts = CountAssortItems(assembly, assortsPath);

        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly, itemsPath);
        await wttCommon.CustomAssortSchemeService.CreateCustomAssortSchemes(assembly, assortsPath);

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
}
