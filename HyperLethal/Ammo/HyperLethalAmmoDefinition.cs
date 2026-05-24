using HyperLethal.Items;

namespace HyperLethal.Ammo;

public sealed record HyperLethalAmmoDefinition : HyperLethalItemDefinition
{
    public required int Damage { get; init; }
    public required int Penetration { get; init; }
    public required int ArmorDamage { get; init; }
    public required double FragmentationChance { get; init; }
    public required int InitialSpeed { get; init; }
}
