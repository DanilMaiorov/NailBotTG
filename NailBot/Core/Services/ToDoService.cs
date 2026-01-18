using Microsoft.Extensions.Options;
using NailBot.Core.DataAccess;
using NailBot.Core.Entities;
using NailBot.Core.Exceptions;
using NailBot.Helpers;
using NailBot.Options;

namespace NailBot.Core.Services
{
    class ToDoService : IToDoService
    {
        private readonly IToDoRepository _toDoRepository;

        private readonly int _maxTasksAmount;
        private readonly int _maxTaskLength;

        public ToDoService(IToDoRepository toDoRepository, IOptions<TaskOptions> options)
        {
            _toDoRepository = toDoRepository;

            _maxTasksAmount = options.Value.MaxTasksAmount;
            _maxTaskLength = options.Value.MaxTaskLength;
        }
        
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct) 
        {
            return await _toDoRepository.GetAllByUserId(userId, ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct) 
        {
            return await _toDoRepository.GetActiveByUserId(userId, ct);
        }
        
        public async Task<ToDoItem> Add(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken ct)
        {
            var newToDoItem = new ToDoItem
            {
                Name = name,
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                User = user,
                //UserId = user.UserId,
                StateChangedAt = DateTime.Now,
                Deadline = deadline,
                List = list,
            };
            ;
            await _toDoRepository.Add(newToDoItem, ct);

            return newToDoItem;
        }
        
        public async Task Delete(Guid id, CancellationToken ct)
        {
            var action = "удалять";

            var deleteTask = await GetTask(id, action, ct);

            await _toDoRepository.Delete(id, ct);
        }
        
        public async Task MarkCompleted(Guid id, CancellationToken ct)
        {
            var action = "выполнять";

            var completedTask = await GetTask(id, action, ct);

            await _toDoRepository.Update(completedTask, ct);
        }
        
        public async Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct)
        {
            var tasks = await GetAllByUserId(user.UserId, ct);

            if (tasks.Count == 0)
                throw new EmptyTaskListException("искать");

            bool isContain = await _toDoRepository.ExistsByName(user.UserId, namePrefix, ct);
            
            return await _toDoRepository.Find(user.UserId, item =>
                item.Name.Length >= namePrefix.Length &&
                item.Name.Substring(0, namePrefix.Length) == namePrefix, ct);
        }

        
        public async Task<string> ThrowIfHasDuplicatesOrWhiteSpace(string newTaskName, Guid userId, CancellationToken ct)
        {
            string taskName = Validate.ValidateString(newTaskName, _maxTaskLength);

            var items = await GetAllByUserId(userId, ct);

            if (items.Any(item => item.Name == taskName))
                throw new DuplicateTaskException(taskName);

            return taskName;
        }
        
        private async Task<ToDoItem?> GetTask(Guid id, string message, CancellationToken ct)
        {
            if (id == Guid.Empty)
                throw new EmptyTaskListException(message);
            
            var item = await _toDoRepository.Get(id, ct);

            var result = await GetAllByUserId(item.User.UserId, ct);

            return item == null
                ? throw new NullTaskException("Задача не существует или равна null")
                : result.FirstOrDefault(x => x.Id == id);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            return await _toDoRepository.GetByUserIdAndList(userId, listId, ct);
        }

        public async Task<ToDoItem?> Get(Guid toDoItemId, CancellationToken ct)
        {
            return await _toDoRepository.Get(toDoItemId, ct);
        }
    }
}
