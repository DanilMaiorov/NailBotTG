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
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId) 
        {
            return _toDoRepository.GetAllByUserId(userId);
        }
        //реализация метода интерфейса GetActiveByUserId
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId) 
        {
            return _toDoRepository.GetActiveByUserId(userId);
        }

        // реализация метода интерфейса Add
        public ToDoItem Add(ToDoUser user, string name)
        {
            var tasks = GetAllByUserId(user.UserId);

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

            _toDoRepository.Add(newToDoItem);

            return newToDoItem;
        }

        // реализация метода интерфейса Delete
        public void Delete(Guid id)
        {
            var removedProduct = GetTask(id, "удалять");

            _toDoRepository.Delete(id);
        }

        // реализация метода интерфейса MarkCompleted
        public void MarkCompleted(Guid id)
        {
            var completedTask = GetTask(id, "выполнять");

            completedTask.State = ToDoItemState.Completed;
            completedTask.StateChangedAt = DateTime.Now;

            _toDoRepository.Update(completedTask);
        }

        // реализация метода интерфейса Find
        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            var tasks = GetAllByUserId(user.UserId);

            if (tasks.Count == 0)
            {
                throw new EmptyTaskListException("искать");
            }

            bool isContain = _toDoRepository.ExistsByName(user.UserId, namePrefix);

            if (!isContain)
            {
                throw new ArgumentException($"Задач, начинающихся \"{namePrefix}\" не найдено.\n");
            }
            
            return _toDoRepository.Find(user.UserId, item =>
                item.Name.Length >= namePrefix.Length &&
                item.Name.Substring(0, namePrefix.Length) == namePrefix);
        }

        //проверка получения задачи
        ToDoItem? GetTask(Guid id, string message)
        {
            if (id == Guid.Empty)
            {
                throw new EmptyTaskListException(message);
            }

            var item = _toDoRepository.Get(id);

            return item == null
                ? throw new NullTaskException("Задача не существует или равна null")
                : GetAllByUserId(item.User.UserId).FirstOrDefault(x => x.Id == id);
        }
    }
}
