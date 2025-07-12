using NailBot.Core.DataAccess;
using NailBot.Core.Entities;

namespace NailBot.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> ToDoList = [];

        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            ToDoList.Add(item);
            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1, ct);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            ToDoList.RemoveAll(x => x.Id == id);
            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1, ct);
        }
        
        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            var countList = await GetActiveByUserId(userId, ct);

            return countList.Count;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            if (userId == Guid.Empty)
            {
                return ToDoList.AsReadOnly();
            }

            var result = ToDoList
                .Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active)
                .ToList()
                .AsReadOnly();

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1, ct);

            return result;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            var result = ToDoList
                .Where(x => x.User.UserId == userId)
                .ToList()
                .AsReadOnly();

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1, ct);

            return result;
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var result = ToDoList
                .Where(x => x.User.UserId == userId)
                .Where(predicate)
                .ToList();

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1, ct);

            return result;
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }
            var result = ToDoList
                .Where(x => x.User.UserId == userId)
                .Any(x => x.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase));

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1, ct);

            return result;
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            var item = ToDoList.FirstOrDefault(x => x.Id == id);

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1, ct);

            return item;
        }

        public async Task Update(ToDoItem item, CancellationToken ct)
        {
            var updateIndex = ToDoList.FindIndex(x => x.Id == item.Id);

            if (updateIndex != -1)
            {
                ToDoList[updateIndex] = item;

                //сделаю искусственную задержку для асинхронности
                await Task.Delay(1, ct);
            }
            else
            {
                throw new KeyNotFoundException($"Задча с номером {item.Id} не найдена");
            }
        }

        public Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
