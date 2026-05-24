using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using HyperLethal.Utilities;
using Microsoft.AspNetCore.Http;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Servers.Http;

namespace HyperLethal.Services;

[Injectable(TypePriority = 0)]
public sealed class HyperLethalWebAppListener : IHttpListener
{
    private const string RouteRoot = "/hyperlethal";
    private const string RouteStyles = "/hyperlethal/style.css";
    private const string RouteScript = "/hyperlethal/app.mjs";
    private const string RouteItems = "/hyperlethal/data";
    private const string RouteAssorts = "/hyperlethal/assorts";
    private const string RouteSaveItem = "/hyperlethal/save";
    private const string RouteSaveAssort = "/hyperlethal/save-assort";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _modRootPath = Path.GetDirectoryName(typeof(ModMetadata).Assembly.Location) ?? string.Empty;

    public bool CanHandle(MongoId sessionId, HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        return path.StartsWith(RouteRoot, StringComparison.OrdinalIgnoreCase);
    }

    public async Task Handle(MongoId sessionId, HttpContext context)
    {
        var path = (context.Request.Path.Value ?? string.Empty).TrimEnd('/');
        if (string.IsNullOrWhiteSpace(path))
        {
            path = "/";
        }

        var isGet = context.Request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase);
        var isPost = context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);
        if (isGet) await HandleGet(path, context);
        else if (isPost) await HandlePost(path, context);
        else await WriteStatus(context, StatusCodes.Status405MethodNotAllowed);
    }

    private async Task HandleGet(string path, HttpContext context)
    {
        switch (path)
        {
            case RouteRoot:
                await WriteFileResponse(context, GetWebFilePath("index.html"), "text/html; charset=utf-8");
                return;
            case RouteStyles:
                await WriteFileResponse(context, GetWebFilePath("style.css"), "text/css; charset=utf-8");
                return;
            case RouteScript:
                await WriteFileResponse(context, GetWebFilePath("app.mjs"), "text/javascript; charset=utf-8");
                return;
            case RouteItems:
                await WriteJsonResponse(context, LoadEditorData());
                return;
            case RouteAssorts:
                await WriteJsonResponse(context, LoadAssortData());
                return;
            default:
                await WriteStatus(context, StatusCodes.Status404NotFound);
                return;
        }
    }

    private async Task HandlePost(string path, HttpContext context)
    {
        if (string.Equals(path, RouteSaveItem, StringComparison.OrdinalIgnoreCase))
        {
            await HandleSaveItem(context);
            return;
        }

        if (string.Equals(path, RouteSaveAssort, StringComparison.OrdinalIgnoreCase))
        {
            await HandleSaveAssort(context);
            return;
        }

        await WriteStatus(context, StatusCodes.Status404NotFound);
    }

    private async Task HandleSaveItem(HttpContext context)
    {
        var request = await DeserializeRequest<SaveRequest>(context);

        if (request is null || string.IsNullOrWhiteSpace(request.File))
        {
            await WriteBadRequest(context, "Invalid payload.");
            return;
        }
        
        var validationError = ValidateItemRequest(request);
        if (validationError is not null)
        {
            await WriteBadRequest(context, validationError);
            return;
        }

        if (!TryResolveFilePath(request.File, "db/items", out var targetFilePath))
        {
            await WriteBadRequest(context, "File path is outside db");
            return;
        }

        var root = await ReadJsonObjectFile(targetFilePath);
        if (root is null || !root.TryGetPropertyValue(request.TemplateId, out var itemNode) || itemNode is not JsonObject itemObject)
        {
            await WriteBadRequest(context, "Template ID not found");
            return;
        }

        if (itemObject["overrideProperties"] is not JsonObject overrideProps)
        {
            overrideProps = new JsonObject();
            itemObject["overrideProperties"] = overrideProps;
        }

        overrideProps["Damage"] = request.Damage;
        overrideProps["PenetrationPower"] = request.PenetrationPower;
        overrideProps["ArmorDamage"] = request.ArmorDamage;
        overrideProps["FragmentationChance"] = request.FragmentationChance;
        overrideProps["InitialSpeed"] = request.InitialSpeed;

        itemObject["fleaPriceRoubles"] = request.FleaPriceRoubles;
        itemObject["handbookPriceRoubles"] = request.HandbookPriceRoubles;

        if (itemObject["locales"] is JsonObject localesObj
            && localesObj["en"] is JsonObject enObj)
        {
            enObj["name"] = request.Name;
            enObj["shortName"] = request.ShortName;
            enObj["description"] = request.Description;
        }

        await WriteJsonObjectFile(targetFilePath, root);
        HyperLethalLog.Info("WebApp", $"Saved item {request.TemplateId} in {request.File}");

        await WriteJsonResponse(context, new { ok = true });
    }

    private async Task HandleSaveAssort(HttpContext context)
    {
        var request = await DeserializeRequest<AssortSaveRequest>(context);

        if (request is null || string.IsNullOrWhiteSpace(request.File) || string.IsNullOrWhiteSpace(request.OfferId))
        {
            await WriteBadRequest(context, "Invalid payload.");
            return;
        }
        
        var assortValidationError = ValidateAssortRequest(request);
        if (assortValidationError is not null)
        {
            await WriteBadRequest(context, assortValidationError);
            return;
        }

        if (!TryResolveFilePath(request.File, "db/assorts", out var targetFilePath))
        {
            await WriteBadRequest(context, "File path is outside db");
            return;
        }

        var root = await ReadJsonObjectFile(targetFilePath);
        if (root is null)
        {
            await WriteBadRequest(context, "Invalid assort json.");
            return;
        }

        foreach (var traderEntry in root)
        {
            if (traderEntry.Value is not JsonObject traderObj)
            {
                continue;
            }

            if (traderObj["items"] is JsonArray items)
            {
                foreach (var node in items)
                {
                    if (node is not JsonObject offer)
                    {
                        continue;
                    }

                    if (offer["_id"]?.GetValue<string>() != request.OfferId)
                    {
                        continue;
                    }

                    var upd = offer["upd"] as JsonObject ?? new JsonObject();
                    offer["upd"] = upd;
                    upd["UnlimitedCount"] = request.UnlimitedCount;
                    upd["StackObjectsCount"] = request.StackObjectsCount;
                    upd["BuyRestrictionMax"] = request.BuyRestrictionMax;

                    if (traderObj["barter_scheme"] is JsonObject barterScheme
                        && barterScheme[request.OfferId] is JsonArray lvl1
                        && lvl1.Count > 0
                        && lvl1[0] is JsonArray lvl2
                        && lvl2.Count > 0
                        && lvl2[0] is JsonObject priceObj)
                    {
                        priceObj["count"] = request.PriceRoubles;
                    }

                    if (traderObj["loyal_level_items"] is JsonObject loyals)
                    {
                        loyals[request.OfferId] = request.LoyaltyLevel;
                    }

                    await WriteJsonObjectFile(targetFilePath, root);
                    HyperLethalLog.Info("WebApp", $"Saved assort {request.OfferId} in {request.File}");
                    await WriteJsonResponse(context, new { ok = true });
                    return;
                }
            }
        }

        await WriteNotFound(context, "Offer id not found.");
    }

    private object LoadAssortData()
    {
        var assortsRootPath = Path.Combine(_modRootPath, "db", "assorts");
        var result = new List<object>();
        if (!Directory.Exists(assortsRootPath))
        {
            return new { offers = result };
        }

        var files = Directory.GetFiles(assortsRootPath, "*.json", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var json = File.ReadAllText(file);
            var root = JsonNode.Parse(json) as JsonObject;
            if (root is null)
            {
                continue;
            }

            var relativeFile = Path.GetRelativePath(_modRootPath, file).Replace("\\", "/");
            foreach (var traderEntry in root)
            {
                if (traderEntry.Value is not JsonObject traderObj)
                {
                    continue;
                }

                var traderId = traderEntry.Key;
                var barterScheme = traderObj["barter_scheme"] as JsonObject;
                var loyals = traderObj["loyal_level_items"] as JsonObject;

                if (traderObj["items"] is not JsonArray items)
                {
                    continue;
                }

                foreach (var node in items)
                {
                    if (node is not JsonObject offer)
                    {
                        continue;
                    }

                    var offerId = offer["_id"]?.GetValue<string>() ?? string.Empty;
                    var upd = offer["upd"] as JsonObject;
                    var priceRoubles = 0;
                    if (barterScheme is not null
                        && barterScheme[offerId] is JsonArray lvl1
                        && lvl1.Count > 0
                        && lvl1[0] is JsonArray lvl2
                        && lvl2.Count > 0
                        && lvl2[0] is JsonObject priceObj)
                    {
                        priceRoubles = priceObj["count"]?.GetValue<int>() ?? 0;
                    }

                    var loyalty = loyals?[offerId]?.GetValue<int>() ?? 1;
                    result.Add(new
                    {
                        file = relativeFile,
                        trader = traderId,
                        offerId,
                        templateId = offer["_tpl"]?.GetValue<string>() ?? string.Empty,
                        unlimitedCount = upd?["UnlimitedCount"]?.GetValue<bool>() ?? false,
                        stackObjectsCount = upd?["StackObjectsCount"]?.GetValue<int>() ?? 0,
                        buyRestrictionMax = upd?["BuyRestrictionMax"]?.GetValue<int>() ?? 0,
                        priceRoubles,
                        loyaltyLevel = loyalty
                    });
                }
            }
        }

        return new { offers = result };
    }

    private object LoadEditorData()
    {
        var itemsRootPath = Path.Combine(_modRootPath, "db", "items");
        var result = new List<object>();
        if (!Directory.Exists(itemsRootPath))
        {
            return new { items = result };
        }

        var files = Directory.GetFiles(itemsRootPath, "*.json", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var json = File.ReadAllText(file);
            var root = JsonNode.Parse(json) as JsonObject;
            if (root is null)
            {
                continue;
            }

            var relativeFile = Path.GetRelativePath(_modRootPath, file).Replace("\\", "/");
            foreach (var kvp in root)
            {
                if (kvp.Value is not JsonObject itemObj)
                {
                    continue;
                }

                var props = itemObj["overrideProperties"] as JsonObject;
                var enLocale = itemObj["locales"]?["en"] as JsonObject;
                result.Add(new
                {
                    file = relativeFile,
                    templateId = kvp.Key,
                    name = enLocale?["name"]?.GetValue<string>() ?? kvp.Key,
                    shortName = enLocale?["shortName"]?.GetValue<string>() ?? string.Empty,
                    description = enLocale?["description"]?.GetValue<string>() ?? string.Empty,
                    damage = props?["Damage"]?.GetValue<int>() ?? 0,
                    penetrationPower = props?["PenetrationPower"]?.GetValue<int>() ?? 0,
                    armorDamage = props?["ArmorDamage"]?.GetValue<int>() ?? 0,
                    fragmentationChance = props?["FragmentationChance"]?.GetValue<double>() ?? 0,
                    initialSpeed = props?["InitialSpeed"]?.GetValue<int>() ?? 0,
                    fleaPriceRoubles = itemObj["fleaPriceRoubles"]?.GetValue<int>() ?? 0,
                    handbookPriceRoubles = itemObj["handbookPriceRoubles"]?.GetValue<int>() ?? 0
                });
            }
        }

        return new { items = result };
    }

    private string GetWebFilePath(string fileName)
    {
        return Path.Combine(_modRootPath, "web", "hyperlethal", fileName);
    }

    private bool TryResolveFilePath(string relativeFilePath, string relativeRoot, out string targetFilePath)
    {
        targetFilePath = Path.GetFullPath(Path.Combine(_modRootPath, relativeFilePath));
        var rootPath = Path.GetFullPath(Path.Combine(_modRootPath, relativeRoot));
        return targetFilePath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase) && File.Exists(targetFilePath);
    }

    private static async Task<T?> DeserializeRequest<T>(HttpContext context)
    {
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<T>(body, JsonOptions);
    }

    private static async Task<JsonObject?> ReadJsonObjectFile(string filePath)
    {
        var text = await File.ReadAllTextAsync(filePath);
        return JsonNode.Parse(text) as JsonObject;
    }

    private static async Task WriteJsonObjectFile(string filePath, JsonObject root)
    {
        var updated = root.ToJsonString(JsonOptions);
        await File.WriteAllTextAsync(filePath, updated);
    }

    private static async Task WriteFileResponse(HttpContext context, string filePath, string contentType)
    {
        if (!File.Exists(filePath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.CompleteAsync();
            return;
        }

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = contentType;
        var bytes = await File.ReadAllBytesAsync(filePath);
        await context.Response.Body.WriteAsync(bytes);
        await context.Response.CompleteAsync();
    }

    private static async Task WriteJsonResponse(HttpContext context, object data)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "application/json; charset=utf-8";
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data, JsonOptions);
        await context.Response.Body.WriteAsync(bytes);
        await context.Response.CompleteAsync();
    }

    private static async Task WriteBadRequest(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await WriteJsonResponse(context, new { ok = false, message });
    }

    private static async Task WriteNotFound(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await WriteJsonResponse(context, new { ok = false, message });
    }

    private static async Task WriteStatus(HttpContext context, int statusCode)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.CompleteAsync();
    }

    private sealed record SaveRequest(
        string File,
        string TemplateId,
        string Name,
        string ShortName,
        string Description,
        int Damage,
        int PenetrationPower,
        int ArmorDamage,
        double FragmentationChance,
        int InitialSpeed,
        int FleaPriceRoubles,
        int HandbookPriceRoubles);

    private sealed record AssortSaveRequest(
        string File,
        string OfferId,
        int PriceRoubles,
        int LoyaltyLevel,
        bool UnlimitedCount,
        int StackObjectsCount,
        int BuyRestrictionMax);

    private static string? ValidateItemRequest(SaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TemplateId)) return "TemplateId is required.";
        if (string.IsNullOrWhiteSpace(request.Name)) return "Name is required.";
        if (string.IsNullOrWhiteSpace(request.ShortName)) return "ShortName is required.";
        if (string.IsNullOrWhiteSpace(request.Description)) return "Description is required.";

        if (request.Damage is < 1 or > 500) return "Damage must be between 1 and 500.";
        if (request.PenetrationPower is < 1 or > 200) return "PenetrationPower must be between 1 and 200.";
        if (request.ArmorDamage is < 1 or > 200) return "ArmorDamage must be between 1 and 200.";
        if (request.FragmentationChance is < 0 or > 1) return "FragmentationChance must be between 0.00 and 1.00.";
        if (request.InitialSpeed is < 50 or > 3000) return "InitialSpeed must be between 50 and 3000.";
        if (request.FleaPriceRoubles is < 1 or > 1_000_000) return "FleaPriceRoubles must be between 1 and 1000000.";
        if (request.HandbookPriceRoubles is < 1 or > 1_000_000) return "HandbookPriceRoubles must be between 1 and 1000000.";

        return null;
    }

    private static string? ValidateAssortRequest(AssortSaveRequest request)
    {
        if (request.PriceRoubles is < 1 or > 1_000_000) return "PriceRoubles must be between 1 and 1000000.";
        if (request.LoyaltyLevel is < 1 or > 4) return "LoyaltyLevel must be between 1 and 4.";
        if (request.StackObjectsCount is < 1 or > 999999) return "StackObjectsCount must be between 1 and 999999.";
        if (request.BuyRestrictionMax is < 0 or > 999999) return "BuyRestrictionMax must be between 0 and 999999.";
        return null;
    }
}
