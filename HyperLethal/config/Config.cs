using System.Text.Json;

namespace HyperLethal.Config;

public sealed class Config
{
    public bool ReplaceTraderImage { get; set; }
    public bool Development { get; set; }
    public bool MakeItABitHarder { get; set; }
    public Dictionary<string, JsonElement> HarderDefaultProperties { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Dictionary<string, JsonElement>> HarderItemOverrides { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
