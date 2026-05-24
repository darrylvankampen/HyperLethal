namespace HyperLethal.Items;


public abstract record HyperLethalItemDefinition
{
    public required string BaseTpl { get; init; }
    public required string NewTpl { get; init; }

    public required string ParentId { get; init; }
    public required string Name { get; init; }
    public required string ShortName { get; init; }
    public required string Description { get; init; }
    public required int Price { get; init; }
    public required string HandbookParentId { get; init; }
    public required OpItemType ItemType { get; init; }
}
