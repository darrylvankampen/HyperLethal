using HyperLethal.Constants;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace HyperLethal.Utilities;

public static class BarterSchemeFactory
{
    public static List<List<BarterScheme>> CreateRoubleBarter(int price)
    {
        return
        [
            [
                new BarterScheme
                {
                    Count = price,
                    Template = HyperLethalDefaults.RubCurrencyTpl
                }
            ]
        ];
    }
}
