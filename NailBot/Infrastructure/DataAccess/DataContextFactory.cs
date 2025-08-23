using NailBot.Infrastructure.DataAccess;

namespace NailBot.Infrastructure.DataAccess
{
    public class DataContextFactory : IDataContextFactory<ToDoDataContext>
    {
        private string connectionString = "";
        public ToDoDataContext CreateDataContext()
        {
            return new ToDoDataContext(connectionString);
        }
    }
}