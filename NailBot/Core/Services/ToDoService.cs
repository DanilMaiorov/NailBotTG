using NailBot.Core.DataAccess;
using NailBot.Core.Entities;
using NailBot.Core.Exceptions;
using NailBot.Helpers;
using NailBot.TelegramBot;
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
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct) 
        {
            return await _toDoRepository.GetAllByUserId(userId, ct);
        }
        //реализация метода интерфейса GetActiveByUserId
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct) 
        {
            return await _toDoRepository.GetActiveByUserId(userId, ct);
        }

        // реализация метода интерфейса Add
        public async Task<ToDoItem> Add(ToDoUser user, string name, CancellationToken ct)
        {
            var tasks = await GetAllByUserId(user.UserId, ct);

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

            await _toDoRepository.Add(newToDoItem, ct);

            return newToDoItem;
        }

        // реализация метода интерфейса Delete
        public async Task Delete(Guid id, CancellationToken ct)
        {

            await GetTask(id, "удалять", ct);
            
            await _toDoRepository.Delete(id, ct);
        }

        // реализация метода интерфейса MarkCompleted
        public async Task MarkCompleted(Guid id, CancellationToken ct)
        {
            var completedTask = await GetTask(id, "выполнять", ct);

            completedTask.State = ToDoItemState.Completed;
            completedTask.StateChangedAt = DateTime.Now;

             await _toDoRepository.Update(completedTask, ct);
        }

        // реализация метода интерфейса Find
        public async Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct)
        {
            var tasks = await GetAllByUserId(user.UserId, ct);

            if (tasks.Count == 0)
            {
                throw new EmptyTaskListException("искать");
            }

            bool isContain = await _toDoRepository.ExistsByName(user.UserId, namePrefix, ct);

            if (!isContain)
            {
                throw new ArgumentException($"Задач, начинающихся \"{namePrefix}\" не найдено.\n");
            }

            return await _toDoRepository.Find(user.UserId, item =>
                item.Name.Length >= namePrefix.Length &&
                item.Name.Substring(0, namePrefix.Length) == namePrefix, ct);
        }

        //проверка получения задачи
        async Task<ToDoItem?> GetTask(Guid id, string message, CancellationToken ct)
        {
            if (id == Guid.Empty)
            {
                throw new EmptyTaskListException(message);
            }

            var item = await _toDoRepository.Get(id, ct);

            var result = await GetAllByUserId(item.User.UserId, ct);

            return item == null
                ? throw new NullTaskException("Задача не существует или равна null")
                : result.FirstOrDefault(x => x.Id == id);
        }
    }
}
