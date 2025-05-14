using NailBot.Core.Entities;

namespace NailBot.Core.DataAccess
{
    public interface IUserRepository
    {
        Task<ToDoUser?> GetUser(Guid userId);
        Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId);
        Task Add(ToDoUser user);
    }
}
