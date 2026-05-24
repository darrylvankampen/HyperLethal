using HyperLethal.Constants;
using HyperLethal.Items;
using HyperLethal.Services;

namespace HyperLethal.Ammo;

/// <summary>
/// Fluent builder used by ammo factories and future JSON config ingestion.
/// </summary>
public sealed class AmmoBuilder
{
    private string _baseTpl = string.Empty;
    private string _newTpl = string.Empty;
    private string _parentId = String.Empty;
    private string _name = string.Empty;
    private string _shortName = string.Empty;
    private string _description = string.Empty;
    private int _price = 1;
    private int _damage = 1;
    private int _penetration = 1;
    private int _armorDamage = 1;
    private double _fragmentationChance;
    private int _initialSpeed = 1;
    
    public AmmoBuilder CloneFrom(string baseTpl)
    {
        _baseTpl = baseTpl;
        return this;
    }
    
    // NEW ID FOR ITEM
    public AmmoBuilder WithId(string newTpl)
    {
        _newTpl = newTpl;
        return this;
    }
    
    public AmmoBuilder WithParentId(string parentId)
    {
        _parentId = parentId;
        return this;
    }
    
    public AmmoBuilder WithName(string name, string shortName, string description)
    {
        _name = name;
        _shortName = shortName;
        _description = description;
        return this;
    }
    
    public AmmoBuilder WithDamage(int damage)
    {
        _damage = damage;
        return this;
    }
    
    public AmmoBuilder WithPen(int penetration)
    {
        _penetration = penetration;
        return this;
    }
    
    public AmmoBuilder WithArmorDamage(int armorDamage)
    {
        _armorDamage = armorDamage;
        return this;
    }
    
    public AmmoBuilder WithFrag(double fragChance)
    {
        _fragmentationChance = fragChance;
        return this;
    }
    
    public AmmoBuilder WithVelocity(int velocity)
    {
        _initialSpeed = velocity;
        return this;
    }
    
    public AmmoBuilder WithPrice(int price)
    {
        _price = price;
        return this;
    }
    
    public HyperLethalAmmoDefinition Build() => new()
    {
        BaseTpl = _baseTpl,
        NewTpl = _newTpl,
        ParentId = _parentId,
        Name = _name,
        ShortName = _shortName,
        Description = _description,
        Price = _price,
        HandbookParentId = "5b47574386f77428ca22b33b",
        ItemType = OpItemType.Ammo,
        Damage = _damage,
        Penetration = _penetration,
        ArmorDamage = _armorDamage,
        FragmentationChance = _fragmentationChance,
        InitialSpeed = _initialSpeed
    };
    
    // public void Register(HyperLethalAmmoFactory factory)
    // {
    //     factory.RegisterAmmo(Build());
    // }
}
