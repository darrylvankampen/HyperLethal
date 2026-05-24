using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace HyperLethal.Utilities;

public static class TraderAssortFactory
{
    public static Item CreateSingleItem(string tpl, int stackCount = 30)
    {
        var item = new Item
        {
            Id = tpl,
            Template = tpl,
            ParentId = "hideout",
            SlotId = "hideout"
        };

        EnsureUpdFields(item, stackCount);
        return item;
    }
    // make sure upd fields are there
    private static void EnsureUpdFields(Item item, int stackCount)
    {
        var updProperty = item.GetType().GetProperty("Upd");
        if (updProperty is null)
        {
            return;
        }

        var updValue = updProperty.GetValue(item);
        if (updValue is null)
        {
            var updType = updProperty.PropertyType;
            updValue = Activator.CreateInstance(updType);
            if (updValue is null)
            {
                return;
            }

            updProperty.SetValue(item, updValue);
        }
        
        SetNumericProperty(updValue, "StackObjectsCount", stackCount * 5);
        SetNumericProperty(updValue, "BuyRestrictionMax", stackCount * 5);
        SetNumericProperty(updValue, "BuyRestrictionCurrent", 0);
    }
    
    private static void SetNumericProperty(object target, string propertyName, int value)
    {
        var property = target.GetType().GetProperty(propertyName);
        if (property is null || !property.CanWrite)
        {
            return;
        }

        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        object converted = targetType == typeof(int) ? value
            : targetType == typeof(double) ? (double)value
            : targetType == typeof(decimal) ? (decimal)value
            : Convert.ChangeType(value, targetType);

        property.SetValue(target, converted);
    }
}
