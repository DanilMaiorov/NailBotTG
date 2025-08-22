using LinqToDB;
using LinqToDB.Data;
using NailBot.Core.Entities;

namespace NailBot.Infrastructure.DataAccess
{
        public class ToDoDataContext : DataConnection
        {
            public ToDoDataContext(string connectionString) : base(ProviderName.PostgreSQL, connectionString) { }

            public ITable<ToDoUser> ToDoUsers => this.GetTable<ToDoUser>();
            public ITable<ToDoItem> ToDoItems => this.GetTable<ToDoItem>();
            public ITable<ToDoList> ToDoLists => this.GetTable<ToDoList>();
        }
}
