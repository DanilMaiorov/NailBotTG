using NailBot.Core.DataAccess;
using NailBot.Core.Entities;

namespace NailBot.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> ToDoList = [];

        public async Task Add(ToDoItem item)
        {
            ToDoList.Add(item);
            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1);
        }

        public async Task Delete(Guid id)
        {
            ToDoList.RemoveAll(x => x.Id == id);
            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1);
        }
        
        public async Task<int> CountActive(Guid userId)
        {
            var countList = await GetActiveByUserId(userId);

            return countList.Count;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return ToDoList.AsReadOnly();
            }

            var result = ToDoList
                .Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active)
                .ToList()
                .AsReadOnly();

            return await Task.FromResult(result);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId)
        {
            var result = ToDoList
                .Where(x => x.User.UserId == userId)
                .ToList()
                .AsReadOnly();

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1);

            return result;
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            var result = ToDoList
                .Where(x => x.User.UserId == userId)
                .Where(predicate)
                .ToList();

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1);

            return result;
        }

        public async Task<bool> ExistsByName(Guid userId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }
            var result = ToDoList
                .Where(x => x.User.UserId == userId)
                .Any(x => x.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase));

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1);

            return result;
        }

        public async Task<ToDoItem?> Get(Guid id)
        {
            var item = ToDoList.FirstOrDefault(x => x.Id == id);

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1);

            return item;
        }

        public async Task Update(ToDoItem item)
        {
            var updateIndex = ToDoList.FindIndex(x => x.Id == item.Id);

            if (updateIndex != -1)
            {
                ToDoList[updateIndex] = item;

                //сделаю искусственную задержку для асинхронности
                await Task.Delay(1);
            }
            else
            {
                throw new KeyNotFoundException($"Задча с номером {item.Id} не найдена");
            }
        }
    }
}
