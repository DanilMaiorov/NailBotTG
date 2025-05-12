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
    //заведу делегат
    //delegate IReadOnlyList<ToDoItem> ListGetter(Guid userId);

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

        //Возвращает IReadOnlyList<ToDoItem> для UserId
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId) 
        {
            return _toDoRepository.GetAllByUserId(userId);
        } 
        //Возвращает IReadOnlyList<ToDoItem> для UserId со статусом Active
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId) 
        {
            return _toDoRepository.GetActiveByUserId(userId);
        }

        // реализация метода интерфейса Add
        public ToDoItem Add(ToDoUser user, string name)
        {
            var tasks = GetAllByUserId(user.UserId);

            //проверяю длину листа и выбрасываю исключение если больше лимита
            if (tasks.Count >= maxTaskAmount)
            {
                throw new TaskCountLimitException(maxTaskAmount);
            }

            //валидация строки c проверкой длины введёной задачи и выброс необходимого исключения - ДОБАВИЛ ПЕРЕГУЗКУ МЕТОДА ValidateString
            string newTask = Validate.ValidateString(name, maxTaskLength);

            //проверяю дубликаты введённой задачи
            Helper.CheckDuplicate(newTask, tasks);

            var newToDoItem = new ToDoItem
            {
                Name = newTask,
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                User = user,
                StateChangedAt = DateTime.Now,
            };

            //добавляю в репозиторий
            _toDoRepository.Add(newToDoItem);

            //возвращаю новый объект задачи
            return newToDoItem;
        }

        // реализация метода интерфейса Delete
        public void Delete(Guid id)
        {
            //достану удаляемый объект задачи
            var removedProduct = GetTask(id, "удалять");

            //удаляю задачу
            _toDoRepository.Delete(id);
        }

        // реализация метода интерфейса MarkCompleted
        public void MarkCompleted(Guid id)
        {
            //достану выполняемый объект задачи
            var completedTask = GetTask(id, "выполнять");

            //выполню её
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
            
            //return _toDoRepository.Find(user.UserId, item => item.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));

            return _toDoRepository.Find(user.UserId, item =>
                item.Name.Length >= namePrefix.Length &&
                item.Name.Substring(0, namePrefix.Length) == namePrefix);
        }

        //проверка получения задачи
        ToDoItem? GetTask(Guid id, string message)
        {
            //проверка на наличие переданной задачи
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
