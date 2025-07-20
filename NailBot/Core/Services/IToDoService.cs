using NailBot.Core.Entities;

namespace NailBot.Core.Services
{
    public interface IToDoService
    {
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct);
        //Возвращает ToDoItem для UserId со статусом Active
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct);
        Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken ct);
        Task<ToDoItem> Add(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken ct);
        Task MarkCompleted(Guid id, CancellationToken ct);
        Task Delete(Guid id, CancellationToken ct);

        Task<string> ThrowIfHasDuplicatesOrWhiteSpace(string name, Guid id, CancellationToken ct);


        //ДЗ 11
        Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct);
        //Task Add(ToDoUser userObj, string taskName, DateTime deadline, List<ToDoList> list, CancellationToken ct);
    }
}
