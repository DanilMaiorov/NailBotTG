using NailBot.Core.DataAccess;
using NailBot.Core.Entities;

namespace NailBot.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> ToDoList = new List<ToDoItem>();

        public void Add(ToDoItem item)
        {
            ToDoList.Add(item);
        }

        public void Delete(Guid id)
        {
            ToDoList.RemoveAll(x => x.Id == id);
        }
        
        public int CountActive(Guid userId)
        {
            return GetActiveByUserId(userId).Count();
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            if (userId == null)
            {
                return ToDoList;
            }

            return ToDoList
                .Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active)
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return ToDoList
                .Where(x => x.User.UserId == userId)
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            return ToDoList
                .Where(x => x.User.UserId == userId)
                .Where(predicate)
                .ToList();
        }

        public bool ExistsByName(Guid userId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return ToDoList
                .Where(x => x.User.UserId == userId)
                .Any(x => x.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase)); 
        }

        public ToDoItem? Get(Guid id)
        {
            return ToDoList?.FirstOrDefault(x => x.Id == id);
        }

        public void Update(ToDoItem item)
        {
            var updateItem = ToDoList.Find(x => x.Id == item.Id);

            var updateIndex = ToDoList.FindIndex(x => x.Id == item.Id);

            if (updateIndex != -1)
            {
                ToDoList[updateIndex] = item;
            }
            else
            {
                throw new KeyNotFoundException($"Задча с номером {item.Id} не найдена");
            }
        }
    }
}
