using NailBot.Core.DataAccess;
using NailBot.Core.Entities;
using NailBot.Core.Exceptions;
using NailBot.Helpers;
using NailBot.TelegramBot;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System.Xml.Linq;

namespace NailBot.Core.Services
{
    class ToDoService : IToDoService
    {
        private readonly IToDoRepository _toDoRepository;

        private readonly int maxTaskAmount;
        private readonly int maxTaskLength;

        public ToDoService(IToDoRepository toDoRepository, int taskAmount, int taskLength)
        {
            _toDoRepository = toDoRepository;

            maxTaskAmount = taskAmount;
            maxTaskLength = taskLength;
        }

        //реализация метода интерфейса GetAllByUserId
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId) 
        {
            return await _toDoRepository.GetAllByUserId(userId);
        }
        //реализация метода интерфейса GetActiveByUserId
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId) 
        {
            return await _toDoRepository.GetActiveByUserId(userId);
        }

        // реализация метода интерфейса Add
        public async Task<ToDoItem> Add(ToDoUser user, string name)
        {
            var tasks = await GetAllByUserId(user.UserId);

            if (tasks.Count >= maxTaskAmount)
            {
                throw new TaskCountLimitException(maxTaskAmount);
            }

            string newTask = Validate.ValidateString(name, maxTaskLength);

            Helper.CheckDuplicate(newTask, tasks);

            var newToDoItem = new ToDoItem
            {
                Name = newTask,
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                User = user,
                StateChangedAt = DateTime.Now,
            };

            await _toDoRepository.Add(newToDoItem);

            return newToDoItem;
        }

        // реализация метода интерфейса Delete
        public async Task Delete(Guid id)
        {
            _ = GetTask(id, "удалять");

            await _toDoRepository.Delete(id);
        }

        // реализация метода интерфейса MarkCompleted
        public async Task MarkCompleted(Guid id)
        {
            var completedTask = await GetTask(id, "выполнять");

            completedTask.State = ToDoItemState.Completed;
            completedTask.StateChangedAt = DateTime.Now;

             await _toDoRepository.Update(completedTask);
        }

        // реализация метода интерфейса Find
        public async Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix)
        {
            var tasks = await GetAllByUserId(user.UserId);

            if (tasks.Count == 0)
            {
                throw new EmptyTaskListException("искать");
            }

            bool isContain = await _toDoRepository.ExistsByName(user.UserId, namePrefix);

            if (!isContain)
            {
                throw new ArgumentException($"Задач, начинающихся \"{namePrefix}\" не найдено.\n");
            }

            return await _toDoRepository.Find(user.UserId, item =>
                item.Name.Length >= namePrefix.Length &&
                item.Name.Substring(0, namePrefix.Length) == namePrefix);
        }

        //проверка получения задачи
        async Task<ToDoItem?> GetTask(Guid id, string message)
        {
            if (id == Guid.Empty)
            {
                throw new EmptyTaskListException(message);
            }

            var item = await _toDoRepository.Get(id);

            var result = await GetAllByUserId(item.User.UserId);

            return item == null
                ? throw new NullTaskException("Задача не существует или равна null")
                : result.FirstOrDefault(x => x.Id == id);
        }
    }
}
