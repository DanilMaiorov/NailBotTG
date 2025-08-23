using LinqToDB;
using LinqToDB.Data;
using NailBot.Core.Entities;

namespace NailBot.Infrastructure.DataAccess
{
        public class ToDoDataContext : DataConnection
        {
            public ToDoDataContext(string connectionString) : base(ProviderName.PostgreSQL, connectionString) { }

            public ITable<ToDoUserModel> ToDoUsers => this.GetTable<ToDoUserModel>();
            public ITable<ToDoItemModel> ToDoItems => this.GetTable<ToDoItemModel>();
            public ITable<ToDoListModel> ToDoLists => this.GetTable<ToDoListModel>();
        }
}
