using LinqToDB;
using NailBot.Core.DataAccess;
using NailBot.Core.Entities;
using System.Reflection;

namespace NailBot.Infrastructure.DataAccess
{
    internal class SqlToDoRepository : IToDoRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;
        public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        //РЕАЛИЗОВАНО
        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(item);
            await dbContext.InsertAsync(model, token: ct);
        }
        //РЕАЛИЗОВАНО
        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var active = await GetActiveByUserId(userId, ct);

            return active.Count;
        }
        //РЕАЛИЗОВАНО
        public async Task Delete(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            await dbContext.ToDoItems
                .Where(i => i.Id == id)
                .DeleteAsync(ct);
        }
        //РЕАЛИЗОВАНО
        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                return await Task.FromResult(false);

            using var dbContext = _factory.CreateDataContext();

            return await dbContext.ToDoItems
                .Where(i => i.UserId == userId)
                .AnyAsync(i => i.Name.ToLower().StartsWith(name.ToLower()), ct);
        }
        //РЕАЛИЗОВАНО
        //public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Expression<Func<ToDoItem, bool>> predicate, CancellationToken ct) - Хочется такую сигнатуру для оптимизации
        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var items = dbContext.ToDoItems
                .Where(i => i.UserId == userId)
                .Select(ModelMapper.MapFromModel)
                .ToList()
                .AsReadOnly();

            await Task.Delay(1);

            return items.Where(predicate).ToList();
        }
        //РЕАЛИЗОВАНО
        public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await dbContext.ToDoItems
                .Where(i => i.Id == id)
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .FirstOrDefaultAsync(ct);

            return ModelMapper.MapFromModel(model);
        }
        //РЕАЛИЗОВАНО
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            await Task.Delay(1);

            return dbContext.ToDoItems
                .Where(i => i.UserId == userId)
                .Where(i => i.State == ToDoItemState.Active)
                .Select(ModelMapper.MapFromModel)
                .ToList()
                .AsReadOnly();
        }
        //РЕАЛИЗОВАНО
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            await Task.Delay(1);

            return dbContext.ToDoItems
                .Where(i => i.UserId == userId)
                .LoadWith(i => i.User)
                .Select(ModelMapper.MapFromModel)
                .ToList()
                .AsReadOnly();
        }
        //РЕАЛИЗОВАНО
        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var query = dbContext.ToDoItems
                .Where(i => i.UserId == userId);

            if (listId.HasValue)
                query = query.Where(i => i.ToDoListId == listId.Value);
            else
                query = query.Where(i => i.ToDoListId == null);

            await Task.Delay(1);

            return query
                .LoadWith(i => i.User)
                .LoadWith(i => i.List)
                .Select(ModelMapper.MapFromModel)
                .ToList()
                .AsReadOnly();
        }
        //РЕАЛИЗОВАНО
        public async Task Update(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = ModelMapper.MapToModel(item);

            await dbContext.ToDoItems
                .Where(i => i.Id == model.Id)
                .Set(i => i.State, model.State)
                .Set(i => i.StateChangedAt, model.StateChangedAt)
                .UpdateAsync(ct);
        }
    }
}
