using LinqToDB.Data;

namespace NailBot.Infrastructure.DataAccess
{
    public interface IDataContextFactory<TDataContext> where TDataContext : DataConnection
    {
        TDataContext CreateDataContext();
    }
}
