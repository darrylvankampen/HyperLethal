using HyperLethal.Constants;

namespace HyperLethal.Ammo;

public static class HyperLethalAmmoDefinitions
{
    public static IReadOnlyList<HyperLethalAmmoDefinition> All { get; } =
    [
        new AmmoBuilder()
            .CloneFrom("59e690b686f7746c9f75e848")
            .WithId(HyperLethalIds.AmmoM995Plus)
            .WithParentId("5485a8684bdc2da71d8b4567")
            .WithName("HyperLethal M995+", "HL-M995+", "Overpressured 5.56 with extreme penetration and terminal energy.")
            .WithDamage(250)
            .WithPen(100)
            .WithArmorDamage(100)
            .WithFrag(0.95)
            .WithVelocity(1500)
            .WithPrice(1500)
            .Build()
    ];
}
