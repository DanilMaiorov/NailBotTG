using NailBot.Core.Entities;

namespace NailBot.Core.Services
{
    public interface IToDoService
    {
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId);
        //Возвращает ToDoItem для UserId со статусом Active
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId);
        Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix);
        Task<ToDoItem> Add(ToDoUser user, string name);
        Task MarkCompleted(Guid id);
        Task Delete(Guid id);
    }
}

