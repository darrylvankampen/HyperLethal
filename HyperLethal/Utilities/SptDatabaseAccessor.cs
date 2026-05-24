using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Servers;

namespace HyperLethal.Utilities;

[Injectable]
public sealed class SptDatabaseAccessor(DatabaseServer databaseServer)
{
    public bool TemplateExists(string tpl)
    {
        return databaseServer.GetTables().Templates.Items.ContainsKey(tpl);
    }
    
    public bool TryTemplateExists(string tpl, out bool exists)
    {
        exists = TemplateExists(tpl);
        return true;
    }
    
    public bool TraderExists(string traderId)
    {
        return databaseServer.GetTables().Traders.ContainsKey(traderId);
    }

    public bool TryTraderExists(string traderId, out bool exists)
    {
        exists = TraderExists(traderId);
        return true;
    }

    // unused at this moment
    public bool HandbookParentExists(string handbookParentId)
    {
        return databaseServer.GetTables().Templates.Handbook.Items.Any(x => x.Id == handbookParentId);
    }
    
    // TODO
    public bool TryEnableFlea(string itemTpl)
    {
        return true;
    }

}
