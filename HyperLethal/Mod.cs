using HyperLethal.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Services.Mod;

namespace HyperLethal;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.darihon.hyperlethal";
    public override string Name { get; init; } = "HyperLethal";
    public override string Author { get; init; } = "Darihon";
    public override SemanticVersioning.Version Version { get; init; } = new("0.1.0-beta1");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override string License { get; init; } = "MIT";
    public override bool? IsBundleMod { get; init; } = false;

    public override string Url { get; init; } = "";
    public override List<string> Contributors { get; init; } = [];
    public override Dictionary<string, SemanticVersioning.Range> ModDependencies { get; init; } = new()
    {
        { "com.wtt.commonlib", new SemanticVersioning.Range("~2.0.0") }
    };
    public override List<string> Incompatibilities { get; init; } = [];
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class HyperLethal(
    HyperLethalBootstrapService bootstrapService
) : IOnLoad
{
    public Task OnLoad()
    {
        return bootstrapService.Initialize();
    }
}
