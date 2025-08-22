using LinqToDB.Data;
using NailBot.Core.DataAccess;
using NailBot.Core.Entities;
using System.Data.Common;

namespace NailBot.Infrastructure.DataAccess
{
    internal class SqlToDoRepository : IToDoRepository
    {
        private readonly IDataContextFactory<DataConnection> _factory;
        public SqlToDoRepository(IDataContextFactory<DataConnection> factory) 
        { 
            _factory = factory;
        }





        public Task Add(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            throw new NotImplementedException();
        }

        public Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            throw new NotImplementedException();
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            throw new NotImplementedException();
        }

        public Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            throw new NotImplementedException();
        }

        public Task Update(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            throw new NotImplementedException();
        }
    }
}
