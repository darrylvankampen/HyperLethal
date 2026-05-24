namespace HyperLethal.Traders;

// TODO: future addon?
public sealed record TraderOfferDefinition(
    string TraderId,
    string ItemTpl,
    int PriceRoubles,
    int LoyaltyLevel = 1,
    int StackCount = 30
);
