using LinqToDB;
using NailBot.Core.Entities;
using NailBot.Core.Services;
using NailBot.Helpers;
using Polly;
using System.Threading;

namespace NailBot.Infrastructure.DataAccess
{
    internal class SqlToDoListRepository : IToDoListRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;
        public SqlToDoListRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }
        //РЕАЛИЗОВАНО
        public async Task Add(ToDoList list, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            await dbContext.InsertAsync(ModelMapper.MapToModel(list), token: ct);
        }
        //РЕАЛИЗОВАНО
        public async Task Delete(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var list = await dbContext.ToDoLists
                .Where(i => i.Id == id)
                .DeleteAsync(ct);
        }
        //РЕАЛИЗОВАНО
        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            if (string.IsNullOrWhiteSpace(name))
                return false;

            return await dbContext.ToDoLists
                .Where(l => l.UserId == userId)
                .AnyAsync(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        }
        //РЕАЛИЗОВАНО
        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var list = await dbContext.ToDoLists
                .Where(l => l.Id == id)
                .LoadWith(i => i.User)
                .FirstOrDefaultAsync();

            return ModelMapper.MapFromModel(list);
        }
        //РЕАЛИЗОВАНО
        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var lists = await dbContext.ToDoLists
                .Where(l => l.UserId == userId)
                .LoadWith(i => i.User)
                .ToListAsync();

            return lists
                .Select(ModelMapper.MapFromModel)
                .ToList()
                .AsReadOnly();
        }
    }
}
